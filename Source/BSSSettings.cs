using System.Reflection;
using KSP.Localization;

namespace BetterSpecializationSettings
{
    public abstract class BSSSettings : GameParameters.CustomParameterNode
    {
        protected readonly string _title = "";
        protected readonly string _displaySection = Localizer.Format("#BSS_LOC_section"); // "Better Specialization Settings"
        protected const string _section = "BetterSpecializationSettings";
        protected readonly int _order = int.MaxValue;
        protected readonly bool _hasPresets = true;

        protected const GameParameters.GameMode gmNM = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE | GameParameters.GameMode.SANDBOX;
        protected const GameParameters.GameMode gmSS = GameParameters.GameMode.SCIENCE | GameParameters.GameMode.SANDBOX;
        protected const GameParameters.GameMode gmCrr = GameParameters.GameMode.CAREER;
        protected const GameParameters.GameMode gmSci = GameParameters.GameMode.SCIENCE;
        protected const GameParameters.GameMode gmSnd = GameParameters.GameMode.SANDBOX;

        public BSSSettings() {}
        protected BSSSettings(string title, int order, bool hasPresets = true)
        {
            _title = title;
            _order = order;
            _hasPresets = hasPresets;
        }

        #region Properties
        public override string Title
        {
            get { return _title; }
        }
        public override string DisplaySection
        {
            get { return _displaySection; }
        }
        public override string Section
        {
            get { return _section; }
        }
        public override int SectionOrder
        {
            get { return _order; }
        }
        public override bool HasPresets
        {
            get { return _hasPresets; }
        }
        public override GameParameters.GameMode GameMode
        {
            get { return gmNM; }
        }
        #endregion

        // unfortunately, moving gmDisp* / useXP* here would cause them to be placed
        // at the bottom of the section, below derived classes members
    }
}
