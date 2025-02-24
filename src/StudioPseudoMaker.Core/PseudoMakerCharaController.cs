﻿using ChaCustom;
using ExtensibleSaveFormat;
using HarmonyLib;
using KK_Plugins.MaterialEditor;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using MessagePack;
using MoreAccessoriesKOI;
using PseudoMaker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static KK_Plugins.Pushup;
using static PseudoMaker.PseudoMaker;

namespace PseudoMaker
{
    internal class PseudoMakerCharaController : CharaCustomFunctionController
    {
        internal static readonly Dictionary<ChaControl, PseudoMakerCharaController> allControllers = new Dictionary<ChaControl, PseudoMakerCharaController>();

        #region Save Lists
        private Dictionary<ClothingStorageKey, ColorStorage> OriginalClothingColors = new Dictionary<ClothingStorageKey, ColorStorage>();
        private Dictionary<AccessoryStorageKey, ColorStorage> OriginalAccessoryColors = new Dictionary<AccessoryStorageKey, ColorStorage>();
        private Dictionary<ColorType, ColorStorage> OriginalColors = new Dictionary<ColorType, ColorStorage>();
        private Dictionary<FloatType, FloatStorage> OriginalFloats = new Dictionary<FloatType, FloatStorage>();
        private Dictionary<AccessoryStorageKey, FloatStorage> OriginalAccessoryFloats = new Dictionary<AccessoryStorageKey, FloatStorage>();
        private Dictionary<int, FloatStorage> OriginalBodyShapeValues = new Dictionary<int, FloatStorage>();
        private Dictionary<int, FloatStorage> OriginalFaceShapeValues = new Dictionary<int, FloatStorage>();
        private Dictionary<PushupStorageKey, FloatStorage> OriginalPushupValue = new Dictionary<PushupStorageKey, FloatStorage>();
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

            if (OriginalAccessoryFloats.Count > 0)
                data.data.Add(nameof(OriginalAccessoryFloats), MessagePackSerializer.Serialize(OriginalAccessoryFloats));
            else
                data.data.Add(nameof(OriginalAccessoryFloats), null);

            if (OriginalBodyShapeValues.Count > 0)
                data.data.Add(nameof(OriginalBodyShapeValues), MessagePackSerializer.Serialize(OriginalBodyShapeValues));
            else
                data.data.Add(nameof(OriginalBodyShapeValues), null);

            if (OriginalFaceShapeValues.Count > 0)
                data.data.Add(nameof(OriginalFaceShapeValues), MessagePackSerializer.Serialize(OriginalFaceShapeValues));
            else
                data.data.Add(nameof(OriginalFaceShapeValues), null);

            if (OriginalPushupValue.Count > 0)
                data.data.Add(nameof(OriginalPushupValue), MessagePackSerializer.Serialize(OriginalPushupValue));
            else
                data.data.Add(nameof(OriginalPushupValue), null);

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

            if (data.data.TryGetValue(nameof(OriginalAccessoryFloats), out var originalAccessoryFloats) && originalAccessoryFloats != null)
                OriginalAccessoryFloats = MessagePackSerializer.Deserialize<Dictionary<AccessoryStorageKey, FloatStorage>>((byte[])originalAccessoryFloats);

            if (data.data.TryGetValue(nameof(OriginalFloats), out var originalBustValues) && originalBustValues != null)
                OriginalFloats = MessagePackSerializer.Deserialize<Dictionary<FloatType, FloatStorage>>((byte[])originalBustValues);

            if (data.data.TryGetValue(nameof(OriginalBodyShapeValues), out var originalBodyShapeValues) && originalBodyShapeValues != null)
                OriginalBodyShapeValues = MessagePackSerializer.Deserialize<Dictionary<int, FloatStorage>>((byte[])originalBodyShapeValues);

            if (data.data.TryGetValue(nameof(OriginalFaceShapeValues), out var originalFaceShapeValues) && originalFaceShapeValues != null)
                OriginalFaceShapeValues = MessagePackSerializer.Deserialize<Dictionary<int, FloatStorage>>((byte[])originalFaceShapeValues);

            if (data.data.TryGetValue(nameof(OriginalPushupValue), out var originalPushupValue) && originalPushupValue != null)
                OriginalPushupValue = MessagePackSerializer.Deserialize<Dictionary<PushupStorageKey, FloatStorage>>((byte[])originalPushupValue);
        }

        internal void ResetSavedValues()
        {
            OriginalClothingColors.Clear();
            OriginalAccessoryColors.Clear();
            OriginalColors.Clear();
            OriginalFloats.Clear();
            OriginalAccessoryFloats.Clear();
            OriginalBodyShapeValues.Clear();
            OriginalFaceShapeValues.Clear();
            OriginalPushupValue.Clear();
        }

        public static PseudoMakerCharaController GetController(ChaControl chaCtrl)
        {
            if (chaCtrl == null)
                return null;
            if (allControllers.ContainsKey(chaCtrl))
                return allControllers[chaCtrl];
#if DEBUG
            return chaCtrl.gameObject.GetOrAddComponent<PseudoMakerCharaController>();
#else
            return chaCtrl.gameObject.GetComponent<PseudoMakerCharaController>();
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
                case ColorType.LowerHighlightColor:
                    ChaControl.fileFace.hlDownColor = color;
                    ChaControl.ChangeSettingEyeHiDownColor();
                    break;
                case ColorType.EyeColor1:
                    ChaControl.fileFace.pupil[0].baseColor = color;
                    ChaControl.fileFace.pupil[1].baseColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, true);
                    ChaControl.ChangeSettingEyeR(true, true, true);
                    break;
                case ColorType.EyeColor2:
                    ChaControl.fileFace.pupil[0].subColor = color;
                    ChaControl.fileFace.pupil[1].subColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, true);
                    ChaControl.ChangeSettingEyeR(true, true, true);
                    break;
                case ColorType.EyeColor1Left:
                    ChaControl.fileFace.pupil[0].baseColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, true);
                    break;
                case ColorType.EyeColor2Left:
                    ChaControl.fileFace.pupil[0].subColor = color;
                    ChaControl.ChangeSettingEyeL(true, true, true);
                    break;
                case ColorType.EyeColor1Right:
                    ChaControl.fileFace.pupil[1].baseColor = color;
                    ChaControl.ChangeSettingEyeR(true, true, true);
                    break;
                case ColorType.EyeColor2Right:
                    ChaControl.fileFace.pupil[1].subColor = color;
                    ChaControl.ChangeSettingEyeR(true, true, true);
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
                        ChaControl.ChangeSettingHairOutlineColor(i);
                    }
                    break;
#endif
                case ColorType.HairOutline:
                    for (int i = 0; i < 4; i++)
                    {
                        ChaControl.fileHair.parts[i].outlineColor = color;
                        ChaControl.ChangeSettingHairOutlineColor(i);
                    }
                    break;
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
                case ColorType.LowerHighlightColor:
                    return ChaControl.fileFace.hlDownColor;
                case ColorType.EyeColor1:
                case ColorType.EyeColor1Left:
                    return ChaControl.fileFace.pupil[0].baseColor;
                case ColorType.EyeColor2:
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
                case ColorType.HairOutline:
                    return ChaControl.fileHair.parts[0].outlineColor;
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
                case FloatType.SkinTypeStrength:
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
                case FloatType.BustSoftness:
                    //ChaControl.ChangeBustSoftness(value);
                    SetPushupBaseValue(PushupValue.AdvancedSoftness, value);
                    break;
                case FloatType.BustWeight:
                    //ChaControl.ChangeBustGravity(value);
                    SetPushupBaseValue(PushupValue.AdvancedWeight, value);
                    break;
                case FloatType.NippleGloss:
                    ChaFileControl.custom.body.nipGlossPower = value;
                    ChaControl.ChangeSettingNipGlossPower();
                    break;
                case FloatType.NailGloss:
                    ChaFileControl.custom.body.nailGlossPower = value;
                    ChaControl.ChangeSettingNailGlossPower();
                    break;
                case FloatType.FaceOverlayStrength:
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
                case FloatType.LowerHighlightVertical:
                    ChaFileControl.custom.face.hlDownY = value;
                    ChaControl.ChangeSettingEyeHLDownPosY();
                    break;
#if KKS
                case FloatType.LowerHighlightHorizontal:
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
                case FloatType.EyeGradientStrength:
                    ChaFileControl.custom.face.pupil[0].gradBlend = value;
                    ChaFileControl.custom.face.pupil[1].gradBlend = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientVertical:
                    ChaFileControl.custom.face.pupil[0].gradOffsetY = value;
                    ChaFileControl.custom.face.pupil[1].gradOffsetY = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientStrengthLeft:
                    ChaFileControl.custom.face.pupil[0].gradBlend = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientVerticalLeft:
                    ChaFileControl.custom.face.pupil[0].gradOffsetY = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientStrengthRight:
                    ChaFileControl.custom.face.pupil[1].gradBlend = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientVerticalRight:
                    ChaFileControl.custom.face.pupil[1].gradOffsetY = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientSize:
                    ChaFileControl.custom.face.pupil[0].gradScale = value;
                    ChaFileControl.custom.face.pupil[1].gradScale = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientSizeLeft:
                    ChaFileControl.custom.face.pupil[0].gradScale = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.EyeGradientSizeRight:
                    ChaFileControl.custom.face.pupil[1].gradScale = value;
                    ChaControl.ChangeSettingEye(true, true, true);
                    break;
                case FloatType.LipGloss:
                    ChaFileControl.custom.face.lipGlossPower = value;
                    ChaControl.ChangeSettingLipGlossPower();
                    ChaFileControl.custom.hair.parts[1].length = value;
                    ChaControl.ChangeSettingHairFrontLength();
                    break;
                case FloatType.HairFrontLength:
                    ChaFileControl.custom.hair.parts[1].length = value;
                    ChaControl.ChangeSettingHairFrontLength();
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
                case FloatType.SkinTypeStrength:
                    return ChaFileControl.custom.body.detailPower;
                case FloatType.SkinGloss:
                    return ChaFileControl.custom.body.skinGlossPower;
                case FloatType.DisplaySkinDetailLines:
                    return Convert.ToSingle(ChaFileControl.custom.body.drawAddLine);
                case FloatType.BustSoftness:
                    //return ChaControl.fileBody.bustSoftness;
                    return GetPushupBaseValue(PushupValue.AdvancedSoftness);
                case FloatType.BustWeight:
                    //return ChaControl.fileBody.bustWeight;
                    return GetPushupBaseValue(PushupValue.AdvancedWeight);
                case FloatType.NippleGloss:
                    return ChaFileControl.custom.body.nipGlossPower;
                case FloatType.NailGloss:
                    return ChaFileControl.custom.body.nailGlossPower;
                case FloatType.FaceOverlayStrength:
                    return ChaFileControl.custom.face.detailPower;
                case FloatType.CheekGloss:
                    return ChaFileControl.custom.face.cheekGlossPower;
                case FloatType.UpperHighlightVertical:
                    return ChaFileControl.custom.face.hlUpY;
#if KKS
                case FloatType.UpperHighlightHorizontal:
                    return ChaFileControl.custom.face.hlUpX;
#endif
                case FloatType.LowerHighlightVertical:
                    return ChaFileControl.custom.face.hlDownY;
#if KKS
                case FloatType.LowerHighlightHorizontal:
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
                case FloatType.EyeGradientStrength:
                case FloatType.EyeGradientStrengthLeft:
                    return ChaFileControl.custom.face.pupil[0].gradBlend;
                case FloatType.EyeGradientVertical:
                case FloatType.EyeGradientVerticalLeft:
                    return ChaFileControl.custom.face.pupil[0].gradOffsetY;
                case FloatType.EyeGradientStrengthRight:
                    return ChaFileControl.custom.face.pupil[1].gradBlend;
                case FloatType.EyeGradientVerticalRight:
                    return ChaFileControl.custom.face.pupil[1].gradOffsetY;
                case FloatType.EyeGradientSize:
                case FloatType.EyeGradientSizeLeft:
                    return ChaFileControl.custom.face.pupil[0].gradScale;
                case FloatType.EyeGradientSizeRight:
                    return ChaFileControl.custom.face.pupil[1].gradScale;
                case FloatType.LipGloss:
                    return ChaFileControl.custom.face.lipGlossPower;
                case FloatType.HairFrontLength:
                    return ChaFileControl.custom.hair.parts[1].length;
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

            if (index >= 4 && index <= 13)
                SetPushupBaseValue((PushupValue)index, value);
            else
                ChaControl.SetShapeBodyValue(index, value);
        }

        public float GetCurrentBodyValue(int index)
        {
            if (index >= 4 && index <= 14)
                return GetPushupBaseValue((PushupValue)index);
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
        #endregion

        #region Face
        #region Shape
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
        #endregion

        public void CopyPupil(int from, int to)
        {
            selectedCharacter.fileFace.pupil[from].Copy(selectedCharacter.fileFace.pupil[to]);
            selectedCharacter.ChangeSettingEye(true, true, true);
        }
        #endregion

        #region Clothes
        public void InitBaseCustomTextureClothesIfNotExists(int kind)
        {
            try
            {
                if (selectedCharacter?.ctCreateClothes[kind, 0] == null && selectedCharacter.infoClothes[kind] != null)
                    selectedCharacter.InitBaseCustomTextureClothes(true, kind);
            }
            catch (Exception e)
            {
                PseudoMaker.Logger.LogMessage("Selected option is broken and was switched back to the first option");
                PseudoMaker.Logger.LogError(e);
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
            InitBaseCustomTextureClothesIfNotExists(kind);
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
                {
                    ChaControl.ChangeCustomClothes(true, kind, true, true, true, true, true);
                }
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

        public void ResetClothingColor(int kind, int colorNr, int slotNr = -1, bool isPattern = false, bool getDefault = false)
        {
            Color color = Color.white;
            if (getDefault && !isPattern)
                color = selectedCharacter.GetClothesDefaultColor(kind, colorNr);
            else {
                var clothingColors = new ClothingStorageKey(CurrentOutfitSlot, kind, colorNr, slotNr, isPattern);
                if (OriginalClothingColors.TryGetValue(clothingColors, out var clothingColor))
                    color = clothingColor.OriginalValue;
                else return;
            }
            SetClothingColor(kind, colorNr, color, slotNr, isPattern);
        }

        public Color GetOriginalClothingColor(int kind, int colorNr, int slotNr = -1, bool isPattern = false)
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
            InitBaseCustomTextureClothesIfNotExists(kind);
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

        public bool GetClothingUsesOptPart(int kind, int part)
        {
            var clothesComponent = selectedCharacter.GetCustomClothesComponent(kind);
            if (clothesComponent == null)
                return false;

            if (part == 0) return clothesComponent.objOpt01.Any();
            else if (part == 1) return clothesComponent.objOpt02.Any();
            return false;
        }

        public bool IsClothingKindEdited(int kind)
        {
            return OriginalClothingColors
                .Where(c => c.Key.ClothingKind == kind)
                .Select(c => c.Value)
                .Any(c => c.Value != c.OriginalValue);
        }

        #region Pushup
        public float GetPushupValue(bool bra, PushupValue pushupValue)
        {
            var data = bra ? selectedPushupController.CurrentBraData : selectedPushupController.CurrentTopData;
            switch (pushupValue)
            {
                case PushupValue.Firmness: return data.Firmness;
                case PushupValue.Lift: return data.Lift;
                case PushupValue.PushTogether: return data.PushTogether;
                case PushupValue.Squeeze: return data.Squeeze;
                case PushupValue.CenterNipples: return data.CenterNipples;
                case PushupValue.AdvancedSize: return data.Size;
                case PushupValue.AdvancedVerticalPosition: return data.VerticalPosition;
                case PushupValue.AdvancedHorizontalAngle: return data.HorizontalAngle;
                case PushupValue.AdvancedHorizontalPosition: return data.HorizontalPosition;
                case PushupValue.AdvancedVerticalAngle: return data.VerticalAngle;
                case PushupValue.AdvancedDepth: return data.Depth;
                case PushupValue.AdvancedRoundness: return data.Roundness;
                case PushupValue.AdvancedSoftness: return data.Softness;
                case PushupValue.AdvancedWeight: return data.Weight;
                case PushupValue.AdvancedAreolaDepth: return data.AreolaDepth;
                case PushupValue.AdvancedNippleWidth: return data.NippleWidth;
                case PushupValue.AdvancedNippleDepth: return data.NippleDepth;
            }
            return 0f;
        }

        public float GetPushupBaseValue(PushupValue pushupValue)
        {
            switch (pushupValue)
            {
                case PushupValue.AdvancedSize:
                    return selectedPushupController.BaseData.Size;
                case PushupValue.AdvancedVerticalPosition:
                    return selectedPushupController.BaseData.VerticalPosition;
                case PushupValue.AdvancedHorizontalAngle:
                    return selectedPushupController.BaseData.HorizontalAngle;
                case PushupValue.AdvancedHorizontalPosition:
                    return selectedPushupController.BaseData.HorizontalPosition;
                case PushupValue.AdvancedVerticalAngle:
                    return selectedPushupController.BaseData.VerticalAngle;
                case PushupValue.AdvancedDepth:
                    return selectedPushupController.BaseData.Depth;
                case PushupValue.AdvancedRoundness:
                    return selectedPushupController.BaseData.Roundness;
                case PushupValue.AdvancedAreolaDepth:
                    return selectedPushupController.BaseData.Depth;
                case PushupValue.AdvancedNippleWidth:
                    return selectedPushupController.BaseData.NippleWidth;
                case PushupValue.AdvancedNippleDepth:
                    return selectedPushupController.BaseData.NippleDepth;
                case PushupValue.AdvancedSoftness:
                    return selectedPushupController.BaseData.Softness;
                case PushupValue.AdvancedWeight:
                    return selectedPushupController.BaseData.Weight;
                default:
                    return 0f;
            }
        }

        public void SetPushupBaseValue(PushupValue pushupValue, float value)
        {
            switch (pushupValue)
            {
                case PushupValue.AdvancedSize:
                    selectedPushupController.BaseData.Size = value;
                    break;
                case PushupValue.AdvancedVerticalPosition:
                    selectedPushupController.BaseData.VerticalPosition = value;
                    break;
                case PushupValue.AdvancedHorizontalAngle:
                    selectedPushupController.BaseData.HorizontalAngle = value;
                    break;
                case PushupValue.AdvancedHorizontalPosition:
                    selectedPushupController.BaseData.HorizontalPosition = value;
                    break;
                case PushupValue.AdvancedVerticalAngle:
                    selectedPushupController.BaseData.VerticalAngle = value;
                    break;
                case PushupValue.AdvancedDepth:
                    selectedPushupController.BaseData.Depth = value;
                    break;
                case PushupValue.AdvancedRoundness:
                    selectedPushupController.BaseData.Roundness = value;
                    break;
                case PushupValue.AdvancedAreolaDepth:
                    selectedPushupController.BaseData.Depth = value;
                    break;
                case PushupValue.AdvancedNippleWidth:
                    selectedPushupController.BaseData.NippleWidth = value;
                    break;
                case PushupValue.AdvancedNippleDepth:
                    selectedPushupController.BaseData.NippleDepth = value;
                    break;
                case PushupValue.AdvancedSoftness:
                    selectedPushupController.BaseData.Softness = value;
                    break;
                case PushupValue.AdvancedWeight:
                    selectedPushupController.BaseData.Weight = value;
                    break;
            }
            selectedPushupController.RecalculateBody();
        }

        public float GetOriginalPushupValue(bool bra, PushupValue pushupValue)
        {
            var pushupKey = new PushupStorageKey(CurrentOutfitSlot, pushupValue);
            if (OriginalPushupValue.TryGetValue(pushupKey, out var pushup))
                return pushup.OriginalValue;
            return GetPushupValue(bra, pushupValue);
        }

        public float GetDefaultPushupValue(PushupValue pushupValue)
        {
            switch (pushupValue)
            {
                case PushupValue.Firmness: return ConfigFirmnessDefault.Value;
                case PushupValue.Lift: return ConfigLiftDefault.Value;
                case PushupValue.PushTogether: return ConfigPushTogetherDefault.Value;
                case PushupValue.Squeeze: return ConfigSqueezeDefault.Value;
                case PushupValue.CenterNipples: return ConfigNippleCenteringDefault.Value;
                case PushupValue.AdvancedSize:
                case PushupValue.AdvancedVerticalPosition:
                case PushupValue.AdvancedHorizontalAngle:
                case PushupValue.AdvancedHorizontalPosition:
                case PushupValue.AdvancedVerticalAngle:
                case PushupValue.AdvancedDepth:
                case PushupValue.AdvancedRoundness:
                case PushupValue.AdvancedSoftness:
                case PushupValue.AdvancedWeight:
                case PushupValue.AdvancedAreolaDepth:
                case PushupValue.AdvancedNippleWidth:
                case PushupValue.AdvancedNippleDepth:
                    return 0.5f;
            }
            return 0f;
        }

        public void SetPushupValue(bool bra, PushupValue pushupValue, float value)
        {
            var pushupKey = new PushupStorageKey(CurrentOutfitSlot, pushupValue);
            if (!OriginalPushupValue.Any(x => x.Key == pushupKey))
                OriginalPushupValue[pushupKey] = new FloatStorage(GetPushupValue(bra, pushupValue), value);
            else
                OriginalPushupValue[pushupKey].Value = value;

            var data = bra ? selectedPushupController.CurrentBraData : selectedPushupController.CurrentTopData;
            switch (pushupValue)
            {
                case PushupValue.Firmness:
                    data.Firmness = value;
                    break;
                case PushupValue.Lift:
                    data.Lift = value;
                    break;
                case PushupValue.PushTogether:
                    data.PushTogether = value;
                    break;
                case PushupValue.Squeeze:
                    data.Squeeze = value;
                    break;
                case PushupValue.CenterNipples:
                    data.CenterNipples = value;
                    break;
                case PushupValue.AdvancedSize:
                    data.Size = value;
                    break;
                case PushupValue.AdvancedVerticalPosition:
                    data.VerticalPosition = value;
                    break;
                case PushupValue.AdvancedHorizontalAngle:
                    data.HorizontalAngle = value;
                    break;
                case PushupValue.AdvancedHorizontalPosition:
                    data.HorizontalPosition = value;
                    break;
                case PushupValue.AdvancedVerticalAngle:
                    data.VerticalAngle = value;
                    break;
                case PushupValue.AdvancedDepth:
                    data.Depth = value;
                    break;
                case PushupValue.AdvancedRoundness:
                    data.Roundness = value;
                    break;
                case PushupValue.AdvancedSoftness:
                    data.Softness = value;
                    break;
                case PushupValue.AdvancedWeight:
                    data.Weight = value;
                    break;
                case PushupValue.AdvancedAreolaDepth:
                    data.AreolaDepth = value;
                    break;
                case PushupValue.AdvancedNippleWidth:
                    data.NippleWidth = value;
                    break;
                case PushupValue.AdvancedNippleDepth:
                    data.NippleDepth = value;
                    break;
            }
        }

        public void ResetPushupValue(bool bra, PushupValue pushupValue, bool getDefault = false)
        {
            float value = 0.5f;
            if (getDefault)
                value = GetDefaultPushupValue(pushupValue);
            else
            {
                var pushupKey = new PushupStorageKey(CurrentOutfitSlot, pushupValue);
                if (OriginalPushupValue.TryGetValue(pushupKey, out var pushup))
                    value = pushup.OriginalValue;
                else return;
            }
            SetPushupValue(bra, pushupValue, value);
        }

        public void CopyPushupData(bool bra, BodyData bodyData, bool calculatePush = false)
        {
            var data = bra ? selectedPushupController.CurrentBraData : selectedPushupController.CurrentTopData;
            if (calculatePush)
                selectedPushupController.CalculatePushFromClothes(data, false);
            data.Softness = bodyData.Softness;
            data.Weight = bodyData.Weight;
            data.Size = bodyData.Size;
            data.VerticalPosition = bodyData.VerticalPosition;
            data.HorizontalPosition = bodyData.HorizontalPosition;
            data.VerticalAngle = bodyData.VerticalAngle;
            data.HorizontalAngle = bodyData.HorizontalAngle;
            data.Depth = bodyData.Depth;
            data.Roundness = bodyData.Roundness;
            data.AreolaDepth = bodyData.AreolaDepth;
            data.NippleWidth = bodyData.NippleWidth;
            data.NippleDepth = bodyData.NippleDepth;
            selectedPushupController.RecalculateBody();
        }
        #endregion
        #endregion

        #region Accessories
        public void SetAccessoryColor(int slotNr, int colorNr, Color color)
        {
            if (slotNr < 0 || slotNr >= Accessories.parts.Length) return;
            var accessoryColor = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, colorNr);
            if (!OriginalAccessoryColors.Any(x => x.Key == accessoryColor))
                OriginalAccessoryColors[accessoryColor] = new ColorStorage(GetAccessoryColor(slotNr, colorNr), color);
            else
                OriginalAccessoryColors[accessoryColor].Value = color;
            if (colorNr < 4) 
            {
                Accessories.parts[slotNr].color[colorNr] = color;
                SetAccessories.parts[slotNr].color[colorNr] = color;
                ChaControl.ChangeAccessoryColor(slotNr);
            }
            else if (colorNr == (int)HairColor.AccessoryColor)
            {
                selectedHairAccessoryController.SetAccessoryColor(color, slotNr);
                selectedHairAccessoryController.UpdateAccessory(slotNr);
            }
#if KKS
            else if (colorNr == (int)HairColor.GlossColor)
            {
                selectedHairAccessoryController.SetGlossColor(color, slotNr);
                selectedHairAccessoryController.UpdateAccessory(slotNr);
            }
#endif
            else if (colorNr == (int)HairColor.OutlineColor)
            {
                selectedHairAccessoryController.SetOutlineColor(color, slotNr);
                selectedHairAccessoryController.UpdateAccessory(slotNr);
            }
        }
        public Color GetAccessoryColor(int slotNr, int colorNr)
        {
            if (slotNr < 0 || slotNr >= Accessories.parts.Length) return Color.white;
            if (colorNr < 4)
                return Accessories.parts[slotNr].color[colorNr];
            else if (colorNr == (int)HairColor.AccessoryColor)
                return selectedHairAccessoryController.GetAccessoryColor(slotNr);
#if KKS
            else if (colorNr == (int)HairColor.GlossColor)
                return selectedHairAccessoryController.GetGlossColor(slotNr);
#endif
            else if (colorNr == (int)HairColor.OutlineColor)
                return selectedHairAccessoryController.GetOutlineColor(slotNr);
            return Color.white;
        }

        public void ResetAcessoryColor(int slotNr, int colorNr, bool getDefault = false)
        {
            Color color = Color.white;
            if (getDefault)
                selectedCharacter.GetAccessoryDefaultColor(ref color, slotNr, colorNr);
            else
            {
                var colorKey = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, colorNr);
                if (OriginalAccessoryColors.TryGetValue(colorKey, out var accessoryColor))
                    color = accessoryColor.OriginalValue;
                else return;
            }
            SetAccessoryColor(slotNr, colorNr, color);
        }

        public Color GetOriginalAccessoryColor(int slotNr, int colorNr)
        {
            var accessoryColor = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, colorNr);
            if (OriginalAccessoryColors.TryGetValue(accessoryColor, out var color))
                return color.OriginalValue;
            return GetAccessoryColor(slotNr, colorNr);
        }

        public bool[] CheckAccessoryUseColor(int slotNr)
        {
            var component = selectedCharacter.GetAccessoryComponent(slotNr);
            return new bool[]
            {
                component == null ? false : component.useColor01,
                component == null ? false : component.useColor02,
                component == null ? false : component.useColor03,
                component == null ? false : component.rendAlpha != null && component.rendAlpha.Length > 0
            };
        }

        public bool CheckAccessoryHasAccessoryPart(int slotNr)
        {
            if (selectedHairAccessoryController != null)
                return selectedHairAccessoryController.HasAccessoryPart();
            return false;
        }

        public void SetAccessoryTransform(int slotNr, int correctNr, float value, AccessoryTransform transform, TransformVector vector)
        {
            var accessoryKey = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, 0, correctNr, transform, vector);
            if (!OriginalAccessoryFloats.Any(x => x.Key == accessoryKey))
                OriginalAccessoryFloats[accessoryKey] = new FloatStorage(GetAccessoryTransformValue(slotNr, correctNr, transform, vector), value);
            else
                OriginalAccessoryFloats[accessoryKey].Value = value;

            // Not using the base Set methods because they internally round their values for some reason
            if (transform == AccessoryTransform.Location)
            {
                //selectedCharacter.SetAccessoryPos(slotNr, correctNr, value, false, (int)vector);
                if (vector == TransformVector.X) Accessories.parts[slotNr].addMove[correctNr, 0].x = value;
                else if (vector == TransformVector.Y) Accessories.parts[slotNr].addMove[correctNr, 0].y = value;
                else if (vector == TransformVector.Z) Accessories.parts[slotNr].addMove[correctNr, 0].z = value;
                SetAccessories.parts[slotNr].addMove[correctNr, 0] = Accessories.parts[slotNr].addMove[correctNr, 0];
                selectedCharacter.objAcsMove[slotNr, correctNr].transform.localPosition = Accessories.parts[slotNr].addMove[correctNr, 0] * 0.01f;
            }
            else if (transform == AccessoryTransform.Rotation)
            {
                //selectedCharacter.SetAccessoryRot(slotNr, correctNr, value, false, (int)vector);
                if (vector == TransformVector.X) Accessories.parts[slotNr].addMove[correctNr, 1].x = value;
                else if (vector == TransformVector.Y) Accessories.parts[slotNr].addMove[correctNr, 1].y = value;
                else if (vector == TransformVector.Z) Accessories.parts[slotNr].addMove[correctNr, 1].z = value;
                SetAccessories.parts[slotNr].addMove[correctNr, 1] = Accessories.parts[slotNr].addMove[correctNr, 1];
                selectedCharacter.objAcsMove[slotNr, correctNr].transform.localEulerAngles = Accessories.parts[slotNr].addMove[correctNr, 1];
            }
            else if (transform == AccessoryTransform.Scale)
            {
                //selectedCharacter.SetAccessoryScl(slotNr, correctNr, value, false, (int)vector);
                if (vector == TransformVector.X) Accessories.parts[slotNr].addMove[correctNr, 2].x = value;
                else if (vector == TransformVector.Y) Accessories.parts[slotNr].addMove[correctNr, 2].y = value;
                else if (vector == TransformVector.Z) Accessories.parts[slotNr].addMove[correctNr, 2].z = value;
                SetAccessories.parts[slotNr].addMove[correctNr, 2] = Accessories.parts[slotNr].addMove[correctNr, 2];
                selectedCharacter.objAcsMove[slotNr, correctNr].transform.localScale = Accessories.parts[slotNr].addMove[correctNr, 2];
            }
        }

        public void SetAccessoryTransform(int slotNr, int correctNr, Vector3 value, AccessoryTransform transform)
        {
            SetAccessoryTransform(slotNr, correctNr, value.x, transform, TransformVector.X);
            SetAccessoryTransform(slotNr, correctNr, value.y, transform, TransformVector.Y);
            SetAccessoryTransform(slotNr, correctNr, value.z, transform, TransformVector.Z);
        }

        public float GetAccessoryTransformValue(int slotNr, int correctNr, AccessoryTransform transform, TransformVector vector)
        {
            switch (vector)
            {
                case TransformVector.X:
                    return Accessories.parts[slotNr].addMove[correctNr, (int)transform].x;
                case TransformVector.Y:
                    return Accessories.parts[slotNr].addMove[correctNr, (int)transform].y;
                case TransformVector.Z:
                    return Accessories.parts[slotNr].addMove[correctNr, (int)transform].z;
            }
            return 0f;
        }

        public Transform GetAccessoryTransform(int slotNr, int correctNr)
        {
            return selectedCharacter.objAcsMove[slotNr, correctNr].transform;
        }

        public void ResetAcessoryTransform(int slotNr, int correctNr, AccessoryTransform transform, TransformVector vector, bool getDefault = false)
        {
            float originalValue = 0f;
            if (getDefault)
            {
                if (transform == AccessoryTransform.Scale)
                    originalValue = 1f;
            }
            else
            {
                var accessoryKey = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, 0, correctNr, transform, vector);
                if (OriginalAccessoryFloats.TryGetValue(accessoryKey, out var key))
                    originalValue = key.OriginalValue;
                else return;
            }
            SetAccessoryTransform(slotNr, correctNr, originalValue, transform, vector);
        }

        public float GetOriginalAccessoryTransform(int slotNr, int correctNr, AccessoryTransform transform, TransformVector vector)
        {
            var accessoryKey = new AccessoryStorageKey(CurrentOutfitSlot, slotNr, 0, correctNr, transform, vector);
            if (OriginalAccessoryFloats.TryGetValue(accessoryKey, out var key))
                return key.OriginalValue;
            return GetAccessoryTransformValue(slotNr, correctNr, transform, vector);
        }

        public bool CheckAccessoryUsesSecondTransform(int slotNr)
        {
            return selectedCharacter.objAcsMove[slotNr, 1] != null;
        }

        public int GetCurrentAccessoryType(int slotNr)
        {
            if (slotNr >= 0 && selectedCharacter.infoAccessory[slotNr] != null)
                return selectedCharacter.infoAccessory[slotNr].Category;
            return (int)ChaListDefine.CategoryNo.ao_none;
        }

        public int GetCurrentAccessoryId(int slotNr)
        {
            if (slotNr >= 0 && selectedCharacter.infoAccessory[slotNr] != null)
                return selectedCharacter.infoAccessory[slotNr].Id;
            return 0;
        }

        public string GetCurrentAccessoryParent(int slotNr)
        {
            if (CheckAccessoryHasA12Parent(slotNr))
                return "A12";
            if (slotNr >= 0)
                return Accessories.parts[slotNr].parentKey;
            return "Unknown";
        }

        public void SetAccessory(int slotNr, int type, int id, bool keepParent)
        {
            var parentKey = keepParent ? PseudoMaker.selectedCharacterController.GetCurrentAccessoryParent(slotNr) : "";
            selectedCharacter.ChangeAccessory(slotNr, type, id, parentKey);
            typeof(AccessoriesApi).GetMethod("OnAccessoryKindChanged", AccessTools.all).Invoke(null, new object[] { this, slotNr });
            SetAccessories.parts[slotNr] = Accessories.parts[slotNr];
        }

        public void SetAccessoryParent(int slotNr, string parentKey)
        {
            selectedCharacter.ChangeAccessoryParent(slotNr, parentKey);
            SetAccessories.parts[slotNr] = Accessories.parts[slotNr];
            Compatibility.A12.ChangeAccessoryParent(slotNr);
        }

        public bool CheckAccessoryHasA12Parent(int slotNr)
        {
            var customAccParents = Compatibility.A12.GetCustomAccParents();

            if (customAccParents != null && customAccParents.ContainsKey(CurrentOutfitSlot))
                return customAccParents[CurrentOutfitSlot].ContainsKey(slotNr);
            return false;
        }

        public void AccessorySwapParent(int slotNr)
        {
            var reverseParent = ChaAccessoryDefine.GetReverseParent(GetCurrentAccessoryParent(slotNr));
            if (reverseParent != "")
                SetAccessoryParent(slotNr, reverseParent);
        }

        public bool GetAccessoryNoShake(int slotNr)
        {
            if (slotNr >= 0)
                return Accessories.parts[slotNr].noShake;
            return false;
        }

        public void SetAccessoryNoShake(int slotNr, bool value)
        {
            Accessories.parts[slotNr].noShake = value;
            SetAccessories.parts[slotNr].noShake = value;
            selectedCharacter.ChangeShakeAccessory(slotNr);
        }

        public void AddAccessorySlot(int num)
        {
            var newParts = new ChaFileAccessory.PartsInfo[num];
            for (var i = 0; i < num; i++)
            {
                newParts[i] = new ChaFileAccessory.PartsInfo();
            }

            var nowParts = selectedCharacter.nowCoordinate.accessory.parts;
            var accessory = selectedCharacter.chaFile.coordinate[selectedCharacter.chaFile.status.coordinateType].accessory;

            accessory.parts = selectedCharacter.nowCoordinate.accessory.parts = nowParts.Concat(newParts).ToArray();
            MoreAccessories.ArraySync(selectedCharacter);
        }

        #region HairAccessoryCustomizer
        public bool GetAccessoryIsHair(int slotNr)
        {
            if (selectedHairAccessoryController != null)
                return selectedHairAccessoryController.IsHairAccessory(slotNr);
            return false;
        }

        public bool GetAccessoryColorMatchHair(int slotNr)
        {
            if (selectedHairAccessoryController != null)
                return selectedHairAccessoryController.GetColorMatch(slotNr);
            return false;
        }

        public void SetAccessoryColorMatchHair(int slotNr, bool value)
        {
            if (selectedHairAccessoryController != null)
            {
                selectedHairAccessoryController.SetColorMatch(value, slotNr);
                selectedHairAccessoryController.UpdateAccessory(slotNr);
            }
        }

        public bool GetAccessoryUseGloss(int slotNr)
        {
            if (selectedHairAccessoryController != null)
                return selectedHairAccessoryController.GetHairGloss(slotNr);
            return false;
        }

        public void SetAccessoryUseGloss(int slotNr, bool value)
        {
            if (selectedHairAccessoryController != null)
            {
                selectedHairAccessoryController.SetHairGloss(value, slotNr);
                selectedHairAccessoryController.UpdateAccessory(slotNr);
            }
        }

        public bool CheckAccessoryUsesHairLength(int slotNr)
        {
            if (selectedHairAccessoryController != null)
                return selectedHairAccessoryController.HasLengthTransforms();
            return false;
        }

        public float GetAccessoryHairLength(int slotNr)
        {
            if (selectedHairAccessoryController != null)
                return selectedHairAccessoryController.GetHairLength(slotNr);
            return 0f;
        }

        public void SetAccessoryHairLength(int slotNr, float value)
        {
            if (selectedHairAccessoryController != null)
            {
                selectedHairAccessoryController.SetHairLength(value, slotNr);
                selectedHairAccessoryController.UpdateAccessory(slotNr);
            }
        }

        public bool GetHairNoShake(int part)
        {
            return selectedCharacter.fileHair.parts[part].noShake;
        }

        public void SetHairNoShake(int part, bool value)
        {
            selectedCharacter.fileHair.parts[part].noShake = value;
            selectedCharacter.ChangeShakeHair(part);
        }
        #endregion
        #endregion

        #region Category pickers
        public void SetSelectKind(SelectKindType type, CustomSelectInfo info)
        {
            SetSelectKind(type, info.index);
        }

        public void SetSelectKind(SelectKindType type, int id)
        {
            void UpdateClothesPattern(int kind, int pattern)
            {
                InitBaseCustomTextureClothesIfNotExists(kind);
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
                    selectedCharacter.fileFace.pupil[1].id = id;
                    selectedCharacter.ChangeSettingEye(true, true, true);
                    break;
                case SelectKindType.PupilGrade:
                    selectedCharacter.fileFace.pupil[0].gradMaskId = id;
                    selectedCharacter.fileFace.pupil[1].gradMaskId = id;
                    selectedCharacter.ChangeSettingEye(true, true, true);
                    break;
                case SelectKindType.PupilLeft:
                    selectedCharacter.fileFace.pupil[0].id = id;
                    selectedCharacter.ChangeSettingEye(true, true, true);
                    break;
                case SelectKindType.PupilGradeLeft:
                    selectedCharacter.fileFace.pupil[0].gradMaskId = id;
                    selectedCharacter.ChangeSettingEye(true, true, true);
                    break;
                case SelectKindType.PupilRight:
                    selectedCharacter.fileFace.pupil[1].id = id;
                    selectedCharacter.ChangeSettingEye(true, true, true);
                    break;
                case SelectKindType.PupilGradeRight:
                    selectedCharacter.fileFace.pupil[1].gradMaskId = id;
                    selectedCharacter.ChangeSettingEye(true, true, true);
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
                case SelectKindType.PupilLeft:
                    return selectedCharacter.fileFace.pupil[0].id;
                case SelectKindType.PupilGrade:
                case SelectKindType.PupilGradeLeft:
                    return selectedCharacter.fileFace.pupil[0].gradMaskId;
                case SelectKindType.PupilRight:
                    return selectedCharacter.fileFace.pupil[1].id;
                case SelectKindType.PupilGradeRight:
                    return selectedCharacter.fileFace.pupil[1].gradMaskId;
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

        public static int SubCategoryToKind(SubCategory subCategory)
        {
            int clothingKind = -1;
            if (subCategory == SubCategory.ClothingTop) clothingKind = 0;
            else if (subCategory == SubCategory.ClothingBottom) clothingKind = 1;
            else if (subCategory == SubCategory.ClothingBra) clothingKind = 2;
            else if (subCategory == SubCategory.ClothingUnderwear) clothingKind = 3;
            else if (subCategory == SubCategory.ClothingGloves) clothingKind = 4;
            else if (subCategory == SubCategory.ClothingPantyhose) clothingKind = 5;
            else if (subCategory == SubCategory.ClothingLegwear) clothingKind = 6;
#if KK
            else if (subCategory == SubCategory.ClothingShoesInDoors) clothingKind = 7;
            else if (subCategory == SubCategory.ClothingShoesOutdoors) clothingKind = 8;
#else
            else if (subCategory == SubCategory.ClothingShoes) clothingKind = 8;
#endif
            return clothingKind;
        }

        public static string GetClothingTypeNameByKind(int kind)
        {
            switch (kind)
            {
                case 0: return "Top";
                case 1: return "Bottom";
                case 2: return "Bra";
                case 3: return "Underwear";
                case 4: return "Gloves";
                case 5: return "Pantyhose";
                case 6: return "Legwear";
#if KK
                case 7: return "Shoes Inner";
                case 8: return "Shoes Outer";
#elif KKS
                case 8: return "Shoes";
#endif
            }
            return "Unknown";
        }
        #endregion

        private new void OnDestroy()
        {
            allControllers.Remove(ChaControl);
        }
    }

    public enum AccessoryTransform
    {
        None = -1,
        Location = 0,
        Rotation = 1,
        Scale = 2,
    }

    public enum TransformVector
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
    }

    public enum HairColor
    {
        Color1 = 0,
        Color2 = 1,
        Color3 = 2,
        AccessoryColor = 4,
        OutlineColor = 5,
        GlossColor = 6,
    }

    public enum PushupValue
    {
        Firmness = 20,
        Lift = 21,
        PushTogether = 22,
        Squeeze = 23,
        CenterNipples = 24,
        AdvancedSize = 4,
        AdvancedVerticalPosition = 5,
        AdvancedHorizontalAngle = 6,
        AdvancedHorizontalPosition = 7,
        AdvancedVerticalAngle = 8,
        AdvancedDepth = 9,
        AdvancedRoundness = 10,
        AdvancedSoftness = 14,
        AdvancedWeight = 15,
        AdvancedAreolaDepth = 11,
        AdvancedNippleWidth = 12,
        AdvancedNippleDepth = 13
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
    public struct PushupStorageKey
    {
        [Key("OutfitSlot")]
        public int OutfitSlot { get; set; }
        [Key("PushupValue")]
        public PushupValue PushupValue { get; set; }

        public PushupStorageKey(int outfitSlot, PushupValue pushupValue)
        {
            OutfitSlot = outfitSlot;
            PushupValue = pushupValue;
        }

        public static bool operator ==(PushupStorageKey c1, PushupStorageKey c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(PushupStorageKey c1, PushupStorageKey c2)
        {
            return !c1.Equals(c2);
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
        [Key("CorrectNr")]
        public int CorrectNr { get; set; }
        [Key("AccessoryTransform")]
        public AccessoryTransform AccessoryTransform { get; set; }
        [Key("TransformVector")]
        public TransformVector TransformVector { get; set; }

        public AccessoryStorageKey(int outfitSlot, int slotNr, int colorNr, int correctNr = 0, AccessoryTransform accessoryTransform = AccessoryTransform.None, TransformVector vector = TransformVector.None)
        {
            OutfitSlot = outfitSlot;
            ColorNr = colorNr;
            SlotNr = slotNr;
            CorrectNr = correctNr;
            AccessoryTransform = accessoryTransform;
            TransformVector = vector;
        }

        public static bool operator ==(AccessoryStorageKey c1, AccessoryStorageKey c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(AccessoryStorageKey c1, AccessoryStorageKey c2)
        {
            return !c1.Equals(c2);
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
