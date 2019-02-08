using MahApps.Metro.Controls;

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
        }

        private void HamburgerMenuControl_OnItemInvoked(
            object sender,
            HamburgerMenuItemInvokedEventArgs e)
        {
            this.HamburgerMenuControl.SetCurrentValue(
                ContentProperty,
                e.InvokedItem);
        }
    }
}
