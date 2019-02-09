using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RINGS.Models;
using RINGS.ViewModels;

namespace RINGS.Views
{
    /// <summary>
    /// ChatOverlaySettingsView.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatOverlaySettingsView : UserControl
    {
        public ChatOverlaySettingsView()
        {
            this.InitializeComponent();

            this.ViewModel.ShowSubContentCallback = (content) =>
            {
                this.ParentContent.Visibility = Visibility.Hidden;
                this.SubPagePresenter.Content = content;
            };

            this.ViewModel.ClearSubContentCallback = () =>
            {
                this.SubPagePresenter.Content = null;
                this.ParentContent.Visibility = Visibility.Visible;
            };
        }

        public ChatOverlaySettingsViewModel ViewModel => this.DataContext as ChatOverlaySettingsViewModel;

        private void OverlaySettingsPanel_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var model = (sender as FrameworkElement).DataContext;
                this.ViewModel.EditOverlaySettingsCommand.Execute(model as ChatOverlaySettingsModel);
            }
        }
    }
}
