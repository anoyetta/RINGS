using aframe;
using MahApps.Metro.Controls;
using RINGS.Overlays;

namespace RINGS
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();

            new ChatOverlay().Show();

            WPFHelper.Dispatcher.InvokeAsync(() => ColorDialog.ShowDialog());
        }

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            this.HamburgerMenuControl.SetCurrentValue(ContentProperty, e.InvokedItem);
        }
    }
}
