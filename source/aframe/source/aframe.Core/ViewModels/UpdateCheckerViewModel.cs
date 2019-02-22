using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using aframe.Updater;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;

namespace aframe.ViewModels
{
    public class UpdateCheckerViewModel : BindableBase
    {
        private static readonly ReleaseNote DesignModeModel = new ReleaseNote()
        {
            Version = "1.0.0.0",
            ReleaseChannel = ReleaseChannels.Stable,
            Timestamp = DateTime.Now,
            Description =
                    "本日は晴天なり。\n" +
                    "あいうえおかきくけこさしすせそ\n" +
                    "ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト\n" +
                    "テスト・テスト・テスト"
        };

        public UpdateCheckerViewModel()
        {
            this.Model.Subscribe(x =>
            {
                if (x == null)
                {
                    return;
                }

                this.Version.Value = x.Version;
                this.Channel.Value = x.ReleaseChannel;
                this.Timestamp.Value = x.Timestamp;
                this.Description.Value = x.Description;
            });
        }

        public ReactiveProperty<string> Message { get; } = new ReactiveProperty<string>(
            "新しいバージョンのアプリケーションがリリースされています。");

#if DEBUG
        public ReactiveProperty<ReleaseNote> Model { get; } = new ReactiveProperty<ReleaseNote>(DesignModeModel);
#else
        public ReactiveProperty<ReleaseNote> Model { get; } = new ReactiveProperty<ReleaseNote>(new ReleaseNote());
#endif

        public ReactiveProperty<Assembly> TargetAssembly { get; } = new ReactiveProperty<Assembly>();

        public ReactiveProperty<string> AppName { get; } = new ReactiveProperty<string>("My Application");

        public ReactiveProperty<string> Version { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<ReleaseChannels> Channel { get; } = new ReactiveProperty<ReleaseChannels>(ReleaseChannels.Stable);

        public ReactiveProperty<DateTime> Timestamp { get; } = new ReactiveProperty<DateTime>();

        public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<bool> InProgress { get; } = new ReactiveProperty<bool>(false);

        public ReactiveProperty<string> CurrentProgressMessage { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<double> Maximum { get; } = new ReactiveProperty<double>(100d);

        public ReactiveProperty<double> CurrentValue { get; } = new ReactiveProperty<double>(0d);

        private DelegateCommand updateCommand;

        public DelegateCommand UpdateCommand =>
            this.updateCommand ?? (this.updateCommand = new DelegateCommand(this.ExecuteUpdate));

        public Action CloseViewCallback { get; set; }

        private async void ExecuteUpdate()
        {
            var model = this.Model.Value;
            var temp = Path.GetTempFileName() + ".zip";
            var extractDirectory = string.Empty;
            var result = MessageDialogResult.Negative;

            try
            {
                this.InProgress.Value = true;

                // ダウンロードする
                this.CurrentProgressMessage.Value = "Downloading...";
                await model.DownloadAsync(
                    temp,
                    (sender, e) =>
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            this.Maximum.Value = e.TotalBytesToReceive;
                            this.CurrentValue.Value = e.BytesReceived;
                        });
                    });

                await Task.Delay(100);

                // 展開する
                this.CurrentProgressMessage.Value = "Extracting...";

                extractDirectory = Path.Combine(
                    Path.GetTempPath(),
                    $"{this.AppName.Value}_v{model.Version}_{model.ReleaseChannel}");

                if (Directory.Exists(extractDirectory))
                {
                    Directory.Delete(extractDirectory, true);
                }

                ZipFile.ExtractToDirectory(
                    temp,
                    extractDirectory);

                // 準備完了
                this.CurrentProgressMessage.Value = "Update Ready.";
                await Task.Delay(100);

                if (this.CloseViewCallback != null)
                {
                    WPFHelper.Dispatcher.Invoke(this.CloseViewCallback);
                    await Task.Delay(50);
                }

                // 最後の確認？
                result = await MessageBoxHelper.ShowMessageAsync(
                    "Application Updater",
                    "アップデートの準備が完了しました。\n" +
                    "アプリケーションを再起動してアップデートを実行しますか？",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "はい",
                        NegativeButtonText = "いいえ",
                        AnimateHide = true,
                        AnimateShow = true,
                        DefaultButtonFocus = MessageDialogResult.Negative
                    });
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                this.InProgress.Value = false;
                this.CurrentProgressMessage.Value = string.Empty;
            }

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            const string UpdaterFileName = "aframe.Updater.exe";

            var target = this.TargetAssembly.Value?.Location;
            var destination = Path.GetDirectoryName(target);
            var sourceUpdater = Path.Combine(
                extractDirectory,
                UpdaterFileName);

            var pi = new ProcessStartInfo(sourceUpdater)
            {
                Arguments = string.Join(" ", new[]
                {
                    $@"""{target}""",
                    $@"""{extractDirectory}""",
                    $@"""{destination}"""
                }),
                WorkingDirectory = extractDirectory
            };

            Process.Start(pi);

            if (UpdateChecker.ShutdownCallback != null)
            {
                Thread.Sleep(50);
                UpdateChecker.ShutdownCallback.Invoke();
            }
        }
    }
}
