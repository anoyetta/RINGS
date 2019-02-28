using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using aframe.Models;
using aframe.Updater;
using aframe.Views;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace aframe.ViewModels
{
    public class HelpViewModel : BindableBase
    {
        #region Singleton

        private static HelpViewModel instance;

        public static HelpViewModel Instance => instance ?? (instance = new HelpViewModel());

        #endregion Singleton

        public HelpViewModel()
        {
            this.Logs.CollectionChanged += (x, y) => HelpView.SendScrollToEndLog();
        }

#if DEBUG
        public bool IsDebugBuild => true;
#else
        public bool IsDebugBuild => false;
#endif

        public ObservableCollection<VersionInfoModel> VersionInfos { get; }
            = new ObservableCollection<VersionInfoModel>();

        public ObservableCollection<AppLogOnWriteEventArgs> Logs { get; }
            = new ObservableCollection<AppLogOnWriteEventArgs>();

        public void AddVersionInfos(
            Assembly assembly)
            => Application.Current.Dispatcher.InvokeAsync(
                () => this.VersionInfos.Add(new VersionInfoModel(assembly)),
                DispatcherPriority.Background);

        public void AddVersionInfos(
            VersionInfoModel versionInfo)
            => Application.Current.Dispatcher.InvokeAsync(
                () => this.VersionInfos.Add(versionInfo),
                DispatcherPriority.Background);

        public void AddLog(
            AppLogOnWriteEventArgs args)
            => Application.Current.Dispatcher.InvokeAsync(
                () => this.Logs.Add(args),
                DispatcherPriority.ContextIdle);

        public Uri OfficialSiteUri { get; set; }

        public Func<ReleaseChannels> GetReleaseChannelCallback { get; set; }

        public Action<ReleaseChannels> SetReleaseChannelCallback { get; set; }

        public IEnumerable<ReleaseChannels> ReleaseChannelList
            => Enum.GetValues(typeof(ReleaseChannels)).Cast<ReleaseChannels>();

        public void RaiseCurrentReleaseChannelChanged()
            => this.RaisePropertyChanged(nameof(this.CurrentReleaseChannel));

        public ReleaseChannels CurrentReleaseChannel
        {
            get => this.GetReleaseChannelCallback?.Invoke() ?? ReleaseChannels.Stable;
            set
            {
                if (this.GetReleaseChannelCallback?.Invoke() != value)
                {
                    this.SetReleaseChannelCallback?.Invoke(value);
                    this.RaisePropertyChanged();
                }
            }
        }

        private static readonly SaveFileDialog SaveLogFileDialog = new SaveFileDialog()
        {
            RestoreDirectory = true,
            DefaultExt = "zip",
            Filter = "Zip archives (*.zip)|*.txt|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            OverwritePrompt = true,
        };

        public void SetCredits(IEnumerable<CreditEntry> credits)
            => this.CreditList.AddRange(credits);

        private readonly List<CreditEntry> CreditList = new List<CreditEntry>();

        public bool IsExistsCredits => this.CreditList.Any();

        #region Commands

        private DelegateCommand loadedCommand;

        public DelegateCommand LoadedCommand =>
            this.loadedCommand ?? (this.loadedCommand = new DelegateCommand(this.Refresh));

        private DelegateCommand refreshCommand;

        public DelegateCommand RefreshCommand =>
            this.refreshCommand ?? (this.refreshCommand = new DelegateCommand(this.Refresh));

        private DelegateCommand openOfficialSiteCommand;

        public DelegateCommand OpenOfficialSiteCommand =>
            this.openOfficialSiteCommand ?? (this.openOfficialSiteCommand = new DelegateCommand(async () =>
            {
                if (this.OfficialSiteUri != null)
                {
                    await Task.Run(() => Process.Start(this.OfficialSiteUri.ToString()));
                }
            }));

        private DelegateCommand checkUpdateCommand;

        public DelegateCommand CheckUpdateCommand =>
            this.checkUpdateCommand ?? (this.checkUpdateCommand = new DelegateCommand(async () => await this.CheckUpdateAsync()));

        private DelegateCommand openLogCommand;

        public DelegateCommand OpenLogCommand =>
            this.openLogCommand ?? (this.openLogCommand = new DelegateCommand(() =>
            {
                var log = AppLogger.GetCurrentLogFileName();
                if (File.Exists(log))
                {
                    AppLogger.Flush();
                    Process.Start(log);
                }
            }));

        private DelegateCommand saveLogCommand;

        public DelegateCommand SaveLogCommand =>
            this.saveLogCommand ?? (this.saveLogCommand = new DelegateCommand(async () =>
            {
                var appName = "App";
                var firstAssembly = this.VersionInfos.FirstOrDefault();
                if (firstAssembly != null)
                {
                    appName = firstAssembly.ProductName.Value;
                }

                var fileName = $"{appName}Log_{DateTime.Now:yyyyMMdd_HHmm}.zip";

                SaveLogFileDialog.FileName = fileName;
                if (!MessageBoxHelper.ShowSaveFileDialog(SaveLogFileDialog))
                {
                    return;
                }

                await AppLogger.SaveArchiveLogDirectoryAsync(SaveLogFileDialog.FileName);

                MessageBoxHelper.EnqueueSnackMessage("保存しました。");
            }));

        private DelegateCommand showCreditCommand;

        public DelegateCommand ShowCreditCommand =>
            this.showCreditCommand ?? (this.showCreditCommand = new DelegateCommand(() =>
            {
                CreditView.ShowCredits(this.CreditList, Application.Current.MainWindow);
            }));

        #endregion Commands

        private void Refresh()
        {
            this.RaisePropertyChanged(nameof(VersionInfos));
            this.RaisePropertyChanged(nameof(Logs));

            AppLogger.Flush();
            HelpView.SendScrollToEndLog();
        }

        private static readonly DateTimeOffset DummyLastUpdateTimestamp = new DateTimeOffset(DateTime.Parse("1900/1/1"));

        private async Task CheckUpdateAsync()
        {
            var existNewer = await UpdateChecker.IsUpdateAsync(
                DummyLastUpdateTimestamp,
                this.CurrentReleaseChannel);

            if (!existNewer)
            {
                MessageBoxHelper.EnqueueSnackMessage("This application is up-to-date.");
            }
        }
    }

    public class CreditEntry
    {
        public string SubTitle { get; set; }

        public string[] Names { get; set; }
    }
}
