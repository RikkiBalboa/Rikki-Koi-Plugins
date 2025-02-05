using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
