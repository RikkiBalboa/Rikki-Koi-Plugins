using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    public class BaseEditorPanel : MonoBehaviour
    {
        public SubCategory SubCategory;

        public ScrollRect scrollRect;

        public GameObject SliderTemplate;
        public GameObject InputTemplate;
        public GameObject ButtonGroupTemplate;
        public GameObject ColorTemplate;
        public GameObject PickerTemplate;
        public GameObject DropdownTemplate;
        public GameObject ClothingOptionTemplate;
        public GameObject ToggleOptionTemplate;
        public GameObject SplitterTemplate;
        public GameObject HeaderTemplate;

        protected void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();

            SliderTemplate = scrollRect.content.Find("SliderTemplate").gameObject;
            InputTemplate = scrollRect.content.Find("InputFieldTemplate").gameObject;
            ButtonGroupTemplate = scrollRect.content.Find("ButtonGroupTemplate").gameObject;
            ColorTemplate = scrollRect.content.Find("ColorTemplate").gameObject;
            PickerTemplate = scrollRect.content.Find("PickerTemplate").gameObject;
            DropdownTemplate= scrollRect.content.Find("DropdownTemplate").gameObject;
            ClothingOptionTemplate = scrollRect.content.Find("ClothingOptionTemplate").gameObject;
            ToggleOptionTemplate = scrollRect.content.Find("ToggleOptionTemplate").gameObject;
            SplitterTemplate = scrollRect.content.Find("SplitterTemplate").gameObject;
            HeaderTemplate = scrollRect.content.Find("HeaderTemplate").gameObject;

            Initialize();

            Destroy(SliderTemplate);
            Destroy(InputTemplate);
            Destroy(ButtonGroupTemplate);
            Destroy(ColorTemplate);
            Destroy(PickerTemplate);
            Destroy(DropdownTemplate);
            Destroy(ClothingOptionTemplate);
            Destroy(ToggleOptionTemplate);
            Destroy(SplitterTemplate);
            Destroy(HeaderTemplate);
        }

        protected virtual void Initialize() { }

        public GameObject AddSplitter()
        {
            var splitter = Instantiate(SplitterTemplate, SplitterTemplate.transform.parent);
            splitter.name = "Splitter";
            return splitter;
        }

        public GameObject AddHeader(string name)
        {
            var header = Instantiate(HeaderTemplate, SplitterTemplate.transform.parent);
            header.name = "Header";

            header.GetComponentInChildren<Text>().text = name;
            return header;
        }

        public Toggle AddHeaderToggle(string name, Action<bool> onValueChanged)
        {
            var header = Instantiate(HeaderTemplate, SplitterTemplate.transform.parent);
            header.name = "Header";

            var text = header.GetComponentInChildren<Text>();
            text.text = name;
            var toggle = text.gameObject.AddComponent<Toggle>();
            toggle.m_Colors.highlightedColor = new Color(0.74f, 0.8f, 0.96f);
            toggle.onValueChanged.AddListener(value => onValueChanged(value));

            return toggle;
        }

        public ButtonGroupComponent AddButtonGroupRow(Dictionary<string, Action> buttonsMap)
        {
            var buttonGroup = Instantiate(ButtonGroupTemplate, ButtonGroupTemplate.transform.parent);
            buttonGroup.name = "ButtonsGroup";

            var buttonGroupComponent = buttonGroup.AddComponent<ButtonGroupComponent>();
            buttonGroupComponent.ButtonsMap = buttonsMap;

            return buttonGroupComponent;
        }

        public ToggleComponent AddToggleRow(string name, Action<bool> onValueChanged, Func<bool> GetCurrentValue)
        {
            var toggleObject = Instantiate(ToggleOptionTemplate, ToggleOptionTemplate.transform.parent);
            toggleObject.name = $"Toggle{name.Replace(" ", "")}";

            var toggle = toggleObject.AddComponent<ToggleComponent>();
            toggle.Name = name;
            toggle.SetValueAction = onValueChanged;
            toggle.GetCurrentValue = GetCurrentValue;

            return toggle;
        }

        public InputFieldComponent AddInputRow(string name, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2, float incrementValue = 1)
        {
            var inputField = Instantiate(InputTemplate, InputTemplate.transform.parent);
            inputField.name = $"InputField{name.Replace(" ", "")}";

            var inputFieldComponent = inputField.AddComponent<InputFieldComponent>();
            inputFieldComponent.Name = name;
            inputFieldComponent.MinValue = minValue;
            inputFieldComponent.MaxValue = maxValue;
            inputFieldComponent.IncrementValue = incrementValue;
            inputFieldComponent.GetCurrentValue = getCurrentValueAction;
            inputFieldComponent.GetOriginalValue = getOriginalValueAction;
            inputFieldComponent.SetValueAction = setValueAction;
            inputFieldComponent.ResetValueAction = resetValueAction;

            return inputFieldComponent;
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

        public SliderComponent AddSliderRow(string name, FloatType floatType)
        {
            return AddSliderRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetFloatValue(floatType),
                () => PseudoMaker.selectedCharacterController.GetOriginalFloatValue(floatType),
                value => PseudoMaker.selectedCharacterController.SetFloatTypeValue(value, floatType),
                () => PseudoMaker.selectedCharacterController.ResetFloatTypeValue(floatType)
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

        public ColorComponent AddColorRow(string name, ColorType colorType)
        {
            return AddColorRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetColorPropertyValue(colorType),
                () => PseudoMaker.selectedCharacterController.GetOriginalColorPropertyValue(colorType),
                c => PseudoMaker.selectedCharacterController.UpdateColorProperty(c, colorType),
                () => PseudoMaker.selectedCharacterController.ResetColorProperty(colorType)
            );
        }

        public PickerComponent AddPickerRow(SelectKindType selectKind, Action onChange = null)
        {
            var name = UIMappings.GetSelectKindTypeName(selectKind);

            var picker = Instantiate(PickerTemplate, PickerTemplate.transform.parent);
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
            pickerComponent.GetCurrentValue = () => PseudoMaker.selectedCharacterController.GetSelected(selectKind);
            pickerComponent.SetCurrentValue = (value) =>
            {
                PseudoMaker.selectedCharacterController.SetSelectKind(selectKind, value);
                onChange?.Invoke();
            };

            return pickerComponent;
        }

        public DropdownComponent AddDropdownRow(string name, List<string> options, Func<int> getCurrentValueAction, Action<int> setValueAction)
        {
            var dropdown = Instantiate(DropdownTemplate, DropdownTemplate.transform.parent);
            dropdown.name = $"Dropdown{name.Replace(" ", "")}";

            var dropdownComponent = dropdown.AddComponent<DropdownComponent>();
            dropdownComponent.DropdownOptions = options;
            dropdownComponent.Name = name;
            dropdownComponent.GetCurrentValue = getCurrentValueAction;
            dropdownComponent.SetValueAction = setValueAction;

            return dropdownComponent;
        }
    }
}
