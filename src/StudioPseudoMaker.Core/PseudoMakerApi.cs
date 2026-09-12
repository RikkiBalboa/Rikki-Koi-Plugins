using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KK_Plugins.MaterialEditor;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Utilities;
using PseudoMaker.UI;
using Studio;
using System.Linq;
using ChaCustom;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PseudoMaker
{
    internal class PseudoMakerApi
    {
        /// <summary>
        /// Fires after the pseudo maker UI is fully initialized and thus UI elements can be added. Attach your own init code here.
        /// Fired only once.
        /// </summary>
        public static event Action PseudoMakerInitialized;

        /// <summary>
        /// Occurs when the selected accessory in the UI changes.
        /// Send what slotNr is selected and whether the accessory exists (true) or is None (false).
        /// </summary>
        public static event Action<int, bool> SelectedAccessoryChanged;

        public static ChaControl SelectedCharacter => PseudoMaker.selectedCharacter;

        internal static void AnnounceInit()
        {
            PseudoMakerInitialized?.Invoke();
        }

        internal static void ChangeSelectedAccessory(int slotNr, bool exists)
        {
            SelectedAccessoryChanged?.Invoke(slotNr, exists);
        }

        public static Transform GetPanelTransform(SubCategory subCategory)
        {
            return PseudoMaker.MainWindow.CategoryPanels.SelectMany(x => x.Value.SubCategoryPanels).FirstOrDefault(x => x.Value.SubCategory == subCategory).Value.transform;
        }

        public static GameObject AddSplitter(Transform parent)
        {
           return CategoryPanel.EditorPanelTemplate.AddSplitter(parent);
        }

        public static GameObject AddHeader(string name, Transform parent)
        {
            return CategoryPanel.EditorPanelTemplate.AddHeader(name, parent);
        }

        public static ButtonGroupComponent AddButtonGroupRow(Dictionary<string, Action> buttonsMap, Transform parent)
        {
            return CategoryPanel.EditorPanelTemplate.AddButtonGroupRow(buttonsMap, parent);
        }

        public static ButtonGroupComponent AddButtonRow(string name, Transform parent, Action onPressAction)
        {
            return AddButtonGroupRow(new Dictionary<string, Action>() { { name, onPressAction } }, parent);
        }

        public static ToggleComponent AddToggleRow(string name, Transform parent, Action<bool> onValueChanged, Func<bool> GetCurrentValue)
        {
            return CategoryPanel.EditorPanelTemplate.AddToggleRow(name, onValueChanged, GetCurrentValue, parent);
        }

        public static ToggleGroupComponent AddToggleGroupRow(string label, Transform parent, IEnumerable<string> options, Action<int> onValueChanged, Func<int> GetCurrentValue, Func<int> getCurrentAvailableOptions = null)
        {
            return CategoryPanel.EditorPanelTemplate.AddToggleGroupRow(label, options, onValueChanged, GetCurrentValue, getCurrentAvailableOptions, parent);
        }

        public static InputFieldComponent AddInputRow(string name, Transform parent, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2, float incrementValue = 1)
        {
            return CategoryPanel.EditorPanelTemplate.AddInputRow(name, getCurrentValueAction, getOriginalValueAction, setValueAction, resetValueAction, minValue, maxValue, incrementValue, parent);
        }

        public static TextInputFieldComponent AddTextInputRow(string name, Transform parent, Func<string> getCurrentValueAction, Action<string> setValueAction, string placeholder = "Type here...")
        {
            return CategoryPanel.EditorPanelTemplate.AddTextInputRow(name, getCurrentValueAction, setValueAction, placeholder, parent);
        }

        public static SliderComponent AddSliderRow(string name, Transform parent, Func<float> getCurrentValueAction, Func<float> getOriginalValueAction, Action<float> setValueAction, Action resetValueAction, float minValue = -1, float maxValue = 2, Action onLabelClick = null)
        {
            return CategoryPanel.EditorPanelTemplate.AddSliderRow(name, getCurrentValueAction, getOriginalValueAction, setValueAction, resetValueAction, minValue, maxValue, onLabelClick, parent);
        }

        public static ColorComponent AddColorRow(string name, Transform parent, Func<Color> getCurrentValueAction, Func<Color> getOriginalValueAction, Action<Color> setValueAction, Action resetValueAction, Action onLabelClick = null)
        {
            return CategoryPanel.EditorPanelTemplate.AddColorRow(name, getCurrentValueAction, getOriginalValueAction, setValueAction, resetValueAction, onLabelClick, parent);
        }

        public static DropdownComponent AddDropdownRow(string name, Transform parent, List<string> options, Func<int> getCurrentValueAction, Action<int> setValueAction)
        {
            return CategoryPanel.EditorPanelTemplate.AddDropdownRow(name, options, getCurrentValueAction, setValueAction, parent);
        }

        public static ImageComponent AddImageRow(Func<Texture> getCurrentValueAction, Transform parent)
        {
            return CategoryPanel.EditorPanelTemplate.AddImageRow(getCurrentValueAction, parent);
        }
    }
}
