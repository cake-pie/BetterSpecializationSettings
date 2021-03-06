using UnityEngine;
using Harmony;

namespace BetterSpecializationSettings
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class BSSAddon : MonoBehaviour
    {
        public static BSSAddon Instance = null;
        internal static HarmonyInstance harmony = HarmonyInstance.Create("com.github.cake-pie.BetterSpecializationSettings");

        // individual features
        private Autopilot.BSSAutopilot autopilot;
        private Repair.BSSRepair repair;

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

            autopilot = new Autopilot.BSSAutopilot();
            repair = new Repair.BSSRepair();
        }

        private void OnDestroy()
        {
            autopilot.OnDestroy();
        }
        #endregion

        internal static bool kspAtLeast(int major, int minor = 0, int revision = 0)
        {
            int comp = Versioning.version_major.CompareTo(major);
            if (comp > 0) return true;
            if (comp < 0) return false;
            comp = Versioning.version_minor.CompareTo(minor);
            if (comp > 0) return true;
            if (comp < 0) return false;
            comp = Versioning.Revision.CompareTo(revision);
            if (comp >= 0) return true;
            return false;
        }

        internal static void Log(string s, params object[] m)
        {
            Debug.LogFormat($"[BetterSpecializationSettings] {s}", m);
        }
    }

    internal static class BSSExtensions
    {
        internal static bool EnableKerbalExperience(this GameParameters p)
        {
            return p.CustomParams<GameParameters.AdvancedParams>().EnableKerbalExperience;
        }
    }
}
