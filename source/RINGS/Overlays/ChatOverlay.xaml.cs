using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using aframe;
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
            this.ViewModel.ChatOverlaySettings.PropertyChanged += this.ChatOverlaySettings_PropertyChanged;
            this.ResizeMode = this.ViewModel.ChatOverlaySettings.IsLock ?
                ResizeMode.NoResize :
                ResizeMode.CanResizeWithGrip;

            this.FadeoutAnimation.Completed += (_, __) => this.OverlayVisible = false;
            this.ViewModel.HideCallback = () => this.StartFadeout();
            this.ViewModel.ShowCallback = () =>
            {
                if (!this.OverlayVisible)
                {
                    this.StopFadeout();
                    this.OverlayVisible = true;
                }
            };

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
            };

            this.Closed += (_, __) =>
            {
                if (this.ViewModel.ChatOverlaySettings != null)
                {
                    this.ViewModel.ChatOverlaySettings.PropertyChanged -= this.ChatOverlaySettings_PropertyChanged;
                }

                this.ViewModel.Dispose();
            };

            this.MouseLeftButtonDown += (_, e) =>
            {
                if (e.ClickCount == 3)
                {
                    this.MinimizeIcon.Visibility = Visibility.Visible;
                    this.ChatPanel.Visibility = Visibility.Collapsed;
                }
            };

            this.MinimizeIcon.PreviewMouseDown += (_, e) =>
            {
                this.MinimizeIcon.Visibility = Visibility.Collapsed;
                this.ChatPanel.Visibility = Visibility.Visible;
                e.Handled = true;
            };

            this.MinimizeIcon.Visibility = Visibility.Collapsed;
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
            if (!this.OverlayVisible)
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
                this.BeginAnimation(
                    Window.OpacityProperty,
                    this.FadeoutAnimation);
            }
        }

        public void StopFadeout()
        {
            lock (this)
            {
                this.BeginAnimation(
                    Window.OpacityProperty,
                    null,
                    HandoffBehavior.SnapshotAndReplace);
            }
        }

        private void BindableRichTextBox_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            var tb = sender as BindableRichTextBox;

            switch (e.ClickCount)
            {
                case 2:
                    tb.SelectAll();
                    e.Handled = true;
                    break;

                case 3:
                    // チャットオーバーレイを最小化する
                    this.MinimizeIcon.Visibility = Visibility.Visible;
                    this.ChatPanel.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    break;
            }
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            var obj = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer);
            if (obj != null &&
                obj is ScrollBar bar)
            {
                bar.Width = Config.Instance.ChatLogScrollBarWidth;
            }
        }

        #region IOverlay

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
