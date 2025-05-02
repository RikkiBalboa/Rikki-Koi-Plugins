using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class CategoryPanel : MonoBehaviour
    {
        public Category Category;
        public GameObject SubCategorySelectorPanel;

        public ScrollRect PanelScroll;
        public ToggleGroup PanelToggleGroup;

        public Dictionary<SubCategory, BaseEditorPanel> SubCategoryPanels = new Dictionary<SubCategory, BaseEditorPanel>();
        public static GameObject EditorPanelTemplate;

        private void Awake()
        {
            SubCategorySelectorPanel = gameObject.transform.Find("SubCategorySelectorPanel").gameObject;
            PanelScroll = SubCategorySelectorPanel.GetComponentInChildren<ScrollRect>();
            PanelToggleGroup = SubCategorySelectorPanel.GetComponent<ToggleGroup>();

            InitializeSubCategories();
        }

        public void InitializeSubCategories()
        {
            var toggleTemplate = PanelScroll.content.Find("ToggleTemplate").gameObject;
            EditorPanelTemplate = gameObject.transform.Find("EditorPanelTemplate").gameObject;
            EditorPanelTemplate.SetActive(false);

            var toggles = new List<Toggle>();

            foreach (var subCategory in GetSubCategories())
            {
                BaseEditorPanel editorPanel = null;

                if (Category == Category.Accessories)
                {
                    var accessoryPanel = SubCategorySelectorPanel.AddComponent<AccessoryPanel>();
                    AccessoryEditorPanel accessoryPanelEditorPanel = BaseEditorPanel.CreatePanel<AccessoryEditorPanel>(subCategory);
                    accessoryPanel.editorPanel = accessoryPanelEditorPanel;
                    SubCategoryPanels.Add(subCategory, accessoryPanelEditorPanel);
                    AccessoryCopyPanel accessoryPanelCopyPanel = BaseEditorPanel.CreatePanel<AccessoryCopyPanel>(SubCategory.AccessoryCopy);
                    accessoryPanel.copyPanel = accessoryPanelCopyPanel;
                    SubCategoryPanels.Add(SubCategory.AccessoryCopy, accessoryPanelCopyPanel);
                    AccessoryTransferPanel accessoryPanelTransferPanel = BaseEditorPanel.CreatePanel<AccessoryTransferPanel>(SubCategory.AccessoryTransfer);
                    accessoryPanel.transferPanel = accessoryPanelTransferPanel;
                    SubCategoryPanels.Add(SubCategory.AccessoryTransfer, accessoryPanelTransferPanel);
                }
                else
                {
                    if (Category == Category.Body && subCategory == SubCategory.BodySkinOverlays && !Compatibility.HasSkinOverlayPlugin) continue;
                    if (Category == Category.Body && subCategory == SubCategory.FaceEyeOverlays && !Compatibility.HasSkinOverlayPlugin) continue;
                    if (Category == Category.Body && subCategory == SubCategory.BodyPregnancyPlus && !Compatibility.HasPregnancyPlus) continue;

                    var go = Instantiate(toggleTemplate);
                    go.transform.SetParent(PanelScroll.content, false);

                    var toggle = go.GetComponent<Toggle>();
                    toggle.onValueChanged.AddListener((change) => SubCategoryToggleValueChanged(subCategory));
                    toggle.group = PanelToggleGroup;
                    toggles.Add(toggle);

                    var text = go.GetComponentInChildren<Text>(true);
                    text.text = UIMappings.GetSubcategoryName(subCategory);

                    if (Category == Category.Body) editorPanel = BaseEditorPanel.CreatePanel<BodyEditorPanel>(subCategory);
                    else if (Category == Category.Face) editorPanel = BaseEditorPanel.CreatePanel<FaceEditorPanel>(subCategory);
                    else if (Category == Category.Hair) editorPanel = BaseEditorPanel.CreatePanel<HairEditorPanel>(subCategory);
                    else if (Category == Category.Clothing) editorPanel = BaseEditorPanel.CreatePanel<ClothingEditorPanel>(subCategory);
                }

                if (editorPanel != null)
                    SubCategoryPanels[subCategory] = editorPanel;
            }
            if (toggles.Count > 0)
            {
                // For some reason, in KK, this needs to be done. And crashes if set to false above ¯\_(ツ)_/¯
                foreach (var toggle in toggles)
                    toggle.isOn = false;

                toggles[0].isOn = true;
                Destroy(EditorPanelTemplate);
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
