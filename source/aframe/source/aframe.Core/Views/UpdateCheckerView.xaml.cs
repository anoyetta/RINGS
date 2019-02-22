using System.Reflection;
using System.Windows;
using aframe.Updater;
using aframe.ViewModels;
using MahApps.Metro.Controls;

namespace aframe.Views
{
    /// <summary>
    /// UpdateCheckerView.xaml の相互作用ロジック
    /// </summary>
    public partial class UpdateCheckerView : MetroWindow
    {
        private static UpdateCheckerView instance;

        public static void Show(
            string appName,
            Assembly targetAssembly,
            ReleaseNote releaseNote)
        {
            instance = new UpdateCheckerView();

            instance.Owner = WPFHelper.MainWindow;
            instance.ViewModel.AppName.Value = appName;
            instance.ViewModel.TargetAssembly.Value = targetAssembly;
            instance.ViewModel.Model.Value = releaseNote;

            instance.Show();
        }

        public static void CloseUpdateChecker()
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }
        }

        public UpdateCheckerView()
        {
            this.InitializeComponent();
            this.ViewModel.CloseViewCallback = () => this.Close();
        }

        public UpdateCheckerViewModel ViewModel => this.DataContext as UpdateCheckerViewModel;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => this.Close();
    }
}
