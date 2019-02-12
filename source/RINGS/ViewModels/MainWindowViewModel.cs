using System;
using aframe;
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
    }
}
