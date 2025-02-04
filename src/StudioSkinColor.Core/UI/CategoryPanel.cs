using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static RootMotion.FinalIK.GrounderQuadruped;

namespace Plugins
{
    public class CategoryPanel : MonoBehaviour
    {
        public Category Category;
        public GameObject SubCategoryPanel;

        public ScrollRect PanelScroll;
        public ToggleGroup PanelToggleGroup;

        private void Awake()
        {
            SubCategoryPanel = gameObject.transform.Find("SubCategoryPanel").gameObject;
            PanelScroll = SubCategoryPanel.GetComponent<ScrollRect>();
            PanelToggleGroup = SubCategoryPanel.GetComponent<ToggleGroup>();

            InitializeSubCategories();
        }

        public void InitializeSubCategories()
        {
            var toggleTemplate = PanelScroll.content.Find("ToggleTemplate").gameObject;

            var toggles = new List<Toggle>();

            foreach (var subCategory in GetSubCategories())
            {
                var go = Instantiate(toggleTemplate);
                go.transform.SetParent(PanelScroll.content, false);

                var toggle = go.GetComponent<Toggle>();
                //toggle.onValueChanged.AddListener((change) => CategoryToggleValueChanged(category));
                toggle.group = PanelToggleGroup;
                toggles.Add(toggle);

                var text = go.GetComponentInChildren<Text>();
                text.text = UIMappings.GetSubcategoryName(subCategory);

                //var panel = Instantiate(categoryPanelTemplate);
                //panel.SetActive(false);
                //panel.name = $"Category{category}Panel";
                //panel.transform.SetParent(categoryPanelTemplate.transform.parent, false);

                //var categoryPanel = panel.AddComponent<CategoryPanel>();
                //categoryPanel.InitializeSubCategories(category);
                //CategoryPanels[category] = categoryPanel;
            }
            if (toggles.Count > 0)
                toggles[0].isOn = true;
            Destroy(toggleTemplate);
        }

        private IEnumerable<SubCategory> GetSubCategories()
        {
            switch (Category)
            {
                case Category.Body:
                case Category.Face:
                case Category.Clothing:
                    return Enum.GetValues(typeof(SubCategory))
                        .Cast<SubCategory>()
                        .Where(x => x.ToString().Contains(Category.ToString()));
                case Category.Hair:
                case Category.Accessories:
                default:
                    return new List<SubCategory>();
            }
        }
    }
}
