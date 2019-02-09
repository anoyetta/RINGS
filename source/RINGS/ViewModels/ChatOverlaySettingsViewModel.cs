using System;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Models;
using RINGS.Views;

namespace RINGS.ViewModels
{
    public class ChatOverlaySettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        public Action<FrameworkElement> ShowSubContentCallback { get; set; }

        public Action ClearSubContentCallback { get; set; }

        #region Commands

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

        #endregion Commands
    }
}
