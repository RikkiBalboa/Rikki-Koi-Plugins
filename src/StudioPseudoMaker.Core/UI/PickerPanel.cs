using ChaCustom;
using KKAPI.Utilities;
using Sideloader.AutoResolver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class PickerPanel : MonoBehaviour
    {
        private static PickerPanel instance;

        public RectTransform Canvas;
        public RectTransform DragPanel;
        public RectTransform CloseButton;
        public RectTransform ResizeHandle;
        public GameObject Content;

        public static string Id;
        public static Text Title;
        public InputField NameField;
        public Button DecreaseSizeButton;
        public Button IncreaseSizeButton;
        public Button CurrentButton;
        public static InputField SearchField;
        public Button ClearButton;
        public static ScrollRect ScrollRect;
        public GridLayoutGroup GridLayoutGroup;
        public ToggleGroup toggleGroup;

        private static readonly Dictionary<string, string> translationCache = new Dictionary<string, string>();

        private static string titleText;
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

        private static bool canScroll;
        private static bool shouldScroll;

        private GameObject pickerEntryTemplate;

        public static void SetCategory(string id, string name, ChaListDefine.CategoryNo categoryNo, Func<int> getCurrentValue, Action<CustomSelectInfo> setCurrentValue)
        {
            if (Id == id)
            {
                instance.gameObject.SetActive(!instance.gameObject.activeSelf);
                return;
            }

            Id = id;
            titleText = name;
            if (Title != null)
                Title.text = titleText;
            CategoryNo = categoryNo;
            GetCurrentValue = getCurrentValue;
            SetCurrentValue = setCurrentValue;
            itemList = dictSelectInfo[categoryNo];
            isDirty = true;
            instance.gameObject.SetActive(true);
            ScrollRect.content.localPosition = Vector2.zero;
            SearchField.text = "";
            shouldScroll = true;
        }

        private void Awake()
        {
            instance = this;
            Canvas = (RectTransform)transform;

            Canvas.anchoredPosition = new Vector2(70, 65);
            Canvas.offsetMax = new Vector2(
                Canvas.anchoredPosition.x + PseudoMaker.PickerWindowWidth.Value,
                Canvas.anchoredPosition.y + PseudoMaker.PickerWindowHeight.Value
            );

            DragPanel = (RectTransform)transform.Find("DragPanel").transform;
            Title = DragPanel.Find("UITitleText").gameObject.GetComponent<Text>();
            CloseButton = (RectTransform)DragPanel.Find("CloseButton");
            ResizeHandle = (RectTransform)transform.Find("ResizeHandle").transform;
            ScrollRect = transform.Find("Scroll View").gameObject.GetComponent<ScrollRect>();
            Content = ScrollRect.transform.Find("Viewport/Content").gameObject;

            GridLayoutGroup = Content.GetComponent<GridLayoutGroup>();
            GridLayoutGroup.cellSize = new Vector2(PseudoMaker.PickerThumbnailSize.Value, PseudoMaker.PickerThumbnailSize.Value);
            InitialBotPadding = GridLayoutGroup.padding.bottom;
            InitialTopPadding = GridLayoutGroup.padding.top;

            toggleGroup = Content.GetComponent<ToggleGroup>();

            pickerEntryTemplate = GridLayoutGroup.transform.Find("PickerItemTemplate").gameObject;
            pickerEntryTemplate.gameObject.AddComponent<CustomSelectInfoComponent>();
            pickerEntryTemplate.transform.SetParent(GridLayoutGroup.transform.parent);
            pickerEntryTemplate.SetActive(false);

            NameField = transform.Find("NameField").gameObject.GetComponent<InputField>();

            SearchField = transform.Find("SearchInputField").gameObject.GetComponent<InputField>();
            SearchField.onValueChanged.AddListener((value) =>
            {
                if (value != "")
                    itemList = dictSelectInfo[CategoryNo].Where(x => FilterInfo(x, value)).ToList();
                else itemList = dictSelectInfo[CategoryNo];
                isDirty = true;
            });
# if KKS
            SearchField.m_Colors.selectedColor = SearchField.colors.highlightedColor;
#endif

            ClearButton = transform.Find("ClearButton").gameObject.GetComponent<Button>();
            ClearButton.onClick.AddListener(() => SearchField.text = "");

            CurrentButton = transform.Find("CurrentButton").gameObject.GetComponent<Button>();
            CurrentButton.onClick.AddListener(ScrollToSelection);

            DecreaseSizeButton = transform.Find("DecreaseSizeButton").gameObject.GetComponent<Button>();
            DecreaseSizeButton.onClick.AddListener(() => AdjustGridSize(false));

            IncreaseSizeButton = transform.Find("IncreaseSizeButton").gameObject.GetComponent<Button>();
            IncreaseSizeButton.onClick.AddListener(() => AdjustGridSize(true));

            CloseButton.gameObject.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));

            MovableWindow.MakeObjectDraggable(DragPanel, Canvas, false);
            ResizableWindow.MakeObjectResizable(
                ResizeHandle,
                Canvas,
                new Vector2(120, 200),
                PseudoMakerUI.MainWindow.GetComponent<CanvasScaler>(),
                false,
                () => {
                    PopulateEntryCache();
                    AdjustCanvasSizeToColumns();
                    PseudoMaker.PickerWindowWidth.Value = Canvas.sizeDelta.x;
                    PseudoMaker.PickerWindowHeight.Value = Canvas.sizeDelta.y;
                    isDirty = true;
                }
            );

            PopulateEntryCache();
            AdjustCanvasSizeToColumns();

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Title.text = $"{titleText} Picker";
            var current = dictSelectInfo[CategoryNo].FirstOrDefault(x => x.index == GetCurrentValue());
            if (current != null)
                ((Text)NameField.placeholder).text = current.name;
        }

        private void Update()
        {
            var visibleItemCount = itemList.Count(x => !x.disvisible);
            var offscreenItemCount = Mathf.Max(0, visibleItemCount - cachedEntries.Count);

            var rowsAboveViewRect = Mathf.FloorToInt(Mathf.Clamp(ScrollRect.content.localPosition.y / (int)GridLayoutGroup.cellSize.x, 0, offscreenItemCount));
            var itemsAboveViewRect = rowsAboveViewRect * columnCount;

            if (lastItemsAboveViewRect == itemsAboveViewRect && !isDirty && !shouldScroll) return;

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
            if (shouldScroll && canScroll)
            {
                StartCoroutine(ScrollToSelectionCoroutine());
            }

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

        public IEnumerator ScrollToSelectionCoroutine()
        {
            yield return null;
            ScrollToSelection();
            shouldScroll = false;
        }

        public void ScrollToSelection()
        {
            var itemRow =itemList.FindIndex(x => x.index == GetCurrentValue());
            itemRow = itemRow < 0 ? -1 : itemRow / columnCount;

            if (itemRow >= 0)
            {
                var minScroll = (itemRow - 4f) * GridLayoutGroup.cellSize.x;
                var maxScroll = (itemRow + 0.5f) * GridLayoutGroup.cellSize.x;
                var targetScroll = itemRow * GridLayoutGroup.cellSize.x;
                if (ScrollRect.content.localPosition.y < minScroll || ScrollRect.content.localPosition.y > maxScroll)
                    ScrollRect.content.localPosition = new Vector2(ScrollRect.content.localPosition.x, Mathf.Max(0, targetScroll - GridLayoutGroup.cellSize.x * 1.7f));
            }
        }

        private void UpdateSelection()
        {
            cachedEntries.ForEach(x => x.tgl.isOn = false);
            var onToggle = cachedEntries.FirstOrDefault(x => x.info.index == GetCurrentValue());
            if (onToggle != null) onToggle.tgl.isOn = true;
        }

        private bool FilterInfo(CustomSelectInfo info, string search)
        {
            var show = false;
            if (info == null) return show;

            show |= info.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            //show |= info.assetBundle.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            if (translationCache.TryGetValue(info.name, out var translation))
                show |= translation.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            var _info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)info.category, info.index);
            show |= _info?.Author?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            return show;
        }

        private void PopulateEntryCache()
        {
            var rectTransform = ScrollRect.transform as RectTransform;

            var _columnCount = ((int)rectTransform.rect.width) / (int)GridLayoutGroup.cellSize.x;
            var _rowCount = ((int)rectTransform.rect.height) / (int)GridLayoutGroup.cellSize.x + 2;

            var totalVisibleItems = _columnCount * _rowCount;
            var newEntries = totalVisibleItems - cachedEntries.Count;

            rowCount = _rowCount;
            columnCount = _columnCount;

            if (newEntries <= 0)
            {
                if (newEntries < 0)
                {
                    for (var i = 0; i < newEntries * -1; i++)
                    {
                        var component = cachedEntries[cachedEntries.Count - 1];
                        cachedEntries.Remove(component);
                        Destroy(component.gameObject);
                    }
                }
                return;
            }

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
                    {
                        ((Text)NameField.placeholder).text = copyInfoComp.info.name;
                        SetCurrentValue(copyInfoComp.info);
                    }
                });

                var hoverComponent = copy.AddComponent<SelectListInfoHoverComponent>();
                hoverComponent.onEnterAction = () => TranslationHelper.TranslateAsync(copyInfoComp.info.name, value => NameField.text = value);
                hoverComponent.onExitAction = () => NameField.text = "";

                copyInfoComp.img = copy.GetComponent<Image>();

                cachedEntries.Add(copyInfoComp);
                copy.SetActive(false);
            }
            canScroll = true;
        }

        private void AdjustGridSize(bool increase)
        {
            var currentSize = GridLayoutGroup.cellSize.x;
            var newSize = increase ? currentSize + 5 : currentSize - 5;
            PseudoMaker.PickerThumbnailSize.Value = (int)newSize;
            GridLayoutGroup.cellSize = new Vector2(newSize, newSize);
            PopulateEntryCache();
            isDirty = true;
            AdjustCanvasSizeToColumns();
        }

        private void AdjustCanvasSizeToColumns()
        {
            Canvas.offsetMax = new Vector2(
                18 + columnCount * GridLayoutGroup.cellSize.x + Canvas.offsetMin.x + GridLayoutGroup.spacing.x * (columnCount - 1),
                Canvas.offsetMax.y
            );
        }

        public static Sprite GetThumbSprite(CustomSelectInfo item)
        {
            var thumbTex = CommonLib.LoadAsset<Texture2D>(item.assetBundle, item.assetName, false, string.Empty);
            Sprite thumb = null;
            if (thumbTex)
            {
                thumb = Sprite.Create(thumbTex, new Rect(0f, 0f, thumbTex.width, thumbTex.height),
                    new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
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
