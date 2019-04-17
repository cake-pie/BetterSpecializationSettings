using System.Collections.Generic;

namespace BetterSandboxSpecializations.Autopilot
{
    internal class BSSAutopilot
    {
        public BSSAutopilot()
        {
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

        private bool calcUseSandboxAutopilot(Game g)
        {
            return !(
                g.Parameters.EnableKerbalExperience() ||
                g.Parameters.EnableFullSASInSandbox() ||
                g.Parameters.RequirePilotForSAS() );
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
        internal static bool EnableKerbalExperience(this GameParameters p)
        {
            return p.CustomParams<GameParameters.AdvancedParams>().EnableKerbalExperience;
        }

        internal static bool EnableFullSASInSandbox(this GameParameters p)
        {
            return p.CustomParams<GameParameters.AdvancedParams>().EnableFullSASInSandbox;
        }

        internal static bool RequirePilotForSAS(this GameParameters p)
        {
            return p.CustomParams<BSSAutopilotSettings>().requirePilotForSAS;
        }
    }
}
