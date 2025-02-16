using KKAPI.Utilities;
using NodeCanvas.Tasks.Actions;
using System.Collections.Generic;
using System.Linq;
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

        private List<Toggle> toggles = new List<Toggle>();

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
            var accessories = PseudoMaker.selectedCharacter.infoAccessory;
            int processedAccessories = 0;

            for (int i = 0; i < toggles.Count && i < accessories.Length; i++)
            {
                var _i = i;
                var accessory = accessories[i];

                toggles[i].onValueChanged.RemoveAllListeners();
                toggles[i].onValueChanged.AddListener((change) =>
                {
                    editorPanel?.ChangeSelectedAccessory(_i, accessory != null);
                });

                var text = toggles[i].gameObject.GetComponentInChildren<Text>(true);
                if (accessory != null)
                {
                    text.text = $"{i + 1}. {accessory.Name}";
                    TranslationHelper.TranslateAsync(accessory.Name, value => text.text = $"{i + 1}. {value}");
                }
                else text.text = $"Slot {i + 1}";
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
                if (accessory != null)
                {
                    text.text = $"{i + 1}. {accessory.Name}";
                    TranslationHelper.TranslateAsync(accessory.Name, value => text.text = $"{i + 1}. {value}");
                }
                else text.text = $"Slot {i + 1}";

                var toggle = go.GetComponent<Toggle>();
                toggle.isOn = false;
                toggle.onValueChanged.AddListener((change) => 
                {
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
            //toggles[0].isOn = true;
        }
    }
}
