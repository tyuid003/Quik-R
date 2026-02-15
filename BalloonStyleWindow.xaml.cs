using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickReply;

public partial class BalloonStyleWindow : Window
{
    public BalloonStyleWindow(string initialHex, double initialOpacity)
    {
        InitializeComponent();
        SelectedColorHex = string.IsNullOrWhiteSpace(initialHex) ? "#FFFFFF" : initialHex;
        SelectedOpacity = Math.Clamp(initialOpacity, 0.25, 1.0);

        OpacitySlider.Value = SelectedOpacity;
        SelectColorByHex(SelectedColorHex);
        UpdatePreview();

        ColorCombo.SelectionChanged += (_, _) => UpdatePreview();
        OpacitySlider.ValueChanged += (_, _) => UpdatePreview();
    }

    public string SelectedColorHex { get; private set; }
    public double SelectedOpacity { get; private set; }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ColorCombo.SelectedItem is ComboBoxItem item && item.Tag is string colorHex)
        {
            SelectedColorHex = colorHex;
        }

        SelectedOpacity = Math.Round(OpacitySlider.Value, 2);
        DialogResult = true;
        Close();
    }

    private void SelectColorByHex(string colorHex)
    {
        string normalized = colorHex.ToUpperInvariant();

        foreach (object item in ColorCombo.Items)
        {
            if (item is ComboBoxItem combo && combo.Tag is string hex && hex.ToUpperInvariant() == normalized)
            {
                ColorCombo.SelectedItem = combo;
                return;
            }
        }

        ColorCombo.SelectedIndex = 0;
    }

    private void UpdatePreview()
    {
        if (ColorCombo.SelectedItem is not ComboBoxItem item || item.Tag is not string colorHex)
        {
            return;
        }

        System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
        double opacity = OpacitySlider.Value;
        byte alpha = (byte)Math.Round(opacity * 255, MidpointRounding.AwayFromZero);
        System.Windows.Media.Color preview = System.Windows.Media.Color.FromArgb(alpha, color.R, color.G, color.B);
        PreviewBorder.Background = new SolidColorBrush(preview);
    }
}
