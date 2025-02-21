using ChaCustom;
using HarmonyLib;
using KK_Plugins;
using PseudoMaker.UI;
using System;

namespace PseudoMaker
{
    internal class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
        [HarmonyWrapSafe]
        private static void ChangeCoordinateTypePostfix(ChaControl __instance)
        {
            PseudoMaker.MainWindow?.RefreshValues();
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        //[HarmonyWrapSafe]
        //private static void ChangeCustomClothesPostFix(ChaControl __instance)
        //{
        //    PseudoMaker.Logger.LogInfo("ChangeCustomClothesPostFix");
        //    PseudoMaker.MainWindow?.RefreshValues();
        //}

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Getter)]
        [HarmonyPatch(typeof(KKAPI.Maker.MakerAPI), nameof(KKAPI.Maker.MakerAPI.InsideAndLoaded))]
        private static bool MakerInsideAndLoadedPrefix(ref bool __result)
        {
            var stackTrace = new System.Diagnostics.StackTrace().ToString();
            if (stackTrace.Contains("HairAccessoryController"))
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Getter)]
        [HarmonyPatch(typeof(KKAPI.Maker.AccessoriesApi), nameof(KKAPI.Maker.AccessoriesApi.SelectedMakerAccSlot))]
        private static bool MakerSelectedMakerAccSlotPrefix(ref int __result)
        {
            var stackTrace = new System.Diagnostics.StackTrace().ToString();
            if (stackTrace.Contains("HairAccessoryController"))
            {
                __result = AccessoryEditorPanel.currentAccessoryNr;
                return false;
            }
            return true;
        }

        // HairAccessoryCustomizer tries to enable stuff in the UI here. One that of course doesn't exist.
        // This prevents that code from running
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HairAccessoryCustomizer), nameof(HairAccessoryCustomizer.InitCurrentSlot), new Type[0])]
        [HarmonyPatch(typeof(HairAccessoryCustomizer), nameof(HairAccessoryCustomizer.InitCurrentSlot), new Type[] { typeof(HairAccessoryCustomizer.HairAccessoryController), typeof(bool) })]
        [HarmonyPatch(typeof(HairAccessoryCustomizer), nameof(HairAccessoryCustomizer.InitCurrentSlot), new Type[] { typeof(HairAccessoryCustomizer.HairAccessoryController) })]
        private static bool MakerGetCharacterControlPrefix()
        {
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(KKAPI.Maker.MakerAPI), nameof(KKAPI.Maker.MakerAPI.GetCharacterControl))]
        private static bool MakerGetCharacterControlPrefix(ref ChaControl __result)
        {
            __result = PseudoMaker.selectedCharacter;
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Getter)]
        [HarmonyPatch(typeof(CvsAccessory), "nSlotNo")]
        private static bool CvsAccessoryNSlotNoPrefix(ref int __result)
        {
            __result = Compatibility.SelectedSlotNr;
            return false;
        }
    }
}
