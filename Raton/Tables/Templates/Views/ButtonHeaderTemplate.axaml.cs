using Avalonia;
using Avalonia.Controls.Primitives;
using System.Windows.Input;

namespace Raton.Tables.Templates.Views;

public class ButtonHeaderTemplate : TemplatedControl
{

    public static readonly StyledProperty<bool> TemplateIsAddPanelVisibleProperty =
        AvaloniaProperty.Register<ButtonHeaderTemplate, bool>(nameof(TemplateIsAddPanelVisible));

    public bool TemplateIsAddPanelVisible
    {
        get => this.GetValue(TemplateIsAddPanelVisibleProperty);
        set => SetValue(TemplateIsAddPanelVisibleProperty, value);
    }



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

    public static readonly StyledProperty<ICommand> ShowOrHideAddPanelCommandProperty =
        AvaloniaProperty.Register<ButtonHeaderTemplate, ICommand>(nameof(ShowOrHideAddPanelCommandProperty));

    public ICommand ShowOrHideAddPanelCommand
    {
        get => this.GetValue(ShowOrHideAddPanelCommandProperty);
        set => SetValue(ShowOrHideAddPanelCommandProperty, value);
    }
}