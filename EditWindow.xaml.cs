using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace QuickReply;

public partial class EditWindow : Window
{
    private string _selectedColorHex;

    public EditWindow(string initialMessage, string initialDisplayName, string initialColorHex, double initialOpacity)
    {
        InitializeComponent();
        DisplayNameTextBox.Text = initialDisplayName;
        MessageTextBox.Text = initialMessage;
        _selectedColorHex = string.IsNullOrWhiteSpace(initialColorHex) ? "#FFFFFF" : initialColorHex;
        OpacitySlider.Value = Math.Clamp(initialOpacity, 0.25, 1.0);

        OpacitySlider.ValueChanged += (_, _) => UpdatePreview();
        UpdatePreview();

        MessageTextBox.Focus();
        MessageTextBox.CaretIndex = MessageTextBox.Text.Length;
    }

    public string DisplayName => DisplayNameTextBox.Text.Trim();
    public string EditedMessage => MessageTextBox.Text;
    public string SelectedColorHex => _selectedColorHex;
    public double SelectedOpacity => Math.Round(OpacitySlider.Value, 2);

    private void PickColorButton_OnClick(object sender, RoutedEventArgs e)
    {
        WinForms.ColorDialog dialog = new()
        {
            AllowFullOpen = true,
            AnyColor = true,
            FullOpen = true,
            Color = System.Drawing.ColorTranslator.FromHtml(_selectedColorHex)
        };

        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            _selectedColorHex = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
            UpdatePreview();
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void UpdatePreview()
    {
        System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_selectedColorHex);
        byte alpha = (byte)Math.Round(OpacitySlider.Value * 255, MidpointRounding.AwayFromZero);
        System.Windows.Media.Color preview = System.Windows.Media.Color.FromArgb(alpha, color.R, color.G, color.B);
        ColorPreview.Background = new SolidColorBrush(preview);
        ColorHexText.Text = _selectedColorHex;
        OpacityValueText.Text = $"{OpacitySlider.Value:P0}";
    }
}
