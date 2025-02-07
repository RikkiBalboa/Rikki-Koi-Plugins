using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Shared;
using Illusion.Component.UI.ColorPicker;
using static ChaCustom.CustomSelectKind;
using ChaCustom;
using System.Linq;

namespace Plugins
{
    public class PickerComponent : MonoBehaviour
    {
        private Text text;
        private Button pickerButton;
        private Text pickerText;
        private Image thumbnail;

        public string Name;
        public ChaListDefine.CategoryNo CategoryNo;
        public Func<int> GetCurrentValue;
        public Action<CustomSelectInfo> SetCurrentValue;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);

            pickerButton = transform.Find("PickerButton").gameObject.GetComponent<Button>();
            pickerButton.onClick.AddListener(() =>
            {
                PickerPanel.SetCategory(CategoryNo, GetCurrentValue, (info) =>
                {
                    pickerText.text = info.name;
                    thumbnail.sprite = PickerPanel.GetThumbSprite(info);
                    SetCurrentValue(info);
                });
            });

            pickerText = pickerButton.GetComponentInChildren<Text>(true);

            thumbnail = pickerButton.transform.Find("Image").gameObject.GetComponent<Image>();
        }

        private void Start()
        {
            text.text = Name;
        }

        private void OnEnable()
        {
            if (GetCurrentValue == null)
                return;

            var current = PickerPanel.dictSelectInfo[CategoryNo].FirstOrDefault(x => x.index == GetCurrentValue());
            if (current != null)
            {
                pickerText.text = current.name;
                thumbnail.sprite = PickerPanel.GetThumbSprite(current);
            }
        }
    }
}
