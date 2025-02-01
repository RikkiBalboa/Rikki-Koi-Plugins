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
using static MaterialEditorAPI.MaterialAPI;

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
            //ChaControl.AddUpdateCMBodyTexFlags(inpBase, inpSub, inpPaint01, inpPaint02, inpSunburn);
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
                    ChaControl.ChangeSettingWhiteOfEye(false, true);
                    break;
                case FaceColor.ScleraColor2:
                    ChaControl.fileFace.whiteSubColor = color;
                    ChaControl.ChangeSettingWhiteOfEye(false, true);
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
