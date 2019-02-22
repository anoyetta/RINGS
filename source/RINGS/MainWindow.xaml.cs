using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using aframe;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using RINGS.ViewModels;

namespace RINGS
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;

            this.InitializeComponent();

            MessageBoxHelper.EnqueueSnackbarCallback = (message, neverDuplicate) =>
                this.Snackbar.MessageQueue.Enqueue(message, neverDuplicate);

            this.ViewModel.CloseAction = () => this.ToEnd();

            this.StateChanged += this.MainWindow_StateChanged;
            this.Closing += this.MainWindow_Closing;

            if (Config.Instance.IsMinimizeStartup)
            {
                this.ShowInTaskbar = false;
                this.WindowState = WindowState.Minimized;

                this.Loaded += async (_, __) =>
                {
                    this.ToHide();
                    this.ShowInTaskbar = true;

                    await Task.Delay(TimeSpan.FromSeconds(0.1));

                    await WPFHelper.Dispatcher.InvokeAsync(() =>
                    {
                        this.NotifyIcon.ShowBalloonTip(
                            Config.Instance.AppNameWithVersion,
                            "started.",
                            BalloonIcon.Info);
                    },
                    DispatcherPriority.ApplicationIdle);
                };
            }

            App.Instance.CloseMainWindowCallback = () => this.ToEnd();
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

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.NotifyIcon.Visibility = Visibility.Visible;
            }
            else
            {
                this.Show();
                this.NotifyIcon.Visibility = Visibility.Collapsed;
            }
        }

        private volatile bool toEnd;

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!this.toEnd)
            {
                this.ToHide();
                e.Cancel = true;
                return;
            }

            this.NotifyIcon.Visibility = Visibility.Collapsed;
            this.NotifyIcon.Dispose();
        }

        public void ToShow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.NotifyIcon.Visibility = Visibility.Collapsed;
        }

        public async void ToHide()
        {
            this.NotifyIcon.Visibility = Visibility.Visible;
            this.Hide();

            await Task.Run(() => Config.Instance.Save());
        }

        public void ToEnd()
        {
            this.toEnd = true;
            this.Close();
        }
    }
}
