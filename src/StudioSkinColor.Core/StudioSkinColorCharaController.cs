using KKAPI;
using KKAPI.Chara;
using System.Collections.Generic;
using static Plugins.StudioSkinColor;
using UnityEngine;
using KK_Plugins.MaterialEditor;
using System.Linq;
using static KKAPI.Maker.MakerConstants;

namespace Plugins
{
    internal class StudioSkinColorCharaController : CharaCustomFunctionController
    {
        internal static readonly Dictionary<ChaControl, StudioSkinColorCharaController> allControllers = new Dictionary<ChaControl, StudioSkinColorCharaController>();

        #region Save Lists
        private readonly List<ClothingColors> originalClothingColors = new List<ClothingColors>();
        private readonly List<HairColors> originalHairColors = new List<HairColors>();
        private readonly List<BodyColors> originalBodyColors = new List<BodyColors>();
        private readonly List<BustValues> originalBustValues = new List<BustValues>();
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
            originalClothingColors.Clear();
            originalHairColors.Clear();
            originalBodyColors.Clear();
            originalBustValues.Clear();
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
            if (!originalBodyColors.Exists(x => x.ColorType == textureColor))
                originalBodyColors.Add(new BodyColors(textureColor, GetBodyColor(textureColor)));

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

        public void SetBustValue(float value, Bust bust)
        {
            if (!originalBustValues.Exists(x => x.Bust == bust))
                originalBustValues.Add(new BustValues(bust, value));

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
            var value = originalBustValues.FirstOrDefault(x => x.Bust == bust);
            if(value != null)
                SetBustValue(value.Value, bust);
        }

        public float GetBustValue(Bust bust)
        {
            if (bust == Bust.Softness)
                return ChaControl.fileBody.bustSoftness;
            else if (bust == Bust.Weight)
                return ChaControl.fileBody.bustWeight;
            return 1f;
        }

        public void UpdateHairColor(Color color, HairColor hairColor)
        {
            if (!originalHairColors.Exists(x => x.HairColor == hairColor))
                originalHairColors.Add(new HairColors(hairColor, GetHairColor(hairColor)));

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
            var color = originalHairColors.FirstOrDefault(x => x.HairColor == hairColor);
            if (color != null)
                UpdateHairColor(color.Color, hairColor);
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

        public Color GetBodyColor(TextureColor color)
        {
            switch (color)
            {
                case TextureColor.SkinMain:
                    return ChaControl.fileBody.skinMainColor;
                case TextureColor.SkinSub:
                    return ChaControl.fileBody.skinSubColor;
                case TextureColor.Tan:
                    return ChaControl.fileBody.sunburnColor;
            }
            return Color.white;
        }

        public void ResetBodyColor(TextureColor colorType)
        {
            var color = originalBodyColors.FirstOrDefault(x => x.ColorType == colorType);
            if (color != null)
                UpdateTextureColor(color.Color, colorType);
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

            if (!originalClothingColors.Exists(x => x.Compare(CurrentOutfitSlot, kind, colorNr)))
            {
                originalClothingColors.Add(new ClothingColors(CurrentOutfitSlot, kind, colorNr, GetClothingColor(kind, colorNr)));
            }

            Clothes.parts[kind].colorInfo[colorNr].baseColor = color;
            SetClothes.parts[kind].colorInfo[colorNr].baseColor = color;
            if (!IsMultiPartTop(kind))
                selectedCharacter.ChangeCustomClothes(true, kind, true, true, true, true, true);
            else
                for (int i = 0; i < Clothes.subPartsId.Length; i++)
                {
                    ChaControl.ChangeCustomClothes(main: false, i, updateColor: true, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: false);
                }
        }

        public bool[] CheckClothingUseColor(int kind)
        {
            bool[] useCols = new bool[4] { false, false, false, false };

            if (!IsMultiPartTop(kind))
            {
                var clothesComponent = ChaControl.GetCustomClothesComponent(kind);
                if (clothesComponent != null)
                {
                    useCols[0] = clothesComponent.useColorN01;
                    useCols[1] = clothesComponent.useColorN02;
                    useCols[2] = clothesComponent.useColorN03;
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

        public Color GetClothingColor(int kind, int colorNr)
        {
            return Clothes.parts[kind].colorInfo[colorNr].baseColor;
        }

        public void ResetClothingColor(int kind, int colorNr)
        {
            var color = originalClothingColors.FirstOrDefault(x => x.Compare(CurrentOutfitSlot, kind, colorNr));
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

    internal class HairColors
    {
        public HairColor HairColor { get; set; }
        public Color Color { get; set; }

        public HairColors(HairColor hairColor, Color color)
        {
            HairColor = hairColor;
            Color = color;
        }
    }

    internal class BodyColors
    {
        public TextureColor ColorType { get; set; }
        public Color Color { get; set; }

        public BodyColors(TextureColor colorType, Color color)
        {
            ColorType = colorType;
            Color = color;
        }
    }

    internal class BustValues
    {
        public Bust Bust { get; set; }
        public float Value { get; set; }

        public BustValues(Bust bust, float value)
        {
            Bust = bust;
            Value = value;
        }
    }
}
