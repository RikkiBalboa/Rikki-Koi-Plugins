using KKAPI;
using KKAPI.Chara;
using System.Collections.Generic;
using static Plugins.StudioSkinColor;
using UnityEngine;
using KK_Plugins.MaterialEditor;
using System.Linq;

namespace Plugins
{
    internal class StudioSkinColorCharaController : CharaCustomFunctionController
    {
        internal static readonly Dictionary<ChaControl, StudioSkinColorCharaController> allControllers = new Dictionary<ChaControl, StudioSkinColorCharaController>();

        #region Save Lists
        private readonly List<ClothingColors> defaultClothingColors = new List<ClothingColors>();
        #endregion

        #region Character Properties shortcuts
        private int CurrentOutfitSlot => ChaControl.fileStatus.coordinateType;
        private ChaFileClothes Clothes => ChaControl.nowCoordinate.clothes;
        private ChaFileClothes SetClothes => ChaControl.chaFile.coordinate[ChaControl.chaFile.status.coordinateType].clothes;
        #endregion

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            allControllers[ChaControl] = this;
            defaultClothingColors.Clear();
        }

        public static StudioSkinColorCharaController GetController(ChaControl chaCtrl)
        {
            StudioSkinColorCharaController controller = null;
            if (allControllers.ContainsKey(chaCtrl))
                controller = allControllers[chaCtrl];
            if (controller == null)
                controller = chaCtrl.gameObject.GetOrAddComponent<StudioSkinColorCharaController>();
            return controller;
        }

        #region body
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
            return ChaControl.infoClothes[kind].Name == "None";
        }

        public void InitBaseCustomTextureClothesIfNotExists(int kind)
        {
            if (selectedCharacter.ctCreateClothes[kind, 0] == null)
                selectedCharacter.InitBaseCustomTextureClothes(true, kind);
        }

        public void SetClothingColor(int kind, int colorNr, Color color)
        {
            var MEController = MaterialEditorPlugin.GetCharaController(ChaControl);
            if (MEController != null)
            {
                MEController.CustomClothesOverride = true;
                MEController.RefreshClothesMainTex();
            }

            if (!defaultClothingColors.Exists(x => x.Compare(CurrentOutfitSlot, kind, colorNr)))
            {
                defaultClothingColors.Add(new ClothingColors(CurrentOutfitSlot, kind, colorNr, GetClothingColor(kind, colorNr)));
            }

            Clothes.parts[kind].colorInfo[colorNr].baseColor = color;
            SetClothes.parts[kind].colorInfo[colorNr].baseColor = color;
            selectedCharacter.ChangeCustomClothes(true, kind, true, true, true, true, true);
        }

        public Color GetClothingColor(int kind, int colorNr)
        {
            return Clothes.parts[kind].colorInfo[colorNr].baseColor;
        }

        public void ResetClothingColor(int kind, int colorNr)
        {
            var color = defaultClothingColors.FirstOrDefault(x => x.Compare(CurrentOutfitSlot, kind, colorNr));
            if (color != null)
                SetClothingColor(kind, colorNr, color.Color);
        }
        #endregion
    }

    internal class ClothingColors
    {
        public int OutfitSlot { get; set; }
        public int ClothingKind { get; set; }
        public int ColorNr { get; set; }
        public Color Color { get; set; }

        public ClothingColors(int outfitSlot, int kind, int colorNr, Color color)
        {
            OutfitSlot = outfitSlot;
            ClothingKind = kind;
            ColorNr = colorNr;
            Color = color;
        }

        public bool Compare(int outfitSlot, int kind, int colorNr)
        {
            if (
                OutfitSlot == outfitSlot
                && ClothingKind == kind
                && ColorNr == colorNr
            )
                return true;
            return false;
        }
    }
}
