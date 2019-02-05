using System;
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
                DispatcherPriority.Background);

        private static readonly SaveFileDialog SaveLogFileDialog = new SaveFileDialog()
        {
            RestoreDirectory = true,
            DefaultExt = "zip",
            Filter = "Zip archives (*.zip)|*.txt|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            OverwritePrompt = true,
        };

        #region Commands

        private DelegateCommand loadedCommand;

        public DelegateCommand LoadedCommand =>
            this.loadedCommand ?? (this.loadedCommand = new DelegateCommand(this.Refresh));

        private DelegateCommand refreshCommand;

        public DelegateCommand RefreshCommand =>
            this.refreshCommand ?? (this.refreshCommand = new DelegateCommand(this.Refresh));

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

        #endregion Commands

        private void Refresh()
        {
            this.RaisePropertyChanged(nameof(VersionInfos));
            this.RaisePropertyChanged(nameof(Logs));

            AppLogger.Flush();
            HelpView.SendScrollToEndLog();
        }

        private async Task CheckUpdateAsync()
        {
            var existNewer = await UpdateChecker.IsUpdateAsync();
            if (!existNewer)
            {
                MessageBoxHelper.EnqueueSnackMessage("アプリケーションは最新です。");
            }
        }
    }
}
