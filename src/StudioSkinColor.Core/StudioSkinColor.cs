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
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => c.GetChaControl().fileBody.skinMainColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.skinMainColor = c;
                    UpdateTextures(chaCtrl);
                }
            }));

            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => c.GetChaControl().fileBody.skinSubColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.skinSubColor = c;
                    UpdateTextures(chaCtrl);
                }
            }));

            catBody.AddControl(new CurrentStateCategorySwitch("Detail Line", c => c.GetChaControl().fileBody.drawAddLine)).Value.Subscribe(addLine =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.drawAddLine = addLine;
                    chaCtrl.ChangeCustomBodyWithoutCustomTexture();
                }
            });

            catBody.AddControl(new CurrentStateCategorySlider("Detail Power", c => c.GetChaControl().fileBody.detailPower, 0, 1)).Value.Subscribe(power =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.detailPower = power;
                    chaCtrl.ChangeCustomBodyWithoutCustomTexture();
                }
            });

            catBody.AddControl(new CurrentStateColorSlider("Tan", c =>
            c.GetChaControl().fileBody.sunburnColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.sunburnColor = c;
                    UpdateTextures(chaCtrl);
                }
            }));

            catBody.AddControl(new CurrentStateColorSlider("Pubic hair", c => c.GetChaControl().fileBody.underhairColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.underhairColor = c;
                    chaCtrl.ChangeCustomBodyWithoutCustomTexture();
                }
            }));

            catBody.AddControl(new CurrentStateColorSlider("Nipple", c => c.GetChaControl().fileBody.nipColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.nipColor = c;
                    chaCtrl.ChangeCustomBodyWithoutCustomTexture();
                    chaCtrl.ChangeSettingHairColor(0, true, true, true);
                }
            }));

            catBody.AddControl(new CurrentStateCategorySlider("Nipple size", c => c.GetChaControl().fileBody.areolaSize, 0, 1)).Value.Subscribe(size =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileBody.areolaSize = size;
                    chaCtrl.ChangeCustomBodyWithoutCustomTexture();
                }
            });

            var catBust = StudioAPI.GetOrCreateCurrentStateCategory("Bust");
            catBust.AddControl(new CurrentStateCategorySlider("Softness", c => c.GetChaControl().fileBody.bustSoftness, 0, 1)).Value.Subscribe(soft =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.ChangeBustSoftness(soft);
                }
            });

            catBust.AddControl(new CurrentStateCategorySlider("Weight", c => c.GetChaControl().fileBody.bustWeight, 0, 1)).Value.Subscribe(weight =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.ChangeBustGravity(weight);
                }
            });

            var catHair = StudioAPI.GetOrCreateCurrentStateCategory("Hair");
            catHair.AddControl(new CurrentStateColorSlider("Color 1", c => c.GetChaControl().fileHair.parts[0].baseColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileHair.parts[0].baseColor = c;
                    chaCtrl.ChangeSettingHairColor(0, true, true, true);
                }
            }));
            catHair.AddControl(new CurrentStateColorSlider("Color 2", c => c.GetChaControl().fileHair.parts[0].startColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileHair.parts[0].startColor = c;
                    chaCtrl.ChangeSettingHairColor(0, true, true, true);
                }
            }));
            catHair.AddControl(new CurrentStateColorSlider("Color 3", c => c.GetChaControl().fileHair.parts[0].endColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileHair.parts[0].endColor = c;
                    chaCtrl.ChangeSettingHairColor(0, true, true, true);
                }
            }));
            catHair.AddControl(new CurrentStateColorSlider("Gloss", c => c.GetChaControl().fileHair.parts[0].glossColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileHair.parts[0].glossColor = c;
                    chaCtrl.ChangeSettingHairColor(0, true, true, true);
                }
            }));
            catHair.AddControl(new CurrentStateColorSlider("Eyebrow", c => c.GetChaControl().fileFace.eyebrowColor, c =>
            {
                foreach (var cha in StudioAPI.GetSelectedCharacters())
                {
                    var chaCtrl = cha.GetChaControl();
                    chaCtrl.fileFace.eyebrowColor = c;
                    chaCtrl.ChangeSettingEyebrowColor();
                }
            }));
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
    }
}