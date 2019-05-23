using System.Reflection;
using Harmony;

namespace BetterSpecializationSettings.ChuteRepack
{
    internal class BSSChuteRepack
    {
        private const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Static;
        private readonly MethodInfo MP_R_original = null;
        private readonly MethodInfo MP_R_requireEngineer = null;
        private readonly MethodInfo MP_R_overrideLvlReq = null;

        private enum UsePatch {
            None = 0,
            RequireEngineer = 1,
            OverrideLvlReq = 2
        };
        private UsePatch usePatch = UsePatch.None;

        public BSSChuteRepack()
        {
            MP_R_original = typeof(ModuleParachute).GetMethod("Repack");
            MP_R_requireEngineer = typeof(ModuleParachute_Repack).GetMethod("PrefixRequireEngineer", bf);
            MP_R_overrideLvlReq = typeof(ModuleParachute_Repack).GetMethod("PrefixOverrideLvlReq", bf);

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
            // unpatch whenever returning to main menu
            if (gs == GameScenes.MAINMENU) unpatch();
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

            if (g.Parameters.EnableKerbalExperience())
            {
                if (g.Parameters.OverrideLvlReqForChuteRepack())
                {
                    int lvl = g.Parameters.CustomLvlReqForChuteRepack();
                    if (usePatch == UsePatch.OverrideLvlReq && lvl == ModuleParachute_Repack.lvlReq)
                        return;
                    unpatch();
                    patchOverrideLvlReq(lvl);
                }
                else
                    unpatch();
            }
            else
            {
                if (g.Parameters.RequireEngineerForChuteRepack())
                {
                    if (usePatch == UsePatch.RequireEngineer)
                        return;
                    unpatch();
                    patchRequireEngineer();
                }
                else
                    unpatch();
            }
        }

        private void unpatch()
        {
            switch (usePatch)
            {
                case UsePatch.RequireEngineer:
                    unpatchRequireEngineer();
                    break;
                case UsePatch.OverrideLvlReq:
                    unpatchOverrideLvlReq();
                    break;
            }
        }

        private void patchRequireEngineer()
        {
            Log("require engineer ON");
            usePatch = UsePatch.RequireEngineer;
            BSSAddon.harmony.Patch(MP_R_original, new HarmonyMethod(MP_R_requireEngineer));
        }

        private void unpatchRequireEngineer()
        {
            Log("require engineer OFF");
            usePatch = UsePatch.None;
            BSSAddon.harmony.Unpatch(MP_R_original, MP_R_requireEngineer);
        }

        private void patchOverrideLvlReq(int lvl)
        {
            if (lvl < 0 || lvl > 5)
            {
                Log("override required level ABORTED due to invalid level setting " + lvl);
                return;
            }
            Log("override required level ON, level >= " + lvl);
            usePatch = UsePatch.OverrideLvlReq;
            ModuleParachute_Repack.lvlReq = lvl;
            BSSAddon.harmony.Patch(MP_R_original, new HarmonyMethod(MP_R_overrideLvlReq));
        }

        private void unpatchOverrideLvlReq()
        {
            Log("override required level OFF");
            usePatch = UsePatch.None;
            ModuleParachute_Repack.lvlReq = BSSChuteRepackSettings.StockLvlReq;
            BSSAddon.harmony.Unpatch(MP_R_original, MP_R_overrideLvlReq);
        }

        private static void Log(string s, params object[] m)
        {
            BSSAddon.Log("ChuteRepack: "+s, m);
        }
    }

    internal static class BSSChuteRepackExtensions
    {
        internal static bool RequireEngineerForChuteRepack(this GameParameters p)
        {
            return p.CustomParams<BSSChuteRepackSettings>().requireEngineerForChuteRepack;
        }

        internal static bool OverrideLvlReqForChuteRepack(this GameParameters p)
        {
            return p.CustomParams<BSSChuteRepackSettings>().overrideLvlReq;
        }

        internal static int CustomLvlReqForChuteRepack(this GameParameters p)
        {
            return p.CustomParams<BSSChuteRepackSettings>().customLvlReq;
        }
    }
}
