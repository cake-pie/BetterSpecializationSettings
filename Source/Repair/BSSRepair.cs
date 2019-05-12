using System.Reflection;
using Harmony;

namespace BetterSandboxSpecializations.Repair
{
    internal class BSSRepair
    {
        private const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Static;
        private readonly MethodInfo MWD_ERE_original = null;
        private readonly MethodInfo MWD_ERE_requireEngineer = null;
        private readonly MethodInfo MWD_ERE_overrideLvlReq = null;

        private enum UsePatch {
            None = 0,
            RequireEngineer = 1,
            OverrideLvlReq = 2
        };
        private UsePatch usePatch = UsePatch.None;

        public BSSRepair()
        {
            MWD_ERE_original = typeof(ModuleWheels.ModuleWheelDamage).GetMethod("EventRepairExternal");
            MWD_ERE_requireEngineer = typeof(ModuleWheelDamage_EventRepairExternal).GetMethod("PrefixRequireEngineer", bf);
            MWD_ERE_overrideLvlReq = typeof(ModuleWheelDamage_EventRepairExternal).GetMethod("PrefixOverrideLvlReq", bf);

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
                if (g.Parameters.OverrideLvlReqForWheelRep())
                {
                    int lvl = g.Parameters.CustomLvlReqForWheelRep();
                    if (usePatch == UsePatch.OverrideLvlReq && lvl == ModuleWheelDamage_EventRepairExternal.lvlReq)
                        return;
                    unpatch();
                    patchOverrideLvlReq(lvl);
                }
                else
                    unpatch();
            }
            else
            {
                if (g.Parameters.RequireEngineerForWheelRep())
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
            BSSAddon.harmony.Patch(MWD_ERE_original, new HarmonyMethod(MWD_ERE_requireEngineer));
        }

        private void unpatchRequireEngineer()
        {
            Log("require engineer OFF");
            usePatch = UsePatch.None;
            BSSAddon.harmony.Unpatch(MWD_ERE_original, MWD_ERE_requireEngineer);
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
            ModuleWheelDamage_EventRepairExternal.lvlReq = lvl;
            BSSAddon.harmony.Patch(MWD_ERE_original, new HarmonyMethod(MWD_ERE_overrideLvlReq));
        }

        private void unpatchOverrideLvlReq()
        {
            Log("override required level OFF");
            usePatch = UsePatch.None;
            ModuleWheelDamage_EventRepairExternal.lvlReq = BSSRepairSettings.StockLvlReq;
            BSSAddon.harmony.Unpatch(MWD_ERE_original, MWD_ERE_overrideLvlReq);
        }

        private static void Log(string s, params object[] m)
        {
            BSSAddon.Log("Repair: "+s, m);
        }
    }

    internal static class BSSRepairExtensions
    {
        internal static bool RequireEngineerForWheelRep(this GameParameters p)
        {
            return p.CustomParams<BSSRepairSettings>().requireEngineerForWheelRep;
        }

        internal static bool OverrideLvlReqForWheelRep(this GameParameters p)
        {
            return p.CustomParams<BSSRepairSettings>().overrideLvlReq;
        }

        internal static int CustomLvlReqForWheelRep(this GameParameters p)
        {
            return p.CustomParams<BSSRepairSettings>().customLvlReq;
        }
    }
}
