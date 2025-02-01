using ChaCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChaCustom.CustomSelectKind;
using Sideloader.AutoResolver;
using KKAPI.Utilities;
namespace Plugins
{
    public class CategoryPicker
    {

        private ChaControl SelectedCharacter => StudioSkinColor.selectedCharacter;
        private StudioSkinColorCharaController Controller => StudioSkinColorCharaController.GetController(SelectedCharacter);
        private ChaFileClothes Clothes => SelectedCharacter.nowCoordinate.clothes;
        private ChaFileClothes SetClothes => SelectedCharacter.chaFile.coordinate[SelectedCharacter.chaFile.status.coordinateType].clothes;
        private ChaFileAccessory Accessories => SelectedCharacter.nowCoordinate.accessory;
        private ChaFileAccessory SetAccessories => SelectedCharacter.chaFile.coordinate[SelectedCharacter.chaFile.status.coordinateType].accessory;


        private static ChaListControl chaListCtrl;
        private List<CustomSelectInfo> lstSelectInfo;
        private List<CustomSelectInfo> lstSelectInfoFiltered = new List<CustomSelectInfo>();
        private SelectKindType type;

        private Dictionary<string, string> translationCache = new Dictionary<string, string>();

        private int selectedIndex = 0;
        private Texture2D selectedThumbnail;
        private bool scrollToSelected = false;
        private string searchText = "";
        private bool searchTextChanged = false;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (value == null) value = "";
                if (searchText != value)
                {
                    searchText = value;
                    searchTextChanged = true;

                    if (value.Count() > 0)
                        lstSelectInfoFiltered = lstSelectInfo.Where(x => Search(x, value)).ToList();
                    else
                        lstSelectInfoFiltered.Clear();
                }
            }
        }

        private bool Search(CustomSelectInfo info, string search)
        {
            var show = false;
            show |= info.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            //show |= info.assetBundle.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            if (translationCache.TryGetValue(info.name, out var translation))
                show |= translation.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            var _info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)info.category, info.index);
            if (_info != null)
            {
                show |= _info.Author.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return show;
        }

        private Vector2 panelScroll = Vector2.zero;

        public Action OnActivateAction { get; set; }
        public Action OnCloseAction { get; set; }

        public static void InitializeCategories()
        {
            chaListCtrl = new ChaListControl();
            chaListCtrl.LoadListInfoAll();
        }

        public CategoryPicker(SelectKindType type)
        {
            this.type = type;
            ChaListDefine.CategoryNo[] array = new ChaListDefine.CategoryNo[100]
            {
                ChaListDefine.CategoryNo.mt_face_detail,
                ChaListDefine.CategoryNo.mt_eyebrow,
                ChaListDefine.CategoryNo.mt_eyeline_up,
                ChaListDefine.CategoryNo.mt_eyeline_down,
                ChaListDefine.CategoryNo.mt_eye_white,
                ChaListDefine.CategoryNo.mt_eye_hi_up,
                ChaListDefine.CategoryNo.mt_eye_hi_down,
                ChaListDefine.CategoryNo.mt_eye,
                ChaListDefine.CategoryNo.mt_eye_gradation,
                ChaListDefine.CategoryNo.mt_nose,
                ChaListDefine.CategoryNo.mt_lipline,
                ChaListDefine.CategoryNo.mt_mole,
                ChaListDefine.CategoryNo.mt_eyeshadow,
                ChaListDefine.CategoryNo.mt_cheek,
                ChaListDefine.CategoryNo.mt_lip,
                ChaListDefine.CategoryNo.mt_face_paint,
                ChaListDefine.CategoryNo.mt_face_paint,
                ChaListDefine.CategoryNo.mt_body_detail,
                ChaListDefine.CategoryNo.mt_nip,
                ChaListDefine.CategoryNo.mt_underhair,
                ChaListDefine.CategoryNo.mt_sunburn,
                ChaListDefine.CategoryNo.mt_body_paint,
                ChaListDefine.CategoryNo.mt_body_paint,
                ChaListDefine.CategoryNo.bodypaint_layout,
                ChaListDefine.CategoryNo.bodypaint_layout,
                ChaListDefine.CategoryNo.bo_hair_b,
                ChaListDefine.CategoryNo.bo_hair_f,
                ChaListDefine.CategoryNo.bo_hair_s,
                ChaListDefine.CategoryNo.bo_hair_o,
                ChaListDefine.CategoryNo.co_top,
                ChaListDefine.CategoryNo.cpo_sailor_a,
                ChaListDefine.CategoryNo.cpo_sailor_b,
                ChaListDefine.CategoryNo.cpo_sailor_c,
                ChaListDefine.CategoryNo.cpo_jacket_a,
                ChaListDefine.CategoryNo.cpo_jacket_b,
                ChaListDefine.CategoryNo.cpo_jacket_c,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_bot,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_bra,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_shorts,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_gloves,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_panst,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_socks,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_shoes,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.co_shoes,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_pattern,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_hairgloss,
                ChaListDefine.CategoryNo.bo_head,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem,
                ChaListDefine.CategoryNo.mt_emblem
            };

            ChaListDefine.CategoryNo cn = array[(int)type];
            lstSelectInfo = new List<CustomSelectInfo>();
            chaListCtrl.GetCategoryInfo(cn).Values.ToList().ForEach(info =>
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
                lstSelectInfo.Add(customSelectInfo);
            });

            UpdateSelected();
        }

        public void DrawSelectedItem()
        {
            UpdateSelected();

            if (!StudioSkinColor.UseWideLayout.Value)
                GUILayout.Label("Test", GUI.skin.label);

            GUILayout.BeginHorizontal();
            {
                if (StudioSkinColor.UseWideLayout.Value)
                    GUILayout.Label("Test", new GUIStyle(GUI.skin.label)
                    {
                        fixedWidth = 160,
                        wordWrap = false,
                        alignment = TextAnchor.MiddleLeft,
                    });
                if (GUILayout.Button(new GUIContent(lstSelectInfo[selectedIndex].name, selectedThumbnail), new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft }, new GUILayoutOption[] { GUILayout.Height(50) }))
                {
                    scrollToSelected = true;
                    OnActivateAction();
                }
            }
            GUILayout.EndHorizontal();
        }

        private readonly Dictionary<int, GUIContent> shownThumbnails = new Dictionary<int, GUIContent>();

        public void DrawWindow()
        {
            var items = searchText.Count() == 0 ? lstSelectInfo : lstSelectInfoFiltered;

            var selected = Controller.GetSelected(type);

            float width = StudioSkinColor.pickerRect.width - 60;
            int columns = (int)Mathf.Max(Mathf.Floor(width / 100), 3);
            float size = width / columns;

            int totalRows = (int)Mathf.Ceil(items.Count() / (float)columns);
            int firstRow = Mathf.Clamp((int)(panelScroll.y / size), 0, totalRows);
            int maxrow = Mathf.Clamp((int)Mathf.Ceil(StudioSkinColor.pickerRect.height / size) + firstRow, 0, totalRows);

            if (scrollToSelected)
            {
                scrollToSelected = false;
                var index = items.FindIndex(x => x.index == selected);
                if (columns > 0 && index >= 0)
                    panelScroll.y = index / columns * size;
            }

            panelScroll = GUILayout.BeginScrollView(panelScroll, false, true);
            {
                GUILayout.Space(firstRow * size);

                for (int rows = firstRow; rows < maxrow; rows++)
                {
                    GUILayout.BeginHorizontal();
                    for (int column = 0; column < columns; column++)
                    {
                        var index = rows * columns + column;
                        if (index >= items.Count)
                            continue;
                        var item = items[index];

                        if (!shownThumbnails.TryGetValue(item.index, out GUIContent thumbnail))
                        {
                            var texture = CommonLib.LoadAsset<Texture2D>(item.assetBundle, item.assetName);
                            if (thumbnail != null)
                                StudioSkinColor.Logger.LogInfo(texture.width);
                            shownThumbnails[item.index] = new GUIContent(texture);
                        }

                        var c = GUI.color;
                        if (selected == item.index)
                            GUI.color = Color.cyan;

                        if (GUILayout.Button(thumbnail, new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft }, new GUILayoutOption[] { GUILayout.Height(size), GUILayout.Width(size) }))
                        {
                            Controller.SetSelectKind(type, item.index);
                        }
                        GUI.color = c;
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space((totalRows - maxrow) * size);
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUI.changed = false;
                if (searchTextChanged && Event.current.type == EventType.Repaint)
                {
                    searchTextChanged = false;
                    GUI.FocusControl("searchBox");
                }

                GUI.SetNextControlName("searchBox");
                var showToolTip = searchText.Length == 0 && GUI.GetNameOfFocusedControl() != "searchBox";
                var newValue = GUILayout.TextField(showToolTip ? "Search..." : searchText);
                if (GUI.changed)
                    SearchText = newValue;

                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                {
                    SearchText = "";
                    GUI.FocusControl("");
                }
            }
            GUILayout.EndHorizontal();
        }

        public void UpdateSelected()
        {
            int newIndex = lstSelectInfo.FindIndex(x => x.index == Controller.GetSelected(type));
            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                selectedThumbnail = CommonLib.LoadAsset<Texture2D>(lstSelectInfo[selectedIndex].assetBundle, lstSelectInfo[selectedIndex].assetName);
            }
        }
    }
}
