using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using aframe;
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

            this.HideTimer.Tick += this.HideTimer_Tick;
            this.HideTimer.Start();

            this.SubscribeOverlaySettings();
        }

        public void Dispose()
        {
            this.HideTimer.Stop();
            this.HideTimer.Tick -= this.HideTimer_Tick;

            this.UnsubscribeOverlaySettings();
        }

        private void SubscribeOverlaySettings()
        {
            if (this.ChatOverlaySettings != null)
            {
                foreach (var page in this.ChatOverlaySettings.ChatPages)
                {
                    page.CreateLogBuffer();
                    page.LogBuffer.ChatLogAdded += this.LogBuffer_ChatLogAdded;
                }

                this.ChatOverlaySettings.PropertyChanged += this.ChatOverlaySettings_PropertyChanged;
                this.ChatOverlaySettings.ChatPages.CollectionChanged += this.ChatPages_CollectionChanged;
            }
        }

        private void UnsubscribeOverlaySettings()
        {
            if (this.ChatOverlaySettings != null)
            {
                this.ChatOverlaySettings.PropertyChanged -= this.ChatOverlaySettings_PropertyChanged;
                this.ChatOverlaySettings.ChatPages.CollectionChanged -= this.ChatPages_CollectionChanged;

                foreach (var page in this.ChatOverlaySettings.ChatPages)
                {
                    if (page.LogBuffer != null)
                    {
                        page.LogBuffer.ChatLogAdded -= this.LogBuffer_ChatLogAdded;
                    }

                    page.DisposeLogBuffer();
                }
            }
        }

        private async void ChatOverlaySettings_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChatOverlaySettingsModel.PCNameStyle):
                    await WPFHelper.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var page in this.ChatOverlaySettings.ChatPages)
                        {
                            var logs = page.LogBuffer.Buffer.ToArray();
                            foreach (var log in logs)
                            {
                                log.SetSpeaker();
                            }
                        }
                    },
                    DispatcherPriority.ContextIdle);
                    break;
            }
        }

        private void ChatPages_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var page in e.NewItems.Cast<ChatPageSettingsModel>())
                {
                    page.CreateLogBuffer();
                    page.LogBuffer.ChatLogAdded += this.LogBuffer_ChatLogAdded;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var page in e.OldItems.Cast<ChatPageSettingsModel>())
                {
                    if (page.LogBuffer != null)
                    {
                        page.LogBuffer.ChatLogAdded -= this.LogBuffer_ChatLogAdded;
                    }

                    page.DisposeLogBuffer();
                }
            }
        }

        private void LogBuffer_ChatLogAdded(
            object sender,
            ChatLogAddedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

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
            else
            {
                this.ShowCallback?.Invoke();
            }
        }

        private DateTime lastLogAddedTimestamp = DateTime.Now;

        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                if (this.name != value)
                {
                    this.UnsubscribeOverlaySettings();
                }

                if (this.SetProperty(ref this.name, value))
                {
                    this.SubscribeOverlaySettings();
                    this.RaisePropertyChanged(nameof(this.Title));
                    this.RaisePropertyChanged(nameof(this.ChatOverlaySettings));
                }
            }
        }

        public string Title => $"RINGS - {this.Name}";

        public Config Config => Config.Instance;

        public ChatOverlaySettingsModel ChatOverlaySettings => Config.Instance.GetChatOverlaySettings(this.name);
    }
}
