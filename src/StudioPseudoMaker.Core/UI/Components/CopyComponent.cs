using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
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

        private void Awake()
        {
            toggle = GetComponentInChildren<Toggle>(true);
            label = toggle.GetComponentInChildren<Text>(true);

            fromText = transform.Find("Layout/FromText").GetComponent<Text>();
            toText = transform.Find("Layout/ToText").GetComponent<Text>();
        }

        private void Start()
        {
            label.text = LabelName;
        }

        private void OnEnable()
        {
            fromText.text = GetFromName();
            toText.text = GetToName();
        }
    }
}
