using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QuickReply;

internal sealed class AimCaptureService : IDisposable
{
    private IntPtr _hookHandle = IntPtr.Zero;
    private NativeMethods.LowLevelMouseProc? _hookProc;
    private Action<IntPtr, NativeMethods.POINT>? _onCaptured;

    public bool IsCapturing => _hookHandle != IntPtr.Zero;

    public bool StartCapture(Action<IntPtr, NativeMethods.POINT> onCaptured)
    {
        if (IsCapturing)
        {
            return false;
        }

        _onCaptured = onCaptured;
        _hookProc = HookCallback;

        using Process currentProcess = Process.GetCurrentProcess();
        using ProcessModule processModule = currentProcess.MainModule!;

        IntPtr moduleHandle = NativeMethods.GetModuleHandle(processModule.ModuleName);
        _hookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _hookProc, moduleHandle, 0);
        return _hookHandle != IntPtr.Zero;
    }

    public void StopCapture()
    {
        if (_hookHandle == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
        _hookProc = null;
        _onCaptured = null;
    }

    public void Dispose()
    {
        StopCapture();
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_LBUTTONDOWN)
        {
            NativeMethods.MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            NativeMethods.POINT clickPoint = hookStruct.pt;
            IntPtr windowHandle = NativeMethods.WindowFromPoint(clickPoint);
            Action<IntPtr, NativeMethods.POINT>? callback = _onCaptured;

            StopCapture();

            if (callback is not null)
            {
                App.Current.Dispatcher.BeginInvoke(() => callback(windowHandle, clickPoint));
            }

            return (IntPtr)1;
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }
}
