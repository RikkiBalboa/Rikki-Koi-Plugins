using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class CopyComponent : MonoBehaviour
    {
        private Text label;
        private Toggle toggle;

        private Text fromText;
        private Text toText;

        public string LabelName;

        public Func<string> GetFromName;
        public Func<string> GetToName;
        public int index = -1;
        public bool Toggled {get => toggle.isOn; set => toggle.isOn = value; }

        private void Awake()
        {
            toggle = GetComponentInChildren<Toggle>(true);
            label = toggle.GetComponentInChildren<Text>(true);


            Transform fromTextObject = transform.Find("Layout/FromText");
            if (!fromTextObject) fromTextObject = transform.Find("Layout/Text");
            fromText = fromTextObject.GetComponent<Text>();
            Transform toTextObject = transform.Find("Layout/ToText");
            if (!toTextObject) toTextObject = transform.Find("Layout/Text (1)");
            toText = toTextObject.GetComponent<Text>();
        }

        private void Start()
        {
            label.text = LabelName;
        }

        private void OnEnable()
        {
            if (GetFromName != null) fromText.text = GetFromName();
            if (GetToName != null) toText.text = GetToName();
        }

        public void Refresh()
        {
            label.text = LabelName;
            fromText.text = GetFromName();
            toText.text = GetToName();
        }
    }
}
