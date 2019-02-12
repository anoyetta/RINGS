using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using RINGS.Models;

namespace RINGS.Overlays
{
    /// <summary>
    /// ChatOverlay.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatOverlay :
        Window,
        IOverlay
    {
        public ChatOverlay() : this(Config.DefaultChatOverlayName)
        {
        }

        public ChatOverlay(
            string name)
        {
            this.InitializeComponent();
            this.ToNonActive();
            this.Opacity = 0;

            this.ViewModel.Name = name;
            this.ViewModel.ChatOverlaySettings.PropertyChanged += this.ChatOverlaySettings_PropertyChanged;
            this.ResizeMode = this.ViewModel.ChatOverlaySettings.IsLock ?
                ResizeMode.NoResize :
                ResizeMode.CanResizeWithGrip;

            this.ViewModel.ShowCallback = () => this.OverlayVisible = true;
            this.ViewModel.HideCallback = () => this.OverlayVisible = false;
            this.ViewModel.ChangeActivePageCallback = (pageName) =>
            {
                var pages = this.ChatPagesTabControl.Items.Cast<ChatPageSettingsModel>();
                var page = pages?.FirstOrDefault(x => x.Name == pageName);
                if (page != null)
                {
                    this.ChatPagesTabControl.SelectedItem = page;
                }
            };

            this.TitleLabel.MouseLeftButtonDown += (_, __) =>
            {
                if (!this.ViewModel.ChatOverlaySettings.IsLock)
                {
                    this.DragMove();
                }
            };

            this.Loaded += (_, __) =>
            {
                this.OverlayVisible = this.ViewModel.ChatOverlaySettings.IsEnabled;
                this.SubscribeZOrderCorrector();
            };

            this.Closed += (_, __) =>
            {
                if (this.ViewModel.ChatOverlaySettings != null)
                {
                    this.ViewModel.ChatOverlaySettings.PropertyChanged -= this.ChatOverlaySettings_PropertyChanged;
                }

                this.ViewModel.Dispose();
            };
        }

        public ChatOverlayViewModel ViewModel => this.DataContext as ChatOverlayViewModel;

        private void ChatOverlaySettings_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChatOverlaySettingsModel.IsEnabled):
                    this.OverlayVisible = this.ViewModel.ChatOverlaySettings.IsEnabled;
                    break;

                case nameof(ChatOverlaySettingsModel.IsLock):
                    this.ResizeMode = this.ViewModel.ChatOverlaySettings.IsLock ?
                        ResizeMode.NoResize :
                        ResizeMode.CanResizeWithGrip;
                    break;
            }
        }

        #region IOverlay

        private bool overlayVisible;

        public bool OverlayVisible
        {
            get => this.overlayVisible;
            set => this.SetOverlayVisible(ref this.overlayVisible, value);
        }

        #endregion IOverlay
    }

    public class ScrollToBottomAction : TriggerAction<RichTextBox>
    {
        protected override void Invoke(object parameter)
        {
            this.AssociatedObject?.ScrollToEnd();
        }
    }
}
