using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using aframe;

namespace RINGS.Overlays
{
    /// <summary>
    /// SplashOverlay.xaml の相互作用ロジック
    /// </summary>
    public partial class SplashOverlay : Window, IOverlay
    {
        private static readonly Lazy<SplashOverlay> LazyInstance = new Lazy<SplashOverlay>();

        public static SplashOverlay Instance => LazyInstance.Value;

        public SplashOverlay()
        {
            this.InitializeComponent();
            this.ToNonActive();

            this.Loaded += (_, __) =>
            {
                this.ToTransparent();
            };

            this.VersionText.Text = $"{Config.Instance.AppVersionString}-{Config.Instance.AppReleaseChannel}";
            this.FadeoutAnimation.Completed += (_, __) => this.Close();
        }

        public void ShowSplash()
        {
            this.Topmost = true;
            this.Show();

            var fadeoutAction = new Action(async () =>
            {
                this.Topmost = false;
                await Task.Delay(TimeSpan.FromSeconds(2.5));
                this.BeginAnimation(
                    Window.OpacityProperty,
                    this.FadeoutAnimation);
            });

            WPFHelper.Dispatcher.BeginInvoke(
                fadeoutAction,
                DispatcherPriority.ApplicationIdle);
        }

        private readonly DoubleAnimation FadeoutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(1));

        #region IOverlay

        public int ZOrder => int.MaxValue;

        private bool overlayVisible;

        public bool OverlayVisible
        {
            get => this.overlayVisible;
            set => this.SetOverlayVisible(ref this.overlayVisible, value);
        }

        #endregion IOverlay
    }
}
