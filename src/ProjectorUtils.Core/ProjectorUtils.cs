using BepInEx;
using BepInEx.Logging;
using Studio;
using HarmonyLib;
using KKAPI.Studio;
using UnityEngine;
using KKAPI.Studio.SaveLoad;

namespace Plugins
{
    [HarmonyPatch]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class ProjectorUtils : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.projectorutils";
        public const string PluginName = "ProjectorUtils";
        public const string PluginNameInternal = Constants.Prefix + "_ProjectorUtils";
        public const string PluginVersion = "1.0";
        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            Logger = base.Logger;
            _harmony.PatchAll();
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneController>(PluginGUID);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DrawLightLine), nameof(DrawLightLine.OnPostRender))]
        private static void OnPostRenderPostFix()
        {
            foreach (var objectCtrlInfo in StudioAPI.GetSelectedObjects())
                if (objectCtrlInfo is OCIItem item)
                {
                    var projector = item.objectItem.GetComponent<Projector>();
                    if (projector != null)
                        LightLine.DrawSpotLight(item.objectItem.transform.rotation, item.objectItem.transform.position, projector.fieldOfView, projector.farClipPlane, 1f, 1f);
                }
        }
    }
}
