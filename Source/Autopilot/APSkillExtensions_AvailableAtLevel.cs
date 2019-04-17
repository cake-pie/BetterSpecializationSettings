using System;
using Harmony;

namespace BetterSandboxSpecializations.Autopilot
{
    // class APSkillExtensions
    // https://kerbalspaceprogram.com/api/class_a_p_skill_extensions.html

    // public static bool AvailableAtLevel(this VesselAutopilot.AutopilotMode mode, Vessel vessel)
    [HarmonyPatch(typeof(APSkillExtensions))]
    [HarmonyPatch("AvailableAtLevel")]
    [HarmonyPatch(new Type[] { typeof(VesselAutopilot.AutopilotMode), typeof(Vessel) })]
    internal class APSkillExtensions_AvailableAtLevel_vessel
    {
        [HarmonyPrefix]
        private static bool Prefix(VesselAutopilot.AutopilotMode mode, Vessel vessel, ref bool __result)
        {
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
                    // Stock default sandbox behavior ( EnableKerbalExperience FALSE, EnableFullSASInSandbox FALSE ) purports to give all crew full SAS
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

    // public static bool AvailableAtLevel(this VesselAutopilot.AutopilotMode mode, int skillLvl)
    [HarmonyPatch(typeof(APSkillExtensions))]
    [HarmonyPatch("AvailableAtLevel")]
    [HarmonyPatch(new Type[] { typeof(VesselAutopilot.AutopilotMode), typeof(int) })]
    internal class APSkillExtensions_AvailableAtLevel_int
    {
        [HarmonyPrefix]
        private static bool Prefix(VesselAutopilot.AutopilotMode mode, int skillLvl, ref bool __result)
        {
            __result = skillLvl >= mode.GetRequiredSkill();
            return false;
        }
    }
}
