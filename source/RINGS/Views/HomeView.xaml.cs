using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Events;
using RINGS.ViewModels;

namespace RINGS.Views
{
    /// <summary>
    /// HomeView.xaml の相互作用ロジック
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            this.InitializeComponent();

            LogScrollerMessenger.GetEvent<PubSubEvent>().Subscribe(async () =>
            {
                try
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        if (this.IsLoaded)
                        {
                            this.LogScrollViewer.ScrollToEnd();
                        }
                    },
                    DispatcherPriority.Background);
                }
                catch (Exception)
                {
                }
            });

            this.TestChatMessageTextBox.PreviewKeyDown += (sender, e) =>
            {
                var textbox = sender as TextBox;

                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(textbox.Text))
                    {
                        this.ViewModel.SubmitTestMessageCommand.Execute(textbox.Text);
                    }
                }

                if (e.Key == Key.Up)
                {
                    var index = this.TestChatChannelComboBox.SelectedIndex - 1;
                    if (index < 0)
                    {
                        index = 0;
                    }

                    this.TestChatChannelComboBox.SelectedIndex = index;
                }

                if (e.Key == Key.Down)
                {
                    var index = this.TestChatChannelComboBox.SelectedIndex + 1;
                    if (index >= this.TestChatChannelComboBox.Items.Count)
                    {
                        index = this.TestChatChannelComboBox.Items.Count - 1;
                    }

                    this.TestChatChannelComboBox.SelectedIndex = index;
                }
            };
        }

        public HomeViewModel ViewModel => this.DataContext as HomeViewModel;

        private static readonly EventAggregator LogScrollerMessenger = new EventAggregator();

        public static void SendScrollToEndLog()
            => LogScrollerMessenger.GetEvent<PubSubEvent>().Publish();
    }
}
