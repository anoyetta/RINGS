using Prism.Mvvm;
using RINGS.Models;

namespace RINGS.Overlays
{
    public class ChatOverlayViewModel :
        BindableBase
    {
        public Config Config => Config.Instance;

        public ChatOverlaySettingsModel ChatOverlaySettings => Config.Instance.ChatOverlaySettings;
    }
}
