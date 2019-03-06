using System;
using aframe;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Views;

namespace RINGS.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        #region Content View Creator

        public HomeView HomeView => new HomeView();
        public ChatOverlaySettingsView ChatOverlaySettingsView => new ChatOverlaySettingsView();
        public ChatLinkSettingsView ChatLinkSettingsView => new ChatLinkSettingsView();
        public DiscordBotSettingsView DiscordBotSettingsView => new DiscordBotSettingsView();

        public ConfigView ConfigView => new ConfigView();
        public HelpView HelpView => new HelpView();

        #endregion Content View Creator

        public Func<TaskbarIcon> TaskbarIconGetter { get; set; }

        public Action CloseAction { get; set; }

        private DelegateCommand exitCommand;

        public DelegateCommand ExitCommand =>
            this.exitCommand ?? (this.exitCommand = new DelegateCommand(this.ExecuteExitCommand));

        private async void ExecuteExitCommand()
        {
            var title = "Confirm";
            var message = "アプリケーションを終了しますか？";

            var result = await MessageBoxHelper.ShowMessageAsync(
                title,
                message,
                MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                this.CloseAction?.Invoke();
            }
        }

        #region ContextMenu Commands

        private DelegateCommand showCommand;

        public DelegateCommand ShowCommand =>
            this.showCommand ?? (this.showCommand = new DelegateCommand(this.ExecuteShowCommand));

        private void ExecuteShowCommand()
        {
            MainWindow.Instance.ToShow();
        }

        private DelegateCommand resetCommand;

        public DelegateCommand ResetCommand =>
            this.resetCommand ?? (this.resetCommand = new DelegateCommand(async () =>
            await HomeViewModel.ResetSubscribersAsync(
                true,
                () =>
                {
                    TaskbarIconGetter?.Invoke()?.ShowBalloonTip(
                        "Restarted",
                        "sharlayan and DISCORD Bot restarted.",
                        BalloonIcon.Info);
                })));

        private DelegateCommand endCommand;

        public DelegateCommand EndCommand =>
            this.endCommand ?? (this.endCommand = new DelegateCommand(this.ExecuteEndCommand));

        private void ExecuteEndCommand()
        {
            MainWindow.Instance.ToEnd();
        }

        #endregion ContextMenu Commands
    }
}
