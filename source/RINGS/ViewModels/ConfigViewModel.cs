using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class ConfigViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
