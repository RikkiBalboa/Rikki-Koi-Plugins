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

        public Dictionary<SubCategory, BaseEditorPanel> SubCategoryPanels = new Dictionary<SubCategory, BaseEditorPanel>();

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

                BaseEditorPanel editorPanel;
                if (Category == Category.Body) editorPanel = panel.AddComponent<BodyEditorPanel>();
                else if (Category == Category.Face) editorPanel = panel.AddComponent<FaceEditorPanel>();
                else if (Category == Category.Hair) editorPanel = panel.AddComponent<HairEditorPanel>();
                else if (Category == Category.Clothing) editorPanel = panel.AddComponent<ClothingEditorPanel>();
                else if (Category == Category.Accessories)
                {
                    toggle.onValueChanged.RemoveAllListeners();
                    editorPanel = panel.AddComponent<AccessoryEditorPanel>();
                    editorPanel.gameObject.SetActive(true);
                    var accessoryPanel = SubCategorySelectorPanel.AddComponent<AccessoryPanel>();
                    accessoryPanel.editorPanel = editorPanel as AccessoryEditorPanel;
                }
                else editorPanel = panel.AddComponent<BaseEditorPanel>();

                editorPanel.SubCategory = subCategory;
                SubCategoryPanels[subCategory] = editorPanel;
            }
            if (toggles.Count > 0)
            {
                toggles[0].isOn = true;
                Destroy(editorPanelTemplate);
                if (Category != Category.Accessories)
                    Destroy(toggleTemplate);
            }
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
                    return new SubCategory[] { SubCategory.Accessories };
            }
            return new List<SubCategory>();
        }
    }
}
