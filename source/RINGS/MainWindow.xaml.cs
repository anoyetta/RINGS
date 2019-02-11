using System.Threading.Tasks;
using aframe;
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

            MessageBoxHelper.EnqueueSnackbarCallback = (message, neverDuplicate) =>
                this.Snackbar.MessageQueue.Enqueue(message, neverDuplicate);
        }

        private async void HamburgerMenuControl_OnItemInvoked(
            object sender,
            HamburgerMenuItemInvokedEventArgs e)
        {
            this.HamburgerMenuControl.SetCurrentValue(
                ContentProperty,
                e.InvokedItem);

            // ついでにConfigを保存する
            await Task.Run(() => Config.Instance.Save());
        }
    }
}
