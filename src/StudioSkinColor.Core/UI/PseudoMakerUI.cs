using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class PseudoMakerUI : MonoBehaviour
    {
        public static PseudoMakerUI Instance;
        public static GameObject MainWindow;

        public RectTransform MainCanvas;
        public RectTransform DragPanel;
        public RectTransform CloseButton;
        public RectTransform ResizeHandle;

        public GameObject CategorySelectorPanel;
        public ToggleGroup CategoryToggleGroup;

        public Dictionary<Category, CategoryPanel> CategoryPanels = new Dictionary<Category, CategoryPanel>();

        //public GameObject SubCategoryPanelTemplate;
        //public GameObject SubCategoryToggleTemplate;
        //public Dictionary<string, GameObject> CategoryPannels = new Dictionary<string, GameObject>();
        //public ToggleGroup SubCategoryToggleGroup;
        //public ScrollRect SubCategoryScrollRectTemplate;

        public void Awake()
        {
            MainCanvas = (RectTransform)MainWindow.transform.Find("MainCanvas").transform;
            DragPanel = (RectTransform)MainCanvas.transform.Find("DragPanel").transform;
            CloseButton = (RectTransform)DragPanel.Find("CloseButton");
            ResizeHandle = (RectTransform)MainCanvas.transform.Find("ResizeHandle").transform;
            MovableWindow.MakeObjectDraggable(DragPanel, MainCanvas, false);
            ResizableWindow.MakeObjectResizable(ResizeHandle, MainCanvas, new Vector2(100, 100), MainWindow.GetComponent<CanvasScaler>().referenceResolution, false);

            CloseButton.gameObject.GetComponent<Button>().onClick.AddListener(() => MainWindow.SetActive(false));

            InitializeTemplates();
            InitializeCategories();
            ResizeHandle.SetAsLastSibling();
        }

        private void InitializeTemplates()
        {
            CategorySelectorPanel = MainCanvas.transform.Find("CategorySelectorPanel").gameObject;
            CategoryToggleGroup = CategorySelectorPanel.GetComponent<ToggleGroup>();



            //SubCategoryPanelTemplate = CategoryPanelTemplate.transform.Find("SubCategoryPanel").gameObject;
            //SubCategoryToggleGroup = SubCategoryPanelTemplate.GetComponent<ToggleGroup>();
            //SubCategoryScrollRectTemplate = SubCategoryPanelTemplate.GetComponent<ScrollRect>();
            //SubCategoryToggleTemplate = SubCategoryScrollRectTemplate.content.Find("ToggleTemplate").gameObject;
            //SubCategoryToggleTemplate.SetActive(false);
        }

        private void InitializeCategories()
        {
            var categoryToggleTemplate = CategorySelectorPanel.transform.Find("ToggleTemplate").gameObject;
            var categoryPanelTemplate = MainCanvas.transform.Find("CategoryPanelTemplate").gameObject;
            List<Toggle> toggles = new List<Toggle>();
            
            foreach (var category in Enum.GetValues(typeof(Category)).Cast<Category>())
            {
                var go = Instantiate(categoryToggleTemplate);
                go.name = $"Toggle{category}";
                go.transform.SetParent(CategorySelectorPanel.transform, false);

                var toggle = go.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((change) => CategoryToggleValueChanged(category));
                toggle.group = CategoryToggleGroup;
                toggles.Add(toggle);

                var text = go.GetComponentInChildren<Text>();
                text.text = category.ToString();

                var panel = Instantiate(categoryPanelTemplate);
                panel.SetActive(false);
                panel.name = $"Category{category}Panel";
                panel.transform.SetParent(categoryPanelTemplate.transform.parent, false);

                var categoryPanel = panel.AddComponent<CategoryPanel>();
                categoryPanel.Category = category;
                CategoryPanels[category] = categoryPanel;
            }
            toggles[0].isOn = true;

            Destroy(categoryToggleTemplate);
            Destroy(categoryPanelTemplate);
        }

        private void CategoryToggleValueChanged(Category category)
        {
            foreach (var panel in CategoryPanels)
                panel.Value.gameObject.SetActive(false);
            CategoryPanels[category].gameObject.SetActive(true);
        }

        private void InitializeSubCategories(string name, IEnumerable<string> keys)
        {
            //var panel = Instantiate(SubCategoryPanelTemplate);
            //panel.transform.SetParent(SubCategoryPanelTemplate.transform.parent, false);
            //var toggleGroup = panel.GetComponent<ToggleGroup>();
            //var scrollRect = panel.GetComponent<ScrollRect>();

            //CategoryPannels[name] = panel;

            //foreach (var key in keys)
            //{
            //    var go = Instantiate(SubCategoryToggleTemplate);
            //    go.name = $"Toggle{key.Replace(" ", "")}";
            //    go.SetActive(true);
            //    go.transform.SetParent(scrollRect.content, false);
            //    var toggle = go.GetComponent<Toggle>();
            //    toggle.group = toggleGroup;
            //    var text = go.GetComponentInChildren<Text>();
            //    text.text = key;
            //}
        }

        public static GameObject Initialize()
        {
            if (MainWindow != null) return MainWindow;
            //var data = ResourceUtils.GetEmbeddedResource("pseudo_maker_interface");
            //var ab = AssetBundle.LoadFromMemory(data);
            var ab = AssetBundle.LoadFromFile(Path.Combine(Paths.BepInExRootPath, @"scripts\Assets\pseudo_maker_interface.unity3d"));

            var canvasObj = ab.LoadAsset<GameObject>("StudioPseudoMakerCanvas.prefab");
            if (canvasObj == null) throw new ArgumentException("Could not find QuickAccessBoxCanvas.prefab in loaded AB");

            MainWindow = Instantiate(canvasObj);
            //copy.SetActive(false);

            Destroy(canvasObj);
            ab.Unload(false);
            return MainWindow;
        }
    }
}
