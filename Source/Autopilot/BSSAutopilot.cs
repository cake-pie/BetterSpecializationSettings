using System;
using System.Reflection;
using System.Collections.Generic;
using Harmony;

namespace BetterSandboxSpecializations.Autopilot
{
    internal class BSSAutopilot
    {
        internal static bool KSP_1_6_plus = true;

        private const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Static;
        private readonly MethodInfo APSE_AALV_original = null;
        private readonly MethodInfo APSE_AALV_patch = null;
        private readonly MethodInfo APSE_AALI_original = null;
        private readonly MethodInfo APSE_AALI_patch = null;

        private bool usePatch = false;

        public BSSAutopilot()
        {
            KSP_1_6_plus = BSSAddon.kspAtLeast(1,6);

            // Backward compatibility for KSP < 1.6 by patching in 1.6+ API only where available
            if (KSP_1_6_plus)
            {
                BSSAddon.harmony.Patch(typeof(BSSAutopilotExtensions).GetMethod("EnableFullSASInSandbox", bf),
                    prefix: new HarmonyMethod( typeof(BSSAutopilotExtensions_EnableFullSASInSandbox).GetMethod("Prefix", bf) ));
                APSE_AALV_original = typeof(APSkillExtensions).GetMethod("AvailableAtLevel", new Type[] { typeof(VesselAutopilot.AutopilotMode), typeof(Vessel) });
                APSE_AALV_patch = typeof(APSkillExtensions_AvailableAtLevel_vessel).GetMethod("Prefix", bf);
            }
            APSE_AALI_original = typeof(APSkillExtensions).GetMethod("AvailableAtLevel", new Type[] { typeof(VesselAutopilot.AutopilotMode), typeof(int) });
            APSE_AALI_patch = typeof(APSkillExtensions_AvailableAtLevel_int).GetMethod("Prefix", bf);

            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            //GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGameStatePostLoad.Add(OnGameStatePostLoad);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
        }

        public void OnDestroy()
        {
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
            GameEvents.onGameStateCreated.Remove(OnGameStateCreated);
            //GameEvents.onGameStateLoad.Remove(OnGameStateLoad);
            GameEvents.onGameStatePostLoad.Remove(OnGameStatePostLoad);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
        }

        #region GameEvents
        private void OnLevelWasLoaded(GameScenes gs)
        {
            // turn off sandbox autopilot and unpatch whenever returning to main menu
            if (gs == GameScenes.MAINMENU)
            {
                if (usePatch) unpatch();
                updateSandboxAutopilotSkill(false);
            }
        }
        private void OnGameStateCreated(Game g)
        {
            Log("OnGameStateCreated");
            applySettings(g);
        }
        /*private void OnGameStateLoad(ConfigNode node)
        {
            Log("OnGameStateLoad");
            applySettings(HighLogic.CurrentGame);
        }*/
        private void OnGameStatePostLoad(ConfigNode node)
        {
            Log("OnGameStatePostLoad");
            applySettings(HighLogic.CurrentGame);
        }
        private void OnGameSettingsApplied()
        {
            Log("OnGameSettingsApplied");
            applySettings(HighLogic.CurrentGame);
        }
        #endregion

        private void applySettings(Game g)
        {
            // In any game mode other than career, science, or sandbox, which we don't want to mess with
            if (!( g.Mode == Game.Modes.SANDBOX || g.Mode == Game.Modes.SCIENCE_SANDBOX || g.Mode == Game.Modes.CAREER))
                return;

            if (
                // "Enable Kerbal Experience" is on (user adjustable; default ON in career, OFF in science and sandbox)
                //   Stock behavior requires Pilot (ExperienceEffect.AutopilotSkill) for crewed, ModuleSAS for uncrewed
                g.Parameters.EnableKerbalExperience() ||

                // KSP 1.6+ "All SAS Modes on all probes" is on (user adjustable in science and sandbox; default OFF; not available in career)
                //   Stock behavior requires any crew regardless of specialization, whereas uncrewed doesn't require ModuleSAS
                // KSP < 1.6 checks our own implementation of this option
                g.Parameters.EnableFullSASInSandbox()
            )
            {
                if (usePatch) unpatch();
                updateSandboxAutopilotSkill(false);
            }
            else
            {
                if (!usePatch) patch();

                if (g.Parameters.RequirePilotForSAS())
                    updateSandboxAutopilotSkill(false);
                else
                    updateSandboxAutopilotSkill(true);
            }
        }

        private void patch()
        {
            usePatch = true;
            if (KSP_1_6_plus) BSSAddon.harmony.Patch(APSE_AALV_original, new HarmonyMethod(APSE_AALV_patch));
            BSSAddon.harmony.Patch(APSE_AALI_original, new HarmonyMethod(APSE_AALI_patch));
        }

        private void unpatch()
        {
            usePatch = false;
            if (KSP_1_6_plus) BSSAddon.harmony.Unpatch(APSE_AALV_original, APSE_AALV_patch);
            BSSAddon.harmony.Unpatch(APSE_AALI_original, APSE_AALI_patch);
        }

        private void updateSandboxAutopilotSkill(bool use)
        {
            if(SandboxAutopilotSkill.use == use) return;
            Log("Sandbox autopilot skill {0}", (use ? "OFF -> ON" : "ON -> OFF"));
            SandboxAutopilotSkill.use = use;

            if (HighLogic.LoadedSceneIsFlight)
            {
                List<Vessel> vessels = FlightGlobals.VesselsLoaded;
                for (int i = 0; i < vessels.Count; i++)
                {
                    List<Part> parts = vessels[i].parts;
                    for (int j = 0; j < parts.Count; j++)
                    {
                        if (parts[j].CrewCapacity == 0) continue;
                        List<ProtoCrewMember> crew = parts[j].protoModuleCrew;
                        for (int k = 0; k < crew.Count; k++)
                        {
                            SandboxAutopilotSkill skill = crew[j].GetEffect<SandboxAutopilotSkill>() as SandboxAutopilotSkill;
                            if (skill != null)
                            {
                                if (use) skill.Register(parts[j]);
                                else skill.ForceUnregister(parts[j]);
                            }
                        }
                    }
                }
            }
        }

        private static void Log(string s, params object[] m)
        {
            BSSAddon.Log("Autopilot: "+s, m);
        }
    }

    internal static class BSSAutopilotExtensions
    {
        internal static bool EnableFullSASInSandbox(this GameParameters p)
        {
            // This is our own implementation of AdvancedParams EnableFullSASInSandbox for KSP < 1.6
            // For KSP 1.6+ this gets patched out and the stock setting will be used instead
            return p.CustomParams<BSSAutopilotSettings>().enableFullSASInSandbox;
        }

        internal static bool RequirePilotForSAS(this GameParameters p)
        {
            return p.CustomParams<BSSAutopilotSettings>().requirePilotForSAS;
        }
    }

    [HarmonyPatch(typeof(BSSAutopilotExtensions))]
    [HarmonyPatch("EnableFullSASInSandbox")]
    internal class BSSAutopilotExtensions_EnableFullSASInSandbox
    {
        [HarmonyPrefix]
        private static bool Prefix(GameParameters p, ref bool __result)
        {
            __result = p.CustomParams<GameParameters.AdvancedParams>().EnableFullSASInSandbox;
            return false;
        }
    }
}
