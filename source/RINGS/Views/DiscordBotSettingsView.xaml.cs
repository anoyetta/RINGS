using System.Windows;
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

            if (this.ViewModel.Config.DiscordChannelList.Count > 0)
            {
                this.DISCORDChannelsListBox.SelectedIndex = 0;
            }
            else
            {
                this.ChannelPanel.Visibility = Visibility.Hidden;
            }

            if (this.ViewModel.Config.DiscordBotList.Count > 0)
            {
                this.BotListBox.SelectedIndex = 0;
            }
            else
            {
                this.BotPanel.Visibility = Visibility.Hidden;
            }

            this.ViewModel.Config.DiscordChannelList.CollectionChanged += (_, __) =>
            {
                this.ChannelPanel.Visibility =
                    this.ViewModel.Config.DiscordChannelList.Count > 0 ?
                    Visibility.Visible :
                    Visibility.Hidden;
            };

            this.ViewModel.Config.DiscordBotList.CollectionChanged += (_, __) =>
            {
                this.BotPanel.Visibility =
                    this.ViewModel.Config.DiscordBotList.Count > 0 ?
                    Visibility.Visible :
                    Visibility.Hidden;
            };
        }

        public DiscordBotSettingsViewModel ViewModel => this.DataContext as DiscordBotSettingsViewModel;
    }
}
