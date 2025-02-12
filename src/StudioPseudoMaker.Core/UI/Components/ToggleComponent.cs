using System;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class ToggleComponent : MonoBehaviour
    {
        public string Name;
        public Action<bool> SetValueAction;
        public Func<bool> GetCurrentValue;

        private Toggle toggle;
        private Text label;

        private void Awake()
        {
            toggle = gameObject.GetComponentInChildren<Toggle>();
            toggle.onValueChanged.AddListener(UpdateValue);

            label = gameObject.GetComponentInChildren<Text>();
        }

        private void Start()
        {
            label.text = Name;
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            UpdateValue(GetCurrentValue());
        }

        public void UpdateValue(bool value)
        {
            toggle.isOn = value;

            if (value != GetCurrentValue())
                SetValueAction(value);
        }
    }
}
