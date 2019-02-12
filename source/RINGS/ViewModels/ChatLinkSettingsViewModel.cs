using System;
using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Models;

namespace RINGS.ViewModels
{
    public class ChatLinkSettingsViewModel : BindableBase
    {
        public Action<CharacterProfileModel> ChangeSelectedPageCallback;

        public Config Config => Config.Instance;

        private DelegateCommand addProfileCommand;

        public DelegateCommand AddProfileCommand =>
            this.addProfileCommand ?? (this.addProfileCommand = new DelegateCommand(this.ExecuteAddProfileCommand));

        private void ExecuteAddProfileCommand()
        {
            var model = new CharacterProfileModel()
            {
                CharacterName = "Your Character",
                IsEnabled = true,
            };

            model.ChannelLinkerList.AddRange(
                CharacterProfileModel.CreateDefaultChannelLinkers());

            this.Config.CharacterProfileList.Add(model);
            this.ChangeSelectedPageCallback?.Invoke(model);
        }

        private DelegateCommand<CharacterProfileModel> deletePageCommand;

        public DelegateCommand<CharacterProfileModel> DeleteProfileCommand =>
            this.deletePageCommand ?? (this.deletePageCommand = new DelegateCommand<CharacterProfileModel>(this.ExecuteDeleteProfileCommand));

        private void ExecuteDeleteProfileCommand(
            CharacterProfileModel parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (this.Config.CharacterProfileList.Contains(parameter))
            {
                var selectIndex = this.Config.CharacterProfileList.IndexOf(parameter) - 1;
                if (selectIndex < 0)
                {
                    selectIndex = 0;
                }

                this.Config.CharacterProfileList.Remove(parameter);

                if (this.Config.CharacterProfileList.Count > 0)
                {
                    this.ChangeSelectedPageCallback?.Invoke(
                        this.Config.CharacterProfileList[selectIndex]);
                }
            }
        }
    }
}
