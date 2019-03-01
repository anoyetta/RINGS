using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using aframe;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Models;

namespace RINGS.Overlays
{
    public class ChatOverlayViewModel :
        BindableBase,
        IDisposable
    {
        public Window BindingWindow { get; set; }

        public Action<string> ChangeActivePageCallback { get; set; }

        public Action ShowCallback { get; set; }

        public Action HideCallback { get; set; }

        public Action MinimizeCallback { get; set; }

        private readonly DispatcherTimer HideTimer = new DispatcherTimer(DispatcherPriority.ContextIdle)
        {
            Interval = TimeSpan.FromSeconds(1.0),
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

            lock (this)
            {
                this.lastLogAddedTimestamp = DateTime.Now;
                this.ShowCallback?.Invoke();
            }

            if (this.ChatOverlaySettings.IsAutoActivatePage)
            {
                this.ChangeActivePageCallback?.Invoke(e.ParentPage.Name);
            }
        }

        private volatile bool previousIsAutoHide;

        private void HideTimer_Tick(
            object sender,
            EventArgs e)
        {
            lock (this)
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
                    if (this.previousIsAutoHide != this.ChatOverlaySettings.IsAutoHide)
                    {
                        this.ShowCallback?.Invoke();
                    }
                }

                this.previousIsAutoHide = this.ChatOverlaySettings.IsAutoHide;
            }
        }

        public DateTime lastLogAddedTimestamp = DateTime.Now;

        public void ExtendTimeToHide()
        {
            lock (this)
            {
                this.lastLogAddedTimestamp = DateTime.Now.AddSeconds(
                    this.ChatOverlaySettings.TimeToHide / 2d);
            }
        }

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

        private DelegateCommand minimizeCommand;

        public DelegateCommand MinimizeCommand =>
            this.minimizeCommand ?? (this.minimizeCommand = new DelegateCommand(this.ExecuteMinimizeCommand));

        private void ExecuteMinimizeCommand()
        {
            this.MinimizeCallback?.Invoke();
        }

        private DelegateCommand sendFileCommand;

        public DelegateCommand SendFileCommand =>
            this.sendFileCommand ?? (this.sendFileCommand = new DelegateCommand(this.ExecuteSendFileCommand));

        private void ExecuteSendFileCommand()
        {
            AttachmentFileOverlay.SendFile(this.BindingWindow);
        }

        private DelegateCommand sendFromClipboardCommand;

        public DelegateCommand SendFromClipboardCommand =>
            this.sendFromClipboardCommand ?? (this.sendFromClipboardCommand = new DelegateCommand(this.ExecuteSendFromClipboardCommand));

        private async void ExecuteSendFromClipboardCommand()
        {
            var clip = Clipboard.GetDataObject();
            if (clip == null)
            {
                return;
            }

            var temp = string.Empty;
            var result = await Task.Run(() =>
            {
                var bmp = clip?.GetData(typeof(Bitmap)) as Bitmap;
                if (bmp == null)
                {
                    return false;
                }

                temp = Path.Combine(
                    Config.TempDirectory,
                    $"{Guid.NewGuid()}.png");

                try
                {
                    if (!Directory.Exists(Config.TempDirectory))
                    {
                        Directory.CreateDirectory(Config.TempDirectory);
                    }

                    bmp.Save(temp, ImageFormat.Png);
                }
                finally
                {
                    bmp.Dispose();
                    bmp = null;
                }

                return true;
            });

            if (!result)
            {
                return;
            }

            AttachmentFileOverlay.SendFile(this.BindingWindow, temp);
        }

        private DelegateCommand launchSketchCommand;

        public DelegateCommand LaunchSketchCommand =>
            this.launchSketchCommand ?? (this.launchSketchCommand = new DelegateCommand(this.ExecuteLaunchSketchCommand));

        private static readonly string SketchUri = "ms-penworkspace://Capture";

        private void ExecuteLaunchSketchCommand() => Process.Start(SketchUri);
    }
}
