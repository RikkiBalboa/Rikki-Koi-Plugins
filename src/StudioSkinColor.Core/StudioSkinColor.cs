using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.Studio.SaveLoad;
using KKAPI.Studio.UI;
using MessagePack;
using Studio;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class StudioSkinColor : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.studioSkinColor";
        public const string PluginName = "StudioSkinColor";
        public const string PluginNameInternal = Constants.Prefix + "_StudioSkinColor";
        public const string PluginVersion = "1.0";
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
        }

        private void Start()
        {
            if (StudioAPI.InsideStudio)
            {
                RegisterStudioControls();
                TimelineCompatibilityHelper.PopulateTimeline();
            }
        }

        private void RegisterStudioControls()
        {
            var catBody = StudioAPI.GetOrCreateCurrentStateCategory("Body");
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => c.GetChaControl().fileBody.skinMainColor, c => UpdateTextureColor(c, TextureColor.SkinMain)));
            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => c.GetChaControl().fileBody.skinSubColor, c => UpdateTextureColor(c, TextureColor.SkinSub)));
            catBody.AddControl(new CurrentStateColorSlider("Tan", c => c.GetChaControl().fileBody.sunburnColor, c => UpdateTextureColor(c, TextureColor.Tan)));

            var catBust = StudioAPI.GetOrCreateCurrentStateCategory("Bust");
            catBust.AddControl(new CurrentStateCategorySlider("Softness", c => c.GetChaControl().fileBody.bustSoftness, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Softness));
            catBust.AddControl(new CurrentStateCategorySlider("Weight", c => c.GetChaControl().fileBody.bustWeight, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Weight));

            var catHair = StudioAPI.GetOrCreateCurrentStateCategory("Hair");
            catHair.AddControl(new CurrentStateColorSlider("Color 1", c => c.GetChaControl().fileHair.parts[0].baseColor, color => UpdateHairColor(color, HairColor.Base)));
            catHair.AddControl(new CurrentStateColorSlider("Color 2", c => c.GetChaControl().fileHair.parts[0].startColor, color => UpdateHairColor(color, HairColor.Start)));
            catHair.AddControl(new CurrentStateColorSlider("Color 3", c => c.GetChaControl().fileHair.parts[0].endColor, color => UpdateHairColor(color, HairColor.End)));
#if KKS
            catHair.AddControl(new CurrentStateColorSlider("Gloss", c => c.GetChaControl().fileHair.parts[0].glossColor, color => UpdateHairColor(color, HairColor.Gloss)));
#endif
            catHair.AddControl(new CurrentStateColorSlider("Eyebrow", c => c.GetChaControl().fileFace.eyebrowColor, color => UpdateHairColor(color, HairColor.Eyebrow)));
        }

        private static void UpdateTextures(ChaControl chaCtrl)
        {
            chaCtrl.AddUpdateCMFaceTexFlags(true, true, true, true, true, true, true);
            chaCtrl.AddUpdateCMFaceColorFlags(true, true, true, true, true, true, true);
            chaCtrl.CreateFaceTexture();
            chaCtrl.SetFaceBaseMaterial();

            chaCtrl.AddUpdateCMBodyTexFlags(true, true, true, true, true);
            chaCtrl.AddUpdateCMBodyColorFlags(true, true, true, true, true, true);
            chaCtrl.CreateBodyTexture();
            chaCtrl.SetBodyBaseMaterial();
        }

        internal static void UpdateTextureColor(Color color, TextureColor textureColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                UpdateTextureColor(cha, color, textureColor);
        }

        internal static void UpdateTextureColor(OCIChar ociChar, Color color, TextureColor textureColor)
        {
            var chaCtrl = ociChar.GetChaControl();
            switch (textureColor)
            {
                case TextureColor.SkinMain:
                    chaCtrl.fileBody.skinMainColor = color;
                    break;
                case TextureColor.SkinSub:
                    chaCtrl.fileBody.skinSubColor = color;
                    break;
                case TextureColor.Tan:
                    chaCtrl.fileBody.sunburnColor = color;
                    break;
            }
            UpdateTextures(chaCtrl);
        }

        internal static void UpdateBustSoftness(float value, Bust bust)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                UpdateBustSoftness(cha, value, bust);
        }

        internal static void UpdateBustSoftness(OCIChar ociChar, float value, Bust bust)
        {
            var chaCtrl = ociChar.GetChaControl();
            switch (bust)
            {
                case Bust.Softness:
                    chaCtrl.ChangeBustSoftness(value);
                    break;
                case Bust.Weight:
                    chaCtrl.ChangeBustGravity(value);
                    break;
            }
        }

        internal static void UpdateHairColor(Color color, HairColor hairColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                UpdateHairColor(cha, color, hairColor);
        }

        internal static void UpdateHairColor(OCIChar ociChar, Color color, HairColor hairColor)
        {
            var chaCtrl = ociChar.GetChaControl();
            switch (hairColor)
            {
                case HairColor.Base:
                    for(int i = 0;i < 4; i++)
                        chaCtrl.fileHair.parts[i].baseColor = color;
                    break;
                case HairColor.Start:
                    for (int i = 0; i < 4; i++)
                        chaCtrl.fileHair.parts[i].startColor = color;
                    break;
                case HairColor.End:
                    for (int i = 0; i < 4; i++)
                        chaCtrl.fileHair.parts[i].endColor = color;
                    break;
#if KKS
                case HairColor.Gloss:
                    for (int i = 0; i < 4; i++)
                    {
                        chaCtrl.fileHair.parts[i].glossColor = color;
                        chaCtrl.ChangeSettingHairGlossColor(i);
                    }
                    break;
#endif
                case HairColor.Eyebrow:
                    chaCtrl.fileFace.eyebrowColor = color;
                    chaCtrl.ChangeSettingEyebrowColor();
                    break;
            }
            for (int i = 0; i < 4; i++)
                chaCtrl.ChangeSettingHairColor(i, true, true, true);
        }

        internal enum TextureColor
        {
            SkinMain,
            SkinSub,
            Tan,
        }

        internal enum Bust
        {
            Softness,
            Weight,
        }

        internal enum HairColor
        {
            Base,
            Start,
            End,
            Gloss,
            Eyebrow,
        }
    }
}