using DynamicData;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Raton.Services.DbServices;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Raton.Tables.Models;
using Raton.Tables.Templates.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Raton.Models.DbModels.Enums;
using Raton.Models.DbModels;

namespace Raton.Tables.ViewModels
{
    public class AnimalTableViewModel : BaseTableViewModel<TableAnimalModel>
    {
        #region Collections
        public ObservableCollection<string> SexEnum { get; private set; }
        #endregion

        #region Headers
        CustomHeader IDHeader = new CustomHeader("ID");
        CustomHeader SexHeader = new CustomHeader("Sex");
        CustomHeader CommentHeader = new CustomHeader("Comment");
        #endregion

        #region Filtering
        Func<TableAnimalModel, bool> IDFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return animal => true;
            return animal => animal.ID.ToLower().Contains(text.ToLower());
        }
        
        Func<TableAnimalModel, bool> SexFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return animal => true;
            return animal => animal.Sex.ToLower().Contains(text.ToLower());
        }

        Func<TableAnimalModel, bool> CommentFilter(string text)
        {
            if (string.IsNullOrEmpty(text)) return animal => true;
            return animal => !string.IsNullOrEmpty(animal.Comment) && animal.Comment.ToLower().Contains(text.ToLower());
        }
        #endregion

        private readonly IAnimalService _animalService;

        public AnimalTableViewModel(IScreen screen, IAnimalService animalService) : base(screen)
        {
            _animalService = animalService;

            NewItem = new TableAnimalModel();

            Observable.Start(() => {
                UpdateView();
            }, RxApp.TaskpoolScheduler);

            SexEnum = new ObservableCollection<string>(Enum.GetNames<SexEnumClass.SexEnum>());

            #region Filtering
            var filterIDPredicate = this.WhenAnyValue(x => x.IDHeader.SearchText)
                          .Throttle(TimeSpan.FromSeconds(.25))
                          .DistinctUntilChanged()
                          .Select(IDFilter);
            
            var filterSexPredicate = this.WhenAnyValue(x => x.SexHeader.SearchText)
                          .Throttle(TimeSpan.FromSeconds(.25))
                          .DistinctUntilChanged()
                          .Select(SexFilter);

            var filterCommentPredicate = this.WhenAnyValue(x => x.CommentHeader.SearchText)
                          .Throttle(TimeSpan.FromSeconds(.25))
                          .DistinctUntilChanged()
                          .Select(CommentFilter);
            #endregion

            _items.Connect()
                .Filter(filterIDPredicate)
                .Filter(filterSexPredicate)
                .Filter(filterCommentPredicate)
                .Bind(out _itemsList)
                .DisposeMany()
                .Subscribe();

            #region DirtyCheckers
            _items.Connect()
                .WhenPropertyChanged(x => x.ID, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

            _items.Connect()
                .WhenPropertyChanged(x => x.Sex, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });

            _items.Connect()
                .WhenPropertyChanged(x => x.Comment, false)
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    value.Sender.IsDirty = true;
                });
            #endregion

            ItemsTree.Items = _itemsList;

            #region Setup Columns
            ItemsTree.Columns.Insert(
                0,
                new TextColumn<TableAnimalModel, string>(
                        IDHeader,
                        x => x.ID,
                        (r, v) => r.ID = v ?? string.Empty,
                        new GridLength(2, GridUnitType.Star))
                );
            ItemsTree.Columns.Insert(
                1,
                new TemplateColumn<TableAnimalModel>(SexHeader, "SexCell", "SexCellEdit", GridLength.Star)
                );
            ItemsTree.Columns.Insert(
                2,
                new TextColumn<TableAnimalModel, string>(
                        CommentHeader,
                        x => x.Comment,
                        (r, v) => r.Comment = v ?? string.Empty,
                        new GridLength(4, GridUnitType.Star))
                );
            #endregion
        }

        protected override Action<int> SaveItemChanges =>
        async (int tableID) =>
        {
            var animal = _items.Lookup(tableID).Value;

            #region Validate Input
            if (string.IsNullOrEmpty(animal.ID))
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Animal ID can't be empty",
                    ButtonEnum.Ok);

                await box.ShowWindowAsync();

                return;
            }
            #endregion

            var dbAnimal = _animalService.GetByID(tableID);
            
            #region Check Animal Existance
            if (dbAnimal == null)
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Animal was already deleted from the database",
                    ButtonEnum.Ok);

                await box.ShowWindowAsync();

                return;
            }
            #endregion

            #region Check Unique
            var testUnique = _animalService.GetByID(animal.ID);

            if (testUnique is not null)
            {
                var boxUnique = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Animal with the same ID already exists",
                    ButtonEnum.Ok);

                await boxUnique.ShowWindowAsync();
                return;
            }
            #endregion

            dbAnimal.ID = animal.ID;
            dbAnimal.Sex = SexEnumClass.ConvertStringToSexEnum(animal.Sex);
            dbAnimal.Comment = animal.Comment;

            _animalService.Update(dbAnimal);

            animal.IsDirty = false;
        };

        protected override Action<int> DiscardItemChanges =>
        async (int tableID) =>
        {
            var dbAnimal = _animalService.GetByID(tableID);
            if (dbAnimal == null)
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Animal was already deleted from the database",
                    ButtonEnum.Ok);

                await box.ShowWindowAsync();

                return;
            }
            _items.AddOrUpdate(new TableAnimalModel(dbAnimal));
        };

        protected override Action<bool> AddItem =>
        async (bool DiscardEditingValues) =>
        {
            #region Validate Input
            if (NewItem is null)
                throw new ApplicationException();

            if (string.IsNullOrEmpty(NewItem.ID))
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Animal ID can't be empty",
                    ButtonEnum.Ok);

                await box.ShowWindowAsync();
                return;
            }
            #endregion

            #region Check Unique
            var testUnique = _animalService.GetByID(NewItem.ID);

            if (testUnique is not null)
            {
                var boxUnique = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Animal with the same ID already exists",
                    ButtonEnum.Ok);

                await boxUnique.ShowWindowAsync();
                return;
            }
            #endregion

            var dbAnimal = new AnimalModel
            {
                ID = NewItem.ID,
                Sex = SexEnumClass.ConvertStringToSexEnum(NewItem.Sex),
                Comment = NewItem.Comment
            };

            _animalService.Add(dbAnimal);

            _items.AddOrUpdate(new TableAnimalModel(dbAnimal));

            if (DiscardEditingValues)
                NewItem = new TableAnimalModel();
        };

        protected override Action<int> DeleteItem =>
        async (int tableID) =>
        {
            var box = MessageBoxManager
                        .GetMessageBoxStandard("Delete animal",
                        "Are you sure you want to delete this animal?" +
                        "\nAll catches of this animal will also be deleted",
                        ButtonEnum.YesNo);

            var deleteConfirmation = await box.ShowWindowAsync();

            if (deleteConfirmation.Equals(ButtonResult.Yes))
            {
                _animalService.RemoveByPK(tableID);

                _items.Remove(tableID);
            }
        };

        public override void UpdateView()
        {
            var animalSource = _animalService.GetAll();

            _items.Edit(innerList =>
            {
                innerList.Clear();
                foreach (var animal in animalSource)
                {
                    innerList.AddOrUpdate(new TableAnimalModel(animal));
                }
            });

        }
    }
}
