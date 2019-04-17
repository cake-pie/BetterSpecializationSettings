using System;
using Harmony;

namespace BetterSandboxSpecializations.Autopilot
{
    // class APSkillExtensions
    // - public static bool AvailableAtLevel(this VesselAutopilot.AutopilotMode mode, Vessel vessel)
    // https://kerbalspaceprogram.com/api/class_a_p_skill_extensions.html

    [HarmonyPatch(typeof(APSkillExtensions))]
    [HarmonyPatch("AvailableAtLevel")]
    [HarmonyPatch(new Type[] { typeof(VesselAutopilot.AutopilotMode), typeof(Vessel) })]
    internal class APSkillExtensions_AvailableAtLevel
    {
        [HarmonyPrefix]
        private static bool Prefix(VesselAutopilot.AutopilotMode mode, Vessel vessel, ref bool __result)
        {
            // Use stock behavior when any of the following is true
            if (
                // "Enable Kerbal Experience" is on (user adjustable; default ON in career, OFF in science and sandbox)
                //   Stock behavior requires Pilot (ExperienceEffect.AutopilotSkill) for crewed, ModuleSAS for uncrewed
                HighLogic.CurrentGame.Parameters.EnableKerbalExperience() ||

                // "All SAS Modes on all probes" is on (user adjustable in science and sandbox; default OFF; not available in career)
                //   Stock behavior effectively gives all SAS for free all the time, since uncrewed doesn't require ModuleSAS, and lack of qualified crew just falls back on uncrewed anyway
                HighLogic.CurrentGame.Parameters.EnableFullSASInSandbox() ||

                // In any game mode other than career, science, or sandbox, which we don't want to mess with
                !( HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX || HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            )
                return true;

            // Modded behavior
            __result = vessel.VesselValues.AutopilotSkill.value >= mode.GetRequiredSkill();
            return false;

            // Okay seriously, that looks waaaaay simpler than it actually is, so verbose pseudocode + documentation is in order here!
            /*
                if (requirePilotForSAS)
                {
                    // Despite not tracking kerbal experience (i.e. all crew always at Lvl 5) we can make specializations more meaningful
                    // Require Pilot (ExperienceEffect.AutopilotSkill) for crewed, ModuleSAS for uncrewed
                    // Basically, stock default career behavior, except that all pilots are always Lvl 5.
                    // This is the expression you see above
                    __result = vessel.VesselValues.AutopilotSkill.value >= mode.GetRequiredSkill();
                }
                else
                {
                    // Allow any crewmember to provide full SAS, but require ModuleSAS for uncrewed
                    // The stock default sandbox behavior is buggy and brittle, so we have to implement a better version of it
                    // Something like:
                    __result = hasAnyCrewAutopilot(vessel) || hasProbeAutopilot(mode, vessel);

                    // Checking probes is easy:
                    bool hasProbeAutopilot(mode, vessel) {
                        return vessel.VesselValues.AutopilotSASSkill.value >= mode.GetRequiredSkill();
                    }

                    // Checking crew is the part that needs fixing:
                    // Stock default sandbox behavior ( KerbalExperienceEnabled FALSE, EnableFullSASInSandbox FALSE ) purports to give all crew full SAS
                    // But testing shows that it seems to actually check for ExperienceEffect.{ AutopilotSkill, RepairSkill, ScienceSkill } in lieu of { Pilot, Engineer, Scientist } respectively
                    // This means that other specializations (from mods) will not qualify if they don't have one of these three skills!

                    // One way to fix it would be to check for any onboard ProtoCrewMember of pcm.type == KerbalType.Crew rather than KerbalType.Tourist
                    // However, that approach would break expected behavior for other mods that involve disabling kerbal skills, e.g.
                    //  - mods that modify pcm.trait to Tourist (without changing pcm.type)
                    //  - Kerbal Status Effects

                    // Instead, we implement our own skill SandboxAutopilotSkill : ExperienceEffect
                    // and grant it to all non-tourist/civilian specializations (EXPERIENCE_TRAIT)s

                    // Then, naively, the first thought would be to implement hasAnyCrewAutopilot(vessel) check in one of two ways:
                    //  - iterate through all vessel occupants looking for pcm.HasEffect<SandboxAutopilotSkill>() (potentially costly) or
                    //  - build infrastructure similar to PartValues / VesselValues specifically for the SandboxAutopilotSkill skill

                    // Or, we could be smart about it, and have SandboxAutopilotSkill perform OnRegister(part)/OnUnregister(part) selectively
                    // i.e. only when the necessary combination of game mode and GameParameters is satisfied
                    // Registration of the skill to a part adds it to the already existent part.PartValues.AutopilotSkill
                    // This little trick allows us to arrive at the same expression used for the (requirePilotForSAS == true) case!
               }
            */
        }
    }
}
