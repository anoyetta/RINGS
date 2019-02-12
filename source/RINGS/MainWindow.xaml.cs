using System.Threading.Tasks;
using aframe;
using MahApps.Metro.Controls;
using RINGS.ViewModels;

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

            this.ViewModel.CloseAction = () => this.Close();
        }

        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        private async void HamburgerMenuControl_OnItemInvoked(
            object sender,
            HamburgerMenuItemInvokedEventArgs e)
        {
            var item = e.InvokedItem as HamburgerMenuItem;

            if (item.Tag != null)
            {
                this.HamburgerMenuControl.SetCurrentValue(
                    ContentProperty,
                    item);

                // ついでにConfigを保存する
                await Task.Run(() => Config.Instance.Save());
            }
        }
    }
}
