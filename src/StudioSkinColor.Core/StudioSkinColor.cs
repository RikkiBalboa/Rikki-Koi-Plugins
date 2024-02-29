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
using static Plugins.StudioSkinColor;

namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class StudioSkinColor : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.studioSkinColor";
        public const string PluginName = "StudioSkinColor";
        public const string PluginNameInternal = Constants.Prefix + "_StudioSkinColor";
        public const string PluginVersion = "0.2";
        internal static new ManualLogSource Logger;
        public static ConfigEntry<KeyboardShortcut> KeyResetAll { get; private set; }

        internal static Dictionary<OCIChar, Dictionary<ModifiedValue, object>> DefaultValues;

        private void Awake()
        {
            Logger = base.Logger;
            KeyResetAll = Config.Bind(
                "Keyboard Shortcuts", "Reset All",
                new KeyboardShortcut(KeyCode.G, KeyCode.RightControl)
            );
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneController>(PluginGUID);
        }

        private void Update()
        {
            if (KeyResetAll.Value.IsDown() && StudioAPI.InsideStudio)
            {
                foreach(var ociChar in DefaultValues.Keys)
                {
                    foreach (var modifiedValue in DefaultValues[ociChar])
                    {
                        switch (modifiedValue.Key.MethodType)
                        {
                            case MethodType.UpdateTexture:
                                UpdateTextureColor((Color)modifiedValue.Value, (TextureColor)modifiedValue.Key.FieldEnum);
                                break;
                        }
                    }
                        
                }
            }
        }

        private void Start()
        {
            if (StudioAPI.InsideStudio)
            {
                DefaultValues = new Dictionary<OCIChar, Dictionary<ModifiedValue, object>>();
                RegisterStudioControls();
            }
        }

        private void RegisterStudioControls()
        {
            var catBody = StudioAPI.GetOrCreateCurrentStateCategory("Body");
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => SetDefaultValue(c, MethodType.UpdateTexture, (int)TextureColor.SkinMain, c.GetChaControl().fileBody.skinMainColor), c => UpdateTextureColor(c, TextureColor.SkinMain)));
            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => SetDefaultValue(c, MethodType.UpdateTexture, (int)TextureColor.SkinSub, c.GetChaControl().fileBody.skinSubColor), c => UpdateTextureColor(c, TextureColor.SkinSub)));
            catBody.AddControl(new CurrentStateColorSlider("Tan", c => SetDefaultValue(c, MethodType.UpdateTexture, (int)TextureColor.Tan, c.GetChaControl().fileBody.sunburnColor), c => UpdateTextureColor(c, TextureColor.Tan)));

            catBody.AddControl(new CurrentStateCategorySwitch("Detail Line", c => SetDefaultValue(c, MethodType.UpdateCustomBody, (int)CustomBody.DrawAddLine, c.GetChaControl().fileBody.drawAddLine))).Value.Subscribe(value => UpdateCustomBody(value, CustomBody.DrawAddLine));
            catBody.AddControl(new CurrentStateCategorySlider("Detail Power", c => SetDefaultValue(c, MethodType.UpdateCustomBody, (int)CustomBody.DetailPower, c.GetChaControl().fileBody.detailPower), 0, 1)).Value.Subscribe(value => UpdateCustomBody(value, CustomBody.DetailPower));
            catBody.AddControl(new CurrentStateColorSlider("Pubic hair", c => SetDefaultValue(c, MethodType.UpdateCustomBody, (int)CustomBody.PubicHairColor, c.GetChaControl().fileBody.underhairColor), value => UpdateCustomBody(value, CustomBody.PubicHairColor)));
            catBody.AddControl(new CurrentStateColorSlider("Nipple", c => SetDefaultValue(c, MethodType.UpdateCustomBody, (int)CustomBody.NippleColor, c.GetChaControl().fileBody.nipColor), value => UpdateCustomBody(value, CustomBody.NippleColor)));
            catBody.AddControl(new CurrentStateCategorySlider("Nipple size", c => SetDefaultValue(c, MethodType.UpdateCustomBody, (int)CustomBody.AreolaSize, c.GetChaControl().fileBody.areolaSize), 0, 1)).Value.Subscribe(value => UpdateCustomBody(value, CustomBody.AreolaSize));

            var catBust = StudioAPI.GetOrCreateCurrentStateCategory("Bust");
            catBust.AddControl(new CurrentStateCategorySlider("Softness", c => SetDefaultValue(c, MethodType.UpdateBust, (int)Bust.Softness, c.GetChaControl().fileBody.bustSoftness), 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Softness));
            catBust.AddControl(new CurrentStateCategorySlider("Weight", c => SetDefaultValue(c, MethodType.UpdateBust, (int)Bust.Weight, c.GetChaControl().fileBody.bustWeight), 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Weight));

            var catHair = StudioAPI.GetOrCreateCurrentStateCategory("Hair");
            catHair.AddControl(new CurrentStateColorSlider("Color 1", c => SetDefaultValue(c, MethodType.UpdateHairColor, (int)HairColor.Base, c.GetChaControl().fileHair.parts[0].baseColor), color => UpdateHairColor(color, HairColor.Base)));
            catHair.AddControl(new CurrentStateColorSlider("Color 2", c => SetDefaultValue(c, MethodType.UpdateHairColor, (int)HairColor.Start, c.GetChaControl().fileHair.parts[0].startColor), color => UpdateHairColor(color, HairColor.Start)));
            catHair.AddControl(new CurrentStateColorSlider("Color 3", c => SetDefaultValue(c, MethodType.UpdateHairColor, (int)HairColor.End, c.GetChaControl().fileHair.parts[0].endColor), color => UpdateHairColor(color, HairColor.End)));
#if KKS
            catHair.AddControl(new CurrentStateColorSlider("Gloss", c => SetDefaultValue(c, MethodType.UpdateHairColor, 0, c.GetChaControl().fileHair.parts[0].glossColor), color => UpdateHairColor(color, HairColor.Gloss)));
#endif
            catHair.AddControl(new CurrentStateColorSlider("Eyebrow", c => SetDefaultValue(c, MethodType.UpdateHairColor, 0, c.GetChaControl().fileFace.eyebrowColor), color => UpdateHairColor(color, HairColor.Eyebrow)));
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

        private T SetDefaultValue<T>(OCIChar ociChar, MethodType methodType, int fieldEnum, T value)
        {
            if (!DefaultValues.ContainsKey(ociChar))
                DefaultValues[ociChar] = new Dictionary<ModifiedValue, object>();

            ModifiedValue modifiedValue = new ModifiedValue(methodType, fieldEnum);
            if (DefaultValues[ociChar].ContainsKey(modifiedValue))
                return value;

            DefaultValues[ociChar][modifiedValue] = value;
            return value;
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
            Gloss,
            Eyebrow,
        }
        public enum MethodType
        {
            UpdateTexture,
            UpdateCustomBody,
            UpdateBust,
            UpdateHairColor,
        }

        [MessagePackObject]
        public struct ModifiedValue
        {
            [Key(0)]
            public MethodType MethodType { get; }
            [Key(1)]
            public int FieldEnum { get; }

            public ModifiedValue(MethodType methodType, int fieldEnum)
            {
                MethodType = methodType;
                FieldEnum = fieldEnum;
            }
        }
    }
}