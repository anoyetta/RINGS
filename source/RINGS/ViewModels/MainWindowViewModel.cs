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

        public AppSettingsView AppSettingsView => new AppSettingsView();
        public HelpView HelpView => new HelpView();

        #endregion Content View Creator
    }
}
