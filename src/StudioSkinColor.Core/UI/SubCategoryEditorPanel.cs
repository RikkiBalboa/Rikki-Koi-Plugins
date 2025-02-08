using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    public class SubCategoryEditorPanel : MonoBehaviour
    {
        public SubCategory SubCategory;

        public ScrollRect scrollRect;

        public GameObject SliderTemplate;
        public GameObject ColorTemplate;
        public GameObject PickerTemplate;
        public GameObject SplitterTemplate;

        public List<SliderComponent> sliders;

        public void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();

            SliderTemplate = scrollRect.content.Find("SliderTemplate").gameObject;
            ColorTemplate = scrollRect.content.Find("ColorTemplate").gameObject;
            PickerTemplate = scrollRect.content.Find("PickerTemplate").gameObject;
            SplitterTemplate = scrollRect.content.Find("SplitterTemplate").gameObject;

            Initialize();

            Destroy(SliderTemplate);
            Destroy(ColorTemplate);
            Destroy(PickerTemplate);
            Destroy(SplitterTemplate);
        }

        public void Initialize()
        {
            if (UIMappings.ShapeBodyValueMap.TryGetValue(SubCategory, out var values))
                foreach (var value in values)
                    AddSliderRow(
                        value.Value,
                        () => StudioSkinColor.selectedCharacterController.GetCurrentBodyValue(value.Key),
                        () => StudioSkinColor.selectedCharacterController.GetOriginalBodyShapeValue(value.Key),
                        f => StudioSkinColor.selectedCharacterController.UpdateBodyShapeValue(value.Key, f),
                        () => StudioSkinColor.selectedCharacterController.ResetBodyShapeValue(value.Key)
                    );
            if (UIMappings.ShapeFaceValueMap.TryGetValue(SubCategory, out values))
                foreach (var value in values)
                    AddSliderRow(
                        value.Value,
                        () => StudioSkinColor.selectedCharacterController.GetCurrentFaceValue(value.Key),
                        () => StudioSkinColor.selectedCharacterController.GetOriginalFaceShapeValue(value.Key),
                        f => StudioSkinColor.selectedCharacterController.UpdateFaceShapeValue(value.Key, f),
                        () => StudioSkinColor.selectedCharacterController.ResetFaceShapeValue(value.Key)
                    );

            if (
                UIMappings.ShapeBodyValueMap.Where(x => x.Key == SubCategory).Select(x => x.Value).Count()
                + UIMappings.ShapeFaceValueMap.Where(x => x.Key == SubCategory).Select(x => x.Value).Count() > 0
                && SubCategory != SubCategory.BodyChest
            )
                AddSplitter();

            if (SubCategory == SubCategory.BodyGeneral)
            {
                AddPickerRow(SelectKindType.BodyDetail);
                AddSliderRow("Skin Type Strenth", FloatType.SkinTypeStrenth);
                AddSplitter();
                AddColorRow("Main Skin Color", ColorType.SkinMain);
                AddColorRow("Sub Skin Color", ColorType.SkinSub);
                AddSliderRow("Skin Gloss", FloatType.SkinGloss);
                AddSplitter();
                AddColorRow("Nail Color", ColorType.NailColor);
                AddSliderRow("Nail Gloss", FloatType.NailGloss);
            }
            else if (SubCategory == SubCategory.BodyChest)
            {
                AddSliderRow("Breast Weight", FloatType.Weight);
                AddSliderRow("Breast Softness", FloatType.Softness);
                AddSplitter();
                AddPickerRow(SelectKindType.Nip);
                AddColorRow("Nipple Color", ColorType.NippleColor);
                AddSliderRow("Nipple Gloss", FloatType.NippleGloss);
            }
            else if (SubCategory == SubCategory.BodyPubicHair)
            {
                AddPickerRow(SelectKindType.Underhair);
                AddColorRow("Pubic Hair Color", ColorType.PubicHairColor);
            }
            else if (SubCategory == SubCategory.BodySuntan)
            {
                AddPickerRow(SelectKindType.Sunburn);
                AddColorRow("Suntan Color", ColorType.SkinTan);
            }
            else if (SubCategory == SubCategory.FaceGeneral)
            {
                AddPickerRow(SelectKindType.HeadType);
                AddPickerRow(SelectKindType.FaceDetail);
                AddSliderRow("Face Overlay Strength", FloatType.FaceOverlayStrenth);
                AddSplitter();
            }
            else if (SubCategory == SubCategory.FaceCheeks)
            {
                AddSliderRow("Cheek Gloss", FloatType.CheekGloss);
            }
            else if (SubCategory == SubCategory.FaceEyebrows)
            {
                AddPickerRow(SelectKindType.Eyebrow);
                AddColorRow("Eyebrow Color", ColorType.EyebrowColor);
            }
            else if (SubCategory == SubCategory.FaceEyes)
            {
                AddPickerRow(SelectKindType.EyelineUp);
                AddPickerRow(SelectKindType.EyelineDown);
                AddColorRow("Eyeliner Color", ColorType.EyelineColor);
            }
            else if (SubCategory == SubCategory.FaceIris)
            {
                AddPickerRow(SelectKindType.EyeWGrade);
                AddColorRow("Sclera Color 1", ColorType.ScleraColor1);
                AddColorRow("Sclera Color 2", ColorType.ScleraColor2);
                AddSplitter();
                AddPickerRow(SelectKindType.EyeHLUp);
                AddColorRow("Upper Highlight Color", ColorType.UpperHighlightColor);
                AddPickerRow(SelectKindType.EyeHLDown);
                AddColorRow("Lower Highlight Color", ColorType.LowerHightlightColor);
                AddSliderRow("Upper Highlight Vertical", FloatType.UpperHighlightVertical);
                AddSliderRow("Upper Highlight Horizontal", FloatType.UpperHighlightHorizontal);
                AddSliderRow("Lower Highlight Vertical", FloatType.LowerHightlightVertical);
                AddSliderRow("Lower Highlight Horizontaal", FloatType.LowerHightlightHorizontal);
                AddSplitter();
                AddSliderRow("Iris Spacing", FloatType.IrisSpacing);
                AddSliderRow("Iris Vertical Position", FloatType.IrisVerticalPosition);
                AddSliderRow("Iris Width", FloatType.IrisWidth);
                AddSliderRow("Iris Height", FloatType.IrisHeight);
                AddSplitter();
                AddPickerRow(SelectKindType.Pupil);
                AddColorRow("Eye Color 1 (Left)", ColorType.EyeColor1Left);
                AddColorRow("Eye Color 2 (Left)", ColorType.EyeColor2Left);
                AddColorRow("Eye Color 1 (Right)", ColorType.EyeColor1Right);
                AddColorRow("Eye Color 2 (Right)", ColorType.EyeColor2Right);
                AddPickerRow(SelectKindType.PupilGrade);
                AddSliderRow("Eye Gradient Strength", FloatType.EyeGradientStrenth);
                AddSliderRow("Eye Gradient Vertical", FloatType.EyeGradientSize);
            }
            else if (SubCategory == SubCategory.FaceNose)
            {
                AddPickerRow(SelectKindType.Nose);
            }
            else if (SubCategory == SubCategory.FaceMouth)
            {
                AddPickerRow(SelectKindType.Lipline);
                AddColorRow("Lip Line Color", ColorType.LipLineColor);
                AddSplitter();
                AddSliderRow("Lip Gloss", FloatType.LipGloss);
            }
            else if (SubCategory == SubCategory.FaceMakeup)
            {
                AddPickerRow(SelectKindType.Eyebrow);
                AddColorRow("Eye Shadow Color", ColorType.EyeShadowColor);
                AddSplitter();
                AddPickerRow(SelectKindType.Cheek);
                AddColorRow("Eye Shadow Color", ColorType.CheekColor);
                AddSplitter();
                AddPickerRow(SelectKindType.Lip);
                AddColorRow("Lip Color", ColorType.LipColor);
                AddSplitter();
            }
            if (SubCategory.ToString().StartsWith("Clothing"))
                DrawClothesCategories();
        }

        private void DrawClothesCategories()
        {
            SelectKindType selectKindType = SelectKindType.CosTop;
            if (SubCategory == SubCategory.ClothingTop) selectKindType = SelectKindType.CosTop;
            else if (SubCategory == SubCategory.ClothingBottom) selectKindType = SelectKindType.CosBot;
            else if (SubCategory == SubCategory.ClothingBra) selectKindType = SelectKindType.CosBra;
            else if (SubCategory == SubCategory.ClothingUnderwear) selectKindType = SelectKindType.CosShorts;
            else if (SubCategory == SubCategory.ClothingGloves) selectKindType = SelectKindType.CosGloves;
            else if (SubCategory == SubCategory.ClothingPantyhose) selectKindType = SelectKindType.CosPanst;
            else if (SubCategory == SubCategory.ClothingLegwear) selectKindType = SelectKindType.CosSocks;
#if KK
            else if (SubCategory == SubCategory.ClothingShoesInDoors) selectKindType = SelectKindType.CosInnerShoes;
            else if (SubCategory == SubCategory.ClothingShoesOutdoors) selectKindType = SelectKindType.CosOuterShoes;
#else
            else if (SubCategory == SubCategory.ClothingShoes) selectKindType = SelectKindType.CosOuterShoes;
#endif
            AddPickerRow(selectKindType);
            if (SubCategory == SubCategory.ClothingTop)
            {
                AddPickerRow(SelectKindType.CosJacket01);
                AddPickerRow(SelectKindType.CosJacket02);
                AddPickerRow(SelectKindType.CosJacket03);
                AddPickerRow(SelectKindType.CosSailor01);
                AddPickerRow(SelectKindType.CosSailor02);
                AddPickerRow(SelectKindType.CosSailor03);
            }

            for (int i = 0; i < 3; i++)
                AddPatternRows(SubCategory, selectKindType, i);
        }

        private void AddPatternRows(SubCategory subcategory, SelectKindType selectKindType, int colorNr)
        {
            AddSplitter();
            AddColorRow(SubCategory, colorNr);
            AddPickerRow((SelectKindType)Enum.Parse(typeof(SelectKindType), $"{selectKindType}Ptn0{colorNr + 1}", true));
            AddSliderRow(SubCategory, colorNr, PatternValue.Horizontal);
            AddSliderRow(SubCategory, colorNr, PatternValue.Vertical);
#if KKS
            AddSliderRow(SubCategory, colorNr, PatternValue.Rotation);
            AddSliderRow(SubCategory, colorNr, PatternValue.Width);
            AddSliderRow(SubCategory, colorNr, PatternValue.Height);
#endif
            AddColorRow(SubCategory, colorNr, true);
        }

        public GameObject AddSplitter()
        {
            var splitter = Instantiate(SplitterTemplate, SplitterTemplate.transform.parent);
            splitter.name = "Splitter";
            return splitter;
        }

        public SliderComponent AddSliderRow(string name, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2)
        {
            var slider = Instantiate(SliderTemplate, SliderTemplate.transform.parent);
            slider.name = $"Slider{name.Replace(" ", "")}";

            var sliderComponent = slider.AddComponent<SliderComponent>();
            sliderComponent.Name = name;
            sliderComponent.MinValue = minValue;
            sliderComponent.MaxValue = maxValue;
            sliderComponent.GetCurrentValue = getCurrentValueAction;
            sliderComponent.GetOriginalValue = getOriginalValueAction;
            sliderComponent.SetValueAction = setValueAction;
            sliderComponent.ResetValueAction = resetValueAction;


            return sliderComponent;
        }

        private void AddSliderRow(string name, FloatType floatType)
        {
            AddSliderRow(
                name,
                () => StudioSkinColor.selectedCharacterController.GetFloatValue(floatType),
                () => StudioSkinColor.selectedCharacterController.GetOriginalFloatValue(floatType),
                value => StudioSkinColor.selectedCharacterController.SetFloatTypeValue(value, floatType),
                () => StudioSkinColor.selectedCharacterController.ResetFloatTypeValue(floatType)
            );
        }

        private void AddSliderRow(SubCategory subCategory, int colorNr, PatternValue pattern)
        {
            int clothingKind = StudioSkinColorCharaController.SubCategoryToKind(subCategory);

            AddSliderRow(
                $"Pattern {colorNr + 1} {pattern}",
                () => StudioSkinColor.selectedCharacterController.GetPatternValue(clothingKind, colorNr, pattern),
                () => 0.5f,
                value => StudioSkinColor.selectedCharacterController.SetPatternValue(clothingKind, colorNr, pattern, value),
                () => StudioSkinColor.selectedCharacterController.SetPatternValue(clothingKind, colorNr, pattern, 0.5f)
            );
        }

        public ColorComponent AddColorRow(string name, Func<Color> getCurrentValueAction, Func<Color> getOriginalValueAction, Action<Color> setValueAction, Action resetValueAction)
        {
            var button = Instantiate(ColorTemplate, ColorTemplate.transform.parent);
            button.name = $"ColorPicker{name.Replace(" ", "")}";

            var colorComponent = button.AddComponent<ColorComponent>();
            colorComponent.Name = name;
            colorComponent.GetCurrentValue = getCurrentValueAction;
            colorComponent.GetOriginalValue = getOriginalValueAction;
            colorComponent.SetValueAction = setValueAction;
            colorComponent.ResetValueAction = resetValueAction;

            return colorComponent;
        }

        private void AddColorRow(string name, ColorType colorType)
        {
            AddColorRow(
                name,
                () => StudioSkinColor.selectedCharacterController.GetColorPropertyValue(colorType),
                () => StudioSkinColor.selectedCharacterController.GetOriginalColorPropertyValue(colorType),
                c => StudioSkinColor.selectedCharacterController.UpdateColorProperty(c, colorType),
                () => StudioSkinColor.selectedCharacterController.ResetColorProperty(colorType)
            );
        }

        private void AddColorRow(SubCategory subCategory, int colorNr, bool isPattern = false)
        {
            int clothingKind = StudioSkinColorCharaController.SubCategoryToKind(subCategory);

            AddColorRow(
                isPattern ? $"Pattern Color {colorNr + 1}" : $"Cloth Color {colorNr + 1}",
                () => StudioSkinColor.selectedCharacterController.GetClothingColor(clothingKind, colorNr, isPattern: isPattern),
                () => StudioSkinColor.selectedCharacterController.GetOriginalClothingColor(clothingKind, colorNr, isPattern: isPattern),
                c => StudioSkinColor.selectedCharacterController.SetClothingColor(clothingKind, colorNr, c, isPattern: isPattern),
                () => StudioSkinColor.selectedCharacterController.ResetClothingColor(clothingKind, colorNr, isPattern: isPattern)
            );
        }

        public PickerComponent AddPickerRow(SelectKindType selectKind)
        {
            var name = UIMappings.GetSelectKindTypeName(selectKind);

            var picker = Instantiate(PickerTemplate, SliderTemplate.transform.parent);
            picker.name = $"CategoryPicker{name.Replace(" ", "")}";

            ChaListDefine.CategoryNo[] array = new ChaListDefine.CategoryNo[100]
            {
                ChaListDefine.CategoryNo.mt_face_detail,
                ChaListDefine.CategoryNo.mt_eyebrow,
                ChaListDefine.CategoryNo.mt_eyeline_up,
                ChaListDefine.CategoryNo.mt_eyeline_down,
                ChaListDefine.CategoryNo.mt_eye_white,
                ChaListDefine.CategoryNo.mt_eye_hi_up,
                ChaListDefine.CategoryNo.mt_eye_hi_down,
                ChaListDefine.CategoryNo.mt_eye,
                ChaListDefine.CategoryNo.mt_eye_gradation,
                ChaListDefine.CategoryNo.mt_nose,
                ChaListDefine.CategoryNo.mt_lipline,
                ChaListDefine.CategoryNo.mt_mole,
                ChaListDefine.CategoryNo.mt_eyeshadow,
                ChaListDefine.CategoryNo.mt_cheek,
                ChaListDefine.CategoryNo.mt_lip,
                ChaListDefine.CategoryNo.mt_face_paint,
                ChaListDefine.CategoryNo.mt_face_paint,
                ChaListDefine.CategoryNo.mt_body_detail,
                ChaListDefine.CategoryNo.mt_nip,
                ChaListDefine.CategoryNo.mt_underhair,
                ChaListDefine.CategoryNo.mt_sunburn,
                ChaListDefine.CategoryNo.mt_body_paint,
                ChaListDefine.CategoryNo.mt_body_paint,
                ChaListDefine.CategoryNo.bodypaint_layout,
                ChaListDefine.CategoryNo.bodypaint_layout,
                ChaListDefine.CategoryNo.bo_hair_b,
                ChaListDefine.CategoryNo.bo_hair_f,
                ChaListDefine.CategoryNo.bo_hair_s,
                ChaListDefine.CategoryNo.bo_hair_o,
                ChaListDefine.CategoryNo.co_top,
                ChaListDefine.CategoryNo.cpo_sailor_a,
                ChaListDefine.CategoryNo.cpo_sailor_b,
                ChaListDefine.CategoryNo.cpo_sailor_c,
                ChaListDefine.CategoryNo.cpo_jacket_a,
                ChaListDefine.CategoryNo.cpo_jacket_b,
                ChaListDefine.CategoryNo.cpo_jacket_c,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_bot,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_bra,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_shorts,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_gloves,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_panst,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_socks,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_shoes,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_shoes,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_hairgloss,
                ChaListDefine.CategoryNo.bo_head,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem
            };
            ChaListDefine.CategoryNo cn = array[(int)selectKind];

            var pickerComponent = picker.AddComponent<PickerComponent>();
            pickerComponent.Name = name;
            pickerComponent.CategoryNo = cn;
            pickerComponent.GetCurrentValue = () => StudioSkinColor.selectedCharacterController.GetSelected(selectKind);
            pickerComponent.SetCurrentValue = (value) => StudioSkinColor.selectedCharacterController.SetSelectKind(selectKind, value);

            return pickerComponent;
        }
    }
}
