using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;

namespace QuickReply;

public partial class MainWindow : Window
{
    private const int MaxVisibleBalloons = 10;

    private readonly List<BalloonWindow> _balloons = new();
    private readonly string _balloonStorePath;
    private readonly NotifyIcon _trayIcon;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly Icon _trayAppIcon;

    private int _nextBalloonId = 1;
    private int _visibleStartIndex;
    private IntPtr _targetWindowHandle = IntPtr.Zero;
    private NativeMethods.POINT _targetClickPoint;
    private bool _hasTargetPoint;
    private bool _isAimSelecting;
    private bool _isExiting;
    private bool _isShuttingDownWindows;
    private string _searchText = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
        Left = 30;
        Top = 30;

        string appDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Quik-R");
        _balloonStorePath = Path.Combine(appDataRoot, "balloons");

        Directory.CreateDirectory(_balloonStorePath);
        TryMigrateLegacyBalloonData();

        _trayAppIcon = LoadTrayIcon();

        _trayIcon = new NotifyIcon
        {
            Icon = _trayAppIcon,
            Visible = true,
            Text = "QuickReply"
        };

        ContextMenuStrip menu = new();
        menu.Items.Add("Open", null, (_, _) => BringMainToFront());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => BringMainToFront();

        LocationChanged += (_, _) => UpdateBalloonPositions();
        Closing += OnMainWindowClosing;
        Closed += OnMainWindowClosed;

        LoadPersistedBalloons();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        BalloonData data = new()
        {
            Id = _nextBalloonId++,
            DisplayName = string.Empty,
            Text = $"ข้อความด่วน {_nextBalloonId - 1}",
            BackgroundHex = "#FFFFFF",
            Opacity = 0.8,
            Order = _balloons.Count
        };

        BalloonWindow balloon = CreateBalloonWindow(data);
        _balloons.Add(balloon);

        EnsureLastBalloonVisible();
        SaveAllBalloons();
        UpdateBalloonPositions();
    }

    private void Header_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (e.OriginalSource is DependencyObject source)
        {
            if (FindAncestor<System.Windows.Controls.Button>(source) is not null ||
                FindAncestor<System.Windows.Controls.TextBox>(source) is not null)
            {
                return;
            }
        }

        try
        {
            DragMove();
        }
        catch
        {
        }
    }

    private void MainWindow_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollVisibleBalloons(e.Delta);
        e.Handled = true;
    }

    private void SearchTextBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        int caretIndex = SearchTextBox.CaretIndex;
        _searchText = SearchTextBox.Text.Trim();
        _visibleStartIndex = 0;
        UpdateBalloonPositions();

        if (!SearchTextBox.IsKeyboardFocusWithin)
        {
            SearchTextBox.Focus();
        }

        SearchTextBox.CaretIndex = Math.Min(caretIndex, SearchTextBox.Text.Length);
    }

    private void AimButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_isAimSelecting)
        {
            return;
        }

        _isAimSelecting = true;
        AimButton.Content = "◉";

        AimOverlayWindow overlayWindow = new();
        overlayWindow.TargetSelected += (_, targetInfo) =>
        {
            _isAimSelecting = false;
            AimButton.Content = "◎";

            IntPtr clickedHandle = targetInfo.WindowHandle;
            if (clickedHandle == IntPtr.Zero)
            {
                return;
            }

            IntPtr rootWindow = NativeMethods.GetAncestor(clickedHandle, NativeMethods.GA_ROOT);
            _targetWindowHandle = rootWindow == IntPtr.Zero ? clickedHandle : rootWindow;
            _targetClickPoint = targetInfo.ScreenPoint;
            _hasTargetPoint = true;
            ShowTargetPulse(targetInfo.ScreenPoint);
        };

        overlayWindow.SelectionCanceled += (_, _) =>
        {
            _isAimSelecting = false;
            AimButton.Content = "◎";
        };

        overlayWindow.Show();
        overlayWindow.Activate();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        ExitApplication();
    }

    private void CollapseButton_OnClick(object sender, RoutedEventArgs e)
    {
        HideToTray();
    }

    private void BalloonOnEditRequested(object? sender, EventArgs e)
    {
        if (sender is not BalloonWindow balloon)
        {
            return;
        }

        EditWindow editWindow = new(
            balloon.Data.Text,
            balloon.Data.DisplayName,
            balloon.Data.BackgroundHex,
            balloon.Data.Opacity)
        {
            Owner = this
        };

        bool? result = editWindow.ShowDialog();
        if (result == true)
        {
            balloon.Data.DisplayName = editWindow.DisplayName;
            balloon.Data.Text = editWindow.EditedMessage;
            balloon.Data.BackgroundHex = editWindow.SelectedColorHex;
            balloon.Data.Opacity = editWindow.SelectedOpacity;
            balloon.Message = balloon.Data.Text;
            balloon.ApplyStyle(balloon.Data.BackgroundHex, balloon.Data.Opacity);
            balloon.RefreshDisplay();
            SaveBalloon(balloon.Data);
        }
    }

    private void BalloonOnDeleteRequested(object? sender, EventArgs e)
    {
        if (sender is not BalloonWindow balloon)
        {
            return;
        }

        MessageBoxResult confirm = System.Windows.MessageBox.Show(
            "ยืนยันการลบข้อความนี้?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        RemoveBalloon(balloon);
    }

    private async void BalloonOnMessageInvoked(object? sender, string message)
    {
        await PasteMessageToTargetAsync(message, sendAfterPaste: false);
    }

    private async void BalloonOnMessageSendInvoked(object? sender, string message)
    {
        await PasteMessageToTargetAsync(message, sendAfterPaste: true);
    }

    private void BalloonOnWheelScrolled(object? sender, int delta)
    {
        ScrollVisibleBalloons(delta);
    }

    private void BalloonOnReorderDragCompleted(object? sender, double dropCenterY)
    {
        if (sender is not BalloonWindow dragged)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            UpdateBalloonPositions();
            return;
        }

        int start = _visibleStartIndex;
        int end = Math.Min(_balloons.Count, _visibleStartIndex + MaxVisibleBalloons);
        int currentIndex = _balloons.IndexOf(dragged);
        if (currentIndex < start || currentIndex >= end)
        {
            UpdateBalloonPositions();
            return;
        }

        int insertIndex = end - 1;
        for (int index = start; index < end; index++)
        {
            if (_balloons[index] == dragged)
            {
                continue;
            }

            double mid = _balloons[index].Top + (_balloons[index].Height / 2);
            if (dropCenterY < mid)
            {
                insertIndex = index;
                break;
            }
        }

        _balloons.Remove(dragged);
        if (insertIndex > currentIndex)
        {
            insertIndex--;
        }

        insertIndex = Math.Clamp(insertIndex, start, Math.Max(start, end - 1));
        _balloons.Insert(insertIndex, dragged);

        SaveAllBalloons();
        UpdateBalloonPositions();
    }

    private async Task PasteMessageToTargetAsync(string message, bool sendAfterPaste)
    {
        if (_targetWindowHandle == IntPtr.Zero || !_hasTargetPoint)
        {
            return;
        }

        NativeMethods.SetForegroundWindow(_targetWindowHandle);
        await Task.Delay(130);

        IntPtr runtimeTarget = NativeMethods.WindowFromPoint(_targetClickPoint);
        if (runtimeTarget == IntPtr.Zero)
        {
            runtimeTarget = _targetWindowHandle;
        }

        NativeMethods.POINT clientPoint = _targetClickPoint;
        NativeMethods.ScreenToClient(runtimeTarget, ref clientPoint);
        IntPtr clickLParam = NativeMethods.MakeLParam(clientPoint.X, clientPoint.Y);

        NativeMethods.PostMessage(runtimeTarget, NativeMethods.WM_MOUSEACTIVATE, _targetWindowHandle, clickLParam);
        NativeMethods.PostMessage(runtimeTarget, NativeMethods.WM_LBUTTONDOWN, (IntPtr)1, clickLParam);
        NativeMethods.PostMessage(runtimeTarget, NativeMethods.WM_LBUTTONUP, IntPtr.Zero, clickLParam);

        await Task.Delay(70);

        System.Windows.Clipboard.SetText(message, System.Windows.TextDataFormat.UnicodeText);
        await Task.Delay(30);

        NativeMethods.SendMessageTimeout(
            runtimeTarget,
            NativeMethods.WM_PASTE,
            IntPtr.Zero,
            IntPtr.Zero,
            NativeMethods.SMTO_ABORTIFHUNG,
            150,
            out _);

        await Task.Delay(40);

        NativeMethods.keybd_event(NativeMethods.VK_CONTROL, 0, 0, UIntPtr.Zero);
        NativeMethods.keybd_event(NativeMethods.VK_V, 0, 0, UIntPtr.Zero);
        NativeMethods.keybd_event(NativeMethods.VK_V, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        NativeMethods.keybd_event(NativeMethods.VK_CONTROL, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

        if (sendAfterPaste)
        {
            await Task.Delay(35);
            NativeMethods.keybd_event(NativeMethods.VK_RETURN, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_RETURN, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }

    private void UpdateBalloonPositions()
    {
        const double spacing = 2;
        List<BalloonWindow> filtered = GetFilteredBalloons();
        int maxStartIndex = Math.Max(0, filtered.Count - MaxVisibleBalloons);
        _visibleStartIndex = Math.Clamp(_visibleStartIndex, 0, maxStartIndex);
        double nextTop = Top + Height + spacing;
        int visibleEndIndex = Math.Min(filtered.Count, _visibleStartIndex + MaxVisibleBalloons);

        HashSet<BalloonWindow> visibleSet = new();

        for (int index = _visibleStartIndex; index < visibleEndIndex; index++)
        {
            BalloonWindow balloon = filtered[index];
            visibleSet.Add(balloon);

            balloon.Left = Left;
            balloon.Top = nextTop;
            if (!balloon.IsVisible)
            {
                balloon.Show();
            }

            nextTop += balloon.Height + spacing;
        }

        foreach (BalloonWindow balloon in _balloons)
        {
            if (!visibleSet.Contains(balloon) && balloon.IsVisible)
            {
                balloon.Hide();
            }
        }
    }

    private void ScrollVisibleBalloons(int delta)
    {
        List<BalloonWindow> filtered = GetFilteredBalloons();
        if (filtered.Count <= MaxVisibleBalloons)
        {
            return;
        }

        int maxStartIndex = filtered.Count - MaxVisibleBalloons;
        int nextStart = _visibleStartIndex + (delta < 0 ? 1 : -1);
        nextStart = Math.Clamp(nextStart, 0, maxStartIndex);

        if (nextStart == _visibleStartIndex)
        {
            return;
        }

        _visibleStartIndex = nextStart;
        UpdateBalloonPositions();
    }

    private void EnsureLastBalloonVisible()
    {
        List<BalloonWindow> filtered = GetFilteredBalloons();

        if (filtered.Count <= MaxVisibleBalloons)
        {
            _visibleStartIndex = 0;
            return;
        }

        _visibleStartIndex = filtered.Count - MaxVisibleBalloons;
    }

    private BalloonWindow CreateBalloonWindow(BalloonData data)
    {
        BalloonWindow balloon = new(data)
        {
            Left = Left,
            Width = 420,
            Height = 40,
            Topmost = true
        };

        balloon.EditRequested += BalloonOnEditRequested;
        balloon.DeleteRequested += BalloonOnDeleteRequested;
        balloon.MessageInvoked += BalloonOnMessageInvoked;
        balloon.MessageSendInvoked += BalloonOnMessageSendInvoked;
        balloon.WheelScrolled += BalloonOnWheelScrolled;
        balloon.ReorderDragCompleted += BalloonOnReorderDragCompleted;
        balloon.Closed += BalloonOnClosed;

        return balloon;
    }

    private void BalloonOnClosed(object? sender, EventArgs e)
    {
        if (sender is not BalloonWindow balloon)
        {
            return;
        }

        if (_isShuttingDownWindows)
        {
            return;
        }

        if (_balloons.Contains(balloon))
        {
            RemoveBalloon(balloon, fromClosedEvent: true);
        }
    }

    private void RemoveBalloon(BalloonWindow balloon, bool fromClosedEvent = false)
    {
        _balloons.Remove(balloon);

        if (!fromClosedEvent)
        {
            balloon.Close();
        }

        DeleteBalloonFiles(balloon.BalloonId);

        if (_visibleStartIndex > 0 && _visibleStartIndex >= _balloons.Count)
        {
            _visibleStartIndex = Math.Max(0, _balloons.Count - MaxVisibleBalloons);
        }

        SaveAllBalloons();
        UpdateBalloonPositions();
    }

    private void LoadPersistedBalloons()
    {
        List<BalloonData> dataItems = LoadBalloonDataFiles();
        if (dataItems.Count == 0)
        {
            return;
        }

        foreach (BalloonData data in dataItems.OrderBy(static item => item.Order).ThenBy(static item => item.Id))
        {
            BalloonWindow balloon = CreateBalloonWindow(data);
            _balloons.Add(balloon);
        }

        _nextBalloonId = Math.Max(_nextBalloonId, dataItems.Max(static item => item.Id) + 1);
        _visibleStartIndex = 0;
        UpdateBalloonPositions();
    }

    private List<BalloonData> LoadBalloonDataFiles()
    {
        List<BalloonData> result = new();

        foreach (string jsonPath in Directory.GetFiles(_balloonStorePath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(jsonPath, Encoding.UTF8);
                BalloonData? data = JsonSerializer.Deserialize<BalloonData>(json);
                if (data is null)
                {
                    continue;
                }

                if (data.Id <= 0)
                {
                    if (int.TryParse(Path.GetFileNameWithoutExtension(jsonPath), out int idFromFile))
                    {
                        data.Id = idFromFile;
                    }
                }

                if (string.IsNullOrWhiteSpace(data.Text))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(data.BackgroundHex))
                {
                    data.BackgroundHex = "#FFFFFF";
                }

                if (data.DisplayName is null)
                {
                    data.DisplayName = string.Empty;
                }

                if (data.Opacity < 0.25 || data.Opacity > 1.0)
                {
                    data.Opacity = 0.8;
                }

                result.Add(data);
            }
            catch
            {
            }
        }

        foreach (string txtPath in Directory.GetFiles(_balloonStorePath, "*.txt"))
        {
            string fileName = Path.GetFileNameWithoutExtension(txtPath);
            if (!int.TryParse(fileName, out int id))
            {
                continue;
            }

            if (result.Any(item => item.Id == id))
            {
                continue;
            }

            string text = File.ReadAllText(txtPath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            result.Add(new BalloonData
            {
                Id = id,
                DisplayName = string.Empty,
                Text = text,
                BackgroundHex = "#FFFFFF",
                Opacity = 0.8,
                Order = id
            });
        }

        return result;
    }

    private void SaveAllBalloons()
    {
        for (int index = 0; index < _balloons.Count; index++)
        {
            BalloonData data = _balloons[index].Data;
            data.Order = index;
            SaveBalloon(data);
        }
    }

    private void SaveBalloon(BalloonData data)
    {
        string jsonPath = GetBalloonJsonPath(data.Id);
        string json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(jsonPath, json, Encoding.UTF8);

        string txtPath = GetBalloonLegacyTextPath(data.Id);
        File.WriteAllText(txtPath, data.Text, Encoding.UTF8);
    }

    private void DeleteBalloonFiles(int balloonId)
    {
        string jsonPath = GetBalloonJsonPath(balloonId);
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
        }

        string txtPath = GetBalloonLegacyTextPath(balloonId);
        if (File.Exists(txtPath))
        {
            File.Delete(txtPath);
        }
    }

    private string GetBalloonJsonPath(int balloonId)
    {
        return Path.Combine(_balloonStorePath, $"{balloonId}.json");
    }

    private string GetBalloonLegacyTextPath(int balloonId)
    {
        return Path.Combine(_balloonStorePath, $"{balloonId}.txt");
    }

    private void BringMainToFront()
    {
        Show();
        Activate();
        NativeMethods.SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(this).Handle);
        UpdateBalloonPositions();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        Close();
    }

    private void HideToTray()
    {
        Hide();

        foreach (BalloonWindow balloon in _balloons)
        {
            balloon.Hide();
        }
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        _isShuttingDownWindows = true;

        if (_trayIcon.Visible)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        _trayAppIcon.Dispose();

        foreach (BalloonWindow balloon in _balloons.ToArray())
        {
            balloon.Closed -= BalloonOnClosed;
            balloon.Close();
        }
    }

    private void ShowTargetPulse(NativeMethods.POINT clickPoint)
    {
        TargetPulseWindow pulseWindow = new(clickPoint);
        pulseWindow.Show();
    }

    private void OnMainWindowClosing(object? sender, CancelEventArgs e)
    {
        if (_isExiting)
        {
            return;
        }

        e.Cancel = true;
        HideToTray();
    }

    private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject? current = child;
        while (current is not null)
        {
            if (current is T matched)
            {
                return matched;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static Icon LoadTrayIcon()
    {
        string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
        if (File.Exists(iconPath))
        {
            return new Icon(iconPath);
        }

        string currentExePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(currentExePath) && File.Exists(currentExePath))
        {
            System.Drawing.Icon? extracted = System.Drawing.Icon.ExtractAssociatedIcon(currentExePath);
            if (extracted is not null)
            {
                return (Icon)extracted.Clone();
            }
        }

        return (Icon)SystemIcons.Application.Clone();
    }

    private void TryMigrateLegacyBalloonData()
    {
        string legacyDir = Path.Combine(AppContext.BaseDirectory, "balloons");
        if (!Directory.Exists(legacyDir))
        {
            return;
        }

        foreach (string file in Directory.GetFiles(legacyDir, "*.*", SearchOption.TopDirectoryOnly))
        {
            string extension = Path.GetExtension(file).ToLowerInvariant();
            if (extension is not ".json" and not ".txt")
            {
                continue;
            }

            string fileName = Path.GetFileName(file);
            string destination = Path.Combine(_balloonStorePath, fileName);
            if (!File.Exists(destination))
            {
                File.Copy(file, destination, overwrite: false);
            }
        }
    }

    private List<BalloonWindow> GetFilteredBalloons()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return _balloons;
        }

        return _balloons
            .Where(balloon =>
                ContainsIgnoreCase(balloon.Data.DisplayName, _searchText) ||
                ContainsIgnoreCase(balloon.Data.Text, _searchText))
            .ToList();
    }

    private static bool ContainsIgnoreCase(string text, string keyword)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
        {
            return false;
        }

        return text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
