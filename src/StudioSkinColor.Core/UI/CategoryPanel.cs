using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class CategoryPanel : MonoBehaviour
    {
        public Category Category;
        public GameObject SubCategorySelectorPanel;

        public ScrollRect PanelScroll;
        public ToggleGroup PanelToggleGroup;

        public Dictionary<SubCategory, SubCategoryEditorPanel> SubCategoryPanels = new Dictionary<SubCategory, SubCategoryEditorPanel>();

        private void Awake()
        {
            SubCategorySelectorPanel = gameObject.transform.Find("SubCategorySelectorPanel").gameObject;
            PanelScroll = SubCategorySelectorPanel.GetComponent<ScrollRect>();
            PanelToggleGroup = SubCategorySelectorPanel.GetComponent<ToggleGroup>();

            InitializeSubCategories();
        }

        public void InitializeSubCategories()
        {
            var toggleTemplate = PanelScroll.content.Find("ToggleTemplate").gameObject;
            var editorPanelTemplate = gameObject.transform.Find("EditorPanelTemplate").gameObject;

            var toggles = new List<Toggle>();

            foreach (var subCategory in GetSubCategories())
            {
                var go = Instantiate(toggleTemplate);
                go.transform.SetParent(PanelScroll.content, false);

                var toggle = go.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((change) => SubCategoryToggleValueChanged(subCategory));
                toggle.group = PanelToggleGroup;
                toggles.Add(toggle);

                var text = go.GetComponentInChildren<Text>(true);
                text.text = UIMappings.GetSubcategoryName(subCategory);

                var panel = Instantiate(editorPanelTemplate);
                panel.SetActive(false);
                panel.name = $"Category{subCategory}Editor";
                panel.transform.SetParent(editorPanelTemplate.transform.parent, false);

                var categoryPanel = panel.AddComponent<SubCategoryEditorPanel>();
                categoryPanel.SubCategory = subCategory;
                SubCategoryPanels[subCategory] = categoryPanel;
            }
            if (toggles.Count > 0)
                toggles[0].isOn = true;
            Destroy(toggleTemplate);
            Destroy(editorPanelTemplate);
        }

        private void SubCategoryToggleValueChanged(SubCategory subCategory)
        {
            foreach (var panel in SubCategoryPanels)
                panel.Value.gameObject.SetActive(false);
            SubCategoryPanels[subCategory].gameObject.SetActive(true);
        }

        private IEnumerable<SubCategory> GetSubCategories()
        {
            switch (Category)
            {
                case Category.Body:
                case Category.Face:
                case Category.Clothing:
                case Category.Hair:
                    return Enum.GetValues(typeof(SubCategory))
                        .Cast<SubCategory>()
                        .Where(x => x.ToString().StartsWith(Category.ToString()));
                case Category.Accessories:
                default:
                    return new List<SubCategory>();
            }
        }
    }
}
