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
        public bool IsInt = false;
        public Func<float> GetCurrentValue;
        public Func<float> GetOriginalValue;
        public Action<float> SetValueAction;
        public Action ResetValueAction;

        // This value is used to smooth over dragging the label
        // KK(S) internally rounds the values to only one decimal (or in case of rotation, to 0 decimals (as int))
        private float currentValue;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);
            var dragHandler = text.gameObject.AddComponent<OnDragHandler>();
            dragHandler.UpdateAction = value => UpdateValue(currentValue + value * IncrementValue * 100);

            DecreaseButton = transform.Find("DecreaseButton").GetComponent<Button>();
            DecreaseButton.onClick.AddListener(() => UpdateValue(currentValue - IncrementValue));

            IncreaseButton = transform.Find("IncreaseButton").GetComponent<Button>();
            IncreaseButton.onClick.AddListener(() => UpdateValue(currentValue + IncrementValue));

            inputField = GetComponentInChildren<InputField>(true);
            inputField.onEndEdit.AddListener(UpdateValue);

            resetButton = transform.Find("ResetButton").gameObject.GetComponent<Button>();
            resetButton.onClick.AddListener(ResetValue);
        }

        private void Start()
        {
            text.text = Name;
            if (IsInt)
                inputField.contentType = InputField.ContentType.IntegerNumber;
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            currentValue = GetCurrentValue();
            UpdateValue(currentValue);
        }

        public void UpdateValue(float value)
        {
            currentValue = ClampValue(value);
            var stringValue = IsInt ? ((int)currentValue).ToString() : currentValue.ToString("0.0");
            inputField.text = stringValue;

            if (currentValue != GetCurrentValue())
                SetValueAction(currentValue);
        }
        public void UpdateValue(string value)
        {
            if (float.TryParse(value, out var floatValue))
            {
                currentValue = ClampValue(floatValue);
                if (currentValue != GetCurrentValue())
                    SetValueAction(currentValue);
                if (currentValue != floatValue)
                    inputField.text = IsInt ? ((int)currentValue).ToString() : currentValue.ToString("0.0");
            }
        }

        public void ResetValue()
        {
            ResetValueAction();
            UpdateValue(GetCurrentValue());
        }

        private float ClampValue(float value)
        {
            if (Repeat)
                return Mathf.Repeat(value, MaxValue);
            return Mathf.Clamp(value, MinValue, MaxValue);
        }
    }
}
