using KKAPI.Utilities;
using NodeCanvas.Tasks.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class AccessoryPanel : MonoBehaviour
    {
        public AccessoryEditorPanel editorPanel;
        public AccessoryTransferPanel transferPanel;

        public GameObject ToggleTemplate;

        public ScrollRect PanelScroll;
        public ToggleGroup PanelToggleGroup;
        private Toggle addSlotToggle;

        private static List<Toggle> toggles = new List<Toggle>();
        private RectTransform rectTransform;


        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            //rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, rectTransform.offsetMin.y + 66);
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, rectTransform.offsetMin.y + 44);

            gameObject.name = "AccessorySelectorPanel";
            PanelScroll = gameObject.GetComponent<ScrollRect>();
            PanelToggleGroup = gameObject.GetComponent<ToggleGroup>();
            ToggleTemplate = PanelScroll.content.Find("ToggleTemplate").gameObject;
            ToggleTemplate.SetActive(false);


            AddToggle(0, "Transfer", value => {
                editorPanel.gameObject.SetActive(false);
                transferPanel.gameObject.SetActive(true);
            });
            //AddToggle(1, "Copy", value => { });
            addSlotToggle = AddToggle(1, "+1", value => { if (value) addSlotToggle.isOn = false; }, true);
        }

        private Toggle AddToggle(int position, string label, Action<bool> onValueChanged, bool isButton = false)
        {
            var go = Instantiate(ToggleTemplate, transform.parent);
            go.name = $"AccessoryToggle{label}";
            go.SetActive(true);

            var toggleTransform = go.transform as RectTransform;
            var delta = (toggleTransform.sizeDelta.y / 2) * (-position * 2 + 1) - toggleTransform.sizeDelta.y - 2;
            toggleTransform.offsetMin = new Vector2(1, toggleTransform.offsetMin.y - delta);
            toggleTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, toggleTransform.offsetMax.y - delta);

            var text = go.GetComponentInChildren<Text>(true);
            text.text = label;

            var toggle = go.GetComponent<Toggle>();
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(change => onValueChanged(change));
            if (!isButton) toggle.group = PanelToggleGroup;
            return toggle;
        }

        private void OnEnable()
        {
            var accessories = PseudoMaker.selectedCharacter.infoAccessory;
            int processedAccessories = 0;

            for (int i = 0; i < toggles.Count && i < accessories.Length; i++)
            {
                var _i = i;
                var accessory = accessories[i];

                toggles[i].onValueChanged.RemoveAllListeners();
                toggles[i].onValueChanged.AddListener((change) =>
                {
                    editorPanel.gameObject.SetActive(true);
                    transferPanel.gameObject.SetActive(false);
                    editorPanel?.ChangeSelectedAccessory(_i, accessory != null);
                });

                var text = toggles[i].gameObject.GetComponentInChildren<Text>(true);
                SetAccessoryName(i, text);
                processedAccessories++;
            }

            for (int i = processedAccessories; i < accessories.Length; i++)
            {
                var _i = i;
                var accessory = accessories[i];

                var go = Instantiate(ToggleTemplate, PanelScroll.content, false);
                go.name = $"AccessoryToggle{i}";
                go.SetActive(true);

                var text = go.GetComponentInChildren<Text>(true);
                text.resizeTextMaxSize = 14;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 6;
                SetAccessoryName(i, text);

                var toggle = go.GetComponent<Toggle>();
                toggle.isOn = false;
                toggle.onValueChanged.AddListener((change) =>
                {
                    editorPanel.gameObject.SetActive(true);
                    transferPanel.gameObject.SetActive(false);
                    editorPanel?.ChangeSelectedAccessory(_i, accessory != null);
                });
                toggle.group = PanelToggleGroup;
                toggles.Add(toggle);
                processedAccessories++;
            }

            for (var i = toggles.Count - 1; i >= processedAccessories; i--)
            {
                Destroy(toggles[i].gameObject);
                toggles.RemoveAt(i);
            }
        }

        private static void SetAccessoryName(int slotNr, Text text)
        {
            var accessory = PseudoMaker.selectedCharacter.infoAccessory[slotNr];
            if (accessory != null)
            {
                text.text = $"{slotNr + 1}. {accessory.Name}";
                TranslationHelper.TranslateAsync(accessory.Name, value => text.text = $"{slotNr + 1}. {value}");
            }
            else text.text = $"Slot {slotNr + 1}";
        }

        public static void UpdateSlotName(int slotNr)
        {
            if (slotNr < toggles.Count)
                SetAccessoryName(slotNr, toggles[slotNr].gameObject.GetComponentInChildren<Text>(true));
        }
    }
}
