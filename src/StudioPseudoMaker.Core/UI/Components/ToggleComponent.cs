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

        public Toggle toggle;
        private Text label;

        private bool shouldNotUpdate = false;

        private void Awake()
        {
            toggle = gameObject.GetComponentInChildren<Toggle>();
            toggle.onValueChanged.AddListener(value => { if (!shouldNotUpdate) UpdateValue(value); });

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

            shouldNotUpdate = true;
            UpdateValue(GetCurrentValue());
            shouldNotUpdate = false;
        }

        public void UpdateValue(bool value)
        {
            toggle.isOn = value;


            if (value != GetCurrentValue())
                SetValueAction(value);
        }
    }
}
