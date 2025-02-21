using System;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class ClothingOptionComponent : MonoBehaviour
    {
        private Toggle OptionToggle1;
        private Toggle OptionToggle2;

        public Func<int, bool> GetCurrentValue;
        public Action<int, bool> SetValueAction;
        public Func<int, bool> CheckUsePart;

        private void Awake()
        {
            OptionToggle1 = transform.Find("LayoutGroup/OptionToggle1").GetComponent<Toggle>();
            OptionToggle1.onValueChanged.AddListener(value => UpdateValue(0, value));
            OptionToggle2 = transform.Find("LayoutGroup/OptionToggle2").GetComponent<Toggle>();
            OptionToggle2.onValueChanged.AddListener(value => UpdateValue(1, value));
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            OptionToggle1.gameObject.SetActive(CheckUsePart(0));
            OptionToggle2.gameObject.SetActive(CheckUsePart(1));

            UpdateValue(0, GetCurrentValue(0));
            UpdateValue(1, GetCurrentValue(1));
        }

        public void UpdateValue(int option, bool value)
        {
            if (option == 0) OptionToggle1.isOn = value;
            else if (option == 1) OptionToggle2.isOn = value;

            if (value != GetCurrentValue(option))
                SetValueAction(option, value);
        }
    }
}
