using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using aframe.ViewModels;

namespace aframe.Views
{
    /// <summary>
    /// CreditView.xaml の相互作用ロジック
    /// </summary>
    public partial class CreditView : Window
    {
        public CreditView()
        {
            this.InitializeComponent();
            this.ToNonActive();

            this.Loaded += (_, __) =>
            {
                this.Activate();
                this.StartCreadit();
            };

            this.MouseLeftButtonUp += (_, __) =>
            {
                this.Close();
            };

            this.SubTitle.Text = string.Empty;
            this.Names.Text = string.Empty;
        }

        private readonly List<CreditEntry> CreditList = new List<CreditEntry>();
        private Task showCreditTask;
        private volatile bool isCreditRunning = false;

        private void StartCreadit()
        {
            this.isCreditRunning = true;

            Storyboard.SetTargetName(this.CreaditAnimation, nameof(this.Names));
            Storyboard.SetTargetProperty(this.CreaditAnimation, new PropertyPath(TextBlock.OpacityProperty));
            this.CreaditAnimationStory.Children.Add(this.CreaditAnimation);

            this.showCreditTask = Task.Run(() =>
            {
                foreach (var credit in this.CreditList)
                {
                    if (!this.isCreditRunning)
                    {
                        break;
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        this.Names.Opacity = 0;

                        this.SubTitle.Text = credit.SubTitle;
                        this.Names.Text = string.Join(Environment.NewLine, credit.Names);

                        this.SubTitle.Visibility = !string.IsNullOrEmpty(credit.SubTitle) ?
                            Visibility.Visible :
                            Visibility.Collapsed;

                        this.CreaditAnimationStory.Begin(this, true);
                    });

                    Thread.Sleep(TimeSpan.FromSeconds(7));
                }

                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            });
        }

        private void StopCredit()
        {
            this.isCreditRunning = false;
        }

        public static void ShowCredits(
            IEnumerable<CreditEntry> credits,
            Window owner = null)
        {
            var view = new CreditView()
            {
                Owner = owner
            };

            view.CreditList.AddRange(credits);
            view.Show();
        }

        private readonly Storyboard CreaditAnimationStory = new Storyboard();

        private readonly DoubleAnimationUsingKeyFrames CreaditAnimation = new DoubleAnimationUsingKeyFrames()
        {
            KeyFrames = new DoubleKeyFrameCollection()
            {
                new LinearDoubleKeyFrame(0, ToKeyTime("0:0:0")),
                new LinearDoubleKeyFrame(1, ToKeyTime("0:0:1")),
                new LinearDoubleKeyFrame(1, ToKeyTime("0:0:5")),
                new LinearDoubleKeyFrame(0, ToKeyTime("0:0:6")),
            }
        };

        private static readonly KeyTimeConverter KeyTimeConverter = new KeyTimeConverter();

        private static KeyTime ToKeyTime(string time)
            => (KeyTime)KeyTimeConverter.ConvertFromString(time);
    }
}
