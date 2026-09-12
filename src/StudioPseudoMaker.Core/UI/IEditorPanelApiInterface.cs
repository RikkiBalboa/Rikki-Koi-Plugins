using System;
using System.Collections.Generic;
using UnityEngine;

namespace PseudoMaker.UI
{
    public interface IEditorPanelApiInterface
    {
        GameObject AddSplitter(Transform parent = null);
        GameObject AddHeader(string name, Transform parent = null);
        ButtonGroupComponent AddButtonGroupRow(Dictionary<string, Action> buttonsMap, Transform parent = null);
        ButtonGroupComponent AddButtonRow(string name, Action onPressAction, Transform parent = null);
        ToggleComponent AddToggleRow(string name, Action<bool> onValueChanged, Func<bool> GetCurrentValue, Transform parent = null);
        ToggleGroupComponent AddToggleGroupRow(string label, IEnumerable<string> options, Action<int> onValueChanged, Func<int> GetCurrentValue, Func<int> getCurrentAvailableOptions = null, Transform parent = null);
        InputFieldComponent AddInputRow(string name, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2, float incrementValue = 1, Transform parent = null);
        TextInputFieldComponent AddTextInputRow(string name, Func<string> getCurrentValueAction, Action<string> setValueAction, string placeholder = "Type here...", Transform parent = null);
        SliderComponent AddSliderRow(string name, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2, Action onLabelClick = null, Transform parent = null);
        ColorComponent AddColorRow(string name, Func<Color> getCurrentValueAction, Func<Color> getOriginalValueAction, Action<Color> setValueAction, Action resetValueAction, Action onLabelClick = null, Transform parent = null);
        DropdownComponent AddDropdownRow(string name, List<string> options, Func<int> getCurrentValueAction, Action<int> setValueAction, Transform parent = null);
        ImageComponent AddImageRow(Func<Texture> getCurrentValueAction, Transform parent = null);
    }
}
