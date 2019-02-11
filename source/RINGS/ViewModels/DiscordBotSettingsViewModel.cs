using System;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Models;

namespace RINGS.ViewModels
{
    public class DiscordBotSettingsViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        public Action<DiscordChannelModel> ChangeSelectedChannelCallback;
        public Action<DiscordBotModel> ChangeSelectedBotCallback;

        #region Channel Command

        private DelegateCommand addChannelCommand;

        public DelegateCommand AddChannelCommand =>
            this.addChannelCommand ?? (this.addChannelCommand = new DelegateCommand(this.ExecuteAddChannelCommand));

        private void ExecuteAddChannelCommand()
        {
            var model = this.Config.DiscordChannelList;
            var item = new DiscordChannelModel()
            {
                Name = "Channel " + (model.Count + 1)
            };

            model.Add(item);
            this.ChangeSelectedChannelCallback?.Invoke(item);
        }

        private DelegateCommand<DiscordChannelModel> deleteChannelCommand;

        public DelegateCommand<DiscordChannelModel> DeleteChannelCommand =>
            this.deleteChannelCommand ?? (this.deleteChannelCommand = new DelegateCommand<DiscordChannelModel>(this.ExecuteDeleteChannelCommand));

        private void ExecuteDeleteChannelCommand(
            DiscordChannelModel parameter)
        {
            var model = this.Config.DiscordChannelList;
            if (parameter == null)
            {
                return;
            }

            if (model.Contains(parameter))
            {
                var selectIndex = model.IndexOf(parameter) - 1;
                if (selectIndex < 0)
                {
                    selectIndex = 0;
                }

                model.Remove(parameter);

                if (model.Count > 0)
                {
                    this.ChangeSelectedChannelCallback?.Invoke(
                        model[selectIndex]);
                }
            }
        }

        #endregion Channel Command

        #region Bot Command

        private DelegateCommand addBotCommand;

        public DelegateCommand AddBotCommand =>
            this.addBotCommand ?? (this.addBotCommand = new DelegateCommand(this.ExecuteAddBotCommand));

        private void ExecuteAddBotCommand()
        {
            var model = this.Config.DiscordBotList;
            var item = new DiscordBotModel()
            {
                Name = "Bot " + (model.Count + 1)
            };

            model.Add(item);
            this.ChangeSelectedBotCallback?.Invoke(item);
        }

        private DelegateCommand<DiscordBotModel> deleteBotCommand;

        public DelegateCommand<DiscordBotModel> DeleteBotCommand =>
            this.deleteBotCommand ?? (this.deleteBotCommand = new DelegateCommand<DiscordBotModel>(this.ExecuteDeleteBotCommand));

        private void ExecuteDeleteBotCommand(
            DiscordBotModel parameter)
        {
            var model = this.Config.DiscordBotList;
            if (parameter == null)
            {
                return;
            }

            if (model.Contains(parameter))
            {
                var selectIndex = model.IndexOf(parameter) - 1;
                if (selectIndex < 0)
                {
                    selectIndex = 0;
                }

                model.Remove(parameter);

                if (model.Count > 0)
                {
                    this.ChangeSelectedBotCallback?.Invoke(
                        model[selectIndex]);
                }
            }
        }

        #endregion Bot Command
    }
}
