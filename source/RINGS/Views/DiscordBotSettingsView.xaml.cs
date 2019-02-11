using System.Windows.Controls;
using RINGS.ViewModels;

namespace RINGS.Views
{
    /// <summary>
    /// DiscordBotSettingsView.xaml の相互作用ロジック
    /// </summary>
    public partial class DiscordBotSettingsView : UserControl
    {
        public DiscordBotSettingsView()
        {
            this.InitializeComponent();

            this.ViewModel.ChangeSelectedChannelCallback = (item)
                => this.DISCORDChannelsListBox.SelectedItem = item;
            this.ViewModel.ChangeSelectedBotCallback = (item)
                => this.BotListBox.SelectedItem = item;
        }

        public DiscordBotSettingsViewModel ViewModel => this.DataContext as DiscordBotSettingsViewModel;
    }
}
