﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using DirectX12Template.Common;
using TerraFX.Interop;

namespace DirectX12Template
{
    /// <inheritdoc />
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    internal sealed unsafe class App : IFrameworkView
    {
        private static class ReturnCodes
        {
            public const int Success = 0;
        }

        private static int Main()
        {
            var direct3DApplicationSource = new Direct3DApplicationSource();
            CoreApplication.Run(direct3DApplicationSource);
            return ReturnCodes.Success;
        }

        private UWPDirectXMain  _main;
        private bool _windowClosed = false;
        private bool _windowVisible = true;
        private DeviceResources _deviceResources;

        public void Initialize(CoreApplicationView applicationView)
        {
            applicationView.Activated += OnActivated;
            CoreApplication.Suspending += OnSuspending;
            CoreApplication.Resuming += OnResuming;

        }

        public void SetWindow(CoreWindow window)
        {
            window.SizeChanged += OnWindowSizeChanged;
            window.VisibilityChanged += OnVisibilityChanged;
            window.Closed += OnWindowClosed;

            DisplayInformation currentDisplayInformation = DisplayInformation.GetForCurrentView();

            currentDisplayInformation.DpiChanged += OnDpiChanged;
            currentDisplayInformation.OrientationChanged += OnOrientationChanged;
            DisplayInformation.DisplayContentsInvalidated += OnDisplayContentsInvalidated;
        }

        public void Load(string entryPoint)
        {
            if (_main is null)
                _main = new UWPDirectXMain();
        }

        public void Run()
        {
            while (!_windowClosed)
            {
                if (_windowVisible)
                {
                    CoreWindow.GetForCurrentThread().Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

                    ID3D12CommandQueue* commandQueue = GetDeviceResources().CommandQueue;
                    _main.Update();

                    if (_main.Render())
                    {
                        GetDeviceResources().Present();
                    }
                }
                else
                {
                    CoreWindow.GetForCurrentThread().Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessOneAndAllPending);
                }
            }
        }

        public void Uninitialize()
        {
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

            _main.OnSuspending();
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            _main.OnResuming();
        }

        private void OnActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            CoreWindow.GetForCurrentThread().Activate();
        }

        private void OnDisplayContentsInvalidated(DisplayInformation sender, object args)
        {
            GetDeviceResources().ValidateDevice();
        }

        private void OnOrientationChanged(DisplayInformation sender, object args)
        {
            GetDeviceResources().SetCurrentOrientation(sender.CurrentOrientation);
        }

        private void OnDpiChanged(DisplayInformation sender, object args)
        {
            GetDeviceResources().SetDpi(sender.LogicalDpi);
            _main.OnWindowSizeChanged();
        }

        private void OnWindowClosed(CoreWindow sender, CoreWindowEventArgs args)
        {
            _windowClosed = true;
        }

        private void OnVisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            _windowVisible = args.Visible;
        }

        private void OnWindowSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            GetDeviceResources().SetLogicalSize(new Size(sender.Bounds.Width, sender.Bounds.Height));
            _main.OnWindowSizeChanged();
        }

        private DeviceResources GetDeviceResources()
        {
            if (_deviceResources != null && _deviceResources.IsDeviceRemoved)
            {
                // All references to the existing D3D device must be released before a new device
                // can be created.

                _deviceResources = null;
                _main.OnDeviceRemoved();

#if DEBUG
                ComPtr<IDXGIDebug1> dxgiDebug = default;
                Guid iid = DXGIDebug.IID_IDXGIDebug1;
                if (TerraFX.Interop.Windows.SUCCEEDED(DXGI.DXGIGetDebugInterface1(0, &iid, (void**)dxgiDebug.GetAddressOf())))
                {
                    dxgiDebug.Ptr->ReportLiveObjects(new Guid(0xe48ae283, 0xda80, 0x490b, 0x87, 0xe6, 0x43, 0xe9, 0xa9, 0xcf, 0xda, 0x8), 
                        DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_SUMMARY | DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_IGNORE_INTERNAL);
                }
#endif
            }

            if (_deviceResources == null)
            {
                _deviceResources = new DeviceResources();
                _deviceResources.SetWindow(CoreWindow.GetForCurrentThread());
                _main.CreateRenderers(_deviceResources);
            }
            return _deviceResources;
        }
    }

    internal class Direct3DApplicationSource : IFrameworkViewSource
    {
        public IFrameworkView CreateView()
        {
            return new App();
        }
    }
}
