using HarmonyLib;

namespace Plugins
{
    internal class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
        private static void ChangeCoordinateTypePostfix(ChaControl __instance)
        {
            var controller = StudioSkinColorCharaController.GetController(__instance);
            if (controller != null)
                controller.ChangeCoordinateEvent();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        private static void ChangeCustomClothesPostFix(ChaControl __instance)
        {
            var controller = StudioSkinColorCharaController.GetController(__instance);
            if (controller != null)
                controller.ChangeCoordinateEvent();
        }
    }
}
