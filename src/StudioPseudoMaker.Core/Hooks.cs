using HarmonyLib;

namespace Plugins
{
    internal class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
        [HarmonyWrapSafe]
        private static void ChangeCoordinateTypePostfix(ChaControl __instance)
        {
            PseudoMaker.MainWindow?.RefreshValues();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        [HarmonyWrapSafe]
        private static void ChangeCustomClothesPostFix(ChaControl __instance)
        {
            PseudoMaker.MainWindow?.RefreshValues();
        }
    }
}
