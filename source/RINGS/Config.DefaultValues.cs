using System.Collections.Generic;
using System.Windows.Media;
using RINGS.Common;
using RINGS.Models;

namespace RINGS
{
    public partial class Config
    {
        public readonly static string DefaultChatOverlayName = "General";

        public override Dictionary<string, object> DefaultValues => new Dictionary<string, object>()
        {
            { nameof(Scale), 1.0d },
            { nameof(X), 200 },
            { nameof(Y), 100 },
            { nameof(W), 1024 },
            { nameof(H), 576 },
            { nameof(IsStartupWithWindows), false },
            { nameof(IsMinimizeStartup), false },

            { nameof(ChatOverlaySettings), new[]
            {
                new ChatOverlaySettingsModel()
                {
                    Name = DefaultChatOverlayName,
                    X = 20,
                    Y = 20,
                    W = 640,
                    H = 480,
                    IsLock = false,
                    Scale = 1.0d,
                    BackgroundColor = Colors.Black,
                    PCNameStyle = PCNameStyles.FullName,
                    ChatPages = new[]
                    {
                        new ChatPageSettingsModel()
                        {
                            Name = "ALL",
                            ChannelSettings = ChatChannelSettingsModel.CreateDefaultChannels(true),
                        }
                    },
                }
            }}
        };
    }
}
