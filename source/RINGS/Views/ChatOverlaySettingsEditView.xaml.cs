using System.Windows;
using System.Windows.Controls;
using RINGS.ViewModels;

namespace RINGS.Views
{
    /// <summary>
    /// ChatOverlaySettingsEditView.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatOverlaySettingsEditView : UserControl
    {
        public ChatOverlaySettingsEditView()
        {
            this.InitializeComponent();

            this.PagePanel.Visibility = Visibility.Collapsed;
            this.ViewModel.ChangeSelectedPageCallback = (page)
                => this.PagesListBox.SelectedItem = page;
        }

        public ChatOverlaySettingsEditViewModel ViewModel => this.DataContext as ChatOverlaySettingsEditViewModel;

        private void PagesListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            var model = e.AddedItems.Count > 0 ?
                e.AddedItems[0] :
                null;

            this.PagePanel.DataContext = model;
            this.PagePanel.Visibility = model != null ?
                Visibility.Visible :
                Visibility.Collapsed;
        }
    }
}
