using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class ButtonGroupComponent : MonoBehaviour
    {
        private GameObject ButtonTemplate;

        public Dictionary<string, Action> ButtonsMap;

        private void Awake()
        {
            ButtonTemplate = gameObject.GetComponentInChildren<Button>().gameObject;
        }

        private void Start()
        {
            foreach (var buttonMap in ButtonsMap) {
                var buttonObject = Instantiate(ButtonTemplate, ButtonTemplate.transform.parent);
                buttonObject.GetComponentInChildren<Text>().text = buttonMap.Key;
                buttonObject.GetComponent<Button>().onClick.AddListener(() => buttonMap.Value());
            }
            Destroy(ButtonTemplate);
        }
    }
}
