using Prism.Mvvm;

namespace RINGS.ViewModels
{
    public class HelpViewModel : BindableBase
    {
        public Config Config => Config.Instance;
    }
}
