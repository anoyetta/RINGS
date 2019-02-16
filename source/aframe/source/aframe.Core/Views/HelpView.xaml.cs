using System;
using System.Windows.Controls;
using System.Windows.Threading;
using Prism.Events;

namespace aframe.Views
{
    /// <summary>
    /// HelpView.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpView : UserControl
    {
        public HelpView()
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
        }

        private static readonly EventAggregator LogScrollerMessenger = new EventAggregator();

        public static void SendScrollToEndLog()
            => LogScrollerMessenger.GetEvent<PubSubEvent>().Publish();
    }
}
