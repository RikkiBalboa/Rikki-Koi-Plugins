using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class TransferComponent : MonoBehaviour
    {
        private Text label;
        private Toggle fromToggle;
        private Text fromText;
        private Toggle toToggle;
        private Text toText;

        public int SlotNr;
        public string AccessoryName;
        public ToggleGroup FromToggleGroup;
        public ToggleGroup ToToggleGroup;
        public Action<int> fromEnabledAction;
        public Action<int> toEnabledAction;

        private void Awake()
        {
            label = GetComponentInChildren<Text>(true);

            fromToggle = transform.Find("Layout/FromToggle").GetComponentInChildren<Toggle>();
            fromText = fromToggle.gameObject.GetComponent<Text>();

            toToggle = transform.Find("Layout/ToToggle").GetComponentInChildren<Toggle>();
            toText = toToggle.gameObject.GetComponent<Text>();
        }

        private void Start()
        {
            if (SlotNr == 0)
            {
                fromToggle.isOn = true;
                toToggle.isOn = true;
            }

            fromToggle.group = FromToggleGroup;
            fromToggle.onValueChanged.AddListener(value => { if(value) fromEnabledAction(SlotNr); });
            toToggle.group = ToToggleGroup;
            toToggle.onValueChanged.AddListener(value => { if (value) toEnabledAction(SlotNr); });
            RefreshText();
        }

        public void RefreshText()
        {
            label.text = (SlotNr + 1).ToString();
            fromText.text = AccessoryName;
            toText.text = AccessoryName;
        }
    }
}
