using System;
using System.Threading.Tasks;
using System.Windows;
using aframe;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Models;
using RINGS.Overlays;
using RINGS.Views;

namespace RINGS.ViewModels
{
    public class ChatOverlaySettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        public Action<FrameworkElement> ShowSubContentCallback { get; set; }

        public Action ClearSubContentCallback { get; set; }

        #region Commands

        private DelegateCommand addOverlaySettingsCommand;

        public DelegateCommand AddOverlaySettingsCommand =>
            this.addOverlaySettingsCommand ?? (this.addOverlaySettingsCommand = new DelegateCommand(this.ExecuteAddOverlaySettingsCommand));

        private void ExecuteAddOverlaySettingsCommand()
        {
            var model = new ChatOverlaySettingsModel()
            {
                Name = "Overlay " + (this.Config.ChatOverlaySettings.Length + 1),
            };

            var view = new ChatOverlaySettingsEditView();
            view.ViewModel.Model = model;
            view.ViewModel.ClearSubContentCallback = this.ClearSubContentCallback;
            this.ShowSubContentCallback?.Invoke(view);
        }

        private DelegateCommand<ChatOverlaySettingsModel> editOverlaySettingsCommand;

        public DelegateCommand<ChatOverlaySettingsModel> EditOverlaySettingsCommand =>
            this.editOverlaySettingsCommand ?? (this.editOverlaySettingsCommand = new DelegateCommand<ChatOverlaySettingsModel>(model => this.ExecuteEditOverlaySettingsCommand(model)));

        private void ExecuteEditOverlaySettingsCommand(
            ChatOverlaySettingsModel model)
        {
            if (model == null)
            {
                return;
            }

            var view = new ChatOverlaySettingsEditView();
            view.ViewModel.Model = model;
            view.ViewModel.ClearSubContentCallback = this.ClearSubContentCallback;
            this.ShowSubContentCallback?.Invoke(view);
        }

        private DelegateCommand<ChatOverlaySettingsModel> deleteOverlaySettingsCommand;

        public DelegateCommand<ChatOverlaySettingsModel> DeleteOverlaySettingsCommand =>
            this.deleteOverlaySettingsCommand ?? (this.deleteOverlaySettingsCommand = new DelegateCommand<ChatOverlaySettingsModel>(this.ExecuteDeleteOverlaySettingsCommand));

        private async void ExecuteDeleteOverlaySettingsCommand(
            ChatOverlaySettingsModel model)
        {
            if (model == null)
            {
                return;
            }

            var title = "Remove Overlay";
            var message = $"{model.Name} を削除しますか？";

            var result = await MessageBoxHelper.ShowMessageAsync(
                title,
                message,
                MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            this.Config.RemoveChatOverlaySettings(model);

            await Task.Run(() => this.Config.Save());

            ChatOverlaysController.Instance.RefreshOverlays();
        }

        #endregion Commands
    }
}
