using System.Windows;
using aframe.Views;

namespace RINGS.Overlays
{
    /// <summary>
    /// WebViewHelperOverlay.xaml の相互作用ロジック
    /// </summary>
    public partial class WebViewHelperOverlay : Window, IOverlay
    {
        public static WebViewHelperOverlay Instance { get; private set; }

        public WebViewHelperOverlay()
        {
            Instance = this;

            this.InitializeComponent();
            this.ToNonActive();

            this.CloseButton.Click += (_, __) =>
            {
                WebViewOverlay.Instance.Hide();
                this.Close();
            };
        }

        #region IOverlay

        public int ZOrder => int.MaxValue;

        private bool overlayVisible;

        public bool OverlayVisible
        {
            get => this.overlayVisible;
            set => this.SetOverlayVisible(ref this.overlayVisible, value, 0.9d);
        }

        #endregion IOverlay
    }
}
