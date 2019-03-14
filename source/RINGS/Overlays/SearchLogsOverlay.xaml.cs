using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using aframe.Views;
using Prism.Commands;

namespace RINGS.Overlays
{
    /// <summary>
    /// SearchLogsOverlay.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchLogsOverlay :
        Window,
        IOverlay,
        INotifyPropertyChanged
    {
        public SearchLogsOverlay()
        {
            this.InitializeComponent();
            this.ToNonActive();

            this.MouseLeftButtonDown += (_, __) => this.DragMove();

            this.Loaded += (_, __) =>
            {
                this.Activate();

                Thread.Sleep(10);
                this.Focus();

                Thread.Sleep(10);
                this.QueryTextBox.Focus();
            };

            this.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    this.ExecuteSubmitCommand();
                }
            };
        }

        public Config Config => Config.Instance;

        private string queryString = string.Empty;

        public string QueryString
        {
            get => this.queryString;
            set => this.SetProperty(ref this.queryString, value);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private DelegateCommand submitCommand;

        public DelegateCommand SubmitCommand =>
            this.submitCommand ?? (this.submitCommand = new DelegateCommand(this.ExecuteSubmitCommand));

        private void ExecuteSubmitCommand()
        {
            if (string.IsNullOrEmpty(this.QueryString))
            {
                return;
            }

            var name = string.Empty;
            var server = string.Empty;

            if (this.QueryString.Contains("@"))
            {
                var parts = this.QueryString.Split('@');
                name = parts[0].Trim();
                server = parts[1].Trim();
            }
            else
            {
                name = this.QueryString;
            }

            var url = string.Empty;

            if (!string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(server))
            {
                url = $@"https://ja.fflogs.com/character/jp/{Uri.EscapeDataString(server)}/{Uri.EscapeDataString(name)}";
            }
            else
            {
                url = $@"https://ja.fflogs.com/search/?term={Uri.EscapeDataString(name)}";
            }

            if (!Config.Instance.IsUseBuiltInBrowser)
            {
                Process.Start(new ProcessStartInfo(url));
            }
            else
            {
                WebViewOverlay.Instance.ShowUrl(
                    this,
                    url,
                    new Size(1200d, 780d));
            }

            Thread.Sleep(100);
            this.Close();
        }

        #region IOverlay

        public int ZOrder => 0;

        public bool OverlayVisible { get; set; }

        #endregion IOverlay

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(
            [CallerMemberName]string propertyName = null)
            => this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

        protected virtual bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        #endregion INotifyPropertyChanged
    }
}
