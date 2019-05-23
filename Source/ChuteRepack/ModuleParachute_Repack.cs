using UnityEngine;
using KSP.Localization;
using Harmony;

namespace BetterSpecializationSettings.ChuteRepack
{
    // class ModuleParachute
    // https://kerbalspaceprogram.com/api/class_module_parachute.html

    // public void Repack()
    [HarmonyPatch(typeof(ModuleParachute))]
    [HarmonyPatch("Repack")]
    internal class ModuleParachute_Repack
    {
        [HarmonyPrefix]
        private static bool PrefixRequireEngineer(ModuleParachute __instance)
        {
            if (FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < 0)
                ScreenMessages.PostScreenMessage(Localizer.Format("#BSS_LOC_scrmsg_reqengg"));
            else
                RepackChute(__instance);
            return false;
        }

        // cached custom level requirement setting
        internal static int lvlReq = BSSChuteRepackSettings.StockLvlReq;

        [HarmonyPrefix]
        private static bool PrefixOverrideLvlReq(ModuleParachute __instance)
        {
            if (FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < lvlReq)
                // #autoLOC_214609 and #autoLOC_246904 are identical
                // bet that the larger 246904 is for ModuleWheelDamage, use 214609 here
                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_214609", lvlReq.ToString()));
            else
                RepackChute(__instance);
            return false;
        }

        private static void RepackChute(ModuleParachute parachute)
        {
            // update state
            parachute.deploymentState = ModuleParachute.deploymentStates.STOWED;
            parachute.persistentState = "STOWED";

            // TODO need to figure this one out
            // protected attribute which is not exposed as a field
            // looks relevant from API, but unclear what it does
            // parachute.deactivateOnRepack

            // update drag
            // TODO needs further testing / verification
            parachute.part.maximum_drag = parachute.stowedDrag;
            parachute.part.DragCubes.SetCubeWeight("PACKED", 1f);
            parachute.part.DragCubes.SetCubeWeight("SEMIDEPLOYED", 0f);
            parachute.part.DragCubes.SetCubeWeight("DEPLOYED", 0f);

            // update model
            parachute.part.FindModelTransform(parachute.canopyName)?.gameObject.SetActive(false);   // parachute.canopy is protected
            parachute.part.FindModelTransform(parachute.capName)?.gameObject.SetActive(true);       // parachute.cap is protected

            // update UI
            parachute.part.stackIcon.SetIconColor(Color.white); // staging icon
            parachute.Events["CutParachute"].active = false;
            parachute.Events["Disarm"].active = false;
            parachute.Events["Repack"].active = false;
            parachute.Events["Deploy"].active = true;
        }
    }
}
