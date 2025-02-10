using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class InputFieldComponent : MonoBehaviour
    {
        private Text text;
        private Button DecreaseButton;
        private Button IncreaseButton;
        private InputField inputField;
        private Button resetButton;

        public string Name;
        public float MinValue = -1;
        public float MaxValue = 2;
        public float IncrementValue = 1;
        public Func<float> GetCurrentValue;
        public Func<float> GetOriginalValue;
        public Action<float> SetValueAction;
        public Action ResetValueAction;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);
            var dragHandler = text.gameObject.AddComponent<OnDragHandler>();
            dragHandler.UpdateAction = value => UpdateValue((GetCurrentValue() + value));

            DecreaseButton = transform.Find("DecreaseButton").GetComponent<Button>();
            DecreaseButton.onClick.AddListener(() => UpdateValue(GetCurrentValue() - IncrementValue));

            IncreaseButton = transform.Find("IncreaseButton").GetComponent<Button>();
            IncreaseButton.onClick.AddListener(() => UpdateValue(GetCurrentValue() + IncrementValue));

            inputField = GetComponentInChildren<InputField>(true);
            inputField.onEndEdit.AddListener(UpdateValue);

            resetButton = transform.Find("ResetButton").gameObject.GetComponent<Button>();
            resetButton.onClick.AddListener(ResetValue);
        }

        private void Start()
        {
            text.text = Name;
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            UpdateValue(GetCurrentValue());
        }

        public void UpdateValue(float value)
        {
            var _value = Mathf.Clamp(value, MinValue, MaxValue);
            var stringValue = _value.ToString("0.00");
            inputField.text = stringValue;

            if (_value != GetCurrentValue())
                SetValueAction(_value);
        }
        public void UpdateValue(string value)
        {
            if (float.TryParse(value, out var floatValue))
            {
                var _floatValue = Mathf.Clamp(floatValue, MinValue, MaxValue);
                if (_floatValue != GetCurrentValue())
                    SetValueAction(_floatValue);
                if (_floatValue != floatValue)
                    inputField.text = _floatValue.ToString("0.00");
            }
        }

        public void ResetValue()
        {
            ResetValueAction();
            UpdateValue(GetCurrentValue());
        }
    }
}
