using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class ProfilesSettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
