using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using aframe.Views;
using Microsoft.Win32;
using Prism.Commands;
using RINGS.Common;
using RINGS.Controllers;

namespace RINGS.Overlays
{
    /// <summary>
    /// AttachmentFileOverlay.xaml の相互作用ロジック
    /// </summary>
    public partial class AttachmentFileOverlay :
        Window,
        IOverlay,
        INotifyPropertyChanged
    {
        private static readonly OpenFileDialog OpenFileDialog = new OpenFileDialog()
        {
            RestoreDirectory = true,
            Multiselect = true,
            Filter = "All Files (*.*)|*.*",
            FilterIndex = 1
        };

        public static (bool Result, string[] FileNames) SelectFiles()
        {
            var fileNames = new string[0];

            OpenFileDialog.InitialDirectory = Config.Instance.FileDirectory;

            var result = OpenFileDialog.ShowDialog() ?? false;
            if (result)
            {
                fileNames = OpenFileDialog.FileNames;
                Config.Instance.FileDirectory = Path.GetDirectoryName(fileNames.First());
            }

            return (result, fileNames);
        }

        public static void SendFile(
            Window parent,
            string file = null)
        {
            var files = default(string[]);

            if (string.IsNullOrEmpty(file))
            {
                var resultSet = SelectFiles();
                if (!resultSet.Result)
                {
                    return;
                }

                files = resultSet.FileNames;
            }
            else
            {
                files = new[] { file };
            }

            var interopHelper = new WindowInteropHelper(parent);
            var currentScreen = System.Windows.Forms.Screen.FromHandle(interopHelper.Handle);

            var view = new AttachmentFileOverlay();
            view.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            view.Files = files;
            view.Show();

            /*
            view.Left = (currentScreen.Bounds.Width - view.ActualWidth) / 2d;
            view.Top = (currentScreen.Bounds.Height - view.ActualHeight) / 2d;
            */
        }

        public AttachmentFileOverlay()
        {
            this.InitializeComponent();
            this.ToNonActive();

            this.MouseLeftButtonDown += (_, __) => this.DragMove();

            this.Loaded += (_, __) =>
            {
                this.Activate();
            };

            if (this.Config.ActiveProfile != null)
            {
                var channels = this.Config.ActiveProfile.ChannelLinkerList
                    .Where(x => x.IsEnabled);

                this.ChatCodeList.AddRange(channels.Select(x => new ChatCodeContainer()
                {
                    ChatCode = x.ChatCode,
                }));
            }

            var lastChatCode = DiscordBotController.Instance.LastSendChatCode;
            if (this.ChatCodeList.Any(x => x.ChatCode == lastChatCode))
            {
                this.ChatCode = lastChatCode;
            }

            if (string.IsNullOrEmpty(this.ChatCode))
            {
                this.ChatCode = this.ChatCodeList.FirstOrDefault()?.ChatCode;
            }
        }

        public Config Config => Config.Instance;

        public ObservableCollection<ChatCodeContainer> ChatCodeList { get; } = new ObservableCollection<ChatCodeContainer>();

        private string[] files;

        public string[] Files
        {
            get => this.files;
            set
            {
                if (this.SetProperty(ref this.files, value))
                {
                    this.FilesText = string.Join(
                        Environment.NewLine,
                        this.files.Select(x => Path.GetFileName(x)));
                }
            }
        }

        private string filesText;

        public string FilesText
        {
            get => this.filesText;
            private set => this.SetProperty(ref this.filesText, value);
        }

        private string chatCode;

        public string ChatCode
        {
            get => this.chatCode;
            set => this.SetProperty(ref this.chatCode, value);
        }

        private string message;

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private DelegateCommand submitCommand;

        public DelegateCommand SubmitCommand =>
            this.submitCommand ?? (this.submitCommand = new DelegateCommand(this.ExecuteSubmitCommand));

        private void ExecuteSubmitCommand()
        {
            if (this.Config.ActiveProfile == null)
            {
                return;
            }

            var first = true;
            foreach (var fileName in this.Files)
            {
                if (File.Exists(fileName))
                {
                    DiscordBotController.Instance.SendMessage(
                        this.ChatCode,
                        this.Config.ActiveProfile.CharacterName,
                        this.Config.ActiveProfile.Alias,
                        first ? this.Message : string.Empty,
                        fileName);

                    first = false;
                }
            }

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
