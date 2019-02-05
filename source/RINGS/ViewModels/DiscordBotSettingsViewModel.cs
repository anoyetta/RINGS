using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class DiscordBotSettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
