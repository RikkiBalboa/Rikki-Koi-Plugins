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
        public bool Repeat = false;
        public Func<float> GetCurrentValue;
        public Func<float> GetOriginalValue;
        public Action<float> SetValueAction;
        public Action ResetValueAction;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);
            var dragHandler = text.gameObject.AddComponent<OnDragHandler>();
            dragHandler.UpdateAction = value => UpdateValue(GetCurrentValue() + value * IncrementValue * 100);

            DecreaseButton = transform.Find("DecreaseButton").GetComponent<Button>();
            DecreaseButton.onClick.AddListener(() => UpdateValue(GetCurrentValue() - IncrementValue * (Input.GetKey(KeyCode.LeftShift) ? 10f : 1f) / (Input.GetKey(KeyCode.LeftControl) ? 10f : 1f)));

            IncreaseButton = transform.Find("IncreaseButton").GetComponent<Button>();
            IncreaseButton.onClick.AddListener(() => UpdateValue(GetCurrentValue() + IncrementValue * (Input.GetKey(KeyCode.LeftShift) ? 10f : 1f) / (Input.GetKey(KeyCode.LeftControl) ? 10f : 1f)));

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
            var currentValue = ClampValue(value);
            var stringValue = currentValue.ToString("0.000");
            inputField.text = stringValue;

            if (currentValue != GetCurrentValue())
                SetValueAction(currentValue);
        }
        public void UpdateValue(string value)
        {
            if (float.TryParse(value, out var floatValue))
            {
                var currentValue = ClampValue(floatValue);
                if (currentValue != GetCurrentValue())
                    SetValueAction(currentValue);
                if (currentValue != floatValue)
                    inputField.text = currentValue.ToString("0.000");
            }
        }

        public void ResetValue()
        {
            ResetValueAction();
            UpdateValue(GetCurrentValue());
        }

        private float ClampValue(float value)
        {
            if (Repeat) return Mathf.Repeat(value, MaxValue);
            return Mathf.Clamp(value, MinValue, MaxValue);
        }
    }
}
