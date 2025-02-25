using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class DropdownComponent : MonoBehaviour
    {
        private Text text;
        public Dropdown dropdown;

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

            var inputFilter = dropdown.transform.Find("Template").gameObject.AddComponent<InputFilter>();
            inputFilter.Dropdown = dropdown;
        }

        private void Start()
        {
            text.text = Name;

            SetDropdownOptions(DropdownOptions);
        }

        private void OnEnable()
        {
            if (GetCurrentValue != null)
            {
                shouldNotUpdate = true;
                dropdown.value = GetCurrentValue();
                dropdown.RefreshShownValue();
                shouldNotUpdate = false;
            }
        }

        public void SetDropdownOptions(IEnumerable<string> options)
        {
            if (dropdown == null) return;
            var _options = new List<Dropdown.OptionData>();
            foreach (var option in options)
                _options.Add(new Dropdown.OptionData(option));
            dropdown.options = _options;
            dropdown.value = GetCurrentValue();
            dropdown.RefreshShownValue();
        }

        // Shamelessly stolen from https://github.com/IllusionMods/KK_Plugins/blob/master/src/MaterialEditor.Base/UI/UI.DropdownFilter.cs
        private class InputFilter : MonoBehaviour
        {
            public Dropdown Dropdown;

            private InputField inputField;
            private RectTransform content;

            private float itemStride = 20f;

            private void Awake()
            {
                inputField = GetComponentInChildren<InputField>(true);
                inputField.onValueChanged.AddListener(OnChangeFilter);
#if KKS
                inputField.m_Colors.selectedColor = inputField.colors.highlightedColor;
#endif

                content = transform.Find("Viewport/Content") as RectTransform;

                if (content.childCount >= 3)
                {
                    var item1 = (RectTransform)content.GetChild(1);
                    var item2 = (RectTransform)content.GetChild(2);
                    itemStride = Mathf.Abs(item1.offsetMin.y - item2.offsetMin.y);
                }
                else
                {
                    itemStride = 20f;
                }
            }

            private void OnChangeFilter(string filter)
            {
                if (string.IsNullOrEmpty(filter))
                    filter = "*";

                var regex = new Regex(Regex.Escape(filter).Replace("\\*", ".*").Replace("\\?", "."), RegexOptions.IgnoreCase);
                int activeItems = 0;

                foreach (Transform item in content)
                {
                    var name = item.name;
                    int colon = name.IndexOf(":");
                    if (colon < 0)
                        continue;

                    bool isShown = regex.IsMatch(name.Substring(colon + 1).Trim());
                    item.gameObject.SetActive(isShown);
                    if (isShown)
                        ++activeItems;
                }

                var sizeDelta = content.sizeDelta;
                sizeDelta.y = itemStride * activeItems + 8f;
                content.sizeDelta = sizeDelta;

                var scrollbar = GetComponentInChildren<Scrollbar>();
                if (scrollbar != null)
                {
                    scrollbar.value = 0f;
                    scrollbar.Rebuild(CanvasUpdate.Prelayout);
                }
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
