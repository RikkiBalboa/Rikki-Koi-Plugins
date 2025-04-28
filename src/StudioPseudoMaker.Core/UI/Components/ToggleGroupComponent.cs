using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class ToggleGroupComponent : MonoBehaviour
    {
        public string Title;
        public IEnumerable<string> Options;
        public Action<int> SetValueAction;
        public Func<int> GetCurrentValue;
        public Func<int> GetCurrentAvailableOptions;

        private GameObject ToggleTemplate;
        private ToggleGroup toggleGroup;
        private List<Toggle> Toggles = new List<Toggle>();
        private Text label;


        private bool shouldNotUpdate = false;

        private void Awake()
        {
            toggleGroup = gameObject.AddComponent<ToggleGroup>();
            toggleGroup.allowSwitchOff = false;

            ToggleTemplate = gameObject.GetComponentInChildren<Toggle>().gameObject;
            label = gameObject.GetComponentInChildren<Text>();
        }

        private void Start()
        {
            label.text = Title;

            int i = 0;
            foreach (var option in Options)
            {
                int index = i;
                var toggleObject = Instantiate(ToggleTemplate, ToggleTemplate.transform.parent);
                toggleObject.GetComponentInChildren<Text>().text = option;

                var toggle = toggleObject.GetComponent<Toggle>();
                toggle.group = toggleGroup;
                toggle.onValueChanged.AddListener(value => { if (value && !shouldNotUpdate) SetValueAction(index); });
                Toggles.Add(toggle);
                i++;
            }
            DestroyImmediate(ToggleTemplate);

            shouldNotUpdate = true;
            UpdateValue(GetCurrentValue());
            shouldNotUpdate = false;
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            shouldNotUpdate = true;
            UpdateValue(GetCurrentValue());
            shouldNotUpdate = false;
        }

        public void UpdateValue(int value)
        {
            if (GetCurrentAvailableOptions != null)
            {
                var totalOptions = GetCurrentAvailableOptions();
                for (int i = 0; i < Toggles.Count; i++)
                {
                    Toggles[i].gameObject.SetActive(i < totalOptions);
                    Toggles[i].isOn = value == i;
                }
            }

            if (value != GetCurrentValue())
                SetValueAction(value);
        }
    }
}
