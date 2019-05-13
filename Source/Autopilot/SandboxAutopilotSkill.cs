using Experience;

namespace BetterSpecializationSettings.Autopilot
{
    public class SandboxAutopilotSkill : ExperienceEffect
    {
        internal static bool use = false;
        public SandboxAutopilotSkill(ExperienceTrait parent) : base(parent) {}
        public SandboxAutopilotSkill(ExperienceTrait parent, float[] modifiers) : base(parent, modifiers) {}

        protected override void OnRegister(Part part)
        {
            if (use) part.PartValues.AutopilotSkill.Add(new EventValueComparison<int>.OnEvent(this.GetValue));
        }

        protected override void OnUnregister(Part part)
        {
            if (use) part.PartValues.AutopilotSkill.Remove(new EventValueComparison<int>.OnEvent(this.GetValue));
        }

        internal void ForceUnregister(Part part)
        {
            part.PartValues.AutopilotSkill.Remove(new EventValueComparison<int>.OnEvent(this.GetValue));
        }

        private int GetValue() { return 5; }
    }
}
