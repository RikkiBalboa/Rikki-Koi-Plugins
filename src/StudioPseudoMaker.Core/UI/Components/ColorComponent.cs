using Shared;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class ColorComponent : MonoBehaviour
    {
        private Text text;
        private Button colorButton;
        private Image background;
        private Button resetButton;

        public string Name;
        public Func<Color> GetCurrentValue;
        public Func<Color> GetOriginalValue;
        public Action<Color> SetValueAction;
        public Action ResetValueAction;
        public Action OnLabelClick;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);

            colorButton = transform.Find("Alpha/Image").gameObject.GetComponent<Button>();
            colorButton.onClick.AddListener(() => {
                ColorPicker.OpenColorPicker(background.color, (c) =>
                {
                    if (c != background.color)
                        UpdateValue(c);
                }, $"Pseudo Maker - {Name}");
            });

            background = transform.Find("Alpha/Image").gameObject.GetComponent<Image>();

            resetButton = transform.Find("ResetButton").gameObject.GetComponent<Button>();
            resetButton.onClick.AddListener(ResetValue);
        }

        private void Start()
        {
            text.text = Name;

            if (OnLabelClick != null)
                text.gameObject.AddComponent<Button>().onClick.AddListener(() => OnLabelClick());
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            UpdateValue(GetCurrentValue());
        }

        public void UpdateValue(Color value)
        {
            background.color = value;

            if (value != GetCurrentValue())
                SetValueAction(value);
        }

        public void ResetValue()
        {
            ResetValueAction();
            UpdateValue(GetCurrentValue());
        }
    }
}
