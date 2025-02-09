using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class AccessoryPanel : MonoBehaviour
    {
        public AccessoryEditorPanel editorPanel;

        public GameObject ToggleTemplate;

        public ScrollRect PanelScroll;
        public ToggleGroup PanelToggleGroup;

        private void Awake()
        {
            gameObject.name = "AccessorySelectorPanel";
            PanelScroll = gameObject.GetComponent<ScrollRect>();
            PanelToggleGroup = gameObject.GetComponent<ToggleGroup>();
            ToggleTemplate = PanelScroll.content.Find("ToggleTemplate").gameObject;
            ToggleTemplate.SetActive(false);
        }

        private void OnEnable()
        {
            clearChildren();
            var toggles = new List<Toggle>();
            var accessories = PseudoMaker.selectedCharacter.infoAccessory;
            for (int i = 0; i < accessories.Length; i++)
            {
                var _i = i;
                var accessory = accessories[i];

                var go = Instantiate(ToggleTemplate, PanelScroll.content, false);
                go.SetActive(true);

                var text = go.GetComponentInChildren<Text>(true);
                text.resizeTextMaxSize = 14;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 6;
                if (accessory != null) TranslationHelper.TranslateAsync(accessory.Name, value => text.text = $"{i + 1}. {value}");
                else text.text = $"Slot {i + 1}";

                var toggle = go.GetComponent<Toggle>();
                toggle.isOn = false;
                toggle.onValueChanged.AddListener((change) => 
                {
                    editorPanel?.ChangeSelectedAccessory(_i);
                });
                toggle.group = PanelToggleGroup;
                toggles.Add(toggle);
            }
            toggles[0].isOn = true;
        }

        private void clearChildren()
        {
            int i = 0;

            //Array to hold all child obj
            GameObject[] allChildren = new GameObject[PanelScroll.content.childCount];

            //Find all child obj and store to that array
            foreach (Transform child in PanelScroll.content)
            {
                allChildren[i] = child.gameObject;
                i += 1;
            }

            //Now destroy them
            foreach (GameObject child in allChildren.Skip(1))
            {
                DestroyImmediate(child.gameObject);
            }
        } 
    }
}
