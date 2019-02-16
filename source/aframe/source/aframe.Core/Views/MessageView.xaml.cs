using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Prism.Commands;

namespace aframe.Views
{
    /// <summary>
    /// MessageView.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageView :
        UserControl,
        INotifyPropertyChanged
    {
        public MessageView()
        {
            this.InitializeComponent();

            if (WPFHelper.IsDesignMode)
            {
                this.Message = "設定ファイルが見つかりませんでした。\nアプリケーションが正しく配置されているか確認してください。";
                this.Exception = new FileNotFoundException("ファイルが見つかりません。");
            }
        }

        public MessageView(
            Action closeHandler) : this()
            => this.closeHandler = closeHandler;

        private string message;

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        private Exception exception;

        public Exception Exception
        {
            get => this.exception;
            set
            {
                if (this.SetProperty(ref this.exception, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsExistsException));
                    this.RaisePropertyChanged(nameof(this.ExceptionMessage));
                }
            }
        }

        public bool IsExistsException => this.Exception != null;

        public string ExceptionMessage => this.Exception?.ToFormat();

        public Action CloseHandler
        {
            get => this.closeHandler;
            set => this.SetProperty(ref this.closeHandler, value);
        }

        private Action closeHandler;

        private DelegateCommand okCommand;

        public DelegateCommand OKCommand =>
            this.okCommand ?? (this.okCommand = new DelegateCommand(this.ExecuteOKCommand));

        private void ExecuteOKCommand() => this.closeHandler.Invoke();

        private DelegateCommand openLogCommand;

        public DelegateCommand OpenLogCommand =>
            this.openLogCommand ?? (this.openLogCommand = new DelegateCommand(this.ExecuteOpenLogCommand));

        private void ExecuteOpenLogCommand()
        {
            var file = AppLogger.GetCurrentLogFileName();
            if (!string.IsNullOrEmpty(file))
            {
                Process.Start(file);
            }
        }

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
