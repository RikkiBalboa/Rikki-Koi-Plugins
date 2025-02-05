using System;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class SliderComponent : MonoBehaviour
    {
        private Text text;
        private Slider slider;
        private InputField inputField;
        private Button resetButton;

        public string Name;
        public float MinValue = -1;
        public float MaxValue = 2;
        public Func<float> GetCurrentValue;
        public Func<float> GetOriginalValue;
        public Action<float> SetValueAction;
        public Action ResetValueAction;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);
            var dragHandler = text.gameObject.AddComponent<OnDragHandler>();
            dragHandler.UpdateAction = value => inputField.onEndEdit.Invoke((slider.value + value).ToString());

            slider = GetComponentInChildren<Slider>(true);
            slider.minValue = MinValue;
            slider.maxValue = MaxValue;
            slider.onValueChanged.AddListener(UpdateValue);

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
            var stringValue = value.ToString("0.00");
            if (slider.value != value) slider.value = value;
            if (inputField.text != stringValue) inputField.text = stringValue;
            SetValueAction(value);
        }
        public void UpdateValue(string value)
        {
            if (float.TryParse(value, out var floatValue))
            {
                if (slider.value != floatValue) slider.value = floatValue;
                SetValueAction(floatValue);
            }
        }

        public void ResetValue()
        {
            ResetValueAction();
            UpdateValue(GetCurrentValue());
        }
    }
}
