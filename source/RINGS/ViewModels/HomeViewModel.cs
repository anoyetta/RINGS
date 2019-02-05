using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
