using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Harmony;

namespace BetterSandboxSpecializations
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class BSSAddon : MonoBehaviour
    {
        public static BSSAddon Instance = null;
        public static HarmonyInstance harmony = HarmonyInstance.Create("com.github.cake-pie.BetterSandboxSpecializations");

        #region Lifecycle
        private void Awake()
        {
            if (Instance != null)
            {
                // Reloading of GameDatabase causes another copy of addon to spawn at next opportunity. Suppress it.
                // see: https://forum.kerbalspaceprogram.com/index.php?/topic/7542-x/&do=findComment&comment=3574980
                Log("Destroying spurious copy of BSSAddon!");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            //GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGameStatePostLoad.Add(OnGameStatePostLoad);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
        }

        private void OnDestroy()
        {
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
            GameEvents.onGameStateCreated.Remove(OnGameStateCreated);
            //GameEvents.onGameStateLoad.Remove(OnGameStateLoad);
            GameEvents.onGameStatePostLoad.Remove(OnGameStatePostLoad);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
        }
        #endregion

        #region GameEvents
        private void OnLevelWasLoaded(GameScenes gs)
        {
            // turn off sandbox autopilot whenever returning to main menu
            if (gs == GameScenes.MAINMENU)
                updateSandboxAutopilotSkill(false);
        }
        private void OnGameStateCreated(Game g)
        {
            Log("OnGameStateCreated");
            updateSandboxAutopilotSkill(calcUseSandboxAutopilot(g));
        }
        /*private void OnGameStateLoad(ConfigNode node)
        {
            Log("OnGameStateLoad");
            updateSandboxAutopilotSkill(calcUse(HighLogic.CurrentGame));
        }*/
        private void OnGameStatePostLoad(ConfigNode node)
        {
            Log("OnGameStatePostLoad");
            updateSandboxAutopilotSkill(calcUseSandboxAutopilot(HighLogic.CurrentGame));
        }
        private void OnGameSettingsApplied()
        {
            Log("OnGameSettingsApplied");
            updateSandboxAutopilotSkill(calcUseSandboxAutopilot(HighLogic.CurrentGame));
        }
        #endregion

        #region Autopilot
        private bool calcUseSandboxAutopilot(Game g)
        {
            return !(
                g.Parameters.CustomParams<GameParameters.AdvancedParams>().EnableKerbalExperience ||
                g.Parameters.CustomParams<GameParameters.AdvancedParams>().EnableFullSASInSandbox ||
                g.Parameters.CustomParams<BSSAutopilotSettings>().requirePilotForSAS );
        }

        private void updateSandboxAutopilotSkill(bool use)
        {
            Log("Sandbox autopilot skill {0}", (use ? "ON":"OFF"));
            SandboxAutopilotSkill.use = use;

            if (HighLogic.LoadedSceneIsFlight)
            {
                List<Vessel> vessels = FlightGlobals.VesselsLoaded;
                for (int i = 0; i < vessels.Count; i++)
                {
                    List<ProtoCrewMember> crew = vessels[i].GetVesselCrew();
                    for (int j = 0; j < crew.Count; j++)
                    {
                        Part part = crew[j].KerbalRef?.InPart;
                        SandboxAutopilotSkill skill = crew[j].GetEffect<SandboxAutopilotSkill>() as SandboxAutopilotSkill;
                        if (part != null && skill != null)
                        {
                            if (use) skill.Register(part);
                            else skill.ForceUnregister(part);
                        }
                    }
                }
            }

            /* Original code: iterating over all kerbals in roster
            if (HighLogic.LoadedSceneIsGame)
            {
                KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
                for (int i = 0; i < roster.Count; i++)
                {
                    Part part = roster[i].KerbalRef?.InPart;
                    if (part != null)
                    {
                        SandboxAutopilotSkill skill = roster[i].GetEffect<SandboxAutopilotSkill>() as SandboxAutopilotSkill;
                        if (skill != null)
                        {
                            if (use) skill.ForceRegister(part);
                            else skill.ForceUnregister(part);
                        }
                    }
                }
            }
            */
        }
        #endregion

        internal static void Log(string s, params object[] m)
        {
            Debug.LogFormat($"[BetterSandboxSpecializations] {s}", m);
        }
    }
}
