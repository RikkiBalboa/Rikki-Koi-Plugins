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

            var scrollbar = dropdown.GetComponentInChildren<Scrollbar>(true);
            var autoscroll = scrollbar.gameObject.AddComponent<AutoScrollToSelectionWithDropdown>();
            autoscroll.Dropdown = dropdown;
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

        // Shamelessly stolen from https://github.com/IllusionMods/KK_Plugins/blob/master/src/MaterialEditor.Base/UI/UI.AutoScrollToCenter.cs
        private class AutoScrollToSelectionWithDropdown : MonoBehaviour
        {
            public Dropdown Dropdown;
            private bool autoScrolled;
            private void OnEnable()
            {
                //No scrolling until LateUpdate when internal setup is complete
                autoScrolled = false;
            }

            private void LateUpdate()
            {
                if (!autoScrolled)
                {
                    autoScrolled = true;
                    AutoScroll();
                }
            }

            private void AutoScroll()
            {
                if (Dropdown == null)
                    return;

                int items = Dropdown.options.Count;

                if (items <= 1)
                    return;

                var scrollbar = GetComponent<Scrollbar>();

                if (scrollbar == null)
                    return;

                //x = 0, y = 1
                int axis = (scrollbar.direction < Scrollbar.Direction.BottomToTop ? 0 : 1);

                var scrollRect = Dropdown.template.GetComponent<ScrollRect>();

                float viewSize = scrollRect.viewport.rect.size[axis];
                float itemSize = 20f;

                if (Dropdown.itemText != null)
                {
                    var itemRect = (RectTransform)Dropdown.itemText.transform.parent;
                    itemSize = itemRect.rect.size[axis];
                }
                else if (Dropdown.itemImage != null)
                {
                    var itemRect = (RectTransform)Dropdown.itemImage.transform.parent;
                    itemSize = itemRect.rect.size[axis];
                }

                float viewAreaRatio = (viewSize / itemSize) / items;

                float scroll = (float)Dropdown.value / items - viewAreaRatio * 0.5f;
                scroll = Mathf.Clamp(scroll, 0f, 1f - viewAreaRatio);
                scroll = Mathf.InverseLerp(0, 1f - viewAreaRatio, scroll);

                scrollbar.value = Mathf.Clamp01(1.0f - scroll);
            }
        }
    }
}
