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
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => c.GetChaControl().fileBody.skinMainColor, UpdateMainSkinColor));
            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => c.GetChaControl().fileBody.skinSubColor, UpdateSubSkinColor));
            catBody.AddControl(new CurrentStateColorSlider("Tan", c => c.GetChaControl().fileBody.sunburnColor, UpdateTanColor));
            catBody.AddControl(new CurrentStateCategorySwitch("Detail Line", c => c.GetChaControl().fileBody.drawAddLine)).Value.Subscribe(UpdateDetailLine);
            catBody.AddControl(new CurrentStateCategorySlider("Detail Power", c => c.GetChaControl().fileBody.detailPower, 0, 1)).Value.Subscribe(UpdateDetailPower);
            catBody.AddControl(new CurrentStateColorSlider("Pubic hair", c => c.GetChaControl().fileBody.underhairColor, UpdatePubicHairColor));
            catBody.AddControl(new CurrentStateColorSlider("Nipple", c => c.GetChaControl().fileBody.nipColor, UpdateNippleColor));
            catBody.AddControl(new CurrentStateCategorySlider("Nipple size", c => c.GetChaControl().fileBody.areolaSize, 0, 1)).Value.Subscribe(UpdateNippleSizePower);

            var catBust = StudioAPI.GetOrCreateCurrentStateCategory("Bust");
            catBust.AddControl(new CurrentStateCategorySlider("Softness", c => c.GetChaControl().fileBody.bustSoftness, 0, 1)).Value.Subscribe(UpdateBustSoftness);
            catBust.AddControl(new CurrentStateCategorySlider("Weight", c => c.GetChaControl().fileBody.bustWeight, 0, 1)).Value.Subscribe(UpdateBustWeight);

            var catHair = StudioAPI.GetOrCreateCurrentStateCategory("Hair");
            catHair.AddControl(new CurrentStateColorSlider("Color 1", c => c.GetChaControl().fileHair.parts[0].baseColor, UpdateHairBaseColor));
            catHair.AddControl(new CurrentStateColorSlider("Color 2", c => c.GetChaControl().fileHair.parts[0].startColor, UpdateHairStartColor));
            catHair.AddControl(new CurrentStateColorSlider("Color 3", c => c.GetChaControl().fileHair.parts[0].endColor, UpdateHairEndColor));
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

        private void UpdateMainSkinColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.skinMainColor = color;
                UpdateTextures(chaCtrl);
            }
        }

        private void UpdateSubSkinColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.skinSubColor = color;
                UpdateTextures(chaCtrl);
            }
        }

        private void UpdateTanColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.sunburnColor = color;
                UpdateTextures(chaCtrl);
            }
        }

        private void UpdateDetailLine(bool addLine)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.drawAddLine = addLine;
                chaCtrl.ChangeCustomBodyWithoutCustomTexture();
            }
        }

        private void UpdateDetailPower(float power)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.detailPower = power;
                chaCtrl.ChangeCustomBodyWithoutCustomTexture();
            }
        }

        private void UpdatePubicHairColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.underhairColor = color;
                chaCtrl.ChangeCustomBodyWithoutCustomTexture();
            }
        }

        private void UpdateNippleColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.nipColor = color;
                chaCtrl.ChangeCustomBodyWithoutCustomTexture();
            }
        }

        private void UpdateNippleSizePower(float size)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileBody.areolaSize = size;
                chaCtrl.ChangeCustomBodyWithoutCustomTexture();
            }
        }

        private void UpdateBustSoftness(float soft)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.ChangeBustSoftness(soft);
            }
        }

        private void UpdateBustWeight(float weight)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.ChangeBustGravity(weight);
            }
        }

        private void UpdateHairBaseColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileHair.parts[0].baseColor = color;
                chaCtrl.ChangeSettingHairColor(0, true, true, true);
            }
        }

        private void UpdateHairStartColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileHair.parts[0].startColor = color;
                chaCtrl.ChangeSettingHairColor(0, true, true, true);
            }
        }

        private void UpdateHairEndColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileHair.parts[0].endColor = color;
                chaCtrl.ChangeSettingHairColor(0, true, true, true);
            }
        }

        private void UpdateHairGlossColor(Color color)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
            {
                var chaCtrl = cha.GetChaControl();
                chaCtrl.fileHair.parts[0].glossColor = color;
                chaCtrl.ChangeSettingHairColor(0, true, true, true);
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
    }
}