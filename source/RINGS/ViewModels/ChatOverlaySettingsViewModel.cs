using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class ChatOverlaySettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
