using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

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

        public void Start()
        {
            if (!this.refreshTimer.IsEnabled)
            {
                this.refreshTimer.Tick -= this.RefreshTimer_Tick;
                this.refreshTimer.Tick += this.RefreshTimer_Tick;
                this.refreshTimer.Start();
            }
        }

        public void Stop()
        {
            this.refreshTimer.Stop();

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

        public void RefreshOverlays()
        {
            lock (this)
            {
                var overlaySettings = Config.Instance.ChatOverlaySettings.ToArray();

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
            }
        }
    }
}
