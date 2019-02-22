using System.Collections.Generic;

namespace RINGS.Common
{
    public static class ChatCodes
    {
        public static readonly string Say = "000A";
        public static readonly string Yell = "001E";
        public static readonly string Shout = "000B";
        public static readonly string TellOut = "000C";
        public static readonly string TellIn = "000D";

        public static readonly string Party = "000E";
        public static readonly string Alliance = "000F";

        public static readonly string Linkshell1 = "0010";
        public static readonly string Linkshell2 = "0011";
        public static readonly string Linkshell3 = "0012";
        public static readonly string Linkshell4 = "0013";
        public static readonly string Linkshell5 = "0014";
        public static readonly string Linkshell6 = "0015";
        public static readonly string Linkshell7 = "0016";
        public static readonly string Linkshell8 = "0017";

        public static readonly string CrossWorldLinkshell = "0025";

        public static readonly string FreeCompany = "0018";
        public static readonly string FreeCompanyAnnounce = "0245";
        public static readonly string FreeCompanyLogInOut = "2246";

        public static readonly string PvPTeam = "0024";
        public static readonly string PvPTeamAnnounce = "224D";
        public static readonly string PvPTeamLogInOut = "224E";

        public static readonly string Beginners = "001B";
        public static readonly string BeginnersAnnounce = "0A4B";

        public static readonly string StandardEmotes = "001D";
        public static readonly string CustomEmotes = "001C";

        public static readonly string NPC = "0043";
        public static readonly string NPCAnnounce = "0044";

        public static readonly string SystemMessage = "0039";
        public static readonly string PartyRecruiting = "0048";

        public static readonly IDictionary<string, (string DisplayName, string ShortName)> DisplayNames = new Dictionary<string, (string DisplayName, string ShortName)>()
        {
            { Say, ("Say", "") },
            { Yell, ("Yell", "") },
            { Shout, ("Shout", "") },
            { TellOut, ("Tell - Out", "") },
            { TellIn, ("Tell - In", "") },
            { Party, ("Party", "") },
            { Alliance, ("Alliance", "") },

            { Linkshell1, ("Linkshell 1", "LS1") },
            { Linkshell2, ("Linkshell 2", "LS2") },
            { Linkshell3, ("Linkshell 3", "LS3") },
            { Linkshell4, ("Linkshell 4", "LS4") },
            { Linkshell5, ("Linkshell 5", "LS5") },
            { Linkshell6, ("Linkshell 6", "LS6") },
            { Linkshell7, ("Linkshell 7", "LS7") },
            { Linkshell8, ("Linkshell 8", "LS8") },
            { CrossWorldLinkshell, ("Cross World Linkshell", "CWLS") },

            { FreeCompany, ("Free Company", "FC") },
            { FreeCompanyAnnounce, ("Free Company - Announce", "FC") },
            { FreeCompanyLogInOut, ("Free Company - Login/Logout", "FC") },

            { PvPTeam, ("PvP Team", "PvP") },
            { PvPTeamAnnounce, ("PvP Team - Announce", "PvP") },
            { PvPTeamLogInOut, ("PvP Team - Login/Logout", "PvP") },

            { Beginners, ("Beginners", "B") },
            { BeginnersAnnounce, ("Beginners - Announce", "B") },

            { StandardEmotes, ("Standard Emotes", "") },
            { CustomEmotes, ("Custom Emotes", "") },

            { NPC, ("NPC", "") },
            { NPCAnnounce, ("NPC - Announce", "") },

            { SystemMessage, ("System Message", "") },
            { PartyRecruiting, ("Party Recruiting", "") },
        };

        public static readonly IEnumerable<string> All = new[]
        {
            Say, Yell, Shout, Party, Alliance, TellOut, TellIn,
            Linkshell1, Linkshell2, Linkshell3, Linkshell4, Linkshell5, Linkshell6, Linkshell7, Linkshell8,
            CrossWorldLinkshell,
            FreeCompany, FreeCompanyAnnounce, FreeCompanyLogInOut,
            PvPTeam, PvPTeamAnnounce, PvPTeamLogInOut,
            Beginners, BeginnersAnnounce,
            StandardEmotes, CustomEmotes,
            NPC, NPCAnnounce,
            SystemMessage, PartyRecruiting
        };

        public static readonly IEnumerable<string> Publics = new[]
        {
            Say, Yell, Shout
        };

        public static readonly IEnumerable<string> Linkshells = new[]
        {
            Linkshell1, Linkshell2, Linkshell3, Linkshell4, Linkshell5, Linkshell6, Linkshell7, Linkshell8,
            CrossWorldLinkshell,
        };

        public static readonly IEnumerable<string> LinkableChannels = new[]
        {
            CrossWorldLinkshell,
            FreeCompany,
            Linkshell1, Linkshell2, Linkshell3, Linkshell4, Linkshell5, Linkshell6, Linkshell7, Linkshell8,
        };
    }

    public class ChatCodeContainer
    {
        public string ChatCode { get; set; }

        public string DisplayName => ChatCodes.DisplayNames[this.ChatCode].DisplayName;

        public string ShortName => ChatCodes.DisplayNames[this.ChatCode].ShortName;
    }
}
