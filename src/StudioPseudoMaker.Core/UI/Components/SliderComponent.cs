﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class SliderComponent : MonoBehaviour
    {
        internal Text text;
        internal Slider slider;
        internal InputField inputField;
        private Button resetButton;
        private OnDragHandler dragHandler;

        public string Name;
        public float MinValue = -1;
        public float MaxValue = 2;
        public Func<float> GetCurrentValue;
        public Func<float> GetOriginalValue;
        public Action<float> SetValueAction;
        public Action ResetValueAction;
        public Action OnLabelClick;
        internal string displayTemplate = "0.00";

        public SliderComponent[] PairedInputs = null;

        private bool shouldNotUpdate = false;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);

            dragHandler = text.gameObject.AddComponent<OnDragHandler>();
            dragHandler.UpdateAction = value => inputField.onEndEdit.Invoke((slider.value + value).ToString());

            slider = GetComponentInChildren<Slider>(true);
            slider.onValueChanged.AddListener(UpdateValue);

            inputField = GetComponentInChildren<InputField>(true);
            inputField.onEndEdit.AddListener(UpdateValue);
# if KKS
            inputField.m_Colors.selectedColor = inputField.colors.highlightedColor;
#endif

            resetButton = transform.Find("ResetButton").gameObject.GetComponent<Button>();
            resetButton.onClick.AddListener(ResetValue);
        }

        private void Start()
        {
            text.text = Name;
            slider.minValue = MinValue;
            slider.maxValue = MaxValue;

            if (PairedInputs != null)
                dragHandler.PairedUpdateAction = value =>
                {
                    foreach (var input in PairedInputs)
                        input.inputField.onEndEdit.Invoke((input.slider.value + value).ToString());
                };

            PseudoMakerUI.AddTimelineButton(text.gameObject, OnLabelClick);
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            shouldNotUpdate = true;
            UpdateValue(GetCurrentValue());
            shouldNotUpdate = false;
        }

        public void UpdateValue(float value)
        {
            var stringValue = value.ToString(displayTemplate);
            if (slider.value != value) slider.value = value;
            if (inputField.text != stringValue) inputField.text = stringValue;

            if (value != GetCurrentValue() && !shouldNotUpdate)
                SetValueAction(value);
        }

        public void UpdateValue(string value)
        {
            if (float.TryParse(value, out var floatValue))
            {
                if (slider.value != floatValue) slider.value = floatValue;

                if (floatValue != GetCurrentValue() && !shouldNotUpdate)
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
