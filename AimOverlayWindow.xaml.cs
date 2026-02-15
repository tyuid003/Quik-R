using System;
using System.Windows;
using System.Windows.Input;

namespace QuickReply;

public sealed class AimTargetSelectedEventArgs : EventArgs
{
    public AimTargetSelectedEventArgs(IntPtr windowHandle, NativeMethods.POINT screenPoint)
    {
        WindowHandle = windowHandle;
        ScreenPoint = screenPoint;
    }

    public IntPtr WindowHandle { get; }
    public NativeMethods.POINT ScreenPoint { get; }
}

public partial class AimOverlayWindow : Window
{
    public AimOverlayWindow()
    {
        InitializeComponent();

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    public event EventHandler<AimTargetSelectedEventArgs>? TargetSelected;
    public event EventHandler? SelectionCanceled;

    private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        System.Windows.Point screenPoint = PointToScreen(e.GetPosition(this));
        NativeMethods.POINT point = new()
        {
            X = (int)screenPoint.X,
            Y = (int)screenPoint.Y
        };

        Hide();
        IntPtr targetHandle = NativeMethods.WindowFromPoint(point);
        TargetSelected?.Invoke(this, new AimTargetSelectedEventArgs(targetHandle, point));
        Close();
    }

    private void Window_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            SelectionCanceled?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
