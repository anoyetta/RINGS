using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using aframe;
using RINGS.Controllers;

namespace RINGS.Overlays
{
    public class ChatOverlaysController
    {
        #region Lazy Singleton

        private readonly static Lazy<ChatOverlaysController> instance = new Lazy<ChatOverlaysController>();

        public static ChatOverlaysController Instance => instance.Value;

        #endregion Lazy Singleton

        public ChatOverlaysController()
        {
        }

        private readonly Dictionary<string, ChatOverlay> OverlayDictionary = new Dictionary<string, ChatOverlay>();

        private readonly DispatcherTimer refreshTimer = new DispatcherTimer(DispatcherPriority.ContextIdle)
        {
            Interval = TimeSpan.FromSeconds(0.5),
        };

        private static readonly TimeSpan ForgroundAppSubscribeInterval = TimeSpan.FromSeconds(3);

        private readonly Thread ForegroundAppSubscriber = new Thread(SubscribeForegroundApp)
        {
            IsBackground = true,
            Priority = ThreadPriority.Lowest,
        };

        public async Task StartAsync() => await Task.Run(() =>
        {
            if (!this.refreshTimer.IsEnabled)
            {
                this.refreshTimer.Tick -= this.RefreshTimer_Tick;
                this.refreshTimer.Tick += this.RefreshTimer_Tick;
                this.refreshTimer.Start();
            }

            this.ForegroundAppSubscriber.Start();
        });

        public void Stop()
        {
            this.refreshTimer.Stop();
            this.ForegroundAppSubscriber.Abort();

            lock (this)
            {
                foreach (var entry in this.OverlayDictionary)
                {
                    entry.Value.Close();
                    Thread.Yield();
                }

                this.OverlayDictionary.Clear();
            }
        }

        private volatile bool isWorking = false;

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (this.isWorking)
            {
                return;
            }

            try
            {
                this.isWorking = true;
                this.RefreshOverlays();
            }
            finally
            {
                this.isWorking = false;
            }
        }

        public bool IsEditing { get; set; }

        private int counter;

        public void RefreshOverlays(
            bool force = false)
        {
            if (!force)
            {
                if (this.IsEditing)
                {
                    return;
                }
            }

            var overlays = default(IEnumerable<ChatOverlay>);

            lock (this)
            {
                var overlaySettings = Config.Instance.ChatOverlaySettings;

                var news = overlaySettings.Where(x => !this.OverlayDictionary.ContainsKey(x.Name));
                foreach (var settings in news)
                {
                    var overlay = new ChatOverlay(settings.Name);
                    overlay.Show();
                    this.OverlayDictionary[settings.Name] = overlay;
                    Thread.Yield();
                }

                var olds = this.OverlayDictionary.Where(x => !overlaySettings.Any(y => y.Name == x.Key)).ToArray();
                foreach (var entry in olds)
                {
                    entry.Value.Close();
                    this.OverlayDictionary.Remove(entry.Key);
                    Thread.Yield();
                }

                overlays = this.OverlayDictionary.Values.ToArray();
            }

            foreach (var overlay in overlays)
            {
                if (overlay.ViewModel.ChatOverlaySettings?.IsEnabled ?? false)
                {
                    overlay.OverlayVisible = IsFFXIVActive;
                }
            }

            this.counter++;
        }

        private static void SubscribeForegroundApp()
        {
            Thread.Sleep(ForgroundAppSubscribeInterval.Add(ForgroundAppSubscribeInterval));

            while (true)
            {
                try
                {
                    RefreshFFXIVIsActive();
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    AppLogger.Error("Happened exception from Foreground App subscriber.", ex);
                    Thread.Sleep(ForgroundAppSubscribeInterval.Add(ForgroundAppSubscribeInterval));
                }
                finally
                {
                    Thread.Sleep(ForgroundAppSubscribeInterval);
                }
            }
        }

        private static volatile bool IsFFXIVActive;

        private static void RefreshFFXIVIsActive()
        {
            try
            {
                // フォアグラウンドWindowのハンドルを取得する
                var hWnd = NativeMethods.GetForegroundWindow();

                // プロセスIDに変換する
                NativeMethods.GetWindowThreadProcessId(hWnd, out uint pid);

                if (pid == SharlayanController.Instance.HandledProcessID)
                {
                    IsFFXIVActive = true;
                    return;
                }

                // フォアウィンドウのファイル名を取得する
                var p = Process.GetProcessById((int)pid);
                if (p != null)
                {
                    var fileName = Path.GetFileName(
                        p.MainModule.FileName);

                    var currentProcessFileName = Path.GetFileName(
                        Process.GetCurrentProcess().MainModule.FileName);

                    if (fileName.ToLower() == "ffxiv.exe" ||
                        fileName.ToLower() == "ffxiv_dx11.exe" ||
                        fileName.ToLower() == currentProcessFileName.ToLower())
                    {
                        IsFFXIVActive = true;
                    }
                    else
                    {
                        IsFFXIVActive = false;
                    }
                }
            }
            catch (Win32Exception)
            {
            }
        }
    }
}
