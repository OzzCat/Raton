﻿using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Media;
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
using System.Windows.Input;

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
        public ReactiveCommand<ITableModel?, Unit> AddItemCommand { get; }
        public ReactiveCommand<int, Unit> DeleteItemCommand { get; }
        #endregion

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

            var buttonHeader = new Templates.Views.ButtonHeaderTemplate()
            {
                SaveChangesCommand = SaveAllChangesCommand,
                DiscardChangesCommand = DiscardAllChangesCommand,
                AddItemTemplateCommand = AddItemCommand
            };

            ItemsTree = new FlatTreeDataGridSource<T>(new List<T>())
            {
                Columns =
                {
                    new TemplateColumn<T>(buttonHeader,
                        "EditCell",
                        null,
                        GridLength.Auto)
                },
            };
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

        protected virtual Action<ITableModel?> AddItem =>
            (ITableModel? tModel) =>
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
