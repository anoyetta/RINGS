using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aframe;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Common;
using RINGS.Models;
using RINGS.Overlays;

namespace RINGS.ViewModels
{
    public class ChatOverlaySettingsEditViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        public Action ClearSubContentCallback { get; set; }

        private ChatOverlaySettingsModel model;

        public ChatOverlaySettingsModel Model
        {
            get => this.model;
            set
            {
                if (this.SetProperty(ref this.model, value))
                {
                    this.TempName = this.model.Name;
                }
            }
        }

        private string tempName;

        public string TempName
        {
            get => this.tempName;
            set => this.SetProperty(ref this.tempName, value);
        }

        private bool isDummy;

        public bool IsDummy
        {
            get => this.isDummy;
            set
            {
                if (this.SetProperty(ref this.isDummy, value))
                {
                    this.SwithDummyLogs(this.isDummy);
                }
            }
        }

        private async void SwithDummyLogs(
            bool enabled) =>
            await WPFHelper.Dispatcher.InvokeAsync(() =>
            {
                foreach (var page in this.model.ChatPages)
                {
                    if (enabled)
                    {
                        page?.LogBuffer?.LoadDummyLogs();
                    }
                    else
                    {
                        page?.LogBuffer?.RemoveDummyLogs();
                    }
                }
            });

        public IEnumerable<EnumContainer<PCNameStyles>> PCNameStyles =>
            EnumConverter.ToEnumerableContainer<PCNameStyles>();

        #region Command

        private DelegateCommand changeBackgroundCommand;

        public DelegateCommand ChangeBackgroundCommand =>
            this.changeBackgroundCommand ?? (this.changeBackgroundCommand = new DelegateCommand(
                () => CommandHelper.ExecuteChangeColor(
                    () => this.model.BackgroundColor,
                    color => this.model.BackgroundColor = color)));

        private DelegateCommand changeFontCommand;

        public DelegateCommand ChangeFontCommand =>
            this.changeFontCommand ?? (this.changeFontCommand = new DelegateCommand(
                () => CommandHelper.ExecuteChangeFont(
                    () => this.model.Font,
                    font => this.model.Font = font)));

        private DelegateCommand backCommand;

        public DelegateCommand BackCommand =>
            this.backCommand ?? (this.backCommand = new DelegateCommand(this.ExecuteBackCommand));

        private async void ExecuteBackCommand()
        {
            var mesasge = string.Empty;

            if (string.IsNullOrEmpty(this.TempName))
            {
                return;
            }

            if (this.model.Name != this.TempName)
            {
                var any = this.Config.ChatOverlaySettings.Any(x => x.Name == this.TempName);
                if (any)
                {
                    mesasge = $"\"{this.TempName}\" は既に存在しています。";
                }
                else
                {
                    any = this.Config.ChatOverlaySettings.Any(x => x.Name == this.model.Name);
                    if (any)
                    {
                        this.Config.RemoveChatOverlaySettings(this.model);
                    }
                }
            }

            if (!string.IsNullOrEmpty(mesasge))
            {
                MessageBoxHelper.EnqueueSnackMessage(mesasge);
                return;
            }

            this.model.Name = this.TempName;

            if (!this.Config.ChatOverlaySettings.Any(x => x.Name == this.model.Name))
            {
                this.Config.AddChatOverlaySettings(this.model);
            }

            await Task.Run(() => this.Config.Save());

            ChatOverlaysController.Instance.RefreshOverlays();

            this.ClearSubContentCallback?.Invoke();
        }

        #endregion Command

        #region Pages Commands

        public Action<ChatPageSettingsModel> ChangeSelectedPageCallback;

        private DelegateCommand addPageCommand;

        public DelegateCommand AddPageCommand =>
            this.addPageCommand ?? (this.addPageCommand = new DelegateCommand(this.ExecuteAddPageCommand));

        private void ExecuteAddPageCommand()
        {
            var page = new ChatPageSettingsModel()
            {
                Name = "Page " + (this.model.ChatPages.Count + 1)
            };

            this.model.ChatPages.Add(page);
            this.ChangeSelectedPageCallback?.Invoke(page);
        }

        private DelegateCommand<ChatPageSettingsModel> deletePageCommand;

        public DelegateCommand<ChatPageSettingsModel> DeletePageCommand =>
            this.deletePageCommand ?? (this.deletePageCommand = new DelegateCommand<ChatPageSettingsModel>(this.ExecuteDeletePageCommand));

        private void ExecuteDeletePageCommand(
            ChatPageSettingsModel parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (this.model.ChatPages.Contains(parameter))
            {
                var selectIndex = this.model.ChatPages.IndexOf(parameter) - 1;
                if (selectIndex < 0)
                {
                    selectIndex = 0;
                }

                this.model.ChatPages.Remove(parameter);

                if (this.model.ChatPages.Count > 0)
                {
                    this.ChangeSelectedPageCallback?.Invoke(
                        this.model.ChatPages[selectIndex]);
                }
            }
        }

        private DelegateCommand<ChatPageSettingsModel> upPageOrderCommand;

        public DelegateCommand<ChatPageSettingsModel> UpPageOrderCommand =>
            this.upPageOrderCommand ?? (this.upPageOrderCommand = new DelegateCommand<ChatPageSettingsModel>(this.ExecuteUpPageOrderCommand));

        private void ExecuteUpPageOrderCommand(
            ChatPageSettingsModel parameter)
        {
            var removedIdx = this.model.ChatPages.IndexOf(parameter);
            var targetIdx = removedIdx - 1;

            if (targetIdx >= 0)
            {
                this.model.ChatPages.Insert(targetIdx, parameter);
                this.model.ChatPages.RemoveAt(removedIdx + 1);
                this.ChangeSelectedPageCallback?.Invoke(parameter);
            }
        }

        private DelegateCommand<ChatPageSettingsModel> downPageOrderCommand;

        public DelegateCommand<ChatPageSettingsModel> DownPageOrderCommand =>
            this.downPageOrderCommand ?? (this.downPageOrderCommand = new DelegateCommand<ChatPageSettingsModel>(this.ExecuteDownPageOrderCommand));

        private void ExecuteDownPageOrderCommand(
            ChatPageSettingsModel parameter)
        {
            var removedIdx = this.model.ChatPages.IndexOf(parameter);
            var targetIdx = removedIdx + 2;

            if (targetIdx <= this.model.ChatPages.Count)
            {
                this.model.ChatPages.Insert(targetIdx, parameter);
                this.model.ChatPages.RemoveAt(removedIdx);
                this.ChangeSelectedPageCallback?.Invoke(parameter);
            }
        }

        #endregion Pages Commands
    }
}
