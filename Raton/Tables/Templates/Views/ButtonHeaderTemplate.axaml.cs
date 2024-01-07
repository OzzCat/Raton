using Avalonia;
using Avalonia.Controls.Primitives;
using Raton.Tables.Models;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;

namespace Raton.Tables.Templates.Views;

public class ButtonHeaderTemplate : TemplatedControl
{
    public static readonly StyledProperty<ICommand> SaveChangesCommandProperty =
        AvaloniaProperty.Register<ButtonHeaderTemplate, ICommand>(nameof(SaveChangesCommandProperty));

    public ICommand SaveChangesCommand
    {
        get => this.GetValue(SaveChangesCommandProperty);
        set => SetValue(SaveChangesCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> DiscardChangesCommandProperty =
        AvaloniaProperty.Register<ButtonHeaderTemplate, ICommand>(nameof(DiscardChangesCommandProperty));

    public ICommand DiscardChangesCommand
    {
        get => this.GetValue(DiscardChangesCommandProperty);
        set => SetValue(DiscardChangesCommandProperty, value);
    }

    public static readonly StyledProperty<ReactiveCommand<ITableModel?, Unit>> AddItemTemplateCommandProperty =
        AvaloniaProperty.Register<ButtonHeaderTemplate, ReactiveCommand<ITableModel?, Unit>>(nameof(AddItemTemplateCommandProperty));

    public ReactiveCommand<ITableModel?, Unit> AddItemTemplateCommand
    {
        get => this.GetValue(AddItemTemplateCommandProperty);
        set => SetValue(AddItemTemplateCommandProperty, value);
    }
}