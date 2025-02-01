using KKAPI;
using KKAPI.Chara;
using System.Collections.Generic;
using static Plugins.StudioSkinColor;
using UnityEngine;
using KK_Plugins.MaterialEditor;
using System.Linq;
using System.Collections;
using System;
using MessagePack;
using ExtensibleSaveFormat;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    internal class StudioSkinColorCharaController : CharaCustomFunctionController
    {
        internal static readonly Dictionary<ChaControl, StudioSkinColorCharaController> allControllers = new Dictionary<ChaControl, StudioSkinColorCharaController>();

        #region Save Lists
        private Dictionary<ClothingColors, ColorStorage> OriginalClothingColors = new Dictionary<ClothingColors, ColorStorage>();
        private Dictionary<HairColor, ColorStorage> OriginalHairColors = new Dictionary<HairColor, ColorStorage>();
        private Dictionary<BodyColor, ColorStorage> OriginalBodyColors = new Dictionary<BodyColor, ColorStorage>();
        private Dictionary<FaceColor, ColorStorage> OriginalFaceColors = new Dictionary<FaceColor, ColorStorage>();
        private Dictionary<Bust, FloatStorage> OriginalBustValues = new Dictionary<Bust, FloatStorage>();
        private Dictionary<int, FloatStorage> OriginalBodyShapeValues = new Dictionary<int, FloatStorage>();
        private Dictionary<int, FloatStorage> OriginalFaceShapeValues = new Dictionary<int, FloatStorage>();
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
            var data = new PluginData();

            if (OriginalClothingColors.Count > 0)
                data.data.Add(nameof(OriginalClothingColors), MessagePackSerializer.Serialize(OriginalClothingColors));
            else
                data.data.Add(nameof(OriginalClothingColors), null);

            if (OriginalHairColors.Count > 0)
                data.data.Add(nameof(OriginalHairColors), MessagePackSerializer.Serialize(OriginalHairColors));
            else
                data.data.Add(nameof(OriginalHairColors), null);

            if (OriginalBodyColors.Count > 0)
                data.data.Add(nameof(OriginalBodyColors), MessagePackSerializer.Serialize(OriginalBodyColors));
            else
                data.data.Add(nameof(OriginalBodyColors), null);

            if (OriginalFaceColors.Count > 0)
                data.data.Add(nameof(OriginalFaceColors), MessagePackSerializer.Serialize(OriginalFaceColors));
            else
                data.data.Add(nameof(OriginalFaceColors), null);

            if (OriginalBustValues.Count > 0)
                data.data.Add(nameof(OriginalBustValues), MessagePackSerializer.Serialize(OriginalBustValues));
            else
                data.data.Add(nameof(OriginalBustValues), null);

            if (OriginalBodyShapeValues.Count > 0)
                data.data.Add(nameof(OriginalBodyShapeValues), MessagePackSerializer.Serialize(OriginalBodyShapeValues));
            else
                data.data.Add(nameof(OriginalBodyShapeValues), null);

            if (OriginalFaceShapeValues.Count > 0)
                data.data.Add(nameof(OriginalFaceShapeValues), MessagePackSerializer.Serialize(OriginalFaceShapeValues));
            else
                data.data.Add(nameof(OriginalFaceShapeValues), null);

            SetExtendedData(data);
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (maintainState)
                return;

            allControllers[ChaControl] = this;
            OriginalClothingColors.Clear();
            OriginalHairColors.Clear();
            OriginalBodyColors.Clear();
            OriginalFaceColors.Clear();
            OriginalBustValues.Clear();
            OriginalBodyShapeValues.Clear();
            OriginalFaceShapeValues.Clear();

            var data = GetExtendedData();
            if (data == null)
                return;

            if (data.data.TryGetValue(nameof(OriginalClothingColors), out var originalClothingColors) && originalClothingColors != null)
                OriginalClothingColors = MessagePackSerializer.Deserialize<Dictionary<ClothingColors, ColorStorage>>((byte[])originalClothingColors);

            if (data.data.TryGetValue(nameof(OriginalHairColors), out var originalHairColors) && originalHairColors != null)
                OriginalHairColors = MessagePackSerializer.Deserialize<Dictionary<HairColor, ColorStorage>>((byte[])originalHairColors);

            if (data.data.TryGetValue(nameof(OriginalBodyColors), out var originalBodyColors) && originalBodyColors != null)
                OriginalBodyColors = MessagePackSerializer.Deserialize<Dictionary<BodyColor, ColorStorage>>((byte[])originalBodyColors);

            if (data.data.TryGetValue(nameof(OriginalFaceColors), out var originalFaceColors) && originalFaceColors != null)
                OriginalFaceColors = MessagePackSerializer.Deserialize<Dictionary<FaceColor, ColorStorage>>((byte[])originalFaceColors);

            if (data.data.TryGetValue(nameof(OriginalBustValues), out var originalBustValues) && originalBustValues != null)
                OriginalBustValues = MessagePackSerializer.Deserialize<Dictionary<Bust, FloatStorage>>((byte[])originalBustValues);

            if (data.data.TryGetValue(nameof(OriginalBodyShapeValues), out var originalBodyShapeValues) && originalBodyShapeValues != null)
                OriginalBodyShapeValues = MessagePackSerializer.Deserialize<Dictionary<int, FloatStorage>>((byte[])originalBodyShapeValues);

            if (data.data.TryGetValue(nameof(OriginalFaceShapeValues), out var originalFaceShapeValues) && originalFaceShapeValues != null)
                OriginalFaceShapeValues = MessagePackSerializer.Deserialize<Dictionary<int, FloatStorage>>((byte[])originalFaceShapeValues);
        }

        public static StudioSkinColorCharaController GetController(ChaControl chaCtrl)
        {
            if (chaCtrl == null)
                return null;
            if (allControllers.ContainsKey(chaCtrl))
                return allControllers[chaCtrl];
#if DEBUG
            return chaCtrl.gameObject.GetOrAddComponent<StudioSkinColorCharaController>();
#else
            return chaCtrl.gameObject.GetComponent<StudioSkinColorCharaController>();
#endif
        }

        #region Body
        #region Body color
        public void UpdateBodyTextures(bool inpBase = false, bool inpSub = false, bool inpPaint01 = false, bool inpPaint02 = false, bool inpSunburn = false, bool inpNail = false)
        {
            ChaControl.AddUpdateCMBodyTexFlags(inpBase, inpSub, inpPaint01, inpPaint02, inpSunburn);
            ChaControl.AddUpdateCMBodyColorFlags(inpBase, inpSub, inpPaint01, inpPaint02, inpSunburn, inpNail);
            ChaControl.CreateBodyTexture();
            ChaControl.SetBodyBaseMaterial();
        }

        public void UpdateFaceTextures(bool inpBase = false, bool inpSub = false, bool inpPaint01 = false, bool inpPaint02 = false, bool inpCheek = false, bool inpLipLine = false, bool inpMole = false)
        {
            //ChaControl.AddUpdateCMFaceTexFlags(inpBase, inpSub, inpPaint01, inpPaint02, inpCheek, inpLipLine, inpMole);
            // For some reason the face doesn't update texture if these aren't all true
            ChaControl.AddUpdateCMFaceTexFlags(true, true, true, true, true, true, true);
            ChaControl.AddUpdateCMFaceColorFlags(inpBase, inpSub, inpPaint01, inpPaint02, inpCheek, inpLipLine, inpMole);
            ChaControl.CreateFaceTexture();
            ChaControl.SetFaceBaseMaterial();
        }

        public void UpdateBodyColor(Color color, BodyColor textureColor)
        {
            if (!OriginalBodyColors.ContainsKey(textureColor))
                OriginalBodyColors[textureColor] = new ColorStorage(GetBodyColor(textureColor), color);
            else
                OriginalBodyColors[textureColor].Value = color;

            switch (textureColor)
            {
                case BodyColor.SkinMain:
                    ChaControl.fileBody.skinMainColor = color;
                    UpdateFaceTextures(inpBase: true);
                    UpdateBodyTextures(inpBase: true);
                    break;
                case BodyColor.SkinSub:
                    ChaControl.fileBody.skinSubColor = color;
                    UpdateFaceTextures(inpSub: true);
                    UpdateBodyTextures(inpSub: true);
                    break;
                case BodyColor.SkinTan:
                    ChaControl.fileBody.sunburnColor = color;
                    UpdateBodyTextures(inpSunburn: true);
                    break;
                case BodyColor.NippleColor:
                    ChaControl.fileBody.nipColor = color;
                    ChaControl.ChangeSettingNipColor();
                    break;
                case BodyColor.NailColor:
                    ChaControl.fileBody.nailColor = color;
                    UpdateBodyTextures(inpNail: true);
                    break;
                case BodyColor.PubicHairColor:
                    ChaControl.fileBody.underhairColor = color;
                    ChaControl.ChangeSettingUnderhairColor();
                    break;
            }
        }

        public Color GetBodyColor(BodyColor color)
        {
            switch (color)
            {
                case BodyColor.SkinMain:
                    return ChaControl.fileBody.skinMainColor;
                case BodyColor.SkinSub:
                    return ChaControl.fileBody.skinSubColor;
                case BodyColor.SkinTan:
                    return ChaControl.fileBody.sunburnColor;
                case BodyColor.NippleColor:
                    return ChaControl.fileBody.nipColor;
                case BodyColor.NailColor:
                    return ChaControl.fileBody.nailColor;
                case BodyColor.PubicHairColor:
                    return ChaControl.fileBody.underhairColor;
            }
            return Color.white;
        }

        public void ResetBodyColor(BodyColor colorType)
        {
            if (OriginalBodyColors.TryGetValue(colorType, out var color))
                UpdateBodyColor(color.OriginalValue, colorType);
        }

        public Color GetOriginalBodyColor(BodyColor colorType)
        {
            if (OriginalBodyColors.TryGetValue(colorType, out var color))
                return color.OriginalValue;
            return GetBodyColor(colorType);

        }
        #endregion

        #region Bust
        public void SetBustValue(float value, Bust bust)
        {
            if (!OriginalBustValues.ContainsKey(bust))
                OriginalBustValues[bust] = new FloatStorage(GetBustValue(bust), value);
            else
                OriginalBustValues[bust].Value = value;

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
            if (OriginalBustValues.TryGetValue(bust, out var value))
                SetBustValue(value.OriginalValue, bust);
        }

        public float GetOriginalBustValue(Bust bust)
        {
            if (OriginalBustValues.TryGetValue(bust, out var original))
                return original.OriginalValue;
            return GetBustValue(bust);
        }

        public float GetBustValue(Bust bust)
        {
            if (bust == Bust.Softness)
                return ChaControl.fileBody.bustSoftness;
            else if (bust == Bust.Weight)
                return ChaControl.fileBody.bustWeight;
            return 1f;
        }
        #endregion

        #region Body shape
        public void UpdateBodyShapeValue(int index, float value)
        {
            if (!OriginalBodyShapeValues.ContainsKey(index))
                OriginalBodyShapeValues[index] = new FloatStorage(GetCurrentBodyValue(index), value);
            else
                OriginalBodyShapeValues[index].Value = value;
            ChaControl.SetShapeBodyValue(index, value);
        }

        public float GetCurrentBodyValue(int index)
        {
            return ChaControl.GetShapeBodyValue(index);
        }

        public void ResetBodyShapeValue(int index)
        {
            if (OriginalBodyShapeValues.TryGetValue(index, out var shapeValueBody))
                UpdateBodyShapeValue(index, shapeValueBody.OriginalValue);
        }

        public float GetOriginalBodyShapeValue(int index)
        {
            if (OriginalBodyShapeValues.TryGetValue(index, out var original))
                return original.OriginalValue;
            return GetCurrentBodyValue(index);
        }
        #endregion

        public bool IsBodyCategoryEdited(string category)
        {
            var isEdited = false;
            if (shapeBodyValueMap.TryGetValue(category, out var keys))
            {
                isEdited |= OriginalBodyShapeValues
                    .Where(x => keys.ContainsKey(x.Key))
                    .Select(x => x.Value)
                    .Any(x => Mathf.Abs(x.Value - x.OriginalValue) > 0.001f);
            }
            if (category == "General")
                isEdited |= OriginalBodyColors.Any(x => x.Value.Value != x.Value.OriginalValue);
            else if (category == "Chest")
                isEdited |= OriginalBustValues.Any(x => Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f);

            return isEdited;
        }
        #endregion

        #region Face
        public void UpdateFaceColor(Color color, FaceColor faceColor)
        {
            if (!OriginalFaceColors.ContainsKey(faceColor))
                OriginalFaceColors[faceColor] = new ColorStorage(GetFaceColor(faceColor), color);
            else
                OriginalFaceColors[faceColor].Value = color;

            switch (faceColor)
            {
                case FaceColor.EyebrowColor:
                    ChaControl.fileFace.eyebrowColor = color;
                    ChaControl.ChangeSettingEyebrowColor();
                    break;
                case FaceColor.EyelineColor:
                    ChaControl.fileFace.eyelineColor = color;
                    ChaControl.ChangeSettingEyelineColor();
                    break;
                case FaceColor.ScleraColor1:
                    ChaControl.fileFace.whiteBaseColor = color;
                    ChaControl.ChangeSettingWhiteOfEye(true, true);
                    break;
                case FaceColor.ScleraColor2:
                    ChaControl.fileFace.whiteSubColor = color;
                    ChaControl.ChangeSettingWhiteOfEye(true, true);
                    break;
                case FaceColor.UpperHighlightColor:
                    ChaControl.fileFace.hlUpColor = color;
                    ChaControl.ChangeSettingEyeHiUpColor();
                    break;
                case FaceColor.LowerHightlightColor:
                    ChaControl.fileFace.hlDownColor = color;
                    ChaControl.ChangeSettingEyeHiDownColor();
                    break;
                case FaceColor.EyeColor1Left:
                    ChaControl.fileFace.pupil[0].baseColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, false);
                    break;
                case FaceColor.EyeColor2Left:
                    ChaControl.fileFace.pupil[0].subColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, false);
                    break;
                case FaceColor.EyeColor1Right:
                    ChaControl.fileFace.pupil[1].baseColor = color;
                    ChaControl.ChangeSettingEyeR(true, true, false);
                    break;
                case FaceColor.EyeColor2Right:
                    ChaControl.fileFace.pupil[2].subColor = color;
                    ChaControl.ChangeSettingEyeR(true, true, false);
                    break;
                case FaceColor.LipLineColor:
                    ChaControl.fileFace.lipLineColor = color;
                    UpdateFaceTextures(inpLipLine: true);
                    break;
                case FaceColor.EyeShadowColor:
                    ChaControl.fileFace.baseMakeup.eyeshadowColor = color;
                    ChaControl.ChangeSettingEyeShadowColor();
                    break;
                case FaceColor.CheekColor:
                    ChaControl.fileFace.baseMakeup.cheekColor = color;
                    UpdateFaceTextures(inpCheek: true);
                    break;
                case FaceColor.LipColor:
                    ChaControl.fileFace.baseMakeup.lipColor = color;
                    ChaControl.ChangeSettingLipColor();
                    break;
            }
        }

        public Color GetFaceColor(FaceColor color)
        {
            switch (color)
            {
                case FaceColor.EyebrowColor:
                    return ChaControl.fileFace.eyebrowColor;
                case FaceColor.EyelineColor:
                    return ChaControl.fileFace.eyelineColor;
                case FaceColor.ScleraColor1:
                    return ChaControl.fileFace.whiteBaseColor;
                case FaceColor.ScleraColor2:
                    return ChaControl.fileFace.whiteSubColor;
                case FaceColor.UpperHighlightColor:
                    return ChaControl.fileFace.hlUpColor;
                case FaceColor.LowerHightlightColor:
                    return ChaControl.fileFace.hlDownColor;
                case FaceColor.EyeColor1Left:
                    return ChaControl.fileFace.pupil[0].baseColor;
                case FaceColor.EyeColor2Left:
                    return ChaControl.fileFace.pupil[0].subColor;
                case FaceColor.EyeColor1Right:
                    return ChaControl.fileFace.pupil[1].baseColor;
                case FaceColor.EyeColor2Right:
                    return ChaControl.fileFace.pupil[1].subColor;
                case FaceColor.LipLineColor:
                    return ChaControl.fileFace.lipLineColor;
                case FaceColor.EyeShadowColor:
                    return ChaControl.fileFace.baseMakeup.eyeshadowColor;
                case FaceColor.CheekColor:
                    return ChaControl.fileFace.baseMakeup.cheekColor;
                case FaceColor.LipColor:
                    return ChaControl.fileFace.baseMakeup.lipColor;
            }
            return Color.white;
        }

        public void ResetFaceColor(FaceColor colorType)
        {
            if (OriginalFaceColors.TryGetValue(colorType, out var color))
                UpdateFaceColor(color.OriginalValue, colorType);
        }

        public Color GetOriginalFaceColor(FaceColor colorType)
        {
            if (OriginalFaceColors.TryGetValue(colorType, out var color))
                return color.OriginalValue;
            return GetFaceColor(colorType);

        }
        #endregion

        #region Face shape
        public void UpdateFaceShapeValue(int index, float value)
        {
            if (!OriginalFaceShapeValues.ContainsKey(index))
                OriginalFaceShapeValues[index] = new FloatStorage(GetCurrentFaceValue(index), value);
            else
                OriginalFaceShapeValues[index].Value = value;
            ChaControl.SetShapeFaceValue(index, value);
        }

        public float GetCurrentFaceValue(int index)
        {
            return ChaControl.GetShapeFaceValue(index);
        }

        public void ResetFaceShapeValue(int index)
        {
            if (OriginalFaceShapeValues.TryGetValue(index, out var shapeValue))
                UpdateFaceShapeValue(index, shapeValue.OriginalValue);
        }

        public float GetOriginalFaceShapeValue(int index)
        {
            if (OriginalFaceShapeValues.TryGetValue(index, out var shapeValue))
                return shapeValue.OriginalValue;
            return GetCurrentFaceValue(index);
        }

        public bool IsFaceEdited(string category)
        {
            var isEdited = false;
            if (shapeFaceValueMap.TryGetValue(category, out var keys))
            {
                isEdited |= OriginalFaceShapeValues
                    .Where(x => keys.ContainsKey(x.Key))
                    .Select(x => x.Value)
                    .Any(x => Mathf.Abs(x.Value - x.OriginalValue) > 0.001f);
            }
            return isEdited;
        }
        #endregion

        #region Hair
        public void UpdateHairColor(Color color, HairColor hairColor)
        {
            if (!OriginalHairColors.ContainsKey(hairColor))
                OriginalHairColors[hairColor] = new ColorStorage(GetHairColor(hairColor), color);
            else
                OriginalHairColors[hairColor].Value = color;

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
            if (OriginalHairColors.TryGetValue(hairColor, out var color))
                UpdateHairColor(color.OriginalValue, hairColor);
        }

        public Color GetOriginalHairColor(HairColor hairColor)
        {
            if (OriginalHairColors.TryGetValue(hairColor, out var color))
                return color.OriginalValue;
            return GetHairColor(hairColor);
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
        #endregion

        #region Clothes
        public bool ClothingKindExists(int kind)
        {
            return selectedCharacterClothing[ChaControl].Any(c => c.Kind == kind);
        }

        public string GetclothingName(int kind, int slotNr = -1)
        {
            if (slotNr < 0)
                return ChaControl.infoClothes[kind]?.Name;
            return ChaControl.infoAccessory[slotNr]?.Name;
        }

        public void InitBaseCustomTextureClothesIfNotExists(int kind)
        {
            try
            {
                if (selectedCharacter?.ctCreateClothes[kind, 0] == null && selectedCharacter.infoClothes[kind] != null)
                    selectedCharacter.InitBaseCustomTextureClothes(true, kind);
            }
            catch (Exception e)
            {
                StudioSkinColor.Logger.LogMessage("Selected option is broken and was switched back to the first option");
                StudioSkinColor.Logger.LogError(e);
                if (kind == 0) SetSelectKind(SelectKindType.CosTop, 0);
                else if (kind == 1) SetSelectKind(SelectKindType.CosBot, 0);
                else if (kind == 2) SetSelectKind(SelectKindType.CosBra, 0);
                else if (kind == 3) SetSelectKind(SelectKindType.CosShorts, 0);
                else if (kind == 4) SetSelectKind(SelectKindType.CosGloves, 0);
                else if (kind == 5) SetSelectKind(SelectKindType.CosPanst, 0);
                else if (kind == 6) SetSelectKind(SelectKindType.CosSocks, 0);
                else if (kind == 7) SetSelectKind(SelectKindType.CosInnerShoes, 0);
                else if (kind == 8) SetSelectKind(SelectKindType.CosOuterShoes, 0);
            }
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
            if (!OriginalClothingColors.Any(x => x.Key.Compare(CurrentOutfitSlot, kind, colorNr, slotNr)))
                OriginalClothingColors[clothingColors] = new ColorStorage(GetClothingColor(kind, colorNr, slotNr), color);
            else
                OriginalClothingColors[clothingColors].Value = color;


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
            if (OriginalClothingColors.TryGetValue(clothingColors, out var color))
                SetClothingColor(kind, colorNr, color.OriginalValue, slotNr);
        }

        public Color GetOriginalClothingColor(int kind, int colorNr, int slotNr)
        {
            var clothingColors = new ClothingColors(CurrentOutfitSlot, kind, colorNr, slotNr);
            if (OriginalClothingColors.TryGetValue(clothingColors, out var color))
                return color.OriginalValue;
            return GetClothingColor(kind, colorNr, slotNr);
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

        public bool IsClothingKindEdited(int kind)
        {
            return OriginalClothingColors
                .Where(c => c.Key.ClothingKind == kind)
                .Select(c => c.Value)
                .Any(c => c.Value != c.OriginalValue);
        }
        #endregion

        public void SetSelectKind(SelectKindType type, int id)
        {
            void UpdateClothesPattern(int kind, int pattern)
            {
                Clothes.parts[kind].colorInfo[pattern].pattern = id;
                SetClothes.parts[kind].colorInfo[pattern].pattern = id;
                if (IsMultiPartTop(kind))
                {
                    if (pattern == 0)
                    {
                        selectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: true, updateTex02: false, updateTex03: false, updateTex04: false);
                        selectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: true, updateTex02: false, updateTex03: false, updateTex04: false);
                    }
                    else if (pattern == 1)
                    {
                        selectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: false, updateTex02: true, updateTex03: false, updateTex04: false);
                        selectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: false, updateTex02: true, updateTex03: false, updateTex04: false);
                    }
                    else if (pattern == 2)
                    {
                        selectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: true, updateTex04: false);
                        selectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: true, updateTex04: false);
                    }
                    else if (pattern == 3)
                    {
                        selectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: true);
                        selectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: true);
                    }
                }
                if (pattern == 0)
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: true, updateTex02: false, updateTex03: false, updateTex04: false);
                if (pattern == 1)
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: false, updateTex02: true, updateTex03: false, updateTex04: false);
                if (pattern == 2)
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: true, updateTex04: false);
                if (pattern == 3)
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: true);
            }


            if (selectedCharacter == null)
                return;

            switch (type)
            {
                case SelectKindType.FaceDetail:
                    selectedCharacter.fileFace.detailId = id;
                    selectedCharacter.ChangeSettingFaceDetail();
                    break;
                case SelectKindType.Eyebrow:
                    selectedCharacter.fileFace.eyebrowId = id;
                    selectedCharacter.ChangeSettingEyebrow();
                    break;
                case SelectKindType.EyelineUp:
                    selectedCharacter.fileFace.eyelineUpId = id;
                    selectedCharacter.ChangeSettingEyelineUp();
                    break;
                case SelectKindType.EyelineDown:
                    selectedCharacter.fileFace.eyelineDownId = id;
                    selectedCharacter.ChangeSettingEyelineDown();
                    break;
                case SelectKindType.EyeWGrade:
                    selectedCharacter.fileFace.whiteId = id;
                    selectedCharacter.ChangeSettingWhiteOfEye(true, false);
                    break;
                case SelectKindType.EyeHLUp:
                    selectedCharacter.fileFace.hlUpId = id;
                    selectedCharacter.ChangeSettingEyeHiUp();
                    break;
                case SelectKindType.EyeHLDown:
                    selectedCharacter.fileFace.hlDownId = id;
                    selectedCharacter.ChangeSettingEyeHiDown();
                    break;
                case SelectKindType.Pupil:
                    selectedCharacter.fileFace.pupil[0].id = id;
                    selectedCharacter.ChangeSettingEye(true, false, false);
                    break;
                case SelectKindType.PupilGrade:
                    selectedCharacter.fileFace.pupil[0].gradMaskId = id;
                    selectedCharacter.ChangeSettingEye(false, true, false);
                    break;
                case SelectKindType.Nose:
                    selectedCharacter.fileFace.noseId = id;
                    selectedCharacter.ChangeSettingNose();
                    break;
                case SelectKindType.Lipline:
                    selectedCharacter.fileFace.lipLineId = id;
                    UpdateFaceTextures(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLipLine: true, inpMole: false);
                    break;
                case SelectKindType.Mole:
                    selectedCharacter.fileFace.moleId = id;
                    UpdateFaceTextures(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLipLine: false, inpMole: true);
                    break;
                case SelectKindType.Eyeshadow:
                    selectedCharacter.fileFace.baseMakeup.eyeshadowId = id;
                    selectedCharacter.ChangeSettingEyeShadow();
                    break;
                case SelectKindType.Cheek:
                    selectedCharacter.fileFace.baseMakeup.cheekId = id;
                    UpdateFaceTextures(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpCheek: true, inpLipLine: false, inpMole: false);
                    break;
                case SelectKindType.Lip:
                    selectedCharacter.fileFace.baseMakeup.lipId = id;
                    selectedCharacter.ChangeSettingLip();
                    break;
                case SelectKindType.FacePaint01:
                    selectedCharacter.fileFace.baseMakeup.paintId[0] = id;
                    selectedCharacter.AddUpdateCMFaceTexFlags(inpBase: false, inpSub: false, inpPaint01: true, inpPaint02: false, inpCheek: false, inpLipLine: false, inpMole: false);
                    selectedCharacter.CreateFaceTexture();
                    selectedCharacter.SetFaceBaseMaterial();
                    break;
                case SelectKindType.FacePaint02:
                    selectedCharacter.fileFace.baseMakeup.paintId[1] = id;
                    selectedCharacter.AddUpdateCMFaceTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: true, inpCheek: false, inpLipLine: false, inpMole: false);
                    selectedCharacter.CreateFaceTexture();
                    selectedCharacter.SetFaceBaseMaterial();
                    break;
                case SelectKindType.BodyDetail:
                    selectedCharacter.fileBody.detailId = id;
                    selectedCharacter.ChangeSettingBodyDetail();
                    break;
                case SelectKindType.Nip:
                    selectedCharacter.fileBody.nipId = id;
                    selectedCharacter.ChangeSettingNip();
                    break;
                case SelectKindType.Underhair:
                    selectedCharacter.fileBody.underhairId = id;
                    selectedCharacter.ChangeSettingUnderhair();
                    break;
                case SelectKindType.Sunburn:
                    selectedCharacter.fileBody.sunburnId = id;
                    selectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpSunburn: true);
                    selectedCharacter.CreateBodyTexture();
                    selectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint01:
                    selectedCharacter.fileBody.paintId[0] = id;
                    selectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: true, inpPaint02: false, inpSunburn: false);
                    selectedCharacter.CreateBodyTexture();
                    selectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint02:
                    selectedCharacter.fileBody.paintId[1] = id;
                    selectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: true, inpSunburn: false);
                    selectedCharacter.CreateBodyTexture();
                    selectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint01Layout:
                    selectedCharacter.fileBody.paintLayoutId[0] = id;
                    selectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: true, inpPaint02: false, inpSunburn: false);
                    selectedCharacter.CreateBodyTexture();
                    selectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint02Layout:
                    selectedCharacter.fileBody.paintLayoutId[1] = id;
                    selectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: true, inpSunburn: false);
                    selectedCharacter.CreateBodyTexture();
                    selectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.HairBack:
                    selectedCharacter.fileHair.parts[0].id = id;
                    selectedCharacter.ChangeHairBack(true);
                    break;
                case SelectKindType.HairFront:
                    selectedCharacter.fileHair.parts[1].id = id;
                    selectedCharacter.ChangeHairFront(true);
                    break;
                case SelectKindType.HairSide:
                    selectedCharacter.fileHair.parts[2].id = id;
                    selectedCharacter.ChangeHairSide(true);
                    break;
                case SelectKindType.HairExtension:
                    selectedCharacter.fileHair.parts[3].id = id;
                    selectedCharacter.ChangeHairOption(true);
                    break;
                case SelectKindType.CosTop:
                    //clothes.parts[clothesType].sleevesType = 0;
                    //setClothes.parts[clothesType].sleevesType = 0;
                    Clothes.parts[0].id = id;
                    SetClothes.parts[0].id = id;
                    selectedCharacter.ChangeClothesTop(id, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor01:
                    Clothes.subPartsId[0] = id;
                    SetClothes.subPartsId[0] = id;
                    selectedCharacter.ChangeClothesTop(0, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor02:
                    Clothes.subPartsId[1] = id;
                    SetClothes.subPartsId[1] = id;
                    selectedCharacter.ChangeClothesTop(0, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor03:
                    Clothes.subPartsId[2] = id;
                    SetClothes.subPartsId[2] = id;
                    selectedCharacter.ChangeClothesTop(0, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket01:
                    Clothes.subPartsId[0] = id;
                    SetClothes.subPartsId[0] = id;
                    selectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket02:
                    Clothes.subPartsId[1] = id;
                    SetClothes.subPartsId[1] = id;
                    selectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket03:
                    Clothes.subPartsId[2] = id;
                    SetClothes.subPartsId[2] = id;
                    selectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosTopPtn01:
                    UpdateClothesPattern(0, 0);
                    break;
                case SelectKindType.CosTopPtn02:
                    UpdateClothesPattern(0, 1);
                    break;
                case SelectKindType.CosTopPtn03:
                    UpdateClothesPattern(0, 2);
                    break;
                case SelectKindType.CosTopPtn04:
                    UpdateClothesPattern(0, 3);
                    break;
                case SelectKindType.CosTopEmblem:
                    Clothes.parts[0].emblemeId = id;
                    SetClothes.parts[0].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(0, id);
                    break;
                case SelectKindType.CosBot:
                    Clothes.parts[1].id = id;
                    SetClothes.parts[1].id = id;
                    selectedCharacter.ChangeClothesBot(id, true);
                    break;
                case SelectKindType.CosBotPtn01:
                    UpdateClothesPattern(1, 0);
                    break;
                case SelectKindType.CosBotPtn02:
                    UpdateClothesPattern(1, 1);
                    break;
                case SelectKindType.CosBotPtn03:
                    UpdateClothesPattern(1, 2);
                    break;
                case SelectKindType.CosBotPtn04:
                    UpdateClothesPattern(1, 3);
                    break;
                case SelectKindType.CosBotEmblem:
                    Clothes.parts[1].emblemeId = id;
                    SetClothes.parts[1].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(1, id);
                    break;
                case SelectKindType.CosBra:
                    Clothes.parts[2].id = id;
                    SetClothes.parts[2].id = id;
                    selectedCharacter.ChangeClothesBra(id, true);
                    break;
                case SelectKindType.CosBraPtn01:
                    UpdateClothesPattern(2, 0);
                    break;
                case SelectKindType.CosBraPtn02:
                    UpdateClothesPattern(2, 1);
                    break;
                case SelectKindType.CosBraPtn03:
                    UpdateClothesPattern(2, 2);
                    break;
                case SelectKindType.CosBraPtn04:
                    UpdateClothesPattern(2, 3);
                    break;
                case SelectKindType.CosBraEmblem:
                    Clothes.parts[2].emblemeId = id;
                    SetClothes.parts[2].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(2, id);
                    break;
                case SelectKindType.CosShorts:
                    Clothes.parts[3].id = id;
                    SetClothes.parts[3].id = id;
                    selectedCharacter.ChangeClothesShorts(id, true);
                    break;
                case SelectKindType.CosShortsPtn01:
                    UpdateClothesPattern(3, 0);
                    break;
                case SelectKindType.CosShortsPtn02:
                    UpdateClothesPattern(3, 1);
                    break;
                case SelectKindType.CosShortsPtn03:
                    UpdateClothesPattern(3, 2);
                    break;
                case SelectKindType.CosShortsPtn04:
                    UpdateClothesPattern(3, 3);
                    break;
                case SelectKindType.CosShortsEmblem:
                    Clothes.parts[3].emblemeId = id;
                    SetClothes.parts[3].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(3, id);
                    break;
                case SelectKindType.CosGloves:
                    Clothes.parts[4].id = id;
                    SetClothes.parts[4].id = id;
                    selectedCharacter.ChangeClothesGloves(id, true);
                    break;
                case SelectKindType.CosGlovesPtn01:
                    UpdateClothesPattern(4, 0);
                    break;
                case SelectKindType.CosGlovesPtn02:
                    UpdateClothesPattern(4, 1);
                    break;
                case SelectKindType.CosGlovesPtn03:
                    UpdateClothesPattern(4, 2);
                    break;
                case SelectKindType.CosGlovesPtn04:
                    UpdateClothesPattern(4, 3);
                    break;
                case SelectKindType.CosGlovesEmblem:
                    Clothes.parts[4].emblemeId = id;
                    SetClothes.parts[4].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(4, id);
                    break;
                case SelectKindType.CosPanst:
                    Clothes.parts[5].id = id;
                    SetClothes.parts[5].id = id;
                    selectedCharacter.ChangeClothesPanst(id, true);
                    break;
                case SelectKindType.CosPanstPtn01:
                    UpdateClothesPattern(5, 0);
                    break;
                case SelectKindType.CosPanstPtn02:
                    UpdateClothesPattern(5, 1);
                    break;
                case SelectKindType.CosPanstPtn03:
                    UpdateClothesPattern(5, 2);
                    break;
                case SelectKindType.CosPanstPtn04:
                    UpdateClothesPattern(5, 3);
                    break;
                case SelectKindType.CosPanstEmblem:
                    Clothes.parts[5].emblemeId = id;
                    SetClothes.parts[5].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(5, id);
                    break;
                case SelectKindType.CosSocks:
                    Clothes.parts[6].id = id;
                    SetClothes.parts[6].id = id;
                    selectedCharacter.ChangeClothesSocks(id, true);
                    break;
                case SelectKindType.CosSocksPtn01:
                    UpdateClothesPattern(6, 0);
                    break;
                case SelectKindType.CosSocksPtn02:
                    UpdateClothesPattern(6, 1);
                    break;
                case SelectKindType.CosSocksPtn03:
                    UpdateClothesPattern(6, 2);
                    break;
                case SelectKindType.CosSocksPtn04:
                    UpdateClothesPattern(6, 3);
                    break;
                case SelectKindType.CosSocksEmblem:
                    Clothes.parts[6].emblemeId = id;
                    SetClothes.parts[6].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(6, id);
                    break;
                case SelectKindType.CosInnerShoes:
                    Clothes.parts[7].id = id;
                    SetClothes.parts[7].id = id;
                    selectedCharacter.ChangeClothesShoes(0, id, true);
                    break;
                case SelectKindType.CosInnerShoesPtn01:
                    UpdateClothesPattern(7, 0);
                    break;
                case SelectKindType.CosInnerShoesPtn02:
                    UpdateClothesPattern(7, 1);
                    break;
                case SelectKindType.CosInnerShoesPtn03:
                    UpdateClothesPattern(7, 2);
                    break;
                case SelectKindType.CosInnerShoesPtn04:
                    UpdateClothesPattern(7, 3);
                    break;
                case SelectKindType.CosInnerShoesEmblem:
                    Clothes.parts[7].emblemeId = id;
                    SetClothes.parts[7].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(7, id);
                    break;
                case SelectKindType.CosOuterShoes:
                    Clothes.parts[8].id = id;
                    SetClothes.parts[8].id = id;
                    selectedCharacter.ChangeClothesShoes(1, id, true);
                    break;
                case SelectKindType.CosOuterShoesPtn01:
                    UpdateClothesPattern(8, 0);
                    break;
                case SelectKindType.CosOuterShoesPtn02:
                    UpdateClothesPattern(8, 1);
                    break;
                case SelectKindType.CosOuterShoesPtn03:
                    UpdateClothesPattern(8, 2);
                    break;
                case SelectKindType.CosOuterShoesPtn04:
                    UpdateClothesPattern(8, 3);
                    break;
                case SelectKindType.CosOuterShoesEmblem:
                    Clothes.parts[8].emblemeId = id;
                    SetClothes.parts[8].emblemeId = id;
                    selectedCharacter.ChangeCustomEmblem(8, id);
                    break;
                case SelectKindType.HairGloss:
                    selectedCharacter.fileHair.glossId = id;
                    selectedCharacter.LoadHairGlossMask();
                    selectedCharacter.ChangeSettingHairGlossMaskAll();
                    break;
                case SelectKindType.HeadType:
                    selectedCharacter.fileFace.headId = id;
                    selectedCharacter.ChangeHead(id, true);
                    break;
                case SelectKindType.CosTopEmblem2:
                    Clothes.parts[0].emblemeId2 = id;
                    break;
                case SelectKindType.CosBotEmblem2:
                    Clothes.parts[1].emblemeId2 = id;
                    break;
                case SelectKindType.CosBraEmblem2:
                    Clothes.parts[2].emblemeId2 = id;
                    break;
                case SelectKindType.CosShortsEmblem2:
                    Clothes.parts[3].emblemeId2 = id;
                    break;
                case SelectKindType.CosGlovesEmblem2:
                    Clothes.parts[4].emblemeId2 = id;
                    break;
                case SelectKindType.CosPanstEmblem2:
                    Clothes.parts[5].emblemeId2 = id;
                    break;
                case SelectKindType.CosSocksEmblem2:
                    Clothes.parts[6].emblemeId2 = id;
                    break;
                case SelectKindType.CosInnerShoesEmblem2:
                    Clothes.parts[7].emblemeId2 = id;
                    break;
                case SelectKindType.CosOuterShoesEmblem2:
                    Clothes.parts[8].emblemeId2 = id;
                    break;
            }
        }

        public int GetSelected(SelectKindType type)
        {
            if (selectedCharacter == null)
                return 0;

            switch (type)
            {
                case SelectKindType.FaceDetail:
                    return selectedCharacter.fileFace.detailId;
                case SelectKindType.Eyebrow:
                    return selectedCharacter.fileFace.eyebrowId;
                case SelectKindType.EyelineUp:
                    return selectedCharacter.fileFace.eyelineUpId;
                case SelectKindType.EyelineDown:
                    return selectedCharacter.fileFace.eyelineDownId;
                case SelectKindType.EyeWGrade:
                    return selectedCharacter.fileFace.whiteId;
                case SelectKindType.EyeHLUp:
                    return selectedCharacter.fileFace.hlUpId;
                case SelectKindType.EyeHLDown:
                    return selectedCharacter.fileFace.hlDownId;
                case SelectKindType.Pupil:
                    return selectedCharacter.fileFace.pupil[0].id;
                case SelectKindType.PupilGrade:
                    return selectedCharacter.fileFace.pupil[0].gradMaskId;
                case SelectKindType.Nose:
                    return selectedCharacter.fileFace.noseId;
                case SelectKindType.Lipline:
                    return selectedCharacter.fileFace.lipLineId;
                case SelectKindType.Mole:
                    return selectedCharacter.fileFace.moleId;
                case SelectKindType.Eyeshadow:
                    return selectedCharacter.fileFace.baseMakeup.eyeshadowId;
                case SelectKindType.Cheek:
                    return selectedCharacter.fileFace.baseMakeup.cheekId;
                case SelectKindType.Lip:
                    return selectedCharacter.fileFace.baseMakeup.lipId;
                case SelectKindType.FacePaint01:
                    return selectedCharacter.fileFace.baseMakeup.paintId[0];
                case SelectKindType.FacePaint02:
                    return selectedCharacter.fileFace.baseMakeup.paintId[1];
                case SelectKindType.BodyDetail:
                    return selectedCharacter.fileBody.detailId;
                case SelectKindType.Nip:
                    return selectedCharacter.fileBody.nipId;
                case SelectKindType.Underhair:
                    return selectedCharacter.fileBody.underhairId;
                case SelectKindType.Sunburn:
                    return selectedCharacter.fileBody.sunburnId;
                case SelectKindType.BodyPaint01:
                    return selectedCharacter.fileBody.paintId[0];
                case SelectKindType.BodyPaint02:
                    return selectedCharacter.fileBody.paintId[1];
                case SelectKindType.BodyPaint01Layout:
                    return selectedCharacter.fileBody.paintLayoutId[0];
                case SelectKindType.BodyPaint02Layout:
                    return selectedCharacter.fileBody.paintLayoutId[1];
                case SelectKindType.HairBack:
                    return selectedCharacter.fileHair.parts[0].id;
                case SelectKindType.HairFront:
                    return selectedCharacter.fileHair.parts[1].id;
                case SelectKindType.HairSide:
                    return selectedCharacter.fileHair.parts[2].id;
                case SelectKindType.HairExtension:
                    return selectedCharacter.fileHair.parts[3].id;
                case SelectKindType.CosTop:
                    return Clothes.parts[0].id;
                case SelectKindType.CosSailor01:
                    return Clothes.subPartsId[0];
                case SelectKindType.CosSailor02:
                    return Clothes.subPartsId[1];
                case SelectKindType.CosSailor03:
                    return Clothes.subPartsId[2];
                case SelectKindType.CosJacket01:
                    return Clothes.subPartsId[0];
                case SelectKindType.CosJacket02:
                    return Clothes.subPartsId[1];
                case SelectKindType.CosJacket03:
                    return Clothes.subPartsId[2];
                case SelectKindType.CosTopPtn01:
                    return Clothes.parts[0].colorInfo[0].pattern;
                case SelectKindType.CosTopPtn02:
                    return Clothes.parts[0].colorInfo[1].pattern;
                case SelectKindType.CosTopPtn03:
                    return Clothes.parts[0].colorInfo[2].pattern;
                case SelectKindType.CosTopPtn04:
                    return Clothes.parts[0].colorInfo[3].pattern;
                case SelectKindType.CosTopEmblem:
                    return Clothes.parts[0].emblemeId;
                case SelectKindType.CosBot:
                    return Clothes.parts[1].id;
                case SelectKindType.CosBotPtn01:
                    return Clothes.parts[1].colorInfo[0].pattern;
                case SelectKindType.CosBotPtn02:
                    return Clothes.parts[1].colorInfo[1].pattern;
                case SelectKindType.CosBotPtn03:
                    return Clothes.parts[1].colorInfo[2].pattern;
                case SelectKindType.CosBotPtn04:
                    return Clothes.parts[1].colorInfo[3].pattern;
                case SelectKindType.CosBotEmblem:
                    return Clothes.parts[1].emblemeId;
                case SelectKindType.CosBra:
                    return Clothes.parts[2].id;
                case SelectKindType.CosBraPtn01:
                    return Clothes.parts[2].colorInfo[0].pattern;
                case SelectKindType.CosBraPtn02:
                    return Clothes.parts[2].colorInfo[1].pattern;
                case SelectKindType.CosBraPtn03:
                    return Clothes.parts[2].colorInfo[2].pattern;
                case SelectKindType.CosBraPtn04:
                    return Clothes.parts[2].colorInfo[3].pattern;
                case SelectKindType.CosBraEmblem:
                    return Clothes.parts[2].emblemeId;
                case SelectKindType.CosShorts:
                    return Clothes.parts[3].id;
                case SelectKindType.CosShortsPtn01:
                    return Clothes.parts[3].colorInfo[0].pattern;
                case SelectKindType.CosShortsPtn02:
                    return Clothes.parts[3].colorInfo[1].pattern;
                case SelectKindType.CosShortsPtn03:
                    return Clothes.parts[3].colorInfo[2].pattern;
                case SelectKindType.CosShortsPtn04:
                    return Clothes.parts[3].colorInfo[3].pattern;
                case SelectKindType.CosShortsEmblem:
                    return Clothes.parts[3].emblemeId;
                case SelectKindType.CosGloves:
                    return Clothes.parts[4].id;
                case SelectKindType.CosGlovesPtn01:
                    return Clothes.parts[4].colorInfo[0].pattern;
                case SelectKindType.CosGlovesPtn02:
                    return Clothes.parts[4].colorInfo[1].pattern;
                case SelectKindType.CosGlovesPtn03:
                    return Clothes.parts[4].colorInfo[2].pattern;
                case SelectKindType.CosGlovesPtn04:
                    return Clothes.parts[4].colorInfo[3].pattern;
                case SelectKindType.CosGlovesEmblem:
                    return Clothes.parts[4].emblemeId;
                case SelectKindType.CosPanst:
                    return Clothes.parts[5].id;
                case SelectKindType.CosPanstPtn01:
                    return Clothes.parts[5].colorInfo[0].pattern;
                case SelectKindType.CosPanstPtn02:
                    return Clothes.parts[5].colorInfo[1].pattern;
                case SelectKindType.CosPanstPtn03:
                    return Clothes.parts[5].colorInfo[2].pattern;
                case SelectKindType.CosPanstPtn04:
                    return Clothes.parts[5].colorInfo[3].pattern;
                case SelectKindType.CosPanstEmblem:
                    return Clothes.parts[5].emblemeId;
                case SelectKindType.CosSocks:
                    return Clothes.parts[6].id;
                case SelectKindType.CosSocksPtn01:
                    return Clothes.parts[6].colorInfo[0].pattern;
                case SelectKindType.CosSocksPtn02:
                    return Clothes.parts[6].colorInfo[1].pattern;
                case SelectKindType.CosSocksPtn03:
                    return Clothes.parts[6].colorInfo[2].pattern;
                case SelectKindType.CosSocksPtn04:
                    return Clothes.parts[6].colorInfo[3].pattern;
                case SelectKindType.CosSocksEmblem:
                    return Clothes.parts[6].emblemeId;
                case SelectKindType.CosInnerShoes:
                    return Clothes.parts[7].id;
                case SelectKindType.CosInnerShoesPtn01:
                    return Clothes.parts[7].colorInfo[0].pattern;
                case SelectKindType.CosInnerShoesPtn02:
                    return Clothes.parts[7].colorInfo[1].pattern;
                case SelectKindType.CosInnerShoesPtn03:
                    return Clothes.parts[7].colorInfo[2].pattern;
                case SelectKindType.CosInnerShoesPtn04:
                    return Clothes.parts[7].colorInfo[3].pattern;
                case SelectKindType.CosInnerShoesEmblem:
                    return Clothes.parts[7].emblemeId;
                case SelectKindType.CosOuterShoes:
                    return Clothes.parts[8].id;
                case SelectKindType.CosOuterShoesPtn01:
                    return Clothes.parts[8].colorInfo[0].pattern;
                case SelectKindType.CosOuterShoesPtn02:
                    return Clothes.parts[8].colorInfo[1].pattern;
                case SelectKindType.CosOuterShoesPtn03:
                    return Clothes.parts[8].colorInfo[2].pattern;
                case SelectKindType.CosOuterShoesPtn04:
                    return Clothes.parts[8].colorInfo[3].pattern;
                case SelectKindType.CosOuterShoesEmblem:
                    return Clothes.parts[8].emblemeId;
                case SelectKindType.HairGloss:
                    return selectedCharacter.fileHair.glossId;
                case SelectKindType.HeadType:
                    return selectedCharacter.fileFace.headId;
                case SelectKindType.CosTopEmblem2:
                    return Clothes.parts[0].emblemeId2;
                case SelectKindType.CosBotEmblem2:
                    return Clothes.parts[1].emblemeId2;
                case SelectKindType.CosBraEmblem2:
                    return Clothes.parts[2].emblemeId2;
                case SelectKindType.CosShortsEmblem2:
                    return Clothes.parts[3].emblemeId2;
                case SelectKindType.CosGlovesEmblem2:
                    return Clothes.parts[4].emblemeId2;
                case SelectKindType.CosPanstEmblem2:
                    return Clothes.parts[5].emblemeId2;
                case SelectKindType.CosSocksEmblem2:
                    return Clothes.parts[6].emblemeId2;
                case SelectKindType.CosInnerShoesEmblem2:
                    return Clothes.parts[7].emblemeId2;
                case SelectKindType.CosOuterShoesEmblem2:
                    return Clothes.parts[8].emblemeId2;
                default:
                    return 0;
            }
        }

        public bool IsCategoryEdited(SelectedTab tab)
        {
            switch (tab)
            {
                case SelectedTab.Body:
                    return OriginalBodyColors.Any(x => x.Value.Value != x.Value.OriginalValue)
                        || OriginalBustValues.Any(x => Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f)
                        || OriginalBodyShapeValues.Any(x => Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f);
                case SelectedTab.Face:
                    return OriginalFaceShapeValues.Any(x => Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f)
                        || OriginalFaceColors.Any(x => x.Value.Value != x.Value.OriginalValue);
                case SelectedTab.Hair:
                    return OriginalHairColors.Any(x => x.Value.Value != x.Value.OriginalValue);
                case SelectedTab.Clothes:
                    return OriginalClothingColors.Any(x => x.Value.Value != x.Value.OriginalValue);
            }
            return false;
        }

        private new void OnDestroy()
        {
            allControllers.Remove(ChaControl);
        }
    }

    #region Storage classes
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
    public struct ClothingColors
    {
        [Key("OutfitSlot")]
        public int OutfitSlot { get; set; }
        [Key("ClothingKind")]
        public int ClothingKind { get; set; }
        [Key("ColorNr")]
        public int ColorNr { get; set; }
        [Key("SlotNr")]
        public int SlotNr { get; set; }

        public ClothingColors(int outfitSlot, int clothingKind, int colorNr, int slotNr)
        {
            OutfitSlot = outfitSlot;
            ClothingKind = clothingKind;
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
    #endregion
}
