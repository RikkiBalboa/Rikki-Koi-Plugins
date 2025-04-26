using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class TextInputFieldComponent : MonoBehaviour
    {
        private Text text;
        private InputField inputField;
        private Button resetButton;

        public string Name;
        public string Placeholder;
        public Func<string> GetCurrentValue;
        public Action<string> SetValueAction;

        private bool shouldNotUpdate = false;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);

            inputField = GetComponentInChildren<InputField>(true);
            inputField.onEndEdit.AddListener(value => { if (!shouldNotUpdate) UpdateValue(value); });
# if KKS
            inputField.m_Colors.selectedColor = inputField.colors.highlightedColor;
#endif

            resetButton = transform.Find("ResetButton").gameObject.GetComponent<Button>();
            resetButton.onClick.AddListener(ResetValue);
        }

        private void Start()
        {
            text.text = Name;
            inputField.placeholder.GetComponent<Text>().text = Placeholder;
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            shouldNotUpdate = true;
            UpdateValue(GetCurrentValue());
            shouldNotUpdate = false;
        }

        public void UpdateValue(string value)
        {
            inputField.text = value;
            if (value != GetCurrentValue())
                SetValueAction(value);
        }

        public void ResetValue()
        {
            UpdateValue(GetCurrentValue());
        }

        public string GetInputValue()
        {
            return inputField.text;
        }
    }
}
