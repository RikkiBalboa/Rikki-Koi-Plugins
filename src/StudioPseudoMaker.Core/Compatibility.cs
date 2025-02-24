using BepInEx;
using BepInEx.Bootstrap;
using ChaCustom;
using KKAPI.Utilities;
using KoiClothesOverlayX;
using KoiSkinOverlayX;
using PseudoMaker.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PseudoMaker
{
    public static class Compatibility
    {
        public static CvsAccessory CvsAccessory;
        public static int SelectedSlotNr;

        public static bool HasA12 { get; private set; }
        public static bool HasClothesOverlayPlugin { get; private set; }
        public static Version OverlayPluginVersion { get; private set; }

        static Compatibility()
        {
#if KKS
            C2AType = Type.GetType("KK_Plugins.ClothesToAccessoriesPlugin, KKS_ClothesToAccessories", throwOnError: false);
#elif KK
            C2AType = Type.GetType("KK_Plugins.ClothesToAccessoriesPlugin, KK_ClothesToAccessories", throwOnError: false);
#endif

            var plugins = Chainloader.PluginInfos.Values.Select(x => x.Instance)
                .Where(plugin => plugin != null)
                .Union(UnityEngine.Object.FindObjectsOfType(typeof(BaseUnityPlugin)).Cast<BaseUnityPlugin>())
                .ToArray();
            foreach (var plugin in plugins)
                switch (plugin.Info.Metadata.GUID)
                {
                    case "starstorm.aaaaaaaaaaaa": HasA12 = true; break;
                    case "KCOX": 
                        HasClothesOverlayPlugin = true;
                        OverlayPluginVersion = plugin.Info.Metadata.Version;
                        break;
                }
        }

        #region A12
        public static Dictionary<int, Dictionary<int, string>> GetA12A12CustomAccParents()
        {
            if (!HasA12) return null;

            return GetValue();
            Dictionary<int, Dictionary<int, string>> GetValue()
            {
                return PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().customAccParents;
            }
        }

        public static void A12ChangeAccessoryParent(int slotNr)
        {
            if (!HasA12) return;

            RemoveData();
            void RemoveData()
            {
                var coord = PseudoMaker.selectedCharacter.fileStatus.coordinateType;
                var customAccParents = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().customAccParents;
                if (customAccParents.ContainsKey(coord))
                    customAccParents[coord].Remove(slotNr);
            }
        }

        public static string A12GetBoneName(int slotNr)
        {
            if (!HasA12) return "";

            return A12GetBoneTransform(slotNr)?.name ?? "Unknown";
        }

        public static Transform A12GetBoneTransform(int slotNr)
        {
            if (!HasA12) return null;

            return GetBone();
            Transform GetBone()
            {
                var coord = PseudoMaker.selectedCharacter.fileStatus.coordinateType;
                var customAccParents = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().customAccParents;
                var dicHashBones = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().dicHashBones;
                if (customAccParents.ContainsKey(coord))
                    return dicHashBones[customAccParents[coord][slotNr]].bone;
                return null;
            }
        }

        public static void A12RegisterParent(int slotNr)
        {
            if (!HasA12) return;

            RegisterBone();
            void RegisterBone()
            {
                Transform selected = KKABMX.GUI.KKABMX_AdvancedGUI._selectedTransform.Value;
                if (selected == null)
                {
                    PseudoMaker.Logger.LogMessage("[AAAAAAAAAAAA] Please select a bone in ABMX!");
                    return;
                }
                var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();
                var accTransform = PseudoMaker.selectedCharacter.objAccessory[slotNr]?.transform;

                if (accTransform != null && controller.dicTfBones.TryGetValue(accTransform, out var accBone) && controller.dicTfBones.TryGetValue(selected, out var parentBone))
                {
                    // Make sure not to parent anything to itself or its children
                    if (parentBone.IsChildOf(accBone))
                    {
                        PseudoMaker.Logger.LogMessage("[AAAAAAAAAAAA] Can't parent accessory to itself or its children!");
                        return;
                    }

                    accBone.SetParent(parentBone);
                    accBone.PerformBoneUpdate();

                    var coord = PseudoMaker.selectedCharacter.fileStatus.coordinateType;
                    // Save parentage to dictionary
                    if (!controller.customAccParents.ContainsKey(coord))
                    {
                        controller.customAccParents[coord] = new Dictionary<int, string>();
                    }
                    controller.customAccParents[coord][slotNr] = parentBone.Hash;
                }
            }
        }

        public static void A12TransferAccessoryBefore(int toSlotnNr)
        {
            if (!HasA12) return;

            TransferAccessory();
            void TransferAccessory()
            {
                var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();

                if (
                    controller.customAccParents.TryGetValue(PseudoMaker.selectedCharacter.fileStatus.coordinateType, out var dicCoord)
                    && AAAAAAAAAAAA.AAAAAAAAAAAA.TryGetStudioAccBone(controller, toSlotnNr, out var accBone)
                )
                {
                    var toDelete = new List<int>();
                    foreach (var hash in dicCoord)
                        if (controller.dicHashBones.TryGetValue(hash.Value, out var bone) && bone.IsChildOf(accBone))
                            toDelete.Add(hash.Key);
                    foreach(var key in toDelete)
                        dicCoord.Remove(key);
                }
            }
        }

        public static void A12TransferAccessoryAfter()
        {
            if (!HasA12) return;

            TransferAccessory();
            void TransferAccessory()
            {
                var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();

                controller.LoadData();
            }
        }
        #endregion

        #region C2AA
        public static Type C2AType;

        public static bool CheckC2AInstalled()
        {
            return C2AType != null;
        }
        #endregion
        public static bool OverlayVersionHasColorMaskSupport()
        {
            return OverlayPluginVersion >= new Version("6.2");
        }
        public static bool OverlayVersionHasResizeSupport()
        {
            return OverlayPluginVersion >= new Version("6.2");
        }

        public static void OverlayDumpOriginalTexture(string clothesId)
        {
            if (!HasClothesOverlayPlugin) return;

            DumpTexture();
            void DumpTexture()
            {
                var controller = GetOverlayClothesController();
                controller.DumpBaseTexture(clothesId, b => KoiSkinOverlayGui.WriteAndOpenPng(b, clothesId + "_Original"));
            }
        }

        public static KoiClothesOverlayController GetOverlayClothesController()
        {
            return PseudoMaker.selectedCharacter.gameObject.GetComponent<KoiClothesOverlayController>();
        }

        public static string OverlayGetClothesId(bool main, int kind)
        {
            if (!HasClothesOverlayPlugin) return "";

            return GetClothedId();
            string GetClothedId()
            {
                return KoiClothesOverlayController.GetClothesIdFromKind(main, kind);
            }
        }

        public static string OverlayGetClothesId(SubCategory subCategory)
        {
            return OverlayGetClothesId(
                !PseudoMaker.selectedCharacterController.IsMultiPartTop(PseudoMakerCharaController.SubCategoryToKind(subCategory)),
                PseudoMakerCharaController.SubCategoryToKind(subCategory)
            );
        }

        public static void OverlayImportClothesOverlay(string clothesId)
        {
            if (!HasClothesOverlayPlugin) return;

            OpenFile();
            void OpenFile()
            {
                OpenFileDialog.Show(
                    strings => OnFileAccept(strings),
                    "Open overlay image",
                    KoiSkinOverlayGui.GetDefaultLoadDir(),
                    KoiSkinOverlayGui.FileFilter,
                    KoiSkinOverlayGui.FileExt
                );
            }

            void OnFileAccept(string[] strings)
            {
                if (strings == null || strings.Length == 0) return;

                var texPath = strings[0];
                if (string.IsNullOrEmpty(texPath)) return;

                // Game crashes if the texture creation is not done on the main thread
                // No amount of try catching will save it from that crash
                ThreadingHelper.Instance.StartSyncInvoke(() => ReadTex(texPath));
            }

            void ReadTex(string texturePath)
            {
                try
                {
                    var bytes = File.ReadAllBytes(texturePath);
                    var isMask = KoiClothesOverlayController.IsMaskKind(clothesId);

                    // Always save to the card in lossless format
                    var textureFormat = isMask ? TextureFormat.RG16 : TextureFormat.ARGB32;
                    var tex = Util.TextureFromBytes(bytes, textureFormat);
                    if (tex != null)
                        OverlaySetTexAndUpdate(tex, clothesId);

                }
                catch (Exception ex) { }
            }
        }

        public static void OverlaySetTexAndUpdate(Texture2D tex, string texType)
        {
            if (!HasClothesOverlayPlugin) return;

            SetTex();
            void SetTex()
            {
                var ctrl = GetOverlayClothesController();
                var t = ctrl.GetOverlayTex(texType, true);
                t.Texture = tex;
                ctrl.RefreshTexture(texType);
            }
        }

        public static void OverlayExportOverlay(string clothesId)
        {
            if (!HasClothesOverlayPlugin) return;

            Export();
            void Export()
            {
                try
                {
                    var tex = GetOverlayClothesController().GetOverlayTex(clothesId, false)?.TextureBytes;
                    if (tex == null)
                    {
                        PseudoMaker.Logger.LogMessage("Nothing to export");
                        return;
                    }

                    KoiSkinOverlayGui.WriteAndOpenPng(tex, clothesId);
                }
                catch (Exception ex)
                {
                    PseudoMaker.Logger.LogMessage("Failed to export texture - " + ex.Message);
                }
            }
        }

        public static void OverlaySetTextureOverride(string clothesId, bool value)
        {
            var controller = GetOverlayClothesController();
            var texData = controller?.GetOverlayTex(clothesId, true);
            if (texData != null)
            {
                texData.Override = value;
                controller.RefreshTexture(clothesId);
            }
        }

        public static ClothesTexData OverlayGetOverlayTex(string clothesId)
        {
            if (!HasClothesOverlayPlugin) return null;

            return GetOverlay();
            ClothesTexData GetOverlay()
            {
                return GetOverlayClothesController()?.GetOverlayTex(clothesId, false);
            }
        }
        #region Overlays
        #endregion
    }
}
