using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using aframe.Views;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace aframe
{
    public static class MessageBoxHelper
    {
        public async static Task<MessageDialogResult> ShowMessageAsync(
            string title,
            string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings settings = null)
            => await WPFHelper.MainWindow?.ShowMessageAsync(
                title,
                message,
                style,
                settings);

        public static Task ShowExceptionAsync(
            string title,
            string message,
            Exception exception = null,
            MetroDialogSettings settings = null,
            [CallerMemberName]string memberName = null,
            [CallerFilePath]string filePath = null,
            [CallerLineNumber]int lineNumber = 0)
        {
            var metro = WPFHelper.MainWindow;
            if (metro == null)
            {
                return null;
            }

            var dialog = new CustomDialog(
                metro,
                settings)
            {
                Title = title
            };

            dialog.Content = new MessageView(
                () => metro.HideMetroDialogAsync(dialog))
            {
                Message = message,
                Exception = exception,
            };

            return metro.ShowMetroDialogAsync(dialog);
        }

        public static void ShowDialogMessageWindow(
            Window owner,
            string title,
            string message,
            Exception exception = null,
            string windowTitle = null)
            => MessageWindow.Show(owner, title, message, exception, windowTitle, true);

        public static void ShowDialogMessageWindow(
            string title,
            string message,
            Exception exception = null,
            string windowTitle = null)
            => MessageWindow.Show(null, title, message, exception, windowTitle, true);

        public static void ShowMessageWindow(
            string title,
            string message,
            Exception exception = null,
            string windowTitle = null)
            => MessageWindow.Show(title, message, exception, windowTitle);

        public static void ShowMessageWindow(
            Window owner,
            string title,
            string message,
            Exception exception = null,
            string windowTitle = null)
            => MessageWindow.Show(owner, title, message, exception, windowTitle);

        public delegate void EnqueueSnackbarDelegate(string message, bool neverConsiderToBeDuplicate = false);

        public static EnqueueSnackbarDelegate EnqueueSnackbarCallback;

        /// <summary>
        /// スナックバーにメッセージ表示キューを登録する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="neverConsiderToBeDuplicate">重複するメッセージを消去しない？true:消去しない, false:消去する</param>
        public static void EnqueueSnackMessage(
            string message,
            bool neverConsiderToBeDuplicate = false)
            => EnqueueSnackbarCallback?.Invoke(message, neverConsiderToBeDuplicate);

        public delegate bool ShowSaveFileDialogDelegate(SaveFileDialog dialog);

        public static ShowSaveFileDialogDelegate ShowSaveFileDialogCallback;

        /// <summary>
        /// 指定されたSaveFileDialogをメインWindowの子として表示する
        /// </summary>
        /// <param name="dialog">SaveFileDialog</param>
        /// <returns></returns>
        public static bool ShowSaveFileDialog(
            SaveFileDialog dialog)
            => ShowSaveFileDialogCallback?.Invoke(dialog) ?? false;

        public delegate bool ShowOpenFileDialogDelegate(OpenFileDialog dialog);

        public static ShowOpenFileDialogDelegate ShowOpenFileDialogCallback;

        /// <summary>
        /// 指定されたOpenFileDialogをメインWindowの子として表示する
        /// </summary>
        /// <param name="dialog">OpenFileDialog</param>
        /// <returns></returns>
        public static bool ShowOpenFileDialog(
            OpenFileDialog dialog)
            => ShowOpenFileDialogCallback?.Invoke(dialog) ?? false;
    }
}
