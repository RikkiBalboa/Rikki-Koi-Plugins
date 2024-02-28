using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using Sirenix.Serialization.Utilities;
using Studio;
using System;
using UniRx;
using UnityEngine;
using static Plugins.StudioSkinColor;

namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class StudioSkinColor : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.studioSkinColor";
        public const string PluginName = "StudioSkinColor";
        public const string PluginNameInternal = Constants.Prefix + "_StudioSkinColor";
        public const string PluginVersion = "0.1";
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
            }
        }

        private void RegisterStudioControls()
        {
            var catBody = StudioAPI.GetOrCreateCurrentStateCategory("Body");
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => c.GetChaControl().fileBody.skinMainColor, c => UpdateTextureColor(c, TextureColor.SkinMain)));
            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => c.GetChaControl().fileBody.skinSubColor, c => UpdateTextureColor(c, TextureColor.SkinSub)));
            catBody.AddControl(new CurrentStateColorSlider("Tan", c => c.GetChaControl().fileBody.sunburnColor, c => UpdateTextureColor(c, TextureColor.Tan)));
            catBody.AddControl(new CurrentStateCategorySwitch("Detail Line", c => c.GetChaControl().fileBody.drawAddLine)).Value.Subscribe(value => UpdateCustomBody(value, CustomBody.DrawAddLine));
            catBody.AddControl(new CurrentStateCategorySlider("Detail Power", c => c.GetChaControl().fileBody.detailPower, 0, 1)).Value.Subscribe(value => UpdateCustomBody(value, CustomBody.DetailPower));
            catBody.AddControl(new CurrentStateColorSlider("Pubic hair", c => c.GetChaControl().fileBody.underhairColor, value => UpdateCustomBody(value, CustomBody.PubicHairColor)));
            catBody.AddControl(new CurrentStateColorSlider("Nipple", c => c.GetChaControl().fileBody.nipColor, value => UpdateCustomBody(value, CustomBody.NippleColor)));
            catBody.AddControl(new CurrentStateCategorySlider("Nipple size", c => c.GetChaControl().fileBody.areolaSize, 0, 1)).Value.Subscribe(value => UpdateCustomBody(value, CustomBody.AreolaSize));

            var catBust = StudioAPI.GetOrCreateCurrentStateCategory("Bust");
            catBust.AddControl(new CurrentStateCategorySlider("Softness", c => c.GetChaControl().fileBody.bustSoftness, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Softness));
            catBust.AddControl(new CurrentStateCategorySlider("Weight", c => c.GetChaControl().fileBody.bustWeight, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Weight));

            var catHair = StudioAPI.GetOrCreateCurrentStateCategory("Hair");
            catHair.AddControl(new CurrentStateColorSlider("Color 1", c => c.GetChaControl().fileHair.parts[0].baseColor, color => UpdateHairColor(color, HairColor.Base)));
            catHair.AddControl(new CurrentStateColorSlider("Color 2", c => c.GetChaControl().fileHair.parts[0].startColor, color => UpdateHairColor(color, HairColor.Start)));
            catHair.AddControl(new CurrentStateColorSlider("Color 3", c => c.GetChaControl().fileHair.parts[0].endColor, color => UpdateHairColor(color, HairColor.End)));
            catHair.AddControl(new CurrentStateColorSlider("Gloss", c => c.GetChaControl().fileHair.parts[0].glossColor, UpdateHairGlossColor));
            catHair.AddControl(new CurrentStateColorSlider("Eyebrow", c => c.GetChaControl().fileFace.eyebrowColor, UpdateEyebrowColor));
        }

        private void UpdateTextures(ChaControl chaCtrl)
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

        private void UpdateTextureColor(Color color, TextureColor textureColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
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
        }

        private void UpdateCustomBody(object value, CustomBody customBody)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                switch (customBody)
                {
                    case CustomBody.DrawAddLine:
                        chaCtrl.fileBody.drawAddLine = (bool)value;
                        break;
                    case CustomBody.DetailPower:
                        chaCtrl.fileBody.detailPower = (float)value;
                        break;
                    case CustomBody.PubicHairColor:
                        chaCtrl.fileBody.underhairColor = (Color)value;
                        break;
                    case CustomBody.NippleColor:
                        chaCtrl.fileBody.nipColor = (Color)value;
                        break;
                    case CustomBody.AreolaSize:
                        chaCtrl.fileBody.areolaSize = (float)value;
                        break;
                }
                chaCtrl.ChangeCustomBodyWithoutCustomTexture();
            }
        }

        private void UpdateBustSoftness(float value, Bust bust)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
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
        }

        private void UpdateHairColor(Color color, HairColor hairColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                switch (hairColor)
                {
                    case HairColor.Base:
                        chaCtrl.fileHair.parts[0].baseColor = color;
                        break;
                    case HairColor.Start:
                        chaCtrl.fileHair.parts[0].startColor = color;
                        break;
                    case HairColor.End:
                        chaCtrl.fileHair.parts[0].endColor = color;
                        break;
                }
                chaCtrl.ChangeSettingHairColor(0, true, true, true);
            }
        }

        private void UpdateHairGlossColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileHair.parts[0].glossColor = color;
                chaCtrl.ChangeSettingHairGlossColor(0);
            }
        }

        private void UpdateEyebrowColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileFace.eyebrowColor = color;
                chaCtrl.ChangeSettingEyebrowColor();
            }
        }

        internal enum TextureColor
        {
            SkinMain,
            SkinSub,
            Tan,
        }

        internal enum CustomBody
        {
            DrawAddLine,
            DetailPower,
            PubicHairColor,
            NippleColor,
            AreolaSize,
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
        }
    }
}