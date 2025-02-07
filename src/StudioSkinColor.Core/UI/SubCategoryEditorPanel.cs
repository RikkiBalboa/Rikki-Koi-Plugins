using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    AddSlider(
                        value.Value,
                        () => StudioSkinColor.selectedCharacterController.GetCurrentBodyValue(value.Key),
                        () => StudioSkinColor.selectedCharacterController.GetOriginalBodyShapeValue(value.Key),
                        f => StudioSkinColor.selectedCharacterController.UpdateBodyShapeValue(value.Key, f),
                        () => StudioSkinColor.selectedCharacterController.ResetBodyShapeValue(value.Key)
                    );
            if (UIMappings.ShapeFaceValueMap.TryGetValue(SubCategory, out values))
                foreach (var value in values)
                    AddSlider(
                        value.Value,
                        () => StudioSkinColor.selectedCharacterController.GetCurrentFaceValue(value.Key),
                        () => StudioSkinColor.selectedCharacterController.GetOriginalFaceShapeValue(value.Key),
                        f => StudioSkinColor.selectedCharacterController.UpdateFaceShapeValue(value.Key, f),
                        () => StudioSkinColor.selectedCharacterController.ResetFaceShapeValue(value.Key)
                    );

            if (
                UIMappings.ShapeBodyValueMap.Where(x => x.Key == SubCategory).Select(x => x.Value).Count()
                + UIMappings.ShapeFaceValueMap.Where(x => x.Key == SubCategory).Select(x => x.Value).Count() > 0
            )
                AddSplitter();

            if (SubCategory == SubCategory.BodyGeneral)
            {
                AddBodyColorRow("Main Skin Color", ColorType.SkinMain);
                AddPickerRow(SelectKindType.BodyDetail);
            }

        }

        public GameObject AddSplitter()
        {
            return Instantiate(SplitterTemplate, SplitterTemplate.transform.parent);
        }

        public SliderComponent AddSlider(string name, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2)
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

        private void AddBodyColorRow(string name, ColorType colorType)
        {
            AddColorRow(
                name,
                () => StudioSkinColor.selectedCharacterController.GetColorPropertyValue(colorType),
                () => StudioSkinColor.selectedCharacterController.GetOriginalColorPropertyValue(colorType),
                c => StudioSkinColor.selectedCharacterController.UpdateColorProperty(c, colorType),
                () => StudioSkinColor.selectedCharacterController.ResetColorProperty(colorType)
            );
        }
    }
}
