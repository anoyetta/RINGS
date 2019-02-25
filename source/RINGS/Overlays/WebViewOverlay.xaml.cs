using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using aframe;
using RINGS.Models;

namespace RINGS.Overlays
{
    /// <summary>
    /// WebViewOverlay.xaml の相互作用ロジック
    /// </summary>
    public partial class WebViewOverlay :
        Window,
        IOverlay
    {
        private static readonly Lazy<WebViewOverlay> LazyInstance = new Lazy<WebViewOverlay>();

        public static WebViewOverlay Instance => LazyInstance.Value;

        public WebViewOverlay()
        {
            this.InitializeComponent();
            this.ToNonActive();
            this.Visibility = Visibility.Collapsed;

            this.Loaded += (_, __) =>
            {
                this.HideTitleBar();
            };

            this.WebView.Loaded += (_, __) =>
            {
                dynamic activeX = this.WebView.GetType().InvokeMember(
                    "ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    this.WebView,
                    new object[] { });

                activeX.Silent = true;
            };
        }

        public void ShowUrl(
            Window parent,
            string url)
        {
            var interopHelper = new WindowInteropHelper(parent);
            var currentScreen = System.Windows.Forms.Screen.FromHandle(interopHelper.Handle);

            var sizeRatio = Config.Instance.BuiltinBrowserSize / 100d;
            this.Width = currentScreen.Bounds.Width * sizeRatio;
            this.Height = currentScreen.Bounds.Height * sizeRatio;
            this.Left = (currentScreen.Bounds.Width - this.Width) / 2d;
            this.Top = (currentScreen.Bounds.Height - this.Height) / 2d;

            this.WebView.Visibility = Visibility.Collapsed;
            this.ImageView.Visibility = Visibility.Collapsed;
            this.Show();

            if (!url.IsImage())
            {
                this.WebView.Navigate(url);
                this.WebView.Visibility = Visibility.Visible;

                this.ImageView.MouseWheel -= this.Image_MouseWheel;
                this.MouseWheel -= this.Image_MouseWheel;
            }
            else
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url);
                bitmap.EndInit();
                this.ImageView.Source = bitmap;
                this.ImageView.Visibility = Visibility.Visible;

                this.ImageView.MouseWheel += this.Image_MouseWheel;
                this.MouseWheel += this.Image_MouseWheel;
            }

            this.Activate();

            var closeView = new WebViewHelperOverlay();
            closeView.Owner = this;
            closeView.Show();
            closeView.Top = this.Top;
            closeView.Left = this.Left + this.Width - closeView.Width;
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var image = this.ImageView as Image;
            var st = (ScaleTransform)((TransformGroup)image.RenderTransform)
                .Children.First(tr => tr is ScaleTransform);
            var zoom = e.Delta > 0 ? 0.05 : -0.05;

            if (st.ScaleX + zoom >= 0.05)
            {
                st.ScaleX += zoom;
                st.ScaleY += zoom;
            }
        }

        private Point start;
        private Point origin;

        private void ImageView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;

            image.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform)
                .Children.First(tr => tr is TranslateTransform);
            this.start = e.GetPosition(this.ImageBorder);
            this.origin = new Point(tt.X, tt.Y);
        }

        private void ImageView_MouseMove(object sender, MouseEventArgs e)
        {
            var image = sender as Image;

            if (image.IsMouseCaptured)
            {
                var tt = (TranslateTransform)((TransformGroup)image.RenderTransform)
                    .Children.First(tr => tr is TranslateTransform);
                var v = start - e.GetPosition(this.ImageBorder);
                tt.X = origin.X - v.X;
                tt.Y = origin.Y - v.Y;
            }
        }

        private void ImageView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            image.ReleaseMouseCapture();
        }

        private void HideTitleBar()
        {
            var handle = this.GetHandle();

            var windowStyle = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
            windowStyle &= ~NativeMethods.WS_CAPTION;

            NativeMethods.SetWindowLong(
                handle,
                NativeMethods.GWL_STYLE,
                windowStyle);

            NativeMethods.SetWindowPos(
                handle,
                IntPtr.Zero,
                0, 0, 0, 0,
                NativeMethods.SWP_FRAMECHANGED |
                NativeMethods.SWP_NOMOVE |
                NativeMethods.SWP_NOZORDER |
                NativeMethods.SWP_NOSIZE);
        }

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
