using System.Reflection;
using KSP.Localization;

namespace BetterSpecializationSettings.Repair
{
    public class BSSRepairSettings : BSSSettings
    {
        internal const int StockLvlReq = 3;

        public BSSRepairSettings() : base(
            title: Localizer.Format("#BSS_LOC_REPAIR_title"), // "Repair"
            order: 2
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
            "#BSS_LOC_REPAIR_repwheel_title",               // "Require engineer to repair legs / wheels"
            toolTip = "#BSS_LOC_REPAIR_repwheel_tooltip",   // "If off, any crewmember can repair landing legs and wheels.\nIf on, an engineer will be required."
            gameMode = gmNM
        )]
        public bool requireEngineerForWheelRep = true;

        [GameParameters.CustomParameterUI(
            "#BSS_LOC_ovrlvlreq_title",                     // "Override required level"
            toolTip = "#BSS_LOC_ovrlvlreq_tooltip",         // "If off, stock behavior will be used.\nIf on, custom setting will be used."
            gameMode = gmNM
        )]
        public bool overrideLvlReq = false;
        [GameParameters.CustomIntParameterUI(
            "#BSS_LOC_lvlreq_title",                        // "Required level"
            toolTip = "#BSS_LOC_REPAIR_lvlreq_tooltip",     // "Custom setting for the skill level required to repair legs / wheels."
            minValue = 0, maxValue = 5,
            gameMode = gmNM
        )]
        public int customLvlReq = StockLvlReq;

        [GameParameters.CustomStringParameterUI("", autoPersistance = false)]
        public string behaviorStock = Localizer.Format("#BSS_LOC_stock_behavior");      // "Stock behavior is in use:"
        [GameParameters.CustomStringParameterUI("", autoPersistance = false)]
        public string behaviorCustom = Localizer.Format("#BSS_LOC_custom_behavior");    // "Custom behavior is in use:"

        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines=3)]
        public string enggSkillDesc = Localizer.Format("#BSS_LOC_REPAIR_enggskill_desc", StockLvlReq);
        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines=2)]
        public string reqEnggDesc = Localizer.Format("#BSS_LOC_REPAIR_reqengg_desc");
        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines=2)]
        public string anyCrewDesc = Localizer.Format("#BSS_LOC_REPAIR_anycrew_desc");
        #endregion

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "requireEngineerForWheelRep") return true;
            if (member.Name == "overrideLvlReq") return true;
            if (member.Name == "customLvlReq") return true;
            if (member.Name.StartsWith("gmDisp")) return true;

            bool useXP = parameters.EnableKerbalExperience();
            if (member.Name == "useXPon") return useXP;
            if (member.Name == "useXPoff") return !useXP;

            bool reqEngg = parameters.RequireEngineerForWheelRep();
            bool ovrLvl = parameters.OverrideLvlReqForWheelRep();
            if (member.Name == "behaviorStock") return (useXP && !ovrLvl) || (!useXP && !reqEngg);
            if (member.Name == "behaviorCustom") return (useXP && ovrLvl) || (!useXP && reqEngg);
            if (member.Name == "enggSkillDesc")
            {
                if (!useXP) return false;
                if (ovrLvl) enggSkillDesc = Localizer.Format("#BSS_LOC_REPAIR_enggskill_desc", parameters.CustomLvlReqForWheelRep().ToString());
                else enggSkillDesc = Localizer.Format("#BSS_LOC_REPAIR_enggskill_desc", StockLvlReq.ToString());
                return true;
            }
            if (member.Name == "reqEnggDesc") return !useXP && reqEngg;
            if (member.Name == "anyCrewDesc") return !useXP && !reqEngg;
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "requireEngineerForWheelRep")
                return !parameters.EnableKerbalExperience();
            if (member.Name == "overrideLvlReq")
                return parameters.EnableKerbalExperience();
            if (member.Name == "customLvlReq")
                return parameters.EnableKerbalExperience() && parameters.OverrideLvlReqForWheelRep();
            return true;
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    requireEngineerForWheelRep = false;
                    break;
                case GameParameters.Preset.Normal:
                case GameParameters.Preset.Moderate:
                case GameParameters.Preset.Hard:
                    requireEngineerForWheelRep = true;
                    break;
            }
            overrideLvlReq = false;
            customLvlReq = StockLvlReq;
        }
    }
}
