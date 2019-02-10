using System.Windows;
using System.Windows.Controls;
using RINGS.ViewModels;

namespace RINGS.Views
{
    /// <summary>
    /// ChatLinkSettingsView.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatLinkSettingsView : UserControl
    {
        public ChatLinkSettingsView()
        {
            this.InitializeComponent();

            this.CharacterProfilePanel.Visibility = Visibility.Collapsed;
            this.ViewModel.ChangeSelectedPageCallback = (page)
                => this.CharacterProfilesListBox.SelectedItem = page;

            this.CharacterProfilesListBox.SelectedIndex = 0;
        }

        public ChatLinkSettingsViewModel ViewModel => this.DataContext as ChatLinkSettingsViewModel;

        private void CharacterProfilesListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            var model = e.AddedItems.Count > 0 ?
                e.AddedItems[0] :
                null;

            this.CharacterProfilePanel.DataContext = model;
            this.CharacterProfilePanel.Visibility = model != null ?
                Visibility.Visible :
                Visibility.Collapsed;
        }
    }
}
