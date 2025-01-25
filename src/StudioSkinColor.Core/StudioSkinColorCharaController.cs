﻿using KKAPI;
using KKAPI.Chara;
using System.Collections.Generic;
using static Plugins.StudioSkinColor;
using UnityEngine;
using KK_Plugins.MaterialEditor;
using System.Linq;
using System.Collections;
using System;
using MessagePack;

namespace Plugins
{
    internal class StudioSkinColorCharaController : CharaCustomFunctionController
    {
        internal static readonly Dictionary<ChaControl, StudioSkinColorCharaController> allControllers = new Dictionary<ChaControl, StudioSkinColorCharaController>();

        #region Save Lists
        private readonly Dictionary<ClothingColors, ColorStorage> originalClothingColors = new Dictionary<ClothingColors, ColorStorage>();
        private readonly Dictionary<HairColor, ColorStorage> originalHairColors = new Dictionary<HairColor, ColorStorage>();
        private readonly Dictionary<TextureColor, ColorStorage> originalBodyColors = new Dictionary<TextureColor, ColorStorage>();
        private readonly Dictionary<Bust, FloatStorage> originalBustValues = new Dictionary<Bust, FloatStorage>();
        private readonly Dictionary<int, FloatStorage> originalBodyShapeValues = new Dictionary<int, FloatStorage>();
        private readonly Dictionary<int, FloatStorage> originalFaceShapeValues = new Dictionary<int, FloatStorage>();
        #endregion

        #region Character Properties shortcuts
        private int CurrentOutfitSlot => ChaControl.fileStatus.coordinateType;
        private ChaFileClothes Clothes => ChaControl.nowCoordinate.clothes;
        private ChaFileClothes SetClothes => ChaControl.chaFile.coordinate[ChaControl.chaFile.status.coordinateType].clothes;
        private ChaFileAccessory Accessories => ChaControl.nowCoordinate.accessory;
        private ChaFileAccessory SetAccessories => ChaControl.chaFile.coordinate[ChaControl.chaFile.status.coordinateType].accessory;
        #endregion

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (maintainState)
                return;

            allControllers[ChaControl] = this;
            originalClothingColors.Clear();
            originalHairColors.Clear();
            originalBodyColors.Clear();
            originalBustValues.Clear();
            originalBodyShapeValues.Clear();
            originalFaceShapeValues.Clear();
        }

        public static StudioSkinColorCharaController GetController(ChaControl chaCtrl)
        {
            if (allControllers.ContainsKey(chaCtrl))
                return allControllers[chaCtrl];
#if DEBUG
            return chaCtrl.gameObject.GetOrAddComponent<StudioSkinColorCharaController>();
#else
            return chaCtrl.gameObject.GetComponent<StudioSkinColorCharaController>();
#endif
        }

        #region body
        public void UpdateBodyAndFaceTextures()
        {
            ChaControl.AddUpdateCMFaceTexFlags(true, true, true, true, true, true, true);
            ChaControl.AddUpdateCMFaceColorFlags(true, true, true, true, true, true, true);
            ChaControl.CreateFaceTexture();
            ChaControl.SetFaceBaseMaterial();

            ChaControl.AddUpdateCMBodyTexFlags(true, true, true, true, true);
            ChaControl.AddUpdateCMBodyColorFlags(true, true, true, true, true, true);
            ChaControl.CreateBodyTexture();
            ChaControl.SetBodyBaseMaterial();
        }

        public void UpdateTextureColor(Color color, TextureColor textureColor)
        {
            if (!originalBodyColors.ContainsKey(textureColor))
                originalBodyColors[textureColor] = new ColorStorage(GetBodyColor(textureColor), color);
            else
                originalBodyColors[textureColor].Value = color;

            switch (textureColor)
            {
                case TextureColor.SkinMain:
                    ChaControl.fileBody.skinMainColor = color;
                    break;
                case TextureColor.SkinSub:
                    ChaControl.fileBody.skinSubColor = color;
                    break;
                case TextureColor.Tan:
                    ChaControl.fileBody.sunburnColor = color;
                    break;
            }
            UpdateBodyAndFaceTextures();
        }

        public void SetBustValue(float value, Bust bust)
        {
            if (!originalBustValues.ContainsKey(bust))
                originalBustValues[bust] = new FloatStorage(GetBustValue(bust), value);
            else
                originalBustValues[bust].Value = value;

            switch (bust)
            {
                case Bust.Softness:
                    ChaControl.ChangeBustSoftness(value);
                    break;
                case Bust.Weight:
                    ChaControl.ChangeBustGravity(value);
                    break;
            }
        }

        public void ResetBustValue(Bust bust)
        {
            if (originalBustValues.TryGetValue(bust, out var value))
                SetBustValue(value.OriginalValue, bust);
        }

        public float GetBustValue(Bust bust)
        {
            if (bust == Bust.Softness)
                return ChaControl.fileBody.bustSoftness;
            else if (bust == Bust.Weight)
                return ChaControl.fileBody.bustWeight;
            return 1f;
        }

        public void UpdateHairColor(Color color, HairColor hairColor)
        {
            if (!originalHairColors.ContainsKey(hairColor))
                originalHairColors[hairColor] = new ColorStorage(GetHairColor(hairColor), color);
            else
                originalHairColors[hairColor].Value = color;

            switch (hairColor)
            {
                case HairColor.Base:
                    for (int i = 0; i < 4; i++)
                        ChaControl.fileHair.parts[i].baseColor = color;
                    break;
                case HairColor.Start:
                    for (int i = 0; i < 4; i++)
                        ChaControl.fileHair.parts[i].startColor = color;
                    break;
                case HairColor.End:
                    for (int i = 0; i < 4; i++)
                        ChaControl.fileHair.parts[i].endColor = color;
                    break;
#if KKS
                case HairColor.Gloss:
                    for (int i = 0; i < 4; i++)
                    {
                        ChaControl.fileHair.parts[i].glossColor = color;
                        ChaControl.ChangeSettingHairGlossColor(i);
                    }
                    break;
#endif
                case HairColor.Eyebrow:
                    ChaControl.fileFace.eyebrowColor = color;
                    ChaControl.ChangeSettingEyebrowColor();
                    break;
            }
            for (int i = 0; i < 4; i++)
                ChaControl.ChangeSettingHairColor(i, true, true, true);
        }

        public void ResetHairColor(HairColor hairColor)
        {
            if (originalHairColors.TryGetValue(hairColor, out var color))
                UpdateHairColor(color.OriginalValue, hairColor);
        }

        public Color GetHairColor(HairColor color)
        {
            switch (color)
            {
                case HairColor.Base:
                    return ChaControl.fileHair.parts[0].baseColor;
                case HairColor.Start:
                    return ChaControl.fileHair.parts[0].startColor;
                case HairColor.End:
                    return ChaControl.fileHair.parts[0].endColor;
#if KKS
                case HairColor.Gloss:
                    return ChaControl.fileHair.parts[0].glossColor;
#endif
                case HairColor.Eyebrow:
                    return ChaControl.fileFace.eyebrowColor;
            }
            return Color.white;
        }

        public Color GetBodyColor(TextureColor color)
        {
            switch (color)
            {
                case TextureColor.SkinMain:
                    return ChaControl.fileBody.skinMainColor;
                case TextureColor.SkinSub:
                    return ChaControl.fileBody.skinSubColor;
                case TextureColor.Tan:
                    return ChaControl.fileBody.sunburnColor;
            }
            return Color.white;
        }

        public void ResetBodyColor(TextureColor colorType)
        {
            if (originalBodyColors.TryGetValue(colorType, out var color))
                UpdateTextureColor(color.OriginalValue, colorType);
        }

        public void UpdateBodyShapeValue(int index, float value)
        {
            if (!originalBodyShapeValues.ContainsKey(index))
                originalBodyShapeValues[index] = new FloatStorage(GetCurrentBodyValue(index), value);
            else
                originalBodyShapeValues[index].Value = value;
            ChaControl.SetShapeBodyValue(index, value);
        }

        public float GetCurrentBodyValue(int index)
        {
            return ChaControl.GetShapeBodyValue(index);
        }

        public void ResetBodyShapeValue(int index)
        {
            if (originalBodyShapeValues.TryGetValue(index, out var shapeValueBody))
                UpdateBodyShapeValue(index, shapeValueBody.OriginalValue);
        }

        public void UpdateFaceShapeValue(int index, float value)
        {
            if (!originalFaceShapeValues.ContainsKey(index))
                originalFaceShapeValues[index] = new FloatStorage(GetCurrentFaceValue(index), value);
            else
                originalFaceShapeValues[index].Value = value;
            ChaControl.SetShapeFaceValue(index, value);
        }

        public float GetCurrentFaceValue(int index)
        {
            return ChaControl.GetShapeFaceValue(index);
        }

        public void ResetFaceShapeValue(int index)
        {
            if (originalFaceShapeValues.TryGetValue(index, out var shapeValue))
                UpdateFaceShapeValue(index, shapeValue.OriginalValue);
        }
        #endregion

        #region Clothes
        public bool ClothingKindExists(int kind)
        {
            return selectedCharacterClothing[ChaControl].Any(c => c.Kind == kind);
        }

        public string GetclothingName(int kind, int slotNr = -1)
        {
            if (slotNr < 0)
                return ChaControl.infoClothes[kind].Name;
            return ChaControl.infoAccessory[slotNr].Name;
        }

        public void InitBaseCustomTextureClothesIfNotExists(int kind)
        {
            if (selectedCharacter.ctCreateClothes[kind, 0] == null)
                selectedCharacter.InitBaseCustomTextureClothes(true, kind);
        }

        public void SetClothingColor(int kind, int colorNr, Color color, int slotNr = -1)
        {
            var MEController = MaterialEditorPlugin.GetCharaController(ChaControl);
            if (MEController != null)
            {
                MEController.CustomClothesOverride = true;
                MEController.RefreshClothesMainTex();
            }

            var clothingColors = new ClothingColors(CurrentOutfitSlot, kind, colorNr, slotNr);
            if (!originalClothingColors.Any(x => x.Key.Compare(CurrentOutfitSlot, kind, colorNr, slotNr)))
                originalClothingColors[clothingColors] = new ColorStorage(GetClothingColor(kind, colorNr, slotNr), color);
            else
                originalClothingColors[clothingColors].Value = color;


            if (slotNr < 0)
            {
                Clothes.parts[kind].colorInfo[colorNr].baseColor = color;
                SetClothes.parts[kind].colorInfo[colorNr].baseColor = color;
                if (!IsMultiPartTop(kind))
                    ChaControl.ChangeCustomClothes(true, kind, true, true, true, true, true);
                else
                    for (int i = 0; i < Clothes.subPartsId.Length; i++)
                    {
                        ChaControl.ChangeCustomClothes(main: false, i, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: false);
                    }
            }
            else
            {
                Accessories.parts[slotNr].color[colorNr] = color;
                SetAccessories.parts[slotNr].color[colorNr] = color;
                ChaControl.ChangeAccessoryColor(slotNr);
            }
        }

        public bool[] CheckClothingUseColor(int kind, int slotNr = -1)
        {
            bool[] useCols = new bool[4] { false, false, false, false };

            if (slotNr < 0 && !IsMultiPartTop(kind))
            {
                var clothesComponent = ChaControl.GetCustomClothesComponent(kind);
                if (clothesComponent != null)
                {
                    useCols[0] = clothesComponent.useColorN01;
                    useCols[1] = clothesComponent.useColorN02;
                    useCols[2] = clothesComponent.useColorN03;
                }
            }
            else if (slotNr >= 0)
            {
                var accessoryComponent = ChaControl.GetAccessoryComponent(slotNr);
                if (accessoryComponent != null)
                {
                    useCols[0] = accessoryComponent.useColor01;
                    useCols[1] = accessoryComponent.useColor02;
                    useCols[2] = accessoryComponent.useColor03;
                }
            }
            else
            {
                foreach (var clothesComponent in ChaControl.cusClothesSubCmp)
                {
                    if (clothesComponent != null)
                    {
                        useCols[0] |= clothesComponent.useColorN01;
                        useCols[1] |= clothesComponent.useColorN02;
                        useCols[2] |= clothesComponent.useColorN03;
                        useCols[3] |= clothesComponent.rendAccessory != null;
                    }
                }
            }

            return useCols;
        }

        public bool IsMultiPartTop(int kind)
        {
            if (kind == 0 && (ChaControl.infoClothes[kind].Kind == 1 || ChaControl.infoClothes[kind].Kind == 2))
                return true;
            return false;
        }

        public Color GetClothingColor(int kind, int colorNr, int slotNr = -1)
        {
            if (slotNr < 0)
                return Clothes.parts[kind].colorInfo[colorNr].baseColor;
            return Accessories.parts[slotNr].color[colorNr];
        }

        public void ResetClothingColor(int kind, int colorNr, int slotNr)
        {
            var clothingColors = new ClothingColors(CurrentOutfitSlot, kind, colorNr, slotNr);
            if (originalClothingColors.TryGetValue(clothingColors, out var color))
                SetClothingColor(kind, colorNr, color.OriginalValue, slotNr);
        }

        internal void ChangeCoordinateEvent()
        {
            StartCoroutine(ChangeCoordinateTypeCoroutine());
        }

        private IEnumerator ChangeCoordinateTypeCoroutine()
        {
            yield return null;

            selectedCharacterClothing.Remove(ChaControl);
            var characterClothing = new List<CharacterClothing>();

            foreach (var kind in clothingKinds)
            {
                var name = GetclothingName(kind.Value);
                if (name != "None")
                    characterClothing.Add(new CharacterClothing(kind.Value, name, CheckClothingUseColor(kind.Value)));
            }

            if (c2aAIlnstances != null && c2aAIlnstances.Contains(ChaControl))
            {
                foreach (var adapterList in c2aAIlnstances[ChaControl] as IEnumerable)
                {
                    for (int i = 0; i < ((IList)adapterList).Count; i++)
                    {
                        var adapter = ((IList)adapterList)[i];

                        var kind = (int)c2aClothingKindField.GetValue(adapter);
                        var name = ((MonoBehaviour)adapter).gameObject.name;
                        var slotNr = -1;
                        if (name.Contains("ca_slot"))
                            slotNr = Int32.Parse(name.Substring(7));

                        characterClothing.Add(new CharacterClothing(kind, GetclothingName(kind, slotNr), CheckClothingUseColor(kind, slotNr), slotNr));
                    }
                }
            }

            selectedCharacterClothing[ChaControl] = characterClothing;
        }
        #endregion

        private new void OnDestroy()
        {
            allControllers.Remove(ChaControl);
        }
    }

    internal class CharacterClothing
    {
        public int Kind { get; set; }
        public string Name { get; set; }
        public bool IsC2a { get; }
        public int SlotNr { get; set; }
        public bool[] UseColors { get; set; }

        public CharacterClothing(int kind, string name, bool[] useCols, int slotNr = -1)
        {
            Kind = kind;
            Name = name;
            SlotNr = slotNr;
            UseColors = useCols;
            IsC2a = slotNr >= 0;
        }
    }

    [Serializable]
    [MessagePackObject]
    internal struct ClothingColors
    {
        [Key("OutfitSlot")]
        public int OutfitSlot { get; set; }
        [Key("ClothingKind")]
        public int ClothingKind { get; set; }
        [Key("ColorNr")]
        public int ColorNr { get; set; }
        [Key("SlotNr")]
        public int SlotNr { get; set; }

        public ClothingColors(int outfitSlot, int kind, int colorNr, int slotNr)
        {
            OutfitSlot = outfitSlot;
            ClothingKind = kind;
            ColorNr = colorNr;
            SlotNr = slotNr;
        }

        public bool Compare(int outfitSlot, int kind, int colorNr, int slotNr)
        {
            if (
                OutfitSlot == outfitSlot
                && ClothingKind == kind
                && ColorNr == colorNr
                && SlotNr == slotNr
            )
                return true;
            return false;
        }
    }

    [Serializable]
    [MessagePackObject]
    public class FloatStorage
    {
        [Key("OriginalValue")]
        public float OriginalValue { get; set; }
        [Key("Value")]
        public float Value { get; set; }

        public FloatStorage(float originalValue, float value)
        {
            OriginalValue = originalValue;
            Value = value;
        }
    }

    [Serializable]
    [MessagePackObject]
    public class ColorStorage
    {
        [Key("OriginalValue")]
        public Color OriginalValue { get; set; }
        [Key("Value")]
        public Color Value { get; set; }

        public ColorStorage(Color originalValue, Color value)
        {
            OriginalValue = originalValue;
            Value = value;
        }
    }
}
