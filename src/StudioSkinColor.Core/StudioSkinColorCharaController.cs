using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using Studio;
using System;
using System.Collections.Generic;
using System.Text;
using static Plugins.StudioSkinColor;
using UnityEngine;

namespace plugins
{
    internal class StudioSkinColorCharaController : CharaCustomFunctionController
    {
        private static readonly Dictionary<ChaControl, StudioSkinColorCharaController> allControllers = new Dictionary<ChaControl, StudioSkinColorCharaController>();

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            allControllers[ChaControl] = this;
        }

        public static StudioSkinColorCharaController GetController(ChaControl chaCtrl)
        {
            if (allControllers.ContainsKey(chaCtrl))
                return allControllers[chaCtrl];
            return null;
        }

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

        public void UpdateBustSoftness(float value, Bust bust)
        {
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

        public void UpdateHairColor(Color color, HairColor hairColor)
        {
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
    }
}
