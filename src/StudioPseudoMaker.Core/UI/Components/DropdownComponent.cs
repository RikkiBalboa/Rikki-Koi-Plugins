using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class DropdownComponent : MonoBehaviour
    {
        private Text text;
        private Dropdown dropdown;

        public string Name;
        public List<string> DropdownOptions;

        public Func<int> GetCurrentValue;
        public Action<int> SetValueAction;

        private bool shouldNotUpdate = false;

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);
            dropdown = GetComponentInChildren<Dropdown>(true);

            dropdown.onValueChanged.AddListener(idx => { if (!shouldNotUpdate) SetValueAction(idx); });
        }

        private void Start()
        {
            text.text = Name;

            var options = new List<Dropdown.OptionData>();
            foreach (var option in DropdownOptions)
                options.Add(new Dropdown.OptionData(option));
            dropdown.options = options;
        }

        private void OnEnable()
        {
            if (GetCurrentValue != null)
            {
                shouldNotUpdate = true;
                dropdown.value = GetCurrentValue();
                shouldNotUpdate = false;
            }
        }
    }
}
