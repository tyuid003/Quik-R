using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace QuickReply;

public partial class TargetPulseWindow : Window
{
    public TargetPulseWindow(NativeMethods.POINT point)
    {
        InitializeComponent();

        Left = point.X - (Width / 2);
        Top = point.Y - (Height / 2);
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        DoubleAnimation scaleAnimation = new()
        {
            From = 1.0,
            To = 3.8,
            Duration = TimeSpan.FromMilliseconds(700),
            EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
        };

        DoubleAnimation fadeAnimation = new()
        {
            From = 0.95,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(700)
        };

        RingScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleAnimation);
        RingScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleAnimation);
        PulseRing.BeginAnimation(OpacityProperty, fadeAnimation);

        await Task.Delay(760);
        Close();
    }
}
