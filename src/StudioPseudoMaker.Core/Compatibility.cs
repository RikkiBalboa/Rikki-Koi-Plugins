﻿using BepInEx;
using BepInEx.Bootstrap;
using ChaCustom;
using KK_Plugins.MaterialEditor;
using KKAPI.Utilities;
using KoiClothesOverlayX;
using KoiSkinOverlayX;
using PseudoMaker.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KK_Plugins;
using KKAPI.Studio;
using UnityEngine;
using UnityEngine.UI;
using ButtPhysicsEditor;
using Shared;
using a12 = AAAAAAAAAAAA.AAAAAAAAAAAA;
using KK_PregnancyPlus;
using KK_ClothingBlendShape;
using System.Collections;

namespace PseudoMaker
{
    public static class Compatibility
    {
        private static FileSystemWatcher textureChangeWatcher;

        public static CvsAccessory CvsAccessory;
        public static int SelectedSlotNr;

        public static bool HasA12 { get; private set; }
        public static bool HasClothesOverlayPlugin { get; private set; }
        public static bool HasSkinOverlayPlugin { get; private set; }
        public static bool HasStudioSkinColor {  get; private set; }
        public static Version OverlayPluginVersion { get; private set; }
        public static bool HasButtPhysicsEditorPlugin { get; private set; }
        public static bool HasPregnancyPlus {  get; private set; }
        public static bool HasC2A { get; private set; }
        public static bool HasChaAlphaMask { get; private set; }
        public static bool HasClothingBlendshape { get; private set; }

        static Compatibility()
        {
            var plugins = Chainloader.PluginInfos.Values.Select(x => x.Instance)
                .Where(plugin => plugin != null)
                .Union(UnityEngine.Object.FindObjectsOfType(typeof(BaseUnityPlugin)).Cast<BaseUnityPlugin>())
                .ToArray();
            foreach (var plugin in plugins)
                switch (plugin?.Info?.Metadata?.GUID)
                {
                    case "starstorm.aaaaaaaaaaaa": HasA12 = true; break;
                    case "ClothesToAccessories": HasC2A = true; break;
                    case "com.rikkibalboa.bepinex.studioSkinColorControl": HasStudioSkinColor = true; break;
                    case "KCOX": 
                        HasClothesOverlayPlugin = true;
                        OverlayPluginVersion = plugin.Info.Metadata.Version;
                        break;
                    case "KSOX":
                        HasSkinOverlayPlugin = true;
                        OverlayPluginVersion = plugin.Info.Metadata.Version;
                        break;
                    case "com.rikkibalboa.bepinex.buttEditor": HasButtPhysicsEditorPlugin = true; break;
                    case "KK_PregnancyPlus": HasPregnancyPlus = true; break;
                    case "nakay.kk.ChaAlphaMask": HasChaAlphaMask = true; break;
                    case "nakay.kk.ClothingBlendShape": HasClothingBlendshape = true; break;
                }
        }

        public static class A12
        {
            public static Dictionary<int, Dictionary<int, string>> GetCustomAccParents()
            {
                if (!HasA12) return null;

                return GetValue();
                Dictionary<int, Dictionary<int, string>> GetValue()
                {
                    return PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().customAccParents;
                }
            }

            public static void ChangeAccessoryParent(int slotNr)
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

            public static string GetBoneName(int slotNr)
            {
                if (!HasA12) return "";

                return GetBoneTransform(slotNr)?.name ?? "Unknown";
            }

            public static Transform GetBoneTransform(int slotNr)
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

            public static void RegisterParent(int slotNr)
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

            public static void TransferAccessoryBefore(int toSlotnNr)
            {
                if (!HasA12) return;

                TransferAccessory();
                void TransferAccessory()
                {
                    var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();

                    if (
                        controller.customAccParents.TryGetValue(PseudoMaker.selectedCharacter.fileStatus.coordinateType, out var dicCoord)
                        && a12.TryGetStudioAccBone(controller, toSlotnNr, out var accBone)
                    )
                    {
                        var toDelete = new List<int>();
                        foreach (var hash in dicCoord)
                            if (controller.dicHashBones.TryGetValue(hash.Value, out var bone) && bone.IsChildOf(accBone))
                                toDelete.Add(hash.Key);
                        foreach (var key in toDelete)
                            dicCoord.Remove(key);
                    }
                }
            }

            public static void TransferAccessoryAfter()
            {
                if (!HasA12) return;

                TransferAccessory();
                void TransferAccessory()
                {
                    var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();

                    controller.LoadData();
                }
            }

            public static void CopyAccessoryAfter(int fromSelected, int toSelected, List<int> slots)
            {
                throw new NotImplementedException();
            }
        }

        public static class ClothesOverlays {
            public static bool HasColorMaskSupport()
            {
                return OverlayPluginVersion >= new Version("6.2");
            }
            public static bool HasResizeSupport()
            {
                return OverlayPluginVersion >= new Version("6.3");
            }
            public static bool HasPatternAndOverrideSupport()
            {
                return OverlayPluginVersion >= new Version("7.0");
            }

            public static void DumpOriginalTexture(string clothesId)
            {
                if (!HasClothesOverlayPlugin) return;

                DumpTexture();
                void DumpTexture()
                {
                    var controller = GetController();
                    controller.DumpBaseTexture(clothesId, b => KoiSkinOverlayGui.WriteAndOpenPng(b, clothesId + "_Original"));
                }
            }

            private static KoiClothesOverlayController GetController()
            {
                return PseudoMaker.selectedCharacter.gameObject.GetComponent<KoiClothesOverlayController>();
            }

            public static string GetClothesId(bool main, int kind)
            {
                if (main)
                    switch (kind)
                    {
                        case 0: return "ct_clothesTop";
                        case 1: return "ct_clothesBot";
#if KK || KKS || EC
                        case 2: return "ct_bra";
                        case 3: return "ct_shorts";
#else
                    case 2: return "ct_inner_t";
                    case 3: return "ct_inner_b";
#endif
                        case 4: return "ct_gloves";
                        case 5: return "ct_panst";
                        case 6: return "ct_socks";
#if KK || KKS
                        case 7: return "ct_shoes_inner";
                        case 8: return "ct_shoes_outer";
#else
                    case 7: return "ct_shoes";
#endif
                    }
                else
                    switch (kind)
                    {
                        case 0: return "ct_top_parts_A";
                        case 1: return "ct_top_parts_B";
                        case 2: return "ct_top_parts_C";
                    }
                return null;
            }

            public static string GetClothesId(SubCategory subCategory, bool isMultiPart = false)
            {
                return GetClothesId(
                    !isMultiPart,
                    PseudoMakerCharaController.SubCategoryToKind(subCategory)
                );
            }
            public static string GetClothesId(int kind, bool isMultiPart = false)
            {
                return GetClothesId(!isMultiPart, kind);
            }

            public static int GetSizeOverride(string clothesId)
            {
                if (!HasClothesOverlayPlugin && HasResizeSupport()) return 0;

                return GetSize();
                int GetSize()
                {
                    return GetController()?.GetTextureSizeOverride(clothesId) ?? 0;
                }
            }

            public static void SetSizeOverride(string clothesId, int newSize)
            {
                if (!HasClothesOverlayPlugin && HasResizeSupport()) return;

                GetSize();
                void GetSize()
                {
                    var controller = GetController();
                    var currentSize = controller.GetTextureSizeOverride(clothesId);
                    newSize = controller.SetTextureSizeOverride(clothesId, newSize);
                    if (newSize != currentSize)
                        controller.RefreshTexture(KoiClothesOverlayController.MakeColormaskId(clothesId));
                }
            }

            public static void ImportClothesOverlay(string clothesId, Action onDone = null)
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

                    textureChangeWatcher?.Dispose();
                    var directory = Path.GetDirectoryName(texPath);
                    if (directory != null)
                    {
                        textureChangeWatcher = new FileSystemWatcher(directory, Path.GetFileName(texPath));
                        textureChangeWatcher.Changed += (sender, args) =>
                        {
                            if (File.Exists(texPath))
                                ThreadingHelper.Instance.StartSyncInvoke(() => ReadTex(texPath));
                        };
                        textureChangeWatcher.Deleted += (sender, args) => textureChangeWatcher?.Dispose();
                        textureChangeWatcher.Error += (sender, args) => textureChangeWatcher?.Dispose();
                        textureChangeWatcher.EnableRaisingEvents = true;
                    }
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
                            SetTexAndUpdate(tex, clothesId);
                        onDone?.Invoke();

                    }
                    catch (Exception) { }
                }
            }

            public static void SetTexAndUpdate(Texture2D tex, string texType, Action onDone = null)
            {
                if (!HasClothesOverlayPlugin) return;

                SetTex();
                void SetTex()
                {
                    var ctrl = GetController();
                    var t = ctrl.GetOverlayTex(texType, true);
                    t.Texture = tex;
                    ctrl.RefreshTexture(texType);
                    onDone?.Invoke();
                    if (tex == null) textureChangeWatcher?.Dispose();
                }
            }

            public static void ExportOverlay(string clothesId)
            {
                if (!HasClothesOverlayPlugin) return;

                Export();
                void Export()
                {
                    try
                    {
                        var tex = GetController().GetOverlayTex(clothesId, false)?.TextureBytes;
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

            public static void SetTextureOverride(string clothesId, bool value)
            {
                var controller = GetController();
                var texData = controller?.GetOverlayTex(clothesId, true);
                if (texData != null)
                {
                    texData.Override = value;
                    controller.RefreshTexture(clothesId);
                }
            }

            public static ClothesTexData GetOverlayTex(string clothesId)
            {
                if (!HasClothesOverlayPlugin) return null;

                return GetOverlay();
                ClothesTexData GetOverlay()
                {
                    return GetController()?.GetOverlayTex(clothesId, false);
                }
            }

            public static int GetPatternId()
            {
                if(!HasClothesOverlayPlugin || !HasPatternAndOverrideSupport()) return -57475893;

                return GetId();
                int GetId()
                {
                    return KoiClothesOverlayController.CustomPatternID;
                }
            }

            public static Sprite GetPatternThumbnail()
            {
                if (!HasClothesOverlayPlugin || !HasPatternAndOverrideSupport()) return null;

                return GetThumbnail();
                Sprite GetThumbnail()
                {
                    return KoiClothesOverlayController.GetPatternThumbnail();
                }
            }

            public static void CLothingCopiedEvent(int sourceOutfit, int targetOutfit, List<int> slots)
            {
                var controller = PseudoMaker.selectedCharacter.GetComponent<KoiClothesOverlayController>();
                if (!controller) return;

                var copySlots = KoiClothesOverlayMgr.MainClothesNames.Where((x, i) => slots.Contains(i)).ToList();

                if (slots.Contains(0))
                {
                    copySlots.AddRange(Enum.GetNames(typeof(MaskKind)));
                    copySlots.AddRange(KoiClothesOverlayMgr.SubClothesNames);
                }
                
                Dictionary<string, ClothesTexData> sourceDic = controller.GetOverlayTextures((ChaFileDefine.CoordinateType)sourceOutfit);
                Dictionary<string, ClothesTexData> destinationDic = controller.GetOverlayTextures((ChaFileDefine.CoordinateType)targetOutfit);

                foreach (string copySlot in copySlots)
                {
                    destinationDic.Remove(copySlot);
                    if (sourceDic.TryGetValue(copySlot, out ClothesTexData val))
                        destinationDic[copySlot] = val;
                }

                if (targetOutfit == (int)controller.CurrentCoordinate.Value)
                    controller.RefreshAllTextures();
            }
        }

        public static class SkinOverlays
        {
            private static KoiSkinOverlayController GetController()
            {
                return PseudoMaker.selectedCharacter.gameObject.GetComponent<KoiSkinOverlayController>();
            }

            public static Texture2D GetTex(TexType texType)
            {
                if (!HasSkinOverlayPlugin) return null;

                return GetTex();
                Texture2D GetTex()
                {
                    var ctrl = GetController();
                    var overlayTexture = ctrl.OverlayStorage.GetTexture(texType);
                    return overlayTexture;
                }
            }

            public static void SetTexAndUpdate(byte[] tex, TexType texType)
            {
                if (!HasSkinOverlayPlugin) return;

                SetTex();
                void SetTex()
                {
                    var controller = GetController();
                    var overlay = controller.SetOverlayTex(tex, texType);
                    if (IsPerCoord()) controller.OverlayStorage.CopyToOtherCoords();
                    if (tex == null) textureChangeWatcher?.Dispose();
                }
            }

            public static void ExportOverlay(TexType texType)
            {
                if (!HasSkinOverlayPlugin) return;

                Export();
                void Export()
                {
                    try
                    {
                        var tex = GetTex(texType);
                        if (tex == null) return;
                        // Fix being unable to save some texture formats with EncodeToPNG
                        var texCopy = tex.ToTexture2D();
                        var bytes = texCopy.EncodeToPNG();

                        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
                        var filename = GetUniqueTexDumpFilename(texType.ToString());
                        File.WriteAllBytes(filename, bytes);
                        Util.OpenFileInExplorer(filename);


                        PseudoMaker.Destroy(texCopy);
                    }
                    catch (Exception ex)
                    {
                        PseudoMaker.Logger.LogMessage("Failed to export texture - " + ex.Message);
                    }
                }
            }

            private static string GetUniqueTexDumpFilename(string dumpType)
            {
                if (!HasSkinOverlayPlugin) return "Unknown";

                return GetName();
                string GetName()
                {
                    var path = KoiSkinOverlayMgr.OverlayDirectory;
                    Directory.CreateDirectory(path);
                    var file = Path.Combine(path, $"_Export_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{dumpType}.png");
                    // Normalize just in case for open in explorer call later
                    file = Path.GetFullPath(file);
                    return file;
                }
            }

            public static void ImportOverlay(TexType texType, Action onDone = null)
            {
                if (!HasSkinOverlayPlugin) return;

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

                    textureChangeWatcher?.Dispose();
                    var directory = Path.GetDirectoryName(texPath);
                    if (directory != null)
                    {
                        textureChangeWatcher = new FileSystemWatcher(directory, Path.GetFileName(texPath));
                        textureChangeWatcher.Changed += (sender, args) =>
                        {
                            if (File.Exists(texPath))
                                ThreadingHelper.Instance.StartSyncInvoke(() => ReadTex(texPath));
                        };
                        textureChangeWatcher.Deleted += (sender, args) => textureChangeWatcher?.Dispose();
                        textureChangeWatcher.Error += (sender, args) => textureChangeWatcher?.Dispose();
                        textureChangeWatcher.EnableRaisingEvents = true;
                    }
                }

                void ReadTex(string texturePath)
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(texturePath);
                        var tex = Util.TextureFromBytes(bytes, TextureFormat.ARGB32);
                        if (tex != null)
                            SetTexAndUpdate(tex.EncodeToPNG(), texType);
                        onDone?.Invoke();

                    }
                    catch (Exception) { }
                }
            }

            public static bool IsPerCoord()
            {
                if (!HasSkinOverlayPlugin) return false;

                return PerCoord();
                bool PerCoord()
                {
                    return GetController().OverlayStorage.IsPerCoord();
                }
            }
            public static void SetPerCoord(bool value)
            {
                if (!HasSkinOverlayPlugin) return;

                PerCoord();
                void PerCoord()
                {
                    if (!value) GetController().OverlayStorage.CopyToOtherCoords();
                }
            }
        }

        public static class MaterialEditor
        {
            public static void SetItemType(Func<List<Dropdown.OptionData>, int> findOption)
            {
                MEStudio.Instance.PopulateItemTypeDropdown(PseudoMaker.selectedCharacter);
                var option = findOption(MEStudio.ItemTypeDropDown.options);
                MEStudio.ItemTypeDropDown.value = option;
                MEStudio.Instance.ChangeItemType(option, PseudoMaker.selectedCharacter);
            }

            public static void SetItemType(string filter)
            {
                MEStudio.Instance.PopulateItemTypeDropdown(PseudoMaker.selectedCharacter);
                MEStudio.ItemTypeDropDown.value = 0;
                MEStudio.Instance.ChangeItemType(0, PseudoMaker.selectedCharacter);
                MEStudio.Instance.PopulateList(PseudoMaker.selectedCharacter.gameObject, new ObjectData(0, MaterialEditorCharaController.ObjectType.Character), filter);
            }

            public static string ClothesIndexToString(int kind)
            {
                return MEStudio.Instance.ClothesIndexToString(kind);
            }

            public static void RefreshClothesMaintex()
            {
                var MEController = MaterialEditorPlugin.GetCharaController(PseudoMaker.selectedCharacter);
                if (MEController != null)
                {
                    MEController.CustomClothesOverride = true;
                    MEController.RefreshClothesMainTex();
                }
            }

            public static void ClothingCopiedEvent(int sourceOutfit, int targetOutfit, List<int> slots)
            {
                MaterialEditorCharaController MEController = MaterialEditorPlugin.GetCharaController(PseudoMaker.selectedCharacter);
                MEController.ClothingCopiedEvent(sourceOutfit, targetOutfit, slots);
            }
        }

        public static class ClothingUnlock
        {
            public static bool GetClothingUnlockState()
            {
                ClothingUnlockerController unlockCtrl = PseudoMaker.selectedCharacter.GetComponent<ClothingUnlockerController>();
                return unlockCtrl && unlockCtrl.GetClothingUnlocked();
            }

            public static void ChangeClothingUnlockState(bool value)
            {
                ClothingUnlockerController unlockCtrl = PseudoMaker.selectedCharacter.GetComponent<ClothingUnlockerController>();
                if (!unlockCtrl) return;
                // change clothing unlock
                unlockCtrl.SetClothingUnlocked(value);
                PseudoMaker.selectedCharacter.ChangeClothes(true);
            }
        }

        public static class ButtPhysicsEditorPlugin
        {
            public static void SetButtValue(SliderType type, float value)
            {
                if (!HasButtPhysicsEditorPlugin) return;

                SetValue();
                void SetValue() {
                    PseudoMaker.selectedCharacter.GetComponent<ButtPhysicsEditorCharaController>()?.SetButtValue(type, value);
                }
            }
            public static void SetButtValue(FloatType type, float value)
            {
                if (!HasButtPhysicsEditorPlugin) return;

                SetValue();
                void SetValue()
                {
                    SliderType? _type = FloatTypeToSliderType(type);

                    if (_type != null)
                        PseudoMaker.selectedCharacter.GetComponent<ButtPhysicsEditorCharaController>()?.SetButtValue((SliderType)_type, value);
                }
            }

            public static float GetButtValue(FloatType type)
            {
                if (!HasButtPhysicsEditorPlugin) return 0f;

                return GetValue();
                float GetValue()
                {
                    var controller = PseudoMaker.selectedCharacter.GetComponent<ButtPhysicsEditorCharaController>();
                    var _type = FloatTypeToSliderType(type);

                    if (_type != null)
                        return controller.SavedValues.GetValueOrDefault((SliderType)_type, ButtPhysicsEditorCharaController.defaultValues[(SliderType)_type]);
                    return 0f;
                }
            }

            public static FloatType? SliderTypeTofloatType(SliderType type)
            {
                switch (type)
                {
                    case SliderType.Stiffness: return FloatType.ButtEditorStiffness;
                    case SliderType.Elasticity: return FloatType.ButtEditorElasticity;
                    case SliderType.Dampening: return FloatType.ButtEditorDampening;
                    case SliderType.Weight: return FloatType.ButtEditorWeight;
                    default: return null;
                }
            }

            public static SliderType? FloatTypeToSliderType(FloatType type)
            {
                switch (type)
                {
                    case FloatType.ButtEditorStiffness: return SliderType.Stiffness;
                    case FloatType.ButtEditorElasticity: return SliderType.Elasticity;
                    case FloatType.ButtEditorDampening: return SliderType.Dampening;
                    case FloatType.ButtEditorWeight: return SliderType.Weight;
                    default: return null;
                }
            }
        }

        public static class PregnancyPlus
        {
            private static PregnancyPlusCharaController GetController()
            {
                return PseudoMaker.selectedCharacter.gameObject.GetComponent<PregnancyPlusCharaController>();
            }

            public static void SetFloatValue(FloatType type, float value)
            {
                if (HasPregnancyPlus)
                    SetValue();

                void SetValue()
                {
                    var controller = GetController();

                    switch (type)
                    {
                        case FloatType.PregnancyPlusInflation:
                            if (controller.infConfig.inflationSize == value) return;
                            controller.infConfig.inflationSize = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusMultiplier:
                            if (controller.infConfig.inflationMultiplier == value) return;
                            controller.infConfig.inflationMultiplier = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusRoundness:
                            if (controller.infConfig.inflationRoundness == value) return;
                            controller.infConfig.inflationRoundness = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusMoveY:
                            if (controller.infConfig.inflationMoveY == value) return;
                            controller.infConfig.inflationMoveY = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusMoveZ:
                            if (controller.infConfig.inflationMoveZ == value) return;
                            controller.infConfig.inflationMoveZ = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusStretchX:
                            if (controller.infConfig.inflationStretchX == value) return;
                            controller.infConfig.inflationStretchX = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusStretchY:
                            if (controller.infConfig.inflationStretchY == value) return;
                            controller.infConfig.inflationStretchY = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusShiftY:
                            if (controller.infConfig.inflationShiftY == value) return;
                            controller.infConfig.inflationShiftY = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusShiftZ:
                            if (controller.infConfig.inflationShiftZ == value) return;
                            controller.infConfig.inflationShiftZ = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusTaperY:
                            if (controller.infConfig.inflationTaperY == value) return;
                            controller.infConfig.inflationTaperY = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusTaperZ:
                            if (controller.infConfig.inflationTaperZ == value) return;
                            controller.infConfig.inflationTaperZ = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusDrop:
                            if (controller.infConfig.inflationDrop == value) return;
                            controller.infConfig.inflationDrop = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusClothOffset:
                            if (controller.infConfig.inflationClothOffset == value) return;
                            controller.infConfig.inflationClothOffset = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusFatFold:
                            if (controller.infConfig.inflationFatFold == value) return;
                            controller.infConfig.inflationFatFold = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusFatFoldHeight:
                            if (controller.infConfig.inflationFatFoldHeight == value) return;
                            controller.infConfig.inflationFatFoldHeight = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                        case FloatType.PregnancyPlusFatFoldGap:
                            if (controller.infConfig.inflationFatFoldGap == value) return;
                            controller.infConfig.inflationFatFoldGap = value;
                            controller.MeshInflate(new MeshInflateFlags(controller), "StudioPseudoMaker");
                            break;
                    }
                    //PseudoMaker.RefreshCharacterstatusPanel();
                }
            }

            public static float GetFloatValue(FloatType type)
            {
                if (HasPregnancyPlus)
                    return GetValue();
                else return 0f;

                float GetValue()
                {
                    var controller = GetController();

                    switch (type)
                    {
                        case FloatType.PregnancyPlusInflation:
                            return controller.infConfig.inflationSize;
                        case FloatType.PregnancyPlusMultiplier:
                            return controller.infConfig.inflationMultiplier;
                        case FloatType.PregnancyPlusRoundness:
                            return controller.infConfig.inflationRoundness;
                        case FloatType.PregnancyPlusMoveY:
                            return controller.infConfig.inflationMoveY;
                        case FloatType.PregnancyPlusMoveZ:
                            return controller.infConfig.inflationMoveZ;
                        case FloatType.PregnancyPlusStretchX:
                            return controller.infConfig.inflationStretchX;
                        case FloatType.PregnancyPlusStretchY:
                            return controller.infConfig.inflationStretchY;
                        case FloatType.PregnancyPlusShiftY:
                            return controller.infConfig.inflationShiftY;
                        case FloatType.PregnancyPlusShiftZ:
                            return controller.infConfig.inflationShiftZ;
                        case FloatType.PregnancyPlusTaperY:
                            return controller.infConfig.inflationTaperY;
                        case FloatType.PregnancyPlusTaperZ:
                            return controller.infConfig.inflationTaperZ;
                        case FloatType.PregnancyPlusDrop:
                            return controller.infConfig.inflationDrop;
                        case FloatType.PregnancyPlusClothOffset:
                            return controller.infConfig.inflationClothOffset;
                        case FloatType.PregnancyPlusFatFold:
                            return controller.infConfig.inflationFatFold;
                        case FloatType.PregnancyPlusFatFoldHeight:
                            return controller.infConfig.inflationFatFoldHeight;
                        case FloatType.PregnancyPlusFatFoldGap:
                            return controller.infConfig.inflationFatFoldGap;
                        default: return 0f;
                    }
                }
            }

            public static float[] GetSliderRange(FloatType type)
            {
                if (HasPregnancyPlus)
                    return GetValue();
                else return new float[] { 0f, 0f };

                float[] GetValue()
                {
                    switch (type)
                    {
                        case FloatType.PregnancyPlusInflation:
                            return PregnancyPlusGui.SliderRange.InflationSize;
                        case FloatType.PregnancyPlusMultiplier:
                            return PregnancyPlusGui.SliderRange.InflationMultiplier;
                        case FloatType.PregnancyPlusRoundness:
                            return PregnancyPlusGui.SliderRange.InflationRoundness;
                        case FloatType.PregnancyPlusMoveY:
                            return PregnancyPlusGui.SliderRange.InflationMoveY;
                        case FloatType.PregnancyPlusMoveZ:
                            return PregnancyPlusGui.SliderRange.InflationMoveZ;
                        case FloatType.PregnancyPlusStretchX:
                            return PregnancyPlusGui.SliderRange.InflationStretchX;
                        case FloatType.PregnancyPlusStretchY:
                            return PregnancyPlusGui.SliderRange.InflationStretchY;
                        case FloatType.PregnancyPlusShiftY:
                            return PregnancyPlusGui.SliderRange.InflationShiftY;
                        case FloatType.PregnancyPlusShiftZ:
                            return PregnancyPlusGui.SliderRange.InflationShiftZ;
                        case FloatType.PregnancyPlusTaperY:
                            return PregnancyPlusGui.SliderRange.InflationTaperY;
                        case FloatType.PregnancyPlusTaperZ:
                            return PregnancyPlusGui.SliderRange.InflationTaperZ;
                        case FloatType.PregnancyPlusDrop:
                            return PregnancyPlusGui.SliderRange.InflationDrop;
                        case FloatType.PregnancyPlusClothOffset:
                            return PregnancyPlusGui.SliderRange.InflationClothOffset;
                        case FloatType.PregnancyPlusFatFold:
                            return PregnancyPlusGui.SliderRange.InflationFatFold;
                        case FloatType.PregnancyPlusFatFoldHeight:
                            return PregnancyPlusGui.SliderRange.InflationFatFoldHeight;
                        case FloatType.PregnancyPlusFatFoldGap:
                            return PregnancyPlusGui.SliderRange.InflationFatFoldGap;
                        default: return new float[] { 0f, 0f };
                    }
                }
            }

            public static void CopyBelly()
            {
                if (HasPregnancyPlus)
                    Copy();

                void Copy()
                {
                    PregnancyPlusPlugin.copiedBelly = (PregnancyPlusData)GetController().infConfig.Clone(); 
                }
            }

            public static void PasteBelly(PregnancyPlusData data)
            {
                if (HasPregnancyPlus)
                    Paste();

                void Paste()
                {
                    if (PregnancyPlusPlugin.copiedBelly != null && PregnancyPlusPlugin.copiedBelly.HasAnyValue())
                    {
                        GetController().infConfig.GameplayEnabled = true;

                        SetFloatValue(FloatType.PregnancyPlusInflation, data.inflationSize);
                        SetFloatValue(FloatType.PregnancyPlusMultiplier, data.inflationMultiplier);
                        SetFloatValue(FloatType.PregnancyPlusRoundness, data.inflationRoundness);
                        SetFloatValue(FloatType.PregnancyPlusMoveY, data.inflationMoveY);
                        SetFloatValue(FloatType.PregnancyPlusMoveZ, data.inflationMoveZ);
                        SetFloatValue(FloatType.PregnancyPlusStretchX, data.inflationStretchX);
                        SetFloatValue(FloatType.PregnancyPlusStretchY, data.inflationStretchY);
                        SetFloatValue(FloatType.PregnancyPlusShiftY, data.inflationShiftY);
                        SetFloatValue(FloatType.PregnancyPlusShiftZ, data.inflationShiftZ);
                        SetFloatValue(FloatType.PregnancyPlusTaperY, data.inflationTaperY);
                        SetFloatValue(FloatType.PregnancyPlusTaperZ, data.inflationTaperZ);
                        SetFloatValue(FloatType.PregnancyPlusDrop, data.inflationDrop);
                        SetFloatValue(FloatType.PregnancyPlusClothOffset, data.inflationClothOffset);
                        SetFloatValue(FloatType.PregnancyPlusFatFold, data.inflationFatFold);
                        SetFloatValue(FloatType.PregnancyPlusFatFoldHeight, data.inflationFatFoldHeight);
                        SetFloatValue(FloatType.PregnancyPlusFatFoldGap, data.inflationFatFoldGap);

                        PseudoMaker.RefreshCharacterstatusPanel();
                    }
                }
            }

            public static void ResetBelly()
            {
                if (HasPregnancyPlus)
                    Reset();

                void Reset()
                {
                    var controller = GetController();
                    controller.CleanSlate();
                    controller.isProcessing = false;
                    controller.infConfig.Reset();
                    PseudoMaker.RefreshCharacterstatusPanel();
                }
            }

            public static void OpenBlendshapes()
            {
                if (HasPregnancyPlus)
                    Open();

                void Open()
                {
                    GetController().OnOpenBlendShapeSelected();
                }
            }

#if KKS
            public static void OpenOffsets()
            {
                if (HasPregnancyPlus)
                    Open();

                void Open()
                {
                    GetController().OnOpenClothOffsetSelected();
                }
            }
#endif

            public static void SmoothBelly()
            {
                if (HasPregnancyPlus)
                    Smooth();

                void Smooth()
                {
                    GetController().ApplySmoothing(PregnancyPlusGui.includeClothSmoothing);
                }
            }
        }

        public static class MoreOutfits
        {

            public static void AddOufitSlot()
            {
                KK_Plugins.MoreOutfits.Plugin.AddCoordinateSlot(PseudoMaker.selectedCharacter);
                PseudoMaker.RefreshCharacterstatusPanel();
                PseudoMaker.Logger.LogMessage("Outfit added successfully!");
            }

            public static void RemoveOutfitSlot()
            {
                int outfitCount = PseudoMaker.selectedCharacter.GetOCIChar().charInfo.chaFile.coordinate.Length;
                Studio.Studio.instance?.manipulatePanelCtrl?.charaPanelInfo.mpCharCtrl?.stateInfo.OnClickCosType(outfitCount - 2);
                KK_Plugins.MoreOutfits.Plugin.RemoveCoordinateSlot(PseudoMaker.selectedCharacter);
                PseudoMaker.RefreshCharacterstatusPanel();
                PseudoMaker.Logger.LogMessage("Outfit removed successfully!");
            }

            public static string GetCurrentOutfitName()
            {
                return KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter,
                    PseudoMaker.selectedCharacter.fileStatus.coordinateType);
            }

            public static void SetCurrentOutfitName(string outfitName)
            {
                KK_Plugins.MoreOutfits.Plugin.SetCoordinateName(PseudoMaker.selectedCharacter, PseudoMaker.selectedCharacter.fileStatus.coordinateType, outfitName);
                PseudoMaker.RefreshCharacterstatusPanel();
                PseudoMaker.Logger.LogMessage("Outfit renamed successfully!");
            }

            public static List<string> GetAllOutfits()
            {
                var coordinateList = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).ToList();
                var moreOutfitsController = KK_Plugins.MoreOutfits.Plugin.GetController(PseudoMaker.selectedCharacter);
                for (int coord = coordinateList.Count; coord < PseudoMaker.selectedCharacter.chaFile.coordinate.Length; coord++)
                    coordinateList.Add(moreOutfitsController.GetCoodinateName(coord));
                return coordinateList;
            }
        }

        public static class ClothingBlendshape
        {
            public static ClothingBlendShapeController GetController(ChaControl chaControl = null)
            {
                if (!HasClothingBlendshape) return null;

                return GetController();
                ClothingBlendShapeController GetController()
                {
                    return (chaControl == null ? PseudoMaker.selectedCharacter : chaControl).GetComponent<ClothingBlendShapeController>();
                }
            }

            public static void InitComp(ChaControl chaControl, int kind)
            {
                if (!HasClothingBlendshape || PseudoMaker.instance == null) return;

                PseudoMaker.instance.StartCoroutine(Init());
                IEnumerator Init()
                {
                    yield return null;
                    yield return null;
                    yield return null;
                    var controller = GetController(chaControl);
                    if (controller != null
                        && controller.ClothesDataSet != null
                        && PseudoMaker.selectedCharacterController != null
                        && !controller.ClothesDataSet.Any(x =>
                            x.Coordinate == PseudoMaker.selectedCharacterController.CurrentOutfitSlot
                            && x.Category == PseudoMaker.selectedCharacterController.ChaControl.infoClothes[kind].Category
                        ))
                    {
                        if (controller.ShapesComp[kind] && controller.ShapesComp[kind].ItemNameExists)
                            controller.AddClothesData(chaControl.infoClothes[kind], kind);
                        else
                            controller.RemoveClothesData(chaControl.infoClothes[kind], kind);
                    }
                }
            }

            public static float GetValue(int kind, int blendshape)
            {
                if (!HasClothingBlendshape) return 0;

                return GetValue();
                float GetValue()
                {
                    BlendShapeData clothesData = GetController().GetClothesData(GetListInfo(kind, 0), kind);
                    return (clothesData != null ? clothesData.ShapeWeight[blendshape] : 20f) / 100;
                }
            }

            public static void SetValue(int kind, int blendshape, float value)
            {
                if (!HasClothingBlendshape) return;

                SetValue();
                void SetValue()
                {
                    var ctrl = GetController();
                    ctrl.SetClothesDataWeight(GetListInfo(kind, 0), kind, blendshape, value * 100);
                    ctrl.UpdateClothes(kind);
                }
            }

            public static string GetBlendshapeName(int kind, int blendshape)
            {
                if (!HasClothingBlendshape || blendshape >= 4) return null;

                return GetName();
                string GetName()
                {
                    var controller = GetController();
                    if (!controller.ShapesComp[kind]) return null;
                    var name = controller.ShapesComp[kind].DataSet[blendshape]?.name;
                    return name.IsNullOrEmpty() ? null : name;
                }
            }

            public static ListInfoBase GetListInfo(int clothesKind, int subKind)
            {
                if (!HasClothingBlendshape) return null;

                return GetInfo();
                ListInfoBase GetInfo()
                {
                    var controller = GetController();
                    if (!controller) return null;

                    if (clothesKind == 0 && controller.IsSubParts)
                        return controller.ChaControl.infoParts[subKind];
                    return controller.ChaControl.infoClothes[clothesKind];
                }
            }
        }
    }
}
