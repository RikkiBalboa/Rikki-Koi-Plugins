using ChaCustom;
using KKAPI.Utilities;
using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    public class PickerPanel : MonoBehaviour
    {
        private static PickerPanel instance;

        public RectTransform Canvas;
        public RectTransform DragPanel;
        public RectTransform CloseButton;
        public RectTransform ResizeHandle;
        public GameObject Content;

        public Text Title;
        public InputField NameField;
        public InputField SearchField;
        public Button ClearButton;
        public ScrollRect ScrollRect;
        public GridLayoutGroup GridLayoutGroup;
        public ToggleGroup toggleGroup;

        private static readonly Dictionary<string, string> translationCache = new Dictionary<string, string>();

        private static ChaListDefine.CategoryNo CategoryNo;
        private static Func<int> GetCurrentValue;
        private static Action<CustomSelectInfo> SetCurrentValue;

        internal static Dictionary<ChaListDefine.CategoryNo, List<CustomSelectInfo>> dictSelectInfo;
        private static List<CustomSelectInfo> itemList;

        private static List<CustomSelectInfoComponent> cachedEntries = new List<CustomSelectInfoComponent>();
        private int lastItemsAboveViewRect;
        private static bool isDirty;
        private static int rowCount;
        private static int columnCount;

        public int InitialBotPadding;
        public int InitialTopPadding;

        private bool _selectionChanged;
        private CustomSelectInfo _selectedItem;
        private CustomSelectInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    _selectionChanged = true;
                }
            }
        }

        private GameObject pickerEntryTemplate;

        public static void SetCategory(ChaListDefine.CategoryNo categoryNo, Func<int> getCurrentValue, Action<CustomSelectInfo> setCurrentValue)
        {
            CategoryNo = categoryNo;
            GetCurrentValue = getCurrentValue;
            SetCurrentValue = setCurrentValue;
            itemList = dictSelectInfo[categoryNo];
            isDirty = true;
            instance.gameObject.SetActive(true);
        }

        private void Awake()
        {
            instance = this;
            Canvas = (RectTransform)transform;
            DragPanel = (RectTransform)transform.Find("DragPanel").transform;
            CloseButton = (RectTransform)DragPanel.Find("CloseButton");
            ResizeHandle = (RectTransform)transform.Find("ResizeHandle").transform;
            ScrollRect = transform.Find("Scroll View").gameObject.GetComponent<ScrollRect>();
            Content = ScrollRect.transform.Find("Viewport/Content").gameObject;

            GridLayoutGroup = Content.GetComponent<GridLayoutGroup>();
            InitialBotPadding = GridLayoutGroup.padding.bottom;
            InitialTopPadding = GridLayoutGroup.padding.top;

            toggleGroup = Content.GetComponent<ToggleGroup>();

            pickerEntryTemplate = GridLayoutGroup.transform.Find("PickerItemTemplate").gameObject;
            pickerEntryTemplate.gameObject.AddComponent<CustomSelectInfoComponent>();
            pickerEntryTemplate.transform.SetParent(GridLayoutGroup.transform.parent);
            pickerEntryTemplate.SetActive(false);

            NameField = transform.Find("NameField").gameObject.GetComponent<InputField>();
            SearchField = transform.Find("SearchInputField").gameObject.GetComponent<InputField>();

            ClearButton = transform.Find("ClearButton").gameObject.GetComponent<Button>();
            ClearButton.onClick.AddListener(() => SearchField.text = "");

            CloseButton.gameObject.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));

            MovableWindow.MakeObjectDraggable(DragPanel, Canvas, false);
            ResizableWindow.MakeObjectResizable(
                ResizeHandle,
                Canvas,
                new Vector2(100, 100),
                PseudoMakerUI.MainWindow.GetComponent<CanvasScaler>().referenceResolution,
                false,
                PopulateEntryCache
            );

            PopulateEntryCache();

            gameObject.SetActive(false);
        }

        private void Update()
        {
            var visibleItemCount = itemList.Count(x => !x.disvisible);
            var offscreenItemCount = Mathf.Max(0, visibleItemCount - cachedEntries.Count);

            var rowsAboveViewRect = Mathf.FloorToInt(Mathf.Clamp(ScrollRect.content.localPosition.y / (int)GridLayoutGroup.cellSize.x, 0, offscreenItemCount));
            var itemsAboveViewRect = rowsAboveViewRect * columnCount;

            if (lastItemsAboveViewRect == itemsAboveViewRect && !isDirty) return;

            lastItemsAboveViewRect = itemsAboveViewRect;
            isDirty = false;

            var selectedItem = itemList.Find(x => x.sic != null && x.sic.gameObject == EventSystem.current.currentSelectedGameObject);
            itemList.ForEach(x => x.sic = null);

            var count = 0;
            foreach (var item in itemList.Where(x => !x.disvisible).Skip(itemsAboveViewRect))
            {
                if (count >= cachedEntries.Count) break;

                var cachedEntry = cachedEntries[count];

                count++;

                cachedEntry.info = item;
                item.sic = cachedEntry;

                cachedEntry.Disable(item.disable);

                var thumb = GetThumbSprite(item);
                cachedEntry.img.sprite = thumb;

                if (ReferenceEquals(selectedItem, item))
                    EventSystem.current.SetSelectedGameObject(cachedEntry.gameObject);

                if (!cachedEntry.gameObject.activeSelf)
                    cachedEntry.gameObject.SetActive(true);
            }

            // Disable unused cache items
            if (count < cachedEntries.Count)
            {
                foreach (var cacheEntry in cachedEntries.Skip(count))
                    cacheEntry.gameObject.SetActive(false);
            }

            UpdateSelection();

            // Apply top and bottom offsets to create the illusion of having all of the list items
            var topOffset = Mathf.RoundToInt(rowsAboveViewRect * GridLayoutGroup.cellSize.x);
            GridLayoutGroup.padding.top = InitialTopPadding + topOffset;

            var totalHeight = Mathf.CeilToInt((float)visibleItemCount / columnCount) * GridLayoutGroup.cellSize.x;
            var cacheEntriesHeight = Mathf.CeilToInt((float)cachedEntries.Count / columnCount) * GridLayoutGroup.cellSize.x;
            var trailingHeight = totalHeight - cacheEntriesHeight - topOffset;
            GridLayoutGroup.padding.bottom = Mathf.FloorToInt(Mathf.Max(0, trailingHeight) + InitialBotPadding);

            // Needed after changing padding since it doesn't make the object dirty
            LayoutRebuilder.MarkLayoutForRebuild(Content.transform as RectTransform);
        }

        private void UpdateSelection()
        {
            cachedEntries.ForEach(x => x.tgl.isOn = false);
            var onToggle = cachedEntries.FirstOrDefault(x => x.info.index == GetCurrentValue());
            if (onToggle != null) onToggle.tgl.isOn = true;
            //if (SelectedItem != null)
            //{
            //    if (SelectedItem.sic != null)
            //        SelectedItem.sic.tgl.isOn = true;

                //    //if (_selectionChanged && IsVisible) // Only update the scroll after the list is fully loaded and shown, or it will get reset to 0
                //    //{
                //    //    if (ScrollListsToSelection.Value)
                //    //        ScrollToSelection();
                //    //    _selectionChanged = false;
                //    //}
                //}
        }

        private void PopulateEntryCache()
        {
            var rectTransform = ScrollRect.transform as RectTransform;

            var _columnCount = ((int)rectTransform.rect.width) / (int)GridLayoutGroup.cellSize.x;
            var _rowCount = ((int)rectTransform.rect.height) / (int)GridLayoutGroup.cellSize.x + 2;

            var totalVisibleItems = _columnCount * _rowCount;

            if (totalVisibleItems <= columnCount * rowCount)
                return;

            rowCount = _rowCount;
            columnCount = _columnCount;

            var newEntries = totalVisibleItems - cachedEntries.Count;
            for (int i = 0; i < newEntries; i++)
            {
                var copy = Instantiate(pickerEntryTemplate, GridLayoutGroup.transform, false);
                copy.name = "PickerItem";
                var copyInfoComp = copy.GetComponent<CustomSelectInfoComponent>();

                copyInfoComp.tgl = copy.GetComponent<Toggle>();
                copyInfoComp.tgl.group = toggleGroup;
                copyInfoComp.tgl.isOn = false;
                copyInfoComp.tgl.onValueChanged.AddListener((value) =>
                {
                    if (value && copyInfoComp.info != null && copyInfoComp.info.index != GetCurrentValue())
                        SetCurrentValue(copyInfoComp.info);
                });

                //__instance.SetToggleHandler(copy);

                copyInfoComp.img = copy.GetComponent<Image>();

                cachedEntries.Add(copyInfoComp);
                copy.SetActive(false);
            }
        }

        public static Sprite GetThumbSprite(CustomSelectInfo item)
        {
            var thumbTex = CommonLib.LoadAsset<Texture2D>(item.assetBundle, item.assetName, false, string.Empty);
            Sprite thumb = null;
            if (thumbTex)
            {
                thumb = Sprite.Create(thumbTex, new Rect(0f, 0f, thumbTex.width, thumbTex.height), new Vector2(0.5f, 0.5f));
            }

            return thumb;
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
