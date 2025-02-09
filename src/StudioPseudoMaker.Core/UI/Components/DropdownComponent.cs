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

        private void Awake()
        {
            text = GetComponentInChildren<Text>(true);
            dropdown = GetComponentInChildren<Dropdown>(true);

            dropdown.onValueChanged.AddListener(idx => { PseudoMaker.Logger.LogInfo(idx); });
        }

        private void Start()
        {
            text.text = Name;

            var options = new List<Dropdown.OptionData>();
            foreach (var option in DropdownOptions)
                options.Add(new Dropdown.OptionData(option));
            dropdown.options = options;
        }
    }
}
