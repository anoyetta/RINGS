using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class AppSettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
