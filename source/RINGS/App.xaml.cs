using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using aframe;
using aframe.Updater;
using aframe.ViewModels;
using RINGS.Controllers;
using RINGS.Overlays;

namespace RINGS
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private static readonly Assembly[] ReferenceAssemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            typeof(Sharlayan.Reader).Assembly,
            typeof(Discord.IApplication).Assembly,
        };

        public static App Instance;

        public Action CloseMainWindowCallback { get; set; }

        public App()
        {
            Instance = this;
            AppLogger.Init("RINGSLogger");

            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            this.Startup += this.App_Startup;
            this.Exit += this.App_Exit;

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // Help にAppLogを登録する
            AppLogger.OnWrite += (_, e) => HelpViewModel.Instance.AddLog(e);
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            var c = Config.Instance;
            c.SetStartup(c.IsStartupWithWindows);

            // WebBrowser のIEを最新版に指定する
            WebBrowserHelper.SetUseNewestWebBrowser();

            await Task.WhenAll(
                ChatOverlaysController.Instance.StartAsync(),
                SharlayanController.Instance.StartAsync(),
                DiscordBotController.Instance.StartAsync(),
                Task.Run(() =>
                {
                    HelpViewModel.Instance.OfficialSiteUri = new Uri(@"https://github.com/anoyetta/RINGS");
                    foreach (var asm in ReferenceAssemblies)
                    {
                        HelpViewModel.Instance.AddVersionInfos(asm);
                    }

                    HelpViewModel.Instance.GetReleaseChannelCallback = () => c.UpdateChannel;
                    HelpViewModel.Instance.SetReleaseChannelCallback = value => c.UpdateChannel = value;
                }));

            AppLogger.Write("RINGS is chat communication enhancer for FFXIV, developed by anoyetta and best friends.");
            AppLogger.Write($"{c.AppNameWithVersion} Start.");

            /*
            new SandboxWindow().Show();
            */
            WebViewOverlay.Instance.ShowUrl(
                this.MainWindow,
                @"https://www.jma.go.jp/jp/gms/imgs/0/infrared/1/201902251650-00.png");

            // アップデートを確認する
            UpdateChecker.ShutdownCallback = () => WPFHelper.Dispatcher.Invoke(() => this.CloseMainWindowCallback?.Invoke());
            UpdateChecker.UpdateSourceUri = Config.Instance.UpdateSourceUri;
            UpdateChecker.LastUpdateCheckCallback = (lastUpdateTimestamp) =>
            {
                Config.Instance.LastUpdateTimestamp = lastUpdateTimestamp;
                Config.Instance.Save(Config.FileName);
            };

            await this.Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));

#if !DEBUG
                await UpdateChecker.IsUpdateAsync(
                    Config.Instance.LastUpdateTimestamp,
                    Config.Instance.UpdateChannel);
#else
#if false
                await UpdateChecker.IsUpdateAsync(
                    Config.Instance.LastUpdateTimestamp,
                    Config.Instance.UpdateChannel);
#else
                // デバッグ用
                // 強制的に最新バージョンを取得する
                await UpdateChecker.IsUpdateAsync(
                    Config.Instance.LastUpdateTimestamp,
                    Config.Instance.UpdateChannel,
                    UpdateChecker.DefaultUpdateCheckInterval,
                    true);
#endif
#endif
            },
            DispatcherPriority.ApplicationIdle);
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
                    "Unhandled Exception. 予期しない例外を検知しました。",
                    e.Exception);

                AppLogger.Write("RINGS Abort.");
                AppLogger.Flush();
            }
        }
    }
}
