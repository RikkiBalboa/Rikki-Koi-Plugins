using ADV.Commands.Object;
using KoiSkinOverlayX;
using PseudoMaker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class BaseEditorPanel : MonoBehaviour
    {
        public SubCategory SubCategory;

        protected ScrollRect scrollRect;

        protected GameObject SliderTemplate;
        protected GameObject InputTemplate;
        protected GameObject ButtonGroupTemplate;
        protected GameObject ColorTemplate;
        protected GameObject PickerTemplate;
        protected GameObject DropdownTemplate;
        protected GameObject ClothingOptionTemplate;
        protected GameObject ToggleOptionTemplate;
        protected GameObject SplitterTemplate;
        protected GameObject HeaderTemplate;
        protected GameObject TransferRowTemplate;
        protected GameObject AccessoryCopyRowTemplace;
        protected GameObject ClothingCopyRowTemplace;
        protected GameObject ImageRowTemplate;

        protected void Awake()
        {
            try
            {
                scrollRect = GetComponentInChildren<ScrollRect>();

                SliderTemplate = scrollRect.content.Find("SliderTemplate").gameObject;
                InputTemplate = scrollRect.content.Find("InputFieldTemplate").gameObject;
                ButtonGroupTemplate = scrollRect.content.Find("ButtonGroupTemplate").gameObject;
                ColorTemplate = scrollRect.content.Find("ColorTemplate").gameObject;
                PickerTemplate = scrollRect.content.Find("PickerTemplate").gameObject;
                DropdownTemplate = scrollRect.content.Find("DropdownTemplate").gameObject;
                ClothingOptionTemplate = scrollRect.content.Find("ClothingOptionTemplate").gameObject;
                ToggleOptionTemplate = scrollRect.content.Find("ToggleOptionTemplate").gameObject;
                SplitterTemplate = scrollRect.content.Find("SplitterTemplate").gameObject;
                HeaderTemplate = scrollRect.content.Find("HeaderTemplate").gameObject;
                TransferRowTemplate = scrollRect.content.Find("AccessoryTransferRowRemplate").gameObject;
                AccessoryCopyRowTemplace = scrollRect.content.Find("AccessoryCopyRowTemplate").gameObject;
                ClothingCopyRowTemplace = scrollRect.content.Find("ClothingCopyRowTemplate").gameObject;
                ImageRowTemplate = scrollRect.content.Find("ImageRowTemplate").gameObject;

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
                Destroy(TransferRowTemplate);
                Destroy(AccessoryCopyRowTemplace);
                Destroy(ClothingCopyRowTemplace);
                Destroy(ImageRowTemplate);
            }
            catch (Exception e)
            {
                PseudoMaker.Logger.LogMessage("An error occured during the initialization of the Pseudo Maker UI. This shouldn't happen and is a bug. Check the logs for more details");
                PseudoMaker.Logger.LogError(e);
            }
        }

        public static T CreatePanel<T>(SubCategory subCategory) where T : BaseEditorPanel
        {
            var panel = Instantiate(CategoryPanel.EditorPanelTemplate);
            panel.name = $"Category{subCategory}Editor";
            panel.transform.SetParent(CategoryPanel.EditorPanelTemplate.transform.parent, false);

            var editor = panel.AddComponent<T>();
            editor.SubCategory = subCategory;

            return editor;
        }

        protected virtual void Initialize() { }

        public GameObject AddSplitter(Transform parent = null)
        {
            var splitter = Instantiate(SplitterTemplate, parent == null ? scrollRect.content : parent);
            splitter.name = "Splitter";
            return splitter;
        }

        public GameObject AddHeader(string name, Transform parent = null)
        {
            var header = Instantiate(HeaderTemplate, parent == null ? scrollRect.content : parent);
            header.name = "Header";

            header.GetComponentInChildren<Text>().text = name;
            return header;
        }

        public Toggle AddHeaderToggle(string name, Action<bool> onValueChanged, Transform parent = null)
        {
            var header = Instantiate(HeaderTemplate, parent == null ? scrollRect.content : parent);
            header.name = "Header";

            var text = header.GetComponentInChildren<Text>();
            text.text = name;
            var toggle = text.gameObject.AddComponent<Toggle>();
            toggle.m_Colors.highlightedColor = new Color(0.74f, 0.8f, 0.96f);
            toggle.onValueChanged.AddListener(value => onValueChanged(value));

            return toggle;
        }

        public ButtonGroupComponent AddButtonGroupRow(Dictionary<string, Action> buttonsMap, Transform parent = null)
        {
            var buttonGroup = Instantiate(ButtonGroupTemplate, parent == null ? scrollRect.content : parent);
            buttonGroup.name = "ButtonsGroup";

            var buttonGroupComponent = buttonGroup.AddComponent<ButtonGroupComponent>();
            buttonGroupComponent.ButtonsMap = buttonsMap;

            return buttonGroupComponent;
        }

        public ButtonGroupComponent AddButtonRow(string name, Action onPressAction, Transform parent = null)
        {
            return AddButtonGroupRow(new Dictionary<string, Action>() { { name, onPressAction } }, parent);
        }

        public ToggleComponent AddToggleRow(string name, Action<bool> onValueChanged, Func<bool> GetCurrentValue, Transform parent = null)
        {
            var toggleObject = Instantiate(ToggleOptionTemplate, parent == null ? scrollRect.content : parent);
            toggleObject.name = $"Toggle{name.Replace(" ", "")}";

            var toggle = toggleObject.AddComponent<ToggleComponent>();
            toggle.Name = name;
            toggle.SetValueAction = onValueChanged;
            toggle.GetCurrentValue = GetCurrentValue;

            return toggle;
        }

        public InputFieldComponent AddInputRow(string name, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2, float incrementValue = 1, Transform parent = null)
        {
            var inputField = Instantiate(InputTemplate, parent == null ? scrollRect.content : parent);
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

        public SliderComponent AddSliderRow(string name, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2, Action onLabelClick = null, Transform parent = null)
        {
            var slider = Instantiate(SliderTemplate, parent == null ? scrollRect.content : parent);
            slider.name = $"Slider{name.Replace(" ", "")}";

            var sliderComponent = slider.AddComponent<SliderComponent>();
            sliderComponent.Name = name;
            sliderComponent.MinValue = minValue;
            sliderComponent.MaxValue = maxValue;
            sliderComponent.GetCurrentValue = getCurrentValueAction;
            sliderComponent.GetOriginalValue = getOriginalValueAction;
            sliderComponent.SetValueAction = setValueAction;
            sliderComponent.ResetValueAction = resetValueAction;
            sliderComponent.OnLabelClick = onLabelClick;

            return sliderComponent;
        }

        public SliderComponent AddSliderRow(string name, FloatType floatType, Transform parent = null)
        {
            return AddSliderRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetFloatValue(floatType),
                () => PseudoMaker.selectedCharacterController.GetOriginalFloatValue(floatType),
                value => PseudoMaker.selectedCharacterController.SetFloatTypeValue(value, floatType),
                () => PseudoMaker.selectedCharacterController.ResetFloatTypeValue(floatType),
                onLabelClick: () => TimelineCompatibilityHelper.SelectedFloatType = floatType,
                parent: parent
            );
        }

        public ColorComponent AddColorRow(string name, Func<Color> getCurrentValueAction, Func<Color> getOriginalValueAction, Action<Color> setValueAction, Action resetValueAction, Action onLabelClick = null, Transform parent = null)
        {
            var button = Instantiate(ColorTemplate, parent == null ? scrollRect.content : parent);
            button.name = $"ColorPicker{name.Replace(" ", "")}";

            var colorComponent = button.AddComponent<ColorComponent>();
            colorComponent.Name = name;
            colorComponent.GetCurrentValue = getCurrentValueAction;
            colorComponent.GetOriginalValue = getOriginalValueAction;
            colorComponent.SetValueAction = setValueAction;
            colorComponent.ResetValueAction = resetValueAction;
            colorComponent.OnLabelClick = onLabelClick;

            return colorComponent;
        }

        public ColorComponent AddColorRow(string name, ColorType colorType, Transform parent = null)
        {
            return AddColorRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetColorPropertyValue(colorType),
                () => PseudoMaker.selectedCharacterController.GetOriginalColorPropertyValue(colorType),
                c => PseudoMaker.selectedCharacterController.UpdateColorProperty(c, colorType),
                () => PseudoMaker.selectedCharacterController.ResetColorProperty(colorType),
                onLabelClick: () => TimelineCompatibilityHelper.SelectedColorType = colorType,
                parent: parent
            );
        }

        public PickerComponent AddPickerRow(SelectKindType selectKind, Action onChange = null, Transform parent = null)
        {
            var name = UIMappings.GetSelectKindTypeName(selectKind);

            var picker = Instantiate(PickerTemplate, parent == null ? scrollRect.content : parent);
            picker.name = $"CategoryPicker{name.Replace(" ", "")}";

            ChaListDefine.CategoryNo[] array = new ChaListDefine.CategoryNo[104]
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
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_eye,
                ChaListDefine.CategoryNo.mt_eye_gradation,
                ChaListDefine.CategoryNo.mt_eye,
                ChaListDefine.CategoryNo.mt_eye_gradation,
            };
            ChaListDefine.CategoryNo cn = array[(int)selectKind];

            var pickerComponent = picker.AddComponent<PickerComponent>();
            pickerComponent.GetId = () => $"{cn}_{selectKind}";
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

        public DropdownComponent AddDropdownRow(string name, List<string> options, Func<int> getCurrentValueAction, Action<int> setValueAction, Transform parent = null)
        {
            var dropdown = Instantiate(DropdownTemplate, parent == null ? scrollRect.content : parent);
            dropdown.name = $"Dropdown{name.Replace(" ", "")}";

            var dropdownComponent = dropdown.AddComponent<DropdownComponent>();
            dropdownComponent.DropdownOptions = options;
            dropdownComponent.Name = name;
            dropdownComponent.GetCurrentValue = getCurrentValueAction;
            dropdownComponent.SetValueAction = setValueAction;

            return dropdownComponent;
        }

        public ImageComponent AddImageRow(Func<Texture> getCurrentValueAction, Transform parent = null)
        {
            var image = Instantiate(ImageRowTemplate, parent == null ? scrollRect.content : parent);

            var imageComponent = image.AddComponent<ImageComponent>();
            imageComponent.GetCurrentValue = getCurrentValueAction;

            return imageComponent;
        }

        public CopyComponent AddCopyRow(int index, bool isClothing, Func<string> getFromName, Func<string> getToName)
        {
            var row = Instantiate(isClothing ? AccessoryCopyRowTemplace : ClothingCopyRowTemplace, AccessoryCopyRowTemplace.transform.parent);

            var copyComponent = row.AddComponent<CopyComponent>();
            copyComponent.GetFromName = getFromName;
            copyComponent.GetToName = getToName;
            return copyComponent;
        }

        public void AddSkinOverlayRow(TexType texType, string title, Action onDone = null, Func<int> getEyeToEdit = null)
        {
            AddSplitter();

            AddHeader(title);

            AddImageRow(
                () => Compatibility.SkinOverlays.GetTex(GetTexType(true))
            );

            AddButtonRow(
                "Load New Texture",
                () => Compatibility.SkinOverlays.ImportOverlay(GetTexType(false), onDone)
            );

            AddButtonRow(
                "Clear Texture",
                () => {
                    Compatibility.SkinOverlays.SetTexAndUpdate(null, GetTexType(false));
                    onDone?.Invoke();
                }
            );

            AddButtonRow(
                "Export Current Texture",
                () => Compatibility.SkinOverlays.ExportOverlay(GetTexType(true))
            );


            TexType GetTexType(bool cantBeBoth)
            {
                if (getEyeToEdit != null)
                {
                    var eyeToEdit = getEyeToEdit();
                    if (eyeToEdit == 0)
                        return cantBeBoth ? texType + 2 : texType; // left or both
                    if (eyeToEdit == 1)
                        return texType + 2; // left
                    if (eyeToEdit == 2)
                        return texType + 4; // right
                }
                return texType;
            }
        }
    }
}
