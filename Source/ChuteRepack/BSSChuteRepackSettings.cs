using System.Reflection;
using KSP.Localization;

namespace BetterSpecializationSettings.ChuteRepack
{
    public class BSSChuteRepackSettings : BSSSettings
    {
        internal const int StockLvlReq = 1;

        public BSSChuteRepackSettings() : base(
            title: Localizer.Format("#BSS_LOC_CHTRPK_title"), // "Parachute Repacking"
            order: 3
        ) {}

        #region UI Elements
        // #BSS_LOC_gamemode = "The game mode is"
        [GameParameters.CustomStringParameterUI("#BSS_LOC_gamemode", autoPersistance = false, gameMode = gmCrr)]
        public string gmDispCrr = Localizer.Format("#autoLOC_190722"); // #autoLOC_190722 = Career
        [GameParameters.CustomStringParameterUI("#BSS_LOC_gamemode", autoPersistance = false, gameMode = gmSci)]
        public string gmDispSci = Localizer.Format("#autoLOC_190714"); // #autoLOC_190714 = Science
        [GameParameters.CustomStringParameterUI("#BSS_LOC_gamemode", autoPersistance = false, gameMode = gmSnd)]
        public string gmDispSnd = Localizer.Format("#autoLOC_190706"); // #autoLOC_190706 = Sandbox

        // #autoLOC_140970 = Enable Kerbal Experience
        [GameParameters.CustomStringParameterUI("#autoLOC_140970", autoPersistance = false)]
        public string useXPon = Localizer.Format("#BSS_LOC_on");
        [GameParameters.CustomStringParameterUI("#autoLOC_140970", autoPersistance = false)]
        public string useXPoff = Localizer.Format("#BSS_LOC_off");

        [GameParameters.CustomParameterUI(
            "#BSS_LOC_CHTRPK_repack_title",                // "Require engineer to repack parachutes"
            toolTip = "#BSS_LOC_CHTRPK_repack_tooltip",    // "If off, any crewmember can provide repack parachutes.\nIf on, an engineer will be required."
            gameMode = gmNM
        )]
        public bool requireEngineerForChuteRepack = true;

        [GameParameters.CustomParameterUI(
            "#BSS_LOC_ovrlvlreq_title",                     // "Override required level"
            toolTip = "#BSS_LOC_ovrlvlreq_tooltip",         // "If off, stock behavior will be used.\nIf on, custom setting will be used."
            gameMode = gmNM
        )]
        public bool overrideLvlReq = false;
        [GameParameters.CustomIntParameterUI(
            "#BSS_LOC_lvlreq_title",                        // "Required level"
            toolTip = "#BSS_LOC_CHTRPK_lvlreq_tooltip",      // "Custom setting for the skill level required to repack parachutes."
            minValue = 0, maxValue = 5,
            gameMode = gmNM
        )]
        public int customLvlReq = StockLvlReq;

        [GameParameters.CustomStringParameterUI("", autoPersistance = false)]
        public string behaviorStock = Localizer.Format("#BSS_LOC_stock_behavior");      // "Stock behavior is in use:"
        [GameParameters.CustomStringParameterUI("", autoPersistance = false)]
        public string behaviorCustom = Localizer.Format("#BSS_LOC_custom_behavior");    // "Custom behavior is in use:"

        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines=2)]
        public string enggSkillDesc = Localizer.Format("#BSS_LOC_CHTRPK_enggskill_desc", StockLvlReq);
        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines=2)]
        public string reqEnggDesc = Localizer.Format("#BSS_LOC_CHTRPK_reqengg_desc");
        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines=2)]
        public string anyCrewDesc = Localizer.Format("#BSS_LOC_CHTRPK_anycrew_desc");
        #endregion

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "requireEngineerForChuteRepack") return true;
            if (member.Name == "overrideLvlReq") return true;
            if (member.Name == "customLvlReq") return true;
            if (member.Name.StartsWith("gmDisp")) return true;

            bool useXP = parameters.EnableKerbalExperience();
            if (member.Name == "useXPon") return useXP;
            if (member.Name == "useXPoff") return !useXP;

            bool reqEngg = parameters.RequireEngineerForChuteRepack();
            bool ovrLvl = parameters.OverrideLvlReqForChuteRepack();
            if (member.Name == "behaviorStock") return (useXP && !ovrLvl) || (!useXP && !reqEngg);
            if (member.Name == "behaviorCustom") return (useXP && ovrLvl) || (!useXP && reqEngg);
            if (member.Name == "enggSkillDesc")
            {
                if (!useXP) return false;
                if (ovrLvl) enggSkillDesc = Localizer.Format("#BSS_LOC_CHTRPK_enggskill_desc", parameters.CustomLvlReqForChuteRepack().ToString());
                else enggSkillDesc = Localizer.Format("#BSS_LOC_CHTRPK_enggskill_desc", StockLvlReq.ToString());
                return true;
            }
            if (member.Name == "reqEnggDesc") return !useXP && reqEngg;
            if (member.Name == "anyCrewDesc") return !useXP && !reqEngg;
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "requireEngineerForChuteRepack")
                return !parameters.EnableKerbalExperience();
            if (member.Name == "overrideLvlReq")
                return parameters.EnableKerbalExperience();
            if (member.Name == "customLvlReq")
                return parameters.EnableKerbalExperience() && parameters.OverrideLvlReqForChuteRepack();
            return true;
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    requireEngineerForChuteRepack = false;
                    break;
                case GameParameters.Preset.Normal:
                case GameParameters.Preset.Moderate:
                case GameParameters.Preset.Hard:
                    requireEngineerForChuteRepack = true;
                    break;
            }
            overrideLvlReq = false;
            customLvlReq = StockLvlReq;
        }
    }
}
