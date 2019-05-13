using ModuleWheels;
using KSP.Localization;
using Harmony;

namespace BetterSpecializationSettings.Repair
{
    // class ModuleWheels.ModuleWheelDamage
    // https://kerbalspaceprogram.com/api/class_module_wheels_1_1_module_wheel_damage.html

    // public void EventRepairExternal()
    [HarmonyPatch(typeof(ModuleWheelDamage))]
    [HarmonyPatch("EventRepairExternal")]
    internal class ModuleWheelDamage_EventRepairExternal
    {
        [HarmonyPrefix]
        private static bool PrefixRequireEngineer(ModuleWheelDamage __instance)
        {
            if (FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < -1)
                ScreenMessages.PostScreenMessage(Localizer.Format("#BSS_LOC_scrmsg_reqengg"));
            else
                __instance.SetDamaged(false);
            return false;
        }

        // cached custom level requirement setting
        internal static int lvlReq = BSSRepairSettings.StockLvlReq;

        [HarmonyPrefix]
        private static bool PrefixOverrideLvlReq(ModuleWheelDamage __instance)
        {
            if (FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < lvlReq)
                // #autoLOC_214609 and #autoLOC_246904 are identical
                // bet that the smaller 214609 is for ModuleParachute, use 246904 here
                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_246904", lvlReq.ToString()));
            else
                __instance.SetDamaged(false);
            return false;
        }
    }
}
