using System.ComponentModel;
using System.Windows;
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
        public ChatOverlay()
        {
            this.InitializeComponent();
            this.ToNonActive();

            this.Opacity = 0;
            this.ViewModel.ChatOverlaySettings.PropertyChanged += this.ChatOverlaySettings_PropertyChanged;
            this.ResizeMode = this.ViewModel.ChatOverlaySettings.IsLock ?
                ResizeMode.NoResize :
                ResizeMode.CanResizeWithGrip;

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
                this.ViewModel.ChatOverlaySettings.PropertyChanged -= this.ChatOverlaySettings_PropertyChanged;
            };
        }

        public ChatOverlayViewModel ViewModel => this.DataContext as ChatOverlayViewModel;

        private void ChatOverlaySettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
}
