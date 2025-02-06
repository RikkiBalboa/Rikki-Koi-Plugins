using ChaCustom;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class PickerPanel : MonoBehaviour
    {
        public RectTransform Canvas;
        public RectTransform DragPanel;
        public RectTransform CloseButton;
        public RectTransform ResizeHandle;

        public Text Title;
        public InputField NameField;
        public InputField SearchField;
        public Button ClearButton;
        public GridLayoutGroup GridLayoutGroup;

        private static Dictionary<ChaListDefine.CategoryNo, List<CustomSelectInfo>> dictSelectInfo;
        private static readonly Dictionary<string, string> translationCache = new Dictionary<string, string>();

        private List<CustomSelectInfo> itemList;

        private GameObject pickerEntryTemplate;

        private void Awake()
        {
            Canvas = (RectTransform)transform;
            DragPanel = (RectTransform)transform.Find("DragPanel").transform;
            CloseButton = (RectTransform)DragPanel.Find("CloseButton");
            ResizeHandle = (RectTransform)transform.Find("ResizeHandle").transform;
            GridLayoutGroup = transform.Find("Scroll View/Viewport/Content").gameObject.GetComponent<GridLayoutGroup>();

            pickerEntryTemplate = GridLayoutGroup.transform.Find("PickerItemTemplate").gameObject;
            pickerEntryTemplate.transform.SetParent(GridLayoutGroup.transform.parent);
            pickerEntryTemplate.SetActive(false);

            NameField = transform.Find("NameField").gameObject.GetComponent<InputField>();
            SearchField = transform.Find("SearchInputField").gameObject.GetComponent<InputField>();

            ClearButton = transform.Find("ClearButton").gameObject.GetComponent<Button>();
            ClearButton.onClick.AddListener(() => SearchField.text = "");

            CloseButton.gameObject.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));

            MovableWindow.MakeObjectDraggable(DragPanel, Canvas, false);
            ResizableWindow.MakeObjectResizable(ResizeHandle, Canvas, new Vector2(100, 100), PseudoMakerUI.MainWindow.GetComponent<CanvasScaler>().referenceResolution, false);

            gameObject.SetActive(false);
        }

        public static void InitializeCategories()
        {
            if (dictSelectInfo != null) return;

            dictSelectInfo = new Dictionary<ChaListDefine.CategoryNo, List<CustomSelectInfo>>();

            var chaListCtrl = new ChaListControl();
            chaListCtrl.LoadListInfoAll();

            foreach (var category in Enum.GetValues(typeof(ChaListDefine.CategoryNo)).Cast<ChaListDefine.CategoryNo>())
            {
                var list = new List<CustomSelectInfo>();
                chaListCtrl.GetCategoryInfo(category).Values.ToList().ForEach(info =>
                {
                    TranslationHelper.TranslateAsync(info.Name, s => translationCache[info.Name] = s);

                    CustomSelectInfo customSelectInfo = new CustomSelectInfo
                    {
                        category = info.Category,
                        index = info.Id,
                        name = info.Name,
                        assetBundle = info.GetInfo(ChaListDefine.KeyType.ThumbAB),
                        assetName = info.GetInfo(ChaListDefine.KeyType.ThumbTex),
                    };
                    list.Add(customSelectInfo);
                });
                dictSelectInfo[category] = list;
            }
        }
    }
}
