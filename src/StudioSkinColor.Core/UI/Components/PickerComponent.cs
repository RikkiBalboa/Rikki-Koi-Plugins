using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Shared;
using Illusion.Component.UI.ColorPicker;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    public class PickerComponent : MonoBehaviour
    {
        private Text text;
        private Button pickerButton;
        private Text pickerText;
        private Image thumbnail;
        private Button resetButton;

        public string Name;
        public SelectKindType SelectKind;
        public Func<SelectKindType, int> GetCurrentValue;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);

            pickerButton = transform.Find("PickerButton").gameObject.GetComponent<Button>();

            pickerText = pickerButton.GetComponentInChildren<Text>(true);

            thumbnail = pickerButton.GetComponentInChildren<Image>(true);
        }

        private void Start()
        {
            text.text = Name;
        }

        private void OnEnable()
        {
            //if (GetCurrentValue == null)
            //    return;

            //UpdateValue(SelectKind, GetCurrentValue());
        }

        public void UpdateValue(SelectKindType selectKindType, int id)
        {
            //SetValueAction(selectKindType, id);
        }
    }
}
