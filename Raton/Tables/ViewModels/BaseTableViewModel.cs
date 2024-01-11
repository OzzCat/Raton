using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DynamicData;
using Raton.Tables.Models;
using Raton.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Raton.Tables.ViewModels
{
    public class BaseTableViewModel<T> : ReactiveObject, IRoutableViewModel, IUpdatable where T : class, ITableModel
    {
        #region Routing
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; } = nameof(T) + "table";
        #endregion

        #region Collections
        protected SourceCache<T, int> _items;

        protected ReadOnlyObservableCollection<T>? _itemsList;

        public FlatTreeDataGridSource<T> ItemsTree { get; }
        #endregion

        #region DB Commands
        public ReactiveCommand<int, Unit> SaveItemChangesCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAllChangesCommand { get; }
        public ReactiveCommand<int, Unit> DiscardItemChangesCommand { get; }
        public ReactiveCommand<Unit, Unit> DiscardAllChangesCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowOrHideAddPanelCommand { get; }
        public ReactiveCommand<bool, Unit> AddItemCommand { get; }
        public ReactiveCommand<int, Unit> DeleteItemCommand { get; }
        #endregion

        #region Sorting
        protected int IsCheckedSortDescending(T? x, T? y)
        {
            if (x is null || y is null)
                return Comparer<T>.Default.Compare(x, y);
            var a = x.IsDirty;
            var b = y.IsDirty;
            return Comparer<bool>.Default.Compare(b, a);
        }
        #endregion

        private T? _newItem;
        public T? NewItem
        {
            get => _newItem;
            set => this.RaiseAndSetIfChanged(ref _newItem, value);
        }

        private bool _isAddPanelVisible;
        public bool IsAddPanelVisible
        {
            get => _isAddPanelVisible;
            set => this.RaiseAndSetIfChanged(ref _isAddPanelVisible, value);
        }

        public BaseTableViewModel(IScreen screen)
        {
            HostScreen = screen;

            _items = new SourceCache<T, int>(x => x.TableID);

            #region DB Commands
            SaveItemChangesCommand = ReactiveCommand.Create(SaveItemChanges);

            SaveAllChangesCommand = ReactiveCommand.CreateFromTask(async () => 
            SaveAllChanges());

            DiscardItemChangesCommand = ReactiveCommand.Create(DiscardItemChanges);

            DiscardAllChangesCommand = ReactiveCommand.CreateFromTask(async () => 
            DiscardAllChanges());

            AddItemCommand = ReactiveCommand.Create(AddItem);

            DeleteItemCommand = ReactiveCommand.Create(DeleteItem);
            #endregion

            ShowOrHideAddPanelCommand = ReactiveCommand.Create(ShowOrHideAddPanel);

            var buttonHeader = new Templates.Views.ButtonHeaderTemplate()
            {
                SaveChangesCommand = SaveAllChangesCommand,
                DiscardChangesCommand = DiscardAllChangesCommand,
                ShowOrHideAddPanelCommand = ShowOrHideAddPanelCommand,
                TemplateIsAddPanelVisible = IsAddPanelVisible,
            };

            ItemsTree = new FlatTreeDataGridSource<T>(new List<T>())
            {
                Columns =
                {
                    new TemplateColumn<T>(buttonHeader,
                        "EditCell",
                        null,
                        GridLength.Auto,
                        new()
                        {
                            CompareAscending = IsCheckedSortDescending,
                        })
                },
            };

            IsAddPanelVisible = false;
        }

        protected virtual Action<int> SaveItemChanges =>
            (int tableID) =>
            {
                throw new NotImplementedException();
            };

        public virtual void SaveAllChanges()
        {
            var dirtyItems = _items.Items.Where(x => x.IsDirty is true);
            foreach (var item in dirtyItems)
            {
                SaveItemChanges(item.TableID);
            }
        }

        protected virtual Action<int> DiscardItemChanges =>
            (int tableID) =>
            {
                throw new NotImplementedException();
            };

        public virtual void DiscardAllChanges()
        {
            
            var dirtyItems = _items.Items.Where(x => x.IsDirty is true);
            foreach (var item in dirtyItems)
            {
                DiscardItemChanges(item.TableID);
            }
        }

        protected virtual Action ShowOrHideAddPanel =>
            () =>
            {
                IsAddPanelVisible = !IsAddPanelVisible;
            };

        protected virtual Action<bool> AddItem =>
            async (bool DiscardEditingValues) =>
            {
                throw new NotImplementedException();
            };

        protected virtual Action<int> DeleteItem =>
            (int tableID) =>
            {
                throw new NotImplementedException();
            };

        public virtual void UpdateView()
        {
            throw new NotImplementedException();
        }
    }
}
