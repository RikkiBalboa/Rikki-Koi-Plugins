using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class PseudoMakerUI : MonoBehaviour
    {
        public static PseudoMakerUI Instance;
        public static GameObject MainWindow;

        public RectTransform MainCanvas;
        public RectTransform DragPanel;
        public RectTransform CloseButton;
        public RectTransform ResetOriginalsButton;
        public RectTransform ResizeHandle;

        public PickerPanel PickerPanel;

        public GameObject CategorySelectorPanel;
        public ToggleGroup CategoryToggleGroup;

        public Dictionary<Category, CategoryPanel> CategoryPanels = new Dictionary<Category, CategoryPanel>();

        public static Category CurrentCategory {  get; private set; }

        public void Awake()
        {
            MainCanvas = (RectTransform)MainWindow.transform.Find("MainCanvas").transform;
            MainCanvas.anchoredPosition = new Vector2(70, 65);
            MainCanvas.offsetMax = new Vector2(70 + PseudoMaker.MainWindowWidth.Value, 65 + PseudoMaker.MainWindowHeight.Value);
            MainCanvas.offsetMin = new Vector2(70, 65);

            PickerPanel = MainWindow.transform.Find("CategoryPicker").gameObject.AddComponent<PickerPanel>();
            DragPanel = (RectTransform)MainCanvas.transform.Find("DragPanel").transform;
            CloseButton = (RectTransform)DragPanel.Find("CloseButton");
            ResetOriginalsButton = (RectTransform)DragPanel.Find("ResetButton");
            ResizeHandle = (RectTransform)MainCanvas.transform.Find("ResizeHandle").transform;
            MovableWindow.MakeObjectDraggable(DragPanel, MainCanvas, false);
            ResizableWindow.MakeObjectResizable(
                ResizeHandle,
                MainCanvas,
                new Vector2(400, 200), MainWindow.GetComponent<CanvasScaler>(),
                false,
                () => {
                    PseudoMaker.MainWindowWidth.Value = MainCanvas.sizeDelta.x;
                    PseudoMaker.MainWindowHeight.Value = MainCanvas.sizeDelta.y;
                }
            );

            CloseButton.gameObject.GetComponent<Button>().onClick.AddListener(() => MainWindow.SetActive(false));
            ResetOriginalsButton.gameObject.GetComponent<Button>().onClick.AddListener(() => PseudoMaker.selectedCharacterController.ResetSavedValues());


            InitializeTemplates();
            InitializeCategories();
            ResizeHandle.SetAsLastSibling();
        }

        private void InitializeTemplates()
        {
            CategorySelectorPanel = MainCanvas.transform.Find("CategorySelectorPanel").gameObject;
            CategoryToggleGroup = CategorySelectorPanel.GetComponent<ToggleGroup>();
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

                var text = go.GetComponentInChildren<Text>(true);
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
            CurrentCategory = category;
        }

        public static GameObject Initialize()
        {
            if (MainWindow != null) return MainWindow;
            var data = ResourceUtils.GetEmbeddedResource("pseudo_maker_interface.unity3d");
            var ab = AssetBundle.LoadFromMemory(data);

            var canvasObj = ab.LoadAsset<GameObject>("StudioPseudoMakerCanvas.prefab");
            if (canvasObj == null) throw new ArgumentException("Could not find prefab in loaded AB");

            MainWindow = Instantiate(canvasObj);
            MainWindow.SetActive(false);

            Destroy(canvasObj);
            ab.Unload(false);
            return MainWindow;
        }

        public void RefreshValues()
        {
            // Values are always refreshed by the components in OnEnable
            // So toggling the UI off/on refreshed all currently visible values
            // and the UI doesn't flicker at all too
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
        }
    }
}
