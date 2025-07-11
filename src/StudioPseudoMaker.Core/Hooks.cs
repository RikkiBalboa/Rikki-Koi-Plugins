﻿using ChaCustom;
using HarmonyLib;
using KK_Plugins;
using PseudoMaker.UI;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace PseudoMaker
{
    internal class Hooks
    {
        internal static Harmony harmony;

        internal static void Init()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(Hooks));

            var type = Type.GetType("KK_ChaAlphaMask.Patch, KK_ChaAlphaMask", throwOnError: false);
            if (type != null)
                harmony.Patch(
                    type.GetMethod("ChangeCustomClothes_Post", AccessTools.all),
                    new HarmonyMethod(typeof(Hooks).GetMethod("EmptyPatch", AccessTools.all))
                );
        }

        [HarmonyPostfix]
        [HarmonyPatch("KK_ChaAlphaMask.Patch, KK_ChaAlphaMask", "ChangeCustomClothes_Post")]
        private static void EmptyPatch()
        {
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
        [HarmonyWrapSafe]
        private static void ChangeCoordinateTypePostfix(ChaControl __instance)
        {
            if (PseudoMaker.instance == null || PseudoMaker.MainWindow == null) return;
            PseudoMaker.instance.StartCoroutine(Refresh());
            IEnumerator Refresh()
            {
                yield return null;
                PseudoMaker.MainWindow?.RefreshValues();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), typeof(ChaFileDefine.CoordinateType), typeof(bool))]
        [HarmonyWrapSafe]
        private static void ChangeCoordinateTypePrefix(ChaControl __instance)
        {
            // Trim extra empty accessory slots
            // https://github.com/jalil49/MoreAccessories/blob/0cd772cd5b6e03f61cc3611f0cc532048c09ff46/src/Core.MoreAccessories/Patches/SavePatch.cs#L97
            var accessories = __instance.chaFile.coordinate[__instance.fileStatus.coordinateType].accessory;
            if (accessories.parts.Length == 20) return;

            var lastValidSlot = Array.FindLastIndex(accessories.parts, x => x.type != 120) + 1;
            if (lastValidSlot < 20) lastValidSlot = 20;
            if (lastValidSlot == accessories.parts.Length) return; //don't do below since nothing changed
            accessories.parts = accessories.parts.Take(lastValidSlot).ToArray();
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
        [HarmonyPatch(typeof(KKAPI.Maker.MakerAPI), nameof(KKAPI.Maker.MakerAPI.InsideMaker))]
        private static bool MakerInsideAndLoadedPrefix(ref bool __result)
        {
            var stackframe = new StackFrame(2).GetMethod();
            var methodName = stackframe.Name;
            var typeName = stackframe.DeclaringType.Name;
            if (
                (
                    typeName == "HairAccessoryController"
                    && (
                        methodName == "SetColorMatch"
                        || methodName == "SetHairGloss"
                        || methodName == "SetHairLength"
                        || methodName == "SetAccessoryColor"
                        || methodName == "SetGlossColor"
                        || methodName == "SetOutlineColor"
                    )
                ) ||
                (
                    typeName == "Patch" && methodName == "DMD<KK_ChaAlphaMask.Patch::ChangeCustomClothes_Post>"
                )
            )
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
                __result = AccessoryEditorPanel.currentAccessoryNr < 0 ? 0 : AccessoryEditorPanel.currentAccessoryNr;
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Studio.Studio), nameof(Studio.Studio.LoadSceneCoroutine))]
        private static bool LoadScenePrefix()
        {
            PseudoMakerSceneController.Instance.ClearProperties();
            return true;
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesTop))]
        private static void ChangeClothesTopPostFix(ChaControl __instance)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, 0);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesBot))]
        private static void ChangeClothesBotPostfix(ChaControl __instance)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, 1);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesBra))]
        private static void ChangeClothesBraPostfix(ChaControl __instance)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, 2);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesShorts))]
        private static void ChangeClothesShortsPostfix(ChaControl __instance)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, 3);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesGloves))]
        private static void ChangeClothesGlovesPostfix(ChaControl __instance)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, 4);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesPanst))]
        private static void ChangeClothesPanstPostfix(ChaControl __instance)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, 5);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesSocks))]
        private static void ChangeClothesSocksPostfix(ChaControl __instance)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, 6);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeClothesShoes))]
        private static void ChangeClothesShoesPostfix(ChaControl __instance, int id)
        {
            Compatibility.ClothingBlendshape.InitComp(__instance, id == 0 ? 7 : 8);
        }
    }
}
