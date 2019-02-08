using System;
using System.Windows.Threading;
using Prism.Mvvm;
using RINGS.Models;

namespace RINGS.Overlays
{
    public class ChatOverlayViewModel :
        BindableBase,
        IDisposable
    {
        public Action<string> ChangeActivePageCallback { get; set; }

        public Action ShowCallback { get; set; }

        public Action HideCallback { get; set; }

        private readonly DispatcherTimer HideTimer = new DispatcherTimer(DispatcherPriority.ContextIdle)
        {
            Interval = TimeSpan.FromSeconds(0.5),
        };

        public ChatOverlayViewModel() : this(Config.DefaultChatOverlayName)
        {
        }

        public ChatOverlayViewModel(
            string name)
        {
            this.Name = name;

            foreach (var page in this.ChatOverlaySettings.ChatPages)
            {
                page.CreateLogBuffer();
                page.LogBuffer.ChatLogAdded += this.LogBuffer_ChatLogAdded;
            }

            this.HideTimer.Tick += this.HideTimer_Tick;
            this.HideTimer.Start();
        }

        public void Dispose()
        {
            this.HideTimer.Stop();
            this.HideTimer.Tick -= this.HideTimer_Tick;

            foreach (var page in this.ChatOverlaySettings.ChatPages)
            {
                page.LogBuffer.ChatLogAdded -= this.LogBuffer_ChatLogAdded;
                page.DisposeLogBuffer();
            }
        }

        private void LogBuffer_ChatLogAdded(
            object sender,
            ChatLogAddedEventArgs e)
        {
            this.lastLogAddedTimestamp = DateTime.Now;
            this.ShowCallback?.Invoke();

            if (this.ChatOverlaySettings.IsAutoActivatePage)
            {
                this.ChangeActivePageCallback?.Invoke(e.ParentPage.Name);
            }
        }

        private void HideTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (this.ChatOverlaySettings.IsAutoHide)
            {
                if ((DateTime.Now - this.lastLogAddedTimestamp).TotalSeconds >
                    this.ChatOverlaySettings.TimeToHide)
                {
                    this.HideCallback?.Invoke();
                }
            }
        }

        private DateTime lastLogAddedTimestamp = DateTime.Now;

        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Title));
                }
            }
        }

        public string Title => $"RINGS - {this.Name}";

        public Config Config => Config.Instance;

        public ChatOverlaySettingsModel ChatOverlaySettings => Config.Instance.GetChatOverlaySettings(this.name);
    }
}
