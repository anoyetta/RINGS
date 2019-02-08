using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class ChatOverlaySettingsEditViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
