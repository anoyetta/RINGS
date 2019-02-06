using Prism.Mvvm;
using RINGS.Models;

namespace RINGS.Overlays
{
    public class ChatOverlayViewModel :
        BindableBase
    {
        public ChatOverlayViewModel() : this(Config.DefaultChatOverlayName)
        {
        }

        public ChatOverlayViewModel(
            string name)
        {
            this.Name = name;
        }

        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                if (this.SetProperty(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Title));
                }
            }
        }

        public string Title => $"RINGS - {this.Name}";

        public Config Config => Config.Instance;

        public ChatOverlaySettingsModel ChatOverlaySettings => Config.Instance.GetChatOverlaySettings(this.name);
    }
}
