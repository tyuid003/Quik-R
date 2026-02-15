using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace QuickReply;

public partial class App : System.Windows.Application
{
	private const string SingleInstanceMutexName = "QuikR.SingleInstance";
	private const string ActivateEventName = "QuikR.ActivateExisting";

	private Mutex? _singleInstanceMutex;
	private EventWaitHandle? _activateEvent;
	private CancellationTokenSource? _activationCts;
	private bool _ownsMutex;

	protected override void OnStartup(StartupEventArgs e)
	{
		bool createdNew;
		_singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out createdNew);

		if (!createdNew)
		{
			SignalExistingInstance();
			Shutdown();
			return;
		}

		_ownsMutex = true;
		_activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);
		_activationCts = new CancellationTokenSource();
		_ = Task.Run(() => ListenForActivationRequests(_activationCts.Token));

		base.OnStartup(e);

		MainWindow mainWindow = new();
		MainWindow = mainWindow;
		mainWindow.Show();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		if (_activationCts is not null)
		{
			_activationCts.Cancel();
			_activationCts.Dispose();
		}

		_activateEvent?.Dispose();

		if (_ownsMutex)
		{
			_singleInstanceMutex?.ReleaseMutex();
		}

		_singleInstanceMutex?.Dispose();
		base.OnExit(e);
	}

	private void SignalExistingInstance()
	{
		try
		{
			using EventWaitHandle eventHandle = EventWaitHandle.OpenExisting(ActivateEventName);
			eventHandle.Set();
		}
		catch
		{
		}
	}

	private void ListenForActivationRequests(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			if (_activateEvent is null)
			{
				return;
			}

			bool activated = _activateEvent.WaitOne(250);
			if (!activated || cancellationToken.IsCancellationRequested)
			{
				continue;
			}

			Dispatcher.Invoke(BringMainWindowToFront);
		}
	}

	private void BringMainWindowToFront()
	{
		if (MainWindow is not Window window)
		{
			return;
		}

		if (!window.IsVisible)
		{
			window.Show();
		}

		if (window.WindowState == WindowState.Minimized)
		{
			window.WindowState = WindowState.Normal;
		}

		window.Activate();
		NativeMethods.SetForegroundWindow(new WindowInteropHelper(window).Handle);
	}
}
