using System.Windows;
using System.Windows.Threading;
using aframe;
using RINGS.Overlays;

namespace RINGS
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppLogger.Init("RINGSLogger");

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            this.Startup += this.App_Startup;
            this.Exit += this.App_Exit;

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var c = Config.Instance;
            c.SetStartup(c.IsStartupWithWindows);

            ChatOverlaysController.Instance.Start();

            AppLogger.Write($"{c.AppNameWithVersion} Start.");
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Config.Instance.Save();

                ChatOverlaysController.Instance.Stop();
            }
            finally
            {
                AppLogger.Write("RINGS End.");
                AppLogger.Flush();
            }
        }

        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Config.Instance.Save();

                if (this.MainWindow != null)
                {
                    MessageBoxHelper.ShowDialogMessageWindow(
                        "RINGS - Fatal",
                        "予期しない例外を検知しました。アプリケーションを終了します。",
                        e.Exception);
                }
                else
                {
                    MessageBox.Show(
                        "予期しない例外を検知しました。アプリケーションを終了します。\n\n" +
                        e.Exception,
                        "RINGS - Fatal",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
                AppLogger.Fatal(
                    "Unhandled Exception. 予期しない例外が発生しました。",
                    e.Exception);

                AppLogger.Write("RINGS Abort.");
                AppLogger.Flush();
            }
        }
    }
}
