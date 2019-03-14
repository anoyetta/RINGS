using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using aframe;
using aframe.Views;
using RINGS.Models;

namespace RINGS.Overlays
{
    /// <summary>
    /// ChatOverlay.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatOverlay :
        Window,
        IOverlay
    {
        public ChatOverlay() : this(Config.DefaultChatOverlayName)
        {
        }

        public ChatOverlay(
            string name)
        {
            this.InitializeComponent();
            this.ToNonActive();
            this.Opacity = 0;

            this.ViewModel.Name = name;
            this.ViewModel.BindingWindow = this;
            this.ViewModel.ChatOverlaySettings.PropertyChanged += this.ChatOverlaySettings_PropertyChanged;
            this.ResizeMode = this.ViewModel.ChatOverlaySettings.IsLock ?
                ResizeMode.NoResize :
                ResizeMode.CanResizeWithGrip;

            this.FadeoutAnimation.Completed += (_, __) =>
            {
                if (this.ViewModel.IsTimeToHide())
                {
                    this.MinimizeChatPanel();
                }
            };

            this.ViewModel.MinimizeCallback = () => this.MinimizeChatPanel();
            this.ViewModel.HideCallback = () => this.MinimizeChatPanel();
            this.ViewModel.ShowCallback = () => this.ShowChatPanel();

            this.ViewModel.ChangeActivePageCallback = (pageName) =>
            {
                var pages = this.ChatPagesTabControl.Items.Cast<ChatPageSettingsModel>();
                var page = pages?.FirstOrDefault(x => x.Name == pageName);
                if (page != null)
                {
                    this.ChatPagesTabControl.SelectedItem = page;
                }
            };

            this.TitleLabel.MouseLeftButtonDown += (_, __) =>
            {
                if (!this.ViewModel.ChatOverlaySettings.IsLock)
                {
                    this.DragMove();
                }
            };

            this.Loaded += (_, __) =>
            {
                this.OverlayVisible = this.ViewModel.ChatOverlaySettings.IsEnabled;
                this.SubscribeZOrderCorrector();

                // ビルトインブラウザを生成しておく
                if (Config.Instance.IsUseBuiltInBrowser)
                {
                    WPFHelper.Dispatcher.InvokeAsync(() =>
                    {
                        var b = WebViewOverlay.Instance;
                    },
                    DispatcherPriority.ApplicationIdle);
                }
            };

            this.Closed += (_, __) =>
            {
                if (this.ViewModel.ChatOverlaySettings != null)
                {
                    this.ViewModel.ChatOverlaySettings.PropertyChanged -= this.ChatOverlaySettings_PropertyChanged;
                }

                this.ViewModel.Dispose();
            };

            // トリプルクリックは封印する
            /*
            this.MouseLeftButtonDown += (_, e) =>
            {
                if (e.ClickCount == 3)
                {
                    this.MinimizeChatPanel();
                }
            };
            */

            this.MinimizeIcon.PreviewMouseDown += (_, e) =>
            {
                this.ViewModel.IsForceMinimized = false;
                this.ViewModel.ExtendTimeToHide();
                this.ShowChatPanel();
                e.Handled = true;
            };
        }

        public ChatOverlayViewModel ViewModel => this.DataContext as ChatOverlayViewModel;

        private void ChatOverlaySettings_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChatOverlaySettingsModel.IsEnabled):
                    this.Visibility = this.ViewModel.ChatOverlaySettings.IsEnabled ?
                        Visibility.Visible :
                        Visibility.Collapsed;
                    break;

                case nameof(ChatOverlaySettingsModel.IsLock):
                    this.ResizeMode = this.ViewModel.ChatOverlaySettings.IsLock ?
                        ResizeMode.NoResize :
                        ResizeMode.CanResizeWithGrip;
                    break;
            }
        }

        private readonly DoubleAnimation FadeoutAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(1)));
        private DateTime lastFadeoutTimestamp = DateTime.MinValue;

        public void StartFadeout()
        {
            if (this.isMinimized)
            {
                return;
            }

            if ((DateTime.Now - this.lastFadeoutTimestamp).TotalSeconds <= 1.2d)
            {
                return;
            }

            this.lastFadeoutTimestamp = DateTime.Now;
            Timeline.SetDesiredFrameRate(this.FadeoutAnimation, 30);

            lock (this)
            {
                this.BackgroundBorder.BeginAnimation(
                    Border.OpacityProperty,
                    this.FadeoutAnimation);
            }
        }

        public void StopFadeout()
        {
            lock (this)
            {
                this.BackgroundBorder.BeginAnimation(
                    Border.OpacityProperty,
                    null,
                    HandoffBehavior.SnapshotAndReplace);
            }
        }

        private void BindableRichTextBox_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            // 単語単位の選択を有効にするためSelectAllを封印する
            /*
            var tb = sender as BindableRichTextBox;

            switch (e.ClickCount)
            {
                case 2:
                    tb.SelectAll();
                    e.Handled = true;
                    break;
            }
            */
        }

        private void ScrollViewer_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            var obj = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer);
            if (obj != null &&
                obj is ScrollBar bar)
            {
                bar.Width = Config.Instance.ChatLogScrollBarWidth;
            }
        }

        private void ChatLogTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as BindableRichTextBox;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.A:
                        tb.SelectAll();
                        break;

                    case Key.C:
                        if (!tb.Selection.IsEmpty)
                        {
                            var selectedText = new TextRange(
                                tb.Selection.Start,
                                tb.Selection.End).Text;

                            if (!string.IsNullOrEmpty(selectedText))
                            {
                                Clipboard.SetDataObject(selectedText);
                            }
                        }

                        break;
                }
            }
        }

        private volatile bool isMinimized = false;

        private void MinimizeChatPanel()
        {
            lock (this)
            {
                if (!this.isMinimized)
                {
                    this.isMinimized = true;
                    this.MinimizeIcon.Visibility = Visibility.Visible;
                    this.MinimizeButton.Visibility = Visibility.Collapsed;
                    this.BackgroundBorder.Visibility = Visibility.Collapsed;
                    this.ChatPagesTabControl.Visibility = Visibility.Collapsed;
                    this.TitleLabel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowChatPanel()
        {
            lock (this)
            {
                if (this.isMinimized)
                {
                    this.StopFadeout();
                    this.BackgroundBorder.Opacity = 1;
                    this.MinimizeIcon.Visibility = Visibility.Collapsed;
                    this.MinimizeButton.Visibility = Visibility.Visible;
                    this.BackgroundBorder.Visibility = Visibility.Visible;
                    this.ChatPagesTabControl.Visibility = Visibility.Visible;
                    this.TitleLabel.Visibility = Visibility.Visible;
                    this.isMinimized = false;
                }
            }
        }

        private static readonly string ErionesUri = @"https://eriones.com/search?list=on&i=";

        private async void SearchErionesItem_Click(object sender, RoutedEventArgs e)
        {
            var textBox =
                ((e.OriginalSource as MenuItem)?.Parent as ContextMenu)?
                .PlacementTarget as BindableRichTextBox;

            if (textBox == null ||
                textBox.Selection.IsEmpty)
            {
                return;
            }

            var selectedText = new TextRange(
                textBox.Selection.Start,
                textBox.Selection.End).Text;

            await Task.Run(() =>
            {
                var arg = Uri.EscapeDataString(selectedText);
                Process.Start($"{ErionesUri}{arg}");
            });
        }

        #region IOverlay

        public int ZOrder => 0;

        private bool overlayVisible;

        public bool OverlayVisible
        {
            get => this.overlayVisible;
            set => this.SetOverlayVisible(ref this.overlayVisible, value);
        }

        #endregion IOverlay
    }

    public class ScrollToBottomAction : TriggerAction<RichTextBox>
    {
        protected override void Invoke(object parameter)
        {
            this.AssociatedObject?.ScrollToEnd();
        }
    }
}
