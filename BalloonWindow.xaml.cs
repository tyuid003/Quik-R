using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickReply;

public partial class BalloonWindow : Window
{
    private string _message = string.Empty;

    public BalloonWindow(BalloonData data)
    {
        InitializeComponent();
        Data = data;
        BalloonId = data.Id;
        Message = data.Text;
        ApplyStyle(data.BackgroundHex, data.Opacity);
    }

    public int BalloonId { get; }
    public BalloonData Data { get; }

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            RefreshDisplay();
        }
    }

    public event EventHandler? EditRequested;
    public event EventHandler? DeleteRequested;
    public event EventHandler<string>? MessageInvoked;
    public event EventHandler<string>? MessageSendInvoked;
    public event EventHandler<int>? WheelScrolled;
    public event EventHandler<double>? ReorderDragCompleted;

    public void ApplyStyle(string backgroundHex, double opacity)
    {
        System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(backgroundHex);
        byte alpha = (byte)Math.Round(Math.Clamp(opacity, 0.25, 1.0) * 255, MidpointRounding.AwayFromZero);
        RootBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, color.R, color.G, color.B));
    }

    public void RefreshDisplay()
    {
        string displayText = string.IsNullOrWhiteSpace(Data.DisplayName) ? _message : Data.DisplayName;
        MessageButton.Content = new TextBlock
        {
            Text = displayText,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.NoWrap,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void EditButton_OnClick(object sender, RoutedEventArgs e)
    {
        EditRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MessageButton_OnClick(object sender, RoutedEventArgs e)
    {
        MessageInvoked?.Invoke(this, Message);
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DeleteRequested?.Invoke(this, EventArgs.Empty);
    }

    private void SendButton_OnClick(object sender, RoutedEventArgs e)
    {
        MessageSendInvoked?.Invoke(this, Message);
    }

    private void DragButton_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
            ReorderDragCompleted?.Invoke(this, Top + (Height / 2));
        }
        catch
        {
        }
    }

    private void Window_OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        WheelScrolled?.Invoke(this, e.Delta);
        e.Handled = true;
    }
}
