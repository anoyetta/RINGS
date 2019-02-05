using System;
using System.Media;
using System.Reflection;
using System.Windows;
using MahApps.Metro.Controls;

namespace aframe.Views
{
    /// <summary>
    /// MessageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageWindow :
        MetroWindow
    {
        public MessageWindow()
        {
            this.InitializeComponent();
        }

        internal static void ShowDialog(
            string title,
            string message,
            Exception exception = null,
            string windowTitle = null)
            => Show(null, title, message, exception, windowTitle, true);

        internal static void Show(
            string title,
            string message,
            Exception exception = null,
            string windowTitle = null)
            => Show(null, title, message, exception, windowTitle);

        internal static void Show(
            Window owner,
            string title,
            string message,
            Exception exception = null,
            string windowTitle = null,
            bool isModal = false)
        {
            if (string.IsNullOrEmpty(windowTitle))
            {
                windowTitle = Assembly.GetEntryAssembly().GetProduct();
            }

            var window = new MessageWindow()
            {
                Owner = owner,
                Title = windowTitle,
                WindowStartupLocation = owner != null ?
                    WindowStartupLocation.CenterOwner :
                    WindowStartupLocation.CenterScreen
            };

            window.MessageContent.CloseHandler += () => window.Close();
            window.TitleLabel.Content = title;
            window.MessageContent.Message = message;
            window.MessageContent.Exception = exception;

            SystemSounds.Asterisk.Play();

            if (!isModal)
            {
                window.Show();
            }
            else
            {
                window.ShowDialog();
            }
        }
    }
}
