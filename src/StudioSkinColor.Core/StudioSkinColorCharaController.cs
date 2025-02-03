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
using static Illusion.Utils;

namespace Plugins
{
    internal class StudioSkinColorCharaController : CharaCustomFunctionController
    {
        internal static readonly Dictionary<ChaControl, StudioSkinColorCharaController> allControllers = new Dictionary<ChaControl, StudioSkinColorCharaController>();

        #region Save Lists
        private Dictionary<ClothingStorageKey, ColorStorage> OriginalClothingColors = new Dictionary<ClothingStorageKey, ColorStorage>();
        private Dictionary<AccessoryStorageKey, ColorStorage> OriginalAccessoryColors = new Dictionary<AccessoryStorageKey, ColorStorage>();
        private Dictionary<ColorType, ColorStorage> OriginalColors = new Dictionary<ColorType, ColorStorage>();
        private Dictionary<FloatType, FloatStorage> OriginalFloats = new Dictionary<FloatType, FloatStorage>();
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

            if (OriginalAccessoryColors.Count > 0)
                data.data.Add(nameof(OriginalAccessoryColors), MessagePackSerializer.Serialize(OriginalAccessoryColors));
            else
                data.data.Add(nameof(OriginalAccessoryColors), null);

            if (OriginalColors.Count > 0)
                data.data.Add(nameof(OriginalColors), MessagePackSerializer.Serialize(OriginalColors));
            else
                data.data.Add(nameof(OriginalColors), null);

            if (OriginalFloats.Count > 0)
                data.data.Add(nameof(OriginalFloats), MessagePackSerializer.Serialize(OriginalFloats));
            else
                data.data.Add(nameof(OriginalFloats), null);

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
            OriginalColors.Clear();
            OriginalFloats.Clear();
            OriginalBodyShapeValues.Clear();
            OriginalFaceShapeValues.Clear();

            var data = GetExtendedData();
            if (data == null)
                return;

            if (data.data.TryGetValue(nameof(OriginalClothingColors), out var originalClothingColors) && originalClothingColors != null)
                OriginalClothingColors = MessagePackSerializer.Deserialize<Dictionary<ClothingStorageKey, ColorStorage>>((byte[])originalClothingColors);

            if (data.data.TryGetValue(nameof(OriginalAccessoryColors), out var originalAccessoryColors) && originalAccessoryColors != null)
                OriginalAccessoryColors = MessagePackSerializer.Deserialize<Dictionary<AccessoryStorageKey, ColorStorage>>((byte[])originalAccessoryColors);

            if (data.data.TryGetValue(nameof(OriginalColors), out var originalHairColors) && originalHairColors != null)
                OriginalColors = MessagePackSerializer.Deserialize<Dictionary<ColorType, ColorStorage>>((byte[])originalHairColors);

            if (data.data.TryGetValue(nameof(OriginalFloats), out var originalBustValues) && originalBustValues != null)
                OriginalFloats = MessagePackSerializer.Deserialize<Dictionary<FloatType, FloatStorage>>((byte[])originalBustValues);

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

        public void UpdateColorProperty(Color color, ColorType textureColor)
        {
            if (!OriginalColors.ContainsKey(textureColor))
                OriginalColors[textureColor] = new ColorStorage(GetColorPropertyValue(textureColor), color);
            else
                OriginalColors[textureColor].Value = color;

            switch (textureColor)
            {
                case ColorType.SkinMain:
                    ChaControl.fileBody.skinMainColor = color;
                    UpdateFaceTextures(inpBase: true);
                    UpdateBodyTextures(inpBase: true);
                    break;
                case ColorType.SkinSub:
                    ChaControl.fileBody.skinSubColor = color;
                    UpdateFaceTextures(inpSub: true);
                    UpdateBodyTextures(inpSub: true);
                    break;
                case ColorType.SkinTan:
                    ChaControl.fileBody.sunburnColor = color;
                    UpdateBodyTextures(inpSunburn: true);
                    break;
                case ColorType.NippleColor:
                    ChaControl.fileBody.nipColor = color;
                    ChaControl.ChangeSettingNipColor();
                    break;
                case ColorType.NailColor:
                    ChaControl.fileBody.nailColor = color;
                    UpdateBodyTextures(inpNail: true);
                    break;
                case ColorType.PubicHairColor:
                    ChaControl.fileBody.underhairColor = color;
                    ChaControl.ChangeSettingUnderhairColor();
                    break;
                case ColorType.EyebrowColor:
                    ChaControl.fileFace.eyebrowColor = color;
                    ChaControl.ChangeSettingEyebrowColor();
                    break;
                case ColorType.EyelineColor:
                    ChaControl.fileFace.eyelineColor = color;
                    ChaControl.ChangeSettingEyelineColor();
                    break;
                case ColorType.ScleraColor1:
                    ChaControl.fileFace.whiteBaseColor = color;
                    ChaControl.ChangeSettingWhiteOfEye(true, true);
                    break;
                case ColorType.ScleraColor2:
                    ChaControl.fileFace.whiteSubColor = color;
                    ChaControl.ChangeSettingWhiteOfEye(true, true);
                    break;
                case ColorType.UpperHighlightColor:
                    ChaControl.fileFace.hlUpColor = color;
                    ChaControl.ChangeSettingEyeHiUpColor();
                    break;
                case ColorType.LowerHightlightColor:
                    ChaControl.fileFace.hlDownColor = color;
                    ChaControl.ChangeSettingEyeHiDownColor();
                    break;
                case ColorType.EyeColor1Left:
                    ChaControl.fileFace.pupil[0].baseColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, false);
                    break;
                case ColorType.EyeColor2Left:
                    ChaControl.fileFace.pupil[0].subColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, false);
                    break;
                case ColorType.EyeColor1Right:
                    ChaControl.fileFace.pupil[1].baseColor = color;
                    ChaControl.ChangeSettingEyeR(true, true, false);
                    break;
                case ColorType.EyeColor2Right:
                    ChaControl.fileFace.pupil[2].subColor = color;
                    ChaControl.ChangeSettingEyeR(true, true, false);
                    break;
                case ColorType.LipLineColor:
                    ChaControl.fileFace.lipLineColor = color;
                    UpdateFaceTextures(inpLipLine: true);
                    break;
                case ColorType.EyeShadowColor:
                    ChaControl.fileFace.baseMakeup.eyeshadowColor = color;
                    ChaControl.ChangeSettingEyeShadowColor();
                    break;
                case ColorType.CheekColor:
                    ChaControl.fileFace.baseMakeup.cheekColor = color;
                    UpdateFaceTextures(inpCheek: true);
                    break;
                case ColorType.LipColor:
                    ChaControl.fileFace.baseMakeup.lipColor = color;
                    ChaControl.ChangeSettingLipColor();
                    break;
                case ColorType.HairBase:
                    for (int i = 0; i < 4; i++)
                    {
                        ChaControl.fileHair.parts[i].baseColor = color;
                        ChaControl.ChangeSettingHairColor(i, true, true, true);
                    }
                    break;
                case ColorType.HairStart:
                    for (int i = 0; i < 4; i++)
                    {
                        ChaControl.fileHair.parts[i].startColor = color;
                        ChaControl.ChangeSettingHairColor(i, true, true, true);
                    }
                    break;
                case ColorType.HairEnd:
                    for (int i = 0; i < 4; i++)
                    {
                        ChaControl.fileHair.parts[i].endColor = color;
                        ChaControl.ChangeSettingHairColor(i, true, true, true);
                    }
                    break;
#if KKS
                case ColorType.HairGloss:
                    for (int i = 0; i < 4; i++)
                    {
                        ChaControl.fileHair.parts[i].glossColor = color;
                        ChaControl.ChangeSettingHairGlossColor(i);
                    }
                    break;
#endif
                case ColorType.Eyebrow:
                    ChaControl.fileFace.eyebrowColor = color;
                    ChaControl.ChangeSettingEyebrowColor();
                    break;
            }
        }

        public Color GetColorPropertyValue(ColorType color)
        {
            switch (color)
            {
                case ColorType.SkinMain:
                    return ChaControl.fileBody.skinMainColor;
                case ColorType.SkinSub:
                    return ChaControl.fileBody.skinSubColor;
                case ColorType.SkinTan:
                    return ChaControl.fileBody.sunburnColor;
                case ColorType.NippleColor:
                    return ChaControl.fileBody.nipColor;
                case ColorType.NailColor:
                    return ChaControl.fileBody.nailColor;
                case ColorType.PubicHairColor:
                    return ChaControl.fileBody.underhairColor;
                case ColorType.EyebrowColor:
                    return ChaControl.fileFace.eyebrowColor;
                case ColorType.EyelineColor:
                    return ChaControl.fileFace.eyelineColor;
                case ColorType.ScleraColor1:
                    return ChaControl.fileFace.whiteBaseColor;
                case ColorType.ScleraColor2:
                    return ChaControl.fileFace.whiteSubColor;
                case ColorType.UpperHighlightColor:
                    return ChaControl.fileFace.hlUpColor;
                case ColorType.LowerHightlightColor:
                    return ChaControl.fileFace.hlDownColor;
                case ColorType.EyeColor1Left:
                    return ChaControl.fileFace.pupil[0].baseColor;
                case ColorType.EyeColor2Left:
                    return ChaControl.fileFace.pupil[0].subColor;
                case ColorType.EyeColor1Right:
                    return ChaControl.fileFace.pupil[1].baseColor;
                case ColorType.EyeColor2Right:
                    return ChaControl.fileFace.pupil[1].subColor;
                case ColorType.LipLineColor:
                    return ChaControl.fileFace.lipLineColor;
                case ColorType.EyeShadowColor:
                    return ChaControl.fileFace.baseMakeup.eyeshadowColor;
                case ColorType.CheekColor:
                    return ChaControl.fileFace.baseMakeup.cheekColor;
                case ColorType.LipColor:
                    return ChaControl.fileFace.baseMakeup.lipColor;
                case ColorType.HairBase:
                    return ChaControl.fileHair.parts[0].baseColor;
                case ColorType.HairStart:
                    return ChaControl.fileHair.parts[0].startColor;
                case ColorType.HairEnd:
                    return ChaControl.fileHair.parts[0].endColor;
#if KKS
                case ColorType.HairGloss:
                    return ChaControl.fileHair.parts[0].glossColor;
#endif
                case ColorType.Eyebrow:
                    return ChaControl.fileFace.eyebrowColor;
            }
            return Color.white;
        }

        public void ResetColorProperty(ColorType colorType)
        {
            if (OriginalColors.TryGetValue(colorType, out var color))
                UpdateColorProperty(color.OriginalValue, colorType);
        }

        public Color GetOriginalColorPropertyValue(ColorType colorType)
        {
            if (OriginalColors.TryGetValue(colorType, out var color))
                return color.OriginalValue;
            return GetColorPropertyValue(colorType);

        }
        #endregion

        #region Bust
        public void SetFloatTypeValue(float value, FloatType floatType)
        {
            if (!OriginalFloats.ContainsKey(floatType))
                OriginalFloats[floatType] = new FloatStorage(GetFloatValue(floatType), value);
            else
                OriginalFloats[floatType].Value = value;

            switch (floatType)
            {
                case FloatType.SkinTypeStrenth:
                    ChaFileControl.custom.body.detailPower = value;
                    ChaControl.ChangeSettingBodyDetailPower();
                    break;
                case FloatType.SkinGloss:
                    ChaFileControl.custom.body.skinGlossPower = value;
                    ChaControl.ChangeSettingSkinGlossPower();
                    break;
                case FloatType.DisplaySkinDetailLines:
                    ChaFileControl.custom.body.drawAddLine = Convert.ToBoolean(value);
                    ChaControl.VisibleAddBodyLine();
                    break;
                case FloatType.Softness:
                    ChaControl.ChangeBustSoftness(value);
                    break;
                case FloatType.Weight:
                    ChaControl.ChangeBustGravity(value);
                    break;
                case FloatType.NippleGloss:
                    ChaFileControl.custom.body.nipGlossPower = value;
                    ChaControl.ChangeSettingNipGlossPower();
                    break;
                case FloatType.NailGloss:
                    ChaFileControl.custom.body.nailGlossPower = value;
                    ChaControl.ChangeSettingNailGlossPower();
                    break;
                case FloatType.FaceOverlayStrenth:
                    ChaFileControl.custom.face.detailPower = value;
                    ChaControl.ChangeSettingFaceDetailPower();
                    break;
                case FloatType.CheekGloss:
                    ChaFileControl.custom.face.cheekGlossPower = value;
                    ChaControl.ChangeSettingCheekGlossPower();
                    break;
                case FloatType.UpperHighlightVertical:
                    ChaFileControl.custom.face.hlUpY = value;
                    ChaControl.ChangeSettingEyeHLUpPosY();
                    break;
#if KKS
                case FloatType.UpperHighlightHorizontal:
                    ChaFileControl.custom.face.hlUpX = value;
                    ChaControl.ChangeSettingEyeHLUpPosX();
                    break;
#endif
                case FloatType.LowerHightlightVertical:
                    ChaFileControl.custom.face.hlDownY = value;
                    ChaControl.ChangeSettingEyeHLDownPosY();
                    break;
#if KKS
                case FloatType.LowerHightlightHorizontal:
                    ChaFileControl.custom.face.hlDownX = value;
                    ChaControl.ChangeSettingEyeHLDownPosX();
                    break;
#endif
                case FloatType.IrisSpacing:
                    ChaFileControl.custom.face.pupilX = value;
                    ChaControl.ChangeSettingEyePosX();
                    break;
                case FloatType.IrisVerticalPosition:
                    ChaFileControl.custom.face.pupilY = value;
                    ChaControl.ChangeSettingEyePosY();
                    break;
                case FloatType.IrisWidth:
                    ChaFileControl.custom.face.pupilWidth = value;
                    ChaControl.ChangeSettingEyeScaleWidth();
                    break;
                case FloatType.IrisHeight:
                    ChaFileControl.custom.face.pupilHeight = value;
                    ChaControl.ChangeSettingEyeScaleHeight();
                    break;
                case FloatType.EyeGradientStrenth:
                    ChaFileControl.custom.face.pupil[0].gradBlend = value;
                    ChaFileControl.custom.face.pupil[1].gradBlend = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientVertical:
                    ChaFileControl.custom.face.pupil[0].gradOffsetY = value;
                    ChaFileControl.custom.face.pupil[1].gradOffsetY = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientSize:
                    ChaFileControl.custom.face.pupil[0].gradScale = value;
                    ChaFileControl.custom.face.pupil[1].gradScale = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.LipGloss:
                    ChaFileControl.custom.face.lipGlossPower = value;
                    ChaControl.ChangeSettingLipGlossPower();
                    break;
            }
        }

        public void ResetFloatTypeValue(FloatType floatType)
        {
            if (OriginalFloats.TryGetValue(floatType, out var value))
                SetFloatTypeValue(value.OriginalValue, floatType);
        }

        public float GetOriginalFloatValue(FloatType floatType)
        {
            if (OriginalFloats.TryGetValue(floatType, out var original))
                return original.OriginalValue;
            return GetFloatValue(floatType);
        }

        public float GetFloatValue(FloatType floatType)
        {
            switch (floatType)
            {
                case FloatType.SkinTypeStrenth:
                    return ChaFileControl.custom.body.detailPower;
                case FloatType.SkinGloss:
                    return ChaFileControl.custom.body.skinGlossPower;
                case FloatType.DisplaySkinDetailLines:
                    return Convert.ToSingle(ChaFileControl.custom.body.drawAddLine);
                case FloatType.Softness:
                    return ChaControl.fileBody.bustSoftness;
                case FloatType.Weight:
                    return ChaControl.fileBody.bustWeight;
                case FloatType.NippleGloss:
                    return ChaFileControl.custom.body.nipGlossPower;
                case FloatType.NailGloss:
                    return ChaFileControl.custom.body.nailGlossPower;
                case FloatType.FaceOverlayStrenth:
                    return ChaFileControl.custom.face.detailPower;
                case FloatType.CheekGloss:
                    return ChaFileControl.custom.face.cheekGlossPower;
                case FloatType.UpperHighlightVertical:
                    return ChaFileControl.custom.face.hlUpY;
#if KKS
                case FloatType.UpperHighlightHorizontal:
                    return ChaFileControl.custom.face.hlUpX;
#endif
                case FloatType.LowerHightlightVertical:
                    return ChaFileControl.custom.face.hlDownY;
#if KKS
                case FloatType.LowerHightlightHorizontal:
                    return ChaFileControl.custom.face.hlDownX;
#endif
                case FloatType.IrisSpacing:
                    return ChaFileControl.custom.face.pupilX;
                case FloatType.IrisVerticalPosition:
                    return ChaFileControl.custom.face.pupilY;
                case FloatType.IrisWidth:
                    return ChaFileControl.custom.face.pupilWidth;
                case FloatType.IrisHeight:
                    return ChaFileControl.custom.face.pupilHeight;
                case FloatType.EyeGradientStrenth:
                    return ChaFileControl.custom.face.pupil[0].gradBlend;
                case FloatType.EyeGradientVertical:
                    return ChaFileControl.custom.face.pupil[0].gradOffsetY;
                case FloatType.EyeGradientSize:
                    return ChaFileControl.custom.face.pupil[0].gradScale;
                case FloatType.LipGloss:
                    return ChaFileControl.custom.face.lipGlossPower;
                default:
                    return 0f;
            }
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
            isEdited |= OriginalColors.Any(x =>
                GetCategoryTypes(x.Key) == "Body"
                && GetCategoryTypes(x.Key, true) == category
                && x.Value.OriginalValue != x.Value.Value
            );
            isEdited |= OriginalFloats.Any(x =>
                GetCategoryTypes(x.Key) == "Body"
                && GetCategoryTypes(x.Key, true) == category
                && Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f
            );

            return isEdited;
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
            isEdited |= OriginalColors.Any(x =>
                GetCategoryTypes(x.Key) == "Face"
                && GetCategoryTypes(x.Key, true) == category
                && x.Value.OriginalValue != x.Value.Value
            );
            isEdited |= OriginalFloats.Any(x =>
                GetCategoryTypes(x.Key) == "Face"
                && GetCategoryTypes(x.Key, true) == category
                && Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f
            );
            return isEdited;
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

        public void SetClothingColor(int kind, int colorNr, Color color, int slotNr = -1, bool isPattern = false)
        {
            var MEController = MaterialEditorPlugin.GetCharaController(ChaControl);
            if (MEController != null)
            {
                MEController.CustomClothesOverride = true;
                MEController.RefreshClothesMainTex();
            }

            var clothingColors = new ClothingStorageKey(CurrentOutfitSlot, kind, colorNr, slotNr, isPattern);
            if (!OriginalClothingColors.Any(x => x.Key.Compare(CurrentOutfitSlot, kind, colorNr, slotNr, isPattern)))
                OriginalClothingColors[clothingColors] = new ColorStorage(GetClothingColor(kind, colorNr, slotNr, isPattern), color);
            else
                OriginalClothingColors[clothingColors].Value = color;

            if (isPattern)
            {
                Clothes.parts[kind].colorInfo[colorNr].patternColor = color;
                SetClothes.parts[kind].colorInfo[colorNr].patternColor = color;
                if (!IsMultiPartTop(kind))
                    ChaControl.ChangeCustomClothes(true, kind, true, true, true, true, true);
                else
                    for (int i = 0; i < Clothes.subPartsId.Length; i++)
                        ChaControl.ChangeCustomClothes(false, i, true, true, true, true, true);
            }
            else if (slotNr < 0)
            {
                Clothes.parts[kind].colorInfo[colorNr].baseColor = color;
                SetClothes.parts[kind].colorInfo[colorNr].baseColor = color;
                if (!IsMultiPartTop(kind))
                    ChaControl.ChangeCustomClothes(true, kind, true, true, true, true, true);
                else
                    for (int i = 0; i < Clothes.subPartsId.Length; i++)
                        ChaControl.ChangeCustomClothes(false, i, true, true, true, true, true);
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

        public bool ClothingUsesPattern(int kind, int pattern)
        {
            return GetSelected(KindToSelectKind(kind, pattern)) > 0;
        }

        public Color GetClothingColor(int kind, int colorNr, int slotNr = -1, bool isPattern = false)
        {
            if (isPattern)
                return Clothes.parts[kind].colorInfo[colorNr].patternColor;
            else if (slotNr < 0)
                return Clothes.parts[kind].colorInfo[colorNr].baseColor;
            return Accessories.parts[slotNr].color[colorNr];
        }

        public void ResetClothingColor(int kind, int colorNr, int slotNr, bool isPattern = false)
        {
            var clothingColors = new ClothingStorageKey(CurrentOutfitSlot, kind, colorNr, slotNr, isPattern);
            if (OriginalClothingColors.TryGetValue(clothingColors, out var color))
                SetClothingColor(kind, colorNr, color.OriginalValue, slotNr, isPattern);
        }

        public Color GetOriginalClothingColor(int kind, int colorNr, int slotNr, bool isPattern = false)
        {
            var clothingColors = new ClothingStorageKey(CurrentOutfitSlot, kind, colorNr, slotNr, isPattern);
            if (OriginalClothingColors.TryGetValue(clothingColors, out var color))
                return color.OriginalValue;
            return GetClothingColor(kind, colorNr, slotNr, isPattern);
        }

        public float GetPatternValue(int kind, int colorNr, PatternValue patternValue)
        {
            switch (patternValue)
            {
#if KKS
                case PatternValue.Horizontal:
                    return Clothes.parts[kind].colorInfo[colorNr].offset.x;
                case PatternValue.Vertical:
                    return Clothes.parts[kind].colorInfo[colorNr].offset.y;
                case PatternValue.Rotation:
                    return Clothes.parts[kind].colorInfo[colorNr].rotate;
#endif
                case PatternValue.Width:
                    return Clothes.parts[kind].colorInfo[colorNr].tiling.x;
                case PatternValue.Height:
                    return Clothes.parts[kind].colorInfo[colorNr].tiling.y;
                default:
                    return 0f;
            }
        }

        public void SetPatternValue(int kind, int colorNr, PatternValue patternValue, float value)
        {
            Vector2 vector;
            switch (patternValue)
            {
#if KKS
                case PatternValue.Horizontal:
                    vector = Clothes.parts[kind].colorInfo[colorNr].offset;
                    vector.x = value;
                    Clothes.parts[kind].colorInfo[colorNr].offset = vector;
                    SetClothes.parts[kind].colorInfo[colorNr].offset = vector;
                    break;
                case PatternValue.Vertical:
                    vector = Clothes.parts[kind].colorInfo[colorNr].offset;
                    vector.y = value;
                    Clothes.parts[kind].colorInfo[colorNr].offset = vector;
                    SetClothes.parts[kind].colorInfo[colorNr].offset = vector;
                    break;
                case PatternValue.Rotation:
                    Clothes.parts[kind].colorInfo[colorNr].rotate = value;
                    SetClothes.parts[kind].colorInfo[colorNr].rotate = value;
                    break;
#endif
                case PatternValue.Width:
                    vector = Clothes.parts[kind].colorInfo[colorNr].tiling;
                    vector.x = value;
                    Clothes.parts[kind].colorInfo[colorNr].tiling = vector;
                    SetClothes.parts[kind].colorInfo[colorNr].tiling = vector;
                    break;
                case PatternValue.Height:
                    vector = Clothes.parts[kind].colorInfo[colorNr].tiling;
                    vector.y = value;
                    Clothes.parts[kind].colorInfo[colorNr].tiling = vector;
                    SetClothes.parts[kind].colorInfo[colorNr].tiling = vector;
                    break;
                default:
                    break;
            }

            if (!IsMultiPartTop(kind))
                ChaControl.ChangeCustomClothes(true, kind, true, true, true, true, true);
            else
                for (int i = 0; i < Clothes.subPartsId.Length; i++)
                    ChaControl.ChangeCustomClothes(false, i, true, true, true, true, true);
        }

        public bool GetHideOpt(int kind, int option)
        {
            return selectedCharacter.nowCoordinate.clothes.parts[kind].hideOpt[option];
        }

        public void SetHideOpt(int kind, int option, bool value)
        {
            if (Clothes.parts[kind].hideOpt[option] != value)
            {
                Clothes.parts[kind].hideOpt[option] = value;
                SetClothes.parts[kind].hideOpt[option] = value;
            }
        }

        public int GetClothingUsesOptParts (ChaClothesComponent component)
        {
            return Convert.ToInt32(component?.objOpt01?.Any()) + Convert.ToInt32(component?.objOpt02?.Any());
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

        #region Accessories
        public bool[] CheckAccessoryUseColor(int slotNr)
        {
            bool[] useCols = new bool[3] { false, false, false };
            var component = ChaControl.GetAccessoryComponent(slotNr);
            if (component != null)
            {
                useCols[0] = component.useColor01;
                useCols[1] = component.useColor02;
                useCols[2] = component.useColor03;
            }
            return useCols;
        }

        public Color GetAccessoryColor(int slotNr, int colorNr)
        {
            return Accessories.parts[slotNr].color[colorNr];
        }

        public void SetAccessoryColor(int slotNr, int colorNr,  Color color)
        {
            var accessoryColor = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, colorNr);
            if (!OriginalAccessoryColors.Any(x => x.Key.Compare(CurrentOutfitSlot, slotNr, colorNr)))
                OriginalAccessoryColors[accessoryColor] = new ColorStorage(GetAccessoryColor(slotNr, colorNr), color);
            else
                OriginalAccessoryColors[accessoryColor].Value = color;

            Accessories.parts[slotNr].color[colorNr] = color;
            SetAccessories.parts[slotNr].color[colorNr] = color;
            ChaControl.ChangeAccessoryColor(slotNr);
        }

        public void ResetAccessoryColor(int slotNr, int colorNr)
        {
            var accessoryColor = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, colorNr);
            if (OriginalAccessoryColors.TryGetValue(accessoryColor, out var color))
                SetAccessoryColor(slotNr, colorNr, color.OriginalValue);
        }

        public Color GetOriginalAccessoryColor(int slotNr, int colorNr)
        {
            var accessoryColor = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, colorNr);
            if (OriginalAccessoryColors.TryGetValue(accessoryColor, out var color))
                return color.OriginalValue;
            return GetAccessoryColor(slotNr, colorNr);
        }
        #endregion

        #region Category pickers
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
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: true, updateTex01: true, updateTex02: false, updateTex03: false, updateTex04: false);
                if (pattern == 1)
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: true, updateTex01: false, updateTex02: true, updateTex03: false, updateTex04: false);
                if (pattern == 2)
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: true, updateTex04: false);
                if (pattern == 3)
                    selectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: true);
            }

            void ChangeEmblem(int kind)
            {
                Clothes.parts[0].emblemeId = id;
                SetClothes.parts[0].emblemeId = id;
                selectedCharacter.ChangeCustomEmblem(0, id);
                selectedCharacter.ChangeClothesTop(SetClothes.parts[0].id, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
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
                    selectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor02:
                    Clothes.subPartsId[1] = id;
                    SetClothes.subPartsId[1] = id;
                    selectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor03:
                    Clothes.subPartsId[2] = id;
                    SetClothes.subPartsId[2] = id;
                    selectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket01:
                    Clothes.subPartsId[0] = id;
                    SetClothes.subPartsId[0] = id;
                    selectedCharacter.ChangeClothesTop(2, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket02:
                    Clothes.subPartsId[1] = id;
                    SetClothes.subPartsId[1] = id;
                    selectedCharacter.ChangeClothesTop(2, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket03:
                    Clothes.subPartsId[2] = id;
                    SetClothes.subPartsId[2] = id;
                    selectedCharacter.ChangeClothesTop(2, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
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
                    ChangeEmblem(0);
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
                    ChangeEmblem(1);
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
                    ChangeEmblem(2);
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
                    ChangeEmblem(3);
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
                    ChangeEmblem(4);
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
                    ChangeEmblem(5);
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
                    ChangeEmblem(6);
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
                    ChangeEmblem(7);
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
                    ChangeEmblem(8);
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

        public static int SelectKindToIntKind(SelectKindType type)
        {
            switch (type)
            {
                case SelectKindType.CosTop:
                case SelectKindType.CosTopPtn01:
                case SelectKindType.CosTopPtn02:
                case SelectKindType.CosTopPtn03:
                case SelectKindType.CosTopPtn04:
                case SelectKindType.CosTopEmblem:
                case SelectKindType.CosTopEmblem2:
                    return 0;
                case SelectKindType.CosBot:
                case SelectKindType.CosBotPtn01:
                case SelectKindType.CosBotPtn02:
                case SelectKindType.CosBotPtn03:
                case SelectKindType.CosBotPtn04:
                case SelectKindType.CosBotEmblem:
                case SelectKindType.CosBotEmblem2:
                    return 1;
                case SelectKindType.CosBra:
                case SelectKindType.CosBraPtn01:
                case SelectKindType.CosBraPtn02:
                case SelectKindType.CosBraPtn03:
                case SelectKindType.CosBraPtn04:
                case SelectKindType.CosBraEmblem:
                case SelectKindType.CosBraEmblem2:
                    return 2;
                case SelectKindType.CosShorts:
                case SelectKindType.CosShortsPtn01:
                case SelectKindType.CosShortsPtn02:
                case SelectKindType.CosShortsPtn03:
                case SelectKindType.CosShortsPtn04:
                case SelectKindType.CosShortsEmblem:
                case SelectKindType.CosShortsEmblem2:
                    return 3;
                case SelectKindType.CosGloves:
                case SelectKindType.CosGlovesPtn01:
                case SelectKindType.CosGlovesPtn02:
                case SelectKindType.CosGlovesPtn03:
                case SelectKindType.CosGlovesPtn04:
                case SelectKindType.CosGlovesEmblem:
                case SelectKindType.CosGlovesEmblem2:
                    return 4;
                case SelectKindType.CosPanst:
                case SelectKindType.CosPanstPtn01:
                case SelectKindType.CosPanstPtn02:
                case SelectKindType.CosPanstPtn03:
                case SelectKindType.CosPanstPtn04:
                case SelectKindType.CosPanstEmblem:
                case SelectKindType.CosPanstEmblem2:
                    return 5;
                case SelectKindType.CosSocks:
                case SelectKindType.CosSocksPtn01:
                case SelectKindType.CosSocksPtn02:
                case SelectKindType.CosSocksPtn03:
                case SelectKindType.CosSocksPtn04:
                case SelectKindType.CosSocksEmblem:
                case SelectKindType.CosSocksEmblem2:
                    return 6;
                case SelectKindType.CosInnerShoes:
                case SelectKindType.CosInnerShoesPtn01:
                case SelectKindType.CosInnerShoesPtn02:
                case SelectKindType.CosInnerShoesPtn03:
                case SelectKindType.CosInnerShoesPtn04:
                case SelectKindType.CosInnerShoesEmblem:
                case SelectKindType.CosInnerShoesEmblem2:
                    return 7;
                case SelectKindType.CosOuterShoes:
                case SelectKindType.CosOuterShoesPtn01:
                case SelectKindType.CosOuterShoesPtn02:
                case SelectKindType.CosOuterShoesPtn03:
                case SelectKindType.CosOuterShoesPtn04:
                case SelectKindType.CosOuterShoesEmblem:
                case SelectKindType.CosOuterShoesEmblem2:
                    return 8;
                default:
                    return -1;
            }
        }

        public static SelectKindType KindToSelectKind(int kind, int pattern = 0, int emblem = 0)
        {
            switch (kind)
            {
                case 0:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosBot;
                    if (pattern == 1) return SelectKindType.CosTopPtn01;
                    if (pattern == 2) return SelectKindType.CosTopPtn02;
                    if (pattern == 3) return SelectKindType.CosTopPtn03;
                    if (pattern == 4) return SelectKindType.CosTopPtn04;
                    if (emblem == 1) return SelectKindType.CosTopEmblem;
                    if (emblem == 2) return SelectKindType.CosTopEmblem;
                    break;
                case 1:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosBot;
                    if (pattern == 1) return SelectKindType.CosBotPtn01;
                    if (pattern == 2) return SelectKindType.CosBotPtn02;
                    if (pattern == 3) return SelectKindType.CosBotPtn03;
                    if (pattern == 4) return SelectKindType.CosBotPtn04;
                    if (emblem == 1) return SelectKindType.CosBotEmblem;
                    if (emblem == 2) return SelectKindType.CosBotEmblem;
                    break;
                case 2:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosBra;
                    if (pattern == 1) return SelectKindType.CosBraPtn01;
                    if (pattern == 2) return SelectKindType.CosBraPtn02;
                    if (pattern == 3) return SelectKindType.CosBraPtn03;
                    if (pattern == 4) return SelectKindType.CosBraPtn04;
                    if (emblem == 1) return SelectKindType.CosBraEmblem;
                    if (emblem == 2) return SelectKindType.CosBraEmblem;
                    break;
                case 3:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosShorts;
                    if (pattern == 1) return SelectKindType.CosShortsPtn01;
                    if (pattern == 2) return SelectKindType.CosShortsPtn02;
                    if (pattern == 3) return SelectKindType.CosShortsPtn03;
                    if (pattern == 4) return SelectKindType.CosShortsPtn04;
                    if (emblem == 1) return SelectKindType.CosShortsEmblem;
                    if (emblem == 2) return SelectKindType.CosShortsEmblem;
                    break;
                case 4:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosGloves;
                    if (pattern == 1) return SelectKindType.CosGlovesPtn01;
                    if (pattern == 2) return SelectKindType.CosGlovesPtn02;
                    if (pattern == 3) return SelectKindType.CosGlovesPtn03;
                    if (pattern == 4) return SelectKindType.CosGlovesPtn04;
                    if (emblem == 1) return SelectKindType.CosGlovesEmblem;
                    if (emblem == 2) return SelectKindType.CosGlovesEmblem;
                    break;
                case 5:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosPanst;
                    if (pattern == 1) return SelectKindType.CosPanstPtn01;
                    if (pattern == 2) return SelectKindType.CosPanstPtn02;
                    if (pattern == 3) return SelectKindType.CosPanstPtn03;
                    if (pattern == 4) return SelectKindType.CosPanstPtn04;
                    if (emblem == 1) return SelectKindType.CosPanstEmblem;
                    if (emblem == 2) return SelectKindType.CosPanstEmblem;
                    break;
                case 6:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosSocks;
                    if (pattern == 1) return SelectKindType.CosSocksPtn01;
                    if (pattern == 2) return SelectKindType.CosSocksPtn02;
                    if (pattern == 3) return SelectKindType.CosSocksPtn03;
                    if (pattern == 4) return SelectKindType.CosSocksPtn04;
                    if (emblem == 1) return SelectKindType.CosSocksEmblem;
                    if (emblem == 2) return SelectKindType.CosSocksEmblem;
                    break;
                case 7:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosInnerShoes;
                    if (pattern == 1) return SelectKindType.CosInnerShoesPtn01;
                    if (pattern == 2) return SelectKindType.CosInnerShoesPtn02;
                    if (pattern == 3) return SelectKindType.CosInnerShoesPtn03;
                    if (pattern == 4) return SelectKindType.CosInnerShoesPtn04;
                    if (emblem == 1) return SelectKindType.CosInnerShoesEmblem;
                    if (emblem == 2) return SelectKindType.CosInnerShoesEmblem;
                    break;
                case 8:
                    if (pattern == 0 && emblem == 0) return SelectKindType.CosOuterShoes;
                    if (pattern == 1) return SelectKindType.CosOuterShoesPtn01;
                    if (pattern == 2) return SelectKindType.CosOuterShoesPtn02;
                    if (pattern == 3) return SelectKindType.CosOuterShoesPtn03;
                    if (pattern == 4) return SelectKindType.CosOuterShoesPtn04;
                    if (emblem == 1) return SelectKindType.CosOuterShoesEmblem;
                    if (emblem == 2) return SelectKindType.CosOuterShoesEmblem;
                    break;
            }
            return SelectKindType.CosTop;
        }
        #endregion

        #region Category mappings
        public string GetCategoryTypes(ColorType colorType, bool returnSubCategory = false)
        {
            string category;
            string subCategory;

            switch (colorType)
            {
                case ColorType.SkinMain:
                    category = "Body";
                    subCategory = "General";
                    break;
                case ColorType.SkinSub:
                    category = "Body";
                    subCategory = "General";
                    break;
                case ColorType.SkinTan:
                    category = "Body";
                    subCategory = "Suntan";
                    break;
                case ColorType.NippleColor:
                    category = "Body";
                    subCategory = "Chest";
                    break;
                case ColorType.NailColor:
                    category = "Body";
                    subCategory = "General";
                    break;
                case ColorType.PubicHairColor:
                    category = "Body";
                    subCategory = "Pubic Hair";
                    break;
                case ColorType.HairBase:
                    category = "Hair";
                    subCategory = "";
                    break;
                case ColorType.HairStart:
                    category = "Hair";
                    subCategory = "";
                    break;
                case ColorType.HairEnd:
                    category = "Hair";
                    subCategory = "";
                    break;
                case ColorType.HairGloss:
                    category = "Hair";
                    subCategory = "";
                    break;
                case ColorType.Eyebrow:
                    category = "Face";
                    subCategory = "Eyebrows";
                    break;
                case ColorType.EyebrowColor:
                    category = "Face";
                    subCategory = "Eyebrows";
                    break;
                case ColorType.EyelineColor:
                    category = "Face";
                    subCategory = "Eyes";
                    break;
                case ColorType.ScleraColor1:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.ScleraColor2:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.UpperHighlightColor:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.LowerHightlightColor:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.EyeColor1Left:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.EyeColor2Left:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.EyeColor1Right:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.EyeColor2Right:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case ColorType.LipLineColor:
                    category = "Face";
                    subCategory = "Mouth";
                    break;
                case ColorType.EyeShadowColor:
                    category = "Face";
                    subCategory = "Makeup";
                    break;
                case ColorType.CheekColor:
                    category = "Face";
                    subCategory = "Makeup";
                    break;
                case ColorType.LipColor:
                    category = "Face";
                    subCategory = "Makeup";
                    break;
                default:
                    category = "Undefined";
                    subCategory = "Undefined";
                    break;
            }

            return returnSubCategory ? subCategory : category;
        }

        public string GetCategoryTypes(FloatType floatType, bool returnSubCategory = false)
        {
            string category;
            string subCategory;

            switch (floatType)
            {
                case FloatType.SkinTypeStrenth:
                case FloatType.SkinGloss:
                case FloatType.DisplaySkinDetailLines:
                    category = "Body";
                    subCategory = "General";
                    break;
                case FloatType.Softness:
                case FloatType.Weight:
                case FloatType.NippleGloss:
                    category = "Body";
                    subCategory = "Chest";
                    break;
                case FloatType.NailGloss:
                    category = "Body";
                    subCategory = "General";
                    break;
                case FloatType.FaceOverlayStrenth:
                    category = "Face";
                    subCategory = "General";
                    break;
                case FloatType.CheekGloss:
                    category = "Face";
                    subCategory = "Cheeks";
                    break;
                case FloatType.UpperHighlightVertical:
                case FloatType.UpperHighlightHorizontal:
                case FloatType.LowerHightlightVertical:
                case FloatType.LowerHightlightHorizontal:
                case FloatType.IrisSpacing:
                case FloatType.IrisVerticalPosition:
                case FloatType.IrisWidth:
                case FloatType.IrisHeight:
                case FloatType.EyeGradientStrenth:
                case FloatType.EyeGradientVertical:
                case FloatType.EyeGradientSize:
                    category = "Face";
                    subCategory = "Iris";
                    break;
                case FloatType.LipGloss:
                    category = "Face";
                    subCategory = "Mouth";
                    break;
                default:
                    category = "Undefined";
                    subCategory = "Undefined";
                    break;
            }

            return returnSubCategory ? subCategory : category;
        }
        #endregion

        public bool IsCategoryEdited(SelectedTab tab)
        {
            switch (tab)
            {
                case SelectedTab.Body:
                    return OriginalColors.Any(x => GetCategoryTypes(x.Key) == "Body" && x.Value.Value != x.Value.OriginalValue)
                        || OriginalFloats.Any(x => Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f)
                        || OriginalBodyShapeValues.Any(x => Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f);
                case SelectedTab.Face:
                    return OriginalFaceShapeValues.Any(x => Mathf.Abs(x.Value.Value - x.Value.OriginalValue) > 0.001f)
                        || OriginalColors.Any(x => GetCategoryTypes(x.Key) == "Face" && x.Value.Value != x.Value.OriginalValue);
                case SelectedTab.Hair:
                    return OriginalColors.Any(x => GetCategoryTypes(x.Key) == "Hair" && x.Value.Value != x.Value.OriginalValue);
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
    public struct ClothingStorageKey
    {
        [Key("OutfitSlot")]
        public int OutfitSlot { get; set; }
        [Key("ClothingKind")]
        public int ClothingKind { get; set; }
        [Key("ColorNr")]
        public int ColorNr { get; set; }
        [Key("SlotNr")]
        public int SlotNr { get; set; }
        [Key("IsPattern")]
        public bool IsPattern { get; set; }

        public ClothingStorageKey(int outfitSlot, int clothingKind, int colorNr, int slotNr, bool isPattern)
        {
            OutfitSlot = outfitSlot;
            ClothingKind = clothingKind;
            ColorNr = colorNr;
            SlotNr = slotNr;
            IsPattern = isPattern;
        }

        public bool Compare(int outfitSlot, int kind, int colorNr, int slotNr, bool isPattern)
        {
            if (
                OutfitSlot == outfitSlot
                && ClothingKind == kind
                && ColorNr == colorNr
                && SlotNr == slotNr
                && IsPattern == isPattern
            )
                return true;
            return false;
        }
    }

    [Serializable]
    [MessagePackObject]
    public struct AccessoryStorageKey
    {
        [Key("OutfitSlot")]
        public int OutfitSlot { get; set; }
        [Key("ColorNr")]
        public int ColorNr { get; set; }
        [Key("SlotNr")]
        public int SlotNr { get; set; }

        public AccessoryStorageKey(int outfitSlot, int slotNr, int colorNr)
        {
            OutfitSlot = outfitSlot;
            ColorNr = colorNr;
            SlotNr = slotNr;
        }

        public bool Compare(int outfitSlot, int slotNr, int colorNr)
        {
            if (
                OutfitSlot == outfitSlot
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
