using Avalonia;
using Avalonia.Controls.Primitives;
using System;

namespace Raton.Controls;

public class TestControl : TemplatedControl
{

    /// <summary>
    /// IsCapable StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<DateTime> IsCapableProperty =
        AvaloniaProperty.Register<TestControl, DateTime>(nameof(IsCapable), DateTime.Today);

    /// <summary>
    /// Gets or sets the IsCapable property. This StyledProperty
    /// indicates ....
    /// </summary>
    public DateTime IsCapable
    {
        get => this.GetValue(IsCapableProperty);
        set => SetValue(IsCapableProperty, value);
    }


}