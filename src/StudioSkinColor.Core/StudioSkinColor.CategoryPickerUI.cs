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

        public readonly string name = "Undefined";
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
            name = GetName();
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
        }

        public void DrawSelectedItem()
        {
            UpdateSelected();

            if (!StudioSkinColor.UseWideLayout.Value)
                GUILayout.Label(name, GUI.skin.label);

            GUILayout.BeginHorizontal();
            {
                if (StudioSkinColor.UseWideLayout.Value)
                    GUILayout.Label(name, new GUIStyle(GUI.skin.label)
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
            int newIndex = lstSelectInfo.FindIndex(x => x.index == Controller?.GetSelected(type));
            if (newIndex != selectedIndex && newIndex >= 0)
            {
                selectedIndex = newIndex;
                selectedThumbnail = CommonLib.LoadAsset<Texture2D>(lstSelectInfo[selectedIndex].assetBundle, lstSelectInfo[selectedIndex].assetName);
            }
        }


        public string GetName()
        {
            switch (type)
            {
                case SelectKindType.FaceDetail:
                    return "Face Overlay Type";
                case SelectKindType.Eyebrow:
                    return "Eyebrow Type";
                case SelectKindType.EyelineUp:
                    return "Upper eyeliner Type";
                case SelectKindType.EyelineDown:
                    return "Lower Eyeliner Type";
                case SelectKindType.EyeWGrade:
                    return "Sclera Type";
                case SelectKindType.EyeHLUp:
                    return "Upper Highlight Type";
                case SelectKindType.EyeHLDown:
                    return "Lower Highlight Type";
                case SelectKindType.Pupil:
                    return "Eye Type";
                case SelectKindType.PupilGrade:
                    return "Eye Gradient Type";
                case SelectKindType.Nose:
                    return "Nose Type";
                case SelectKindType.Lipline:
                    return "Lip Line Type";
                case SelectKindType.Mole:
                    return "Mole Type";
                case SelectKindType.Eyeshadow:
                    return "Eyeshadow Type";
                case SelectKindType.Cheek:
                    return "Cheek Type";
                case SelectKindType.Lip:
                    return "Lip Type";
                case SelectKindType.FacePaint01:
                    return "Paint 01 Type";
                case SelectKindType.FacePaint02:
                    return "Paint 02 Type";
                case SelectKindType.BodyDetail:
                    return "Skin Type";
                case SelectKindType.Nip:
                    return "Nipple Type";
                case SelectKindType.Underhair:
                    return "Pubic Hair Type";
                case SelectKindType.Sunburn:
                    return "Suntan Type";
                case SelectKindType.BodyPaint01:
                    return "Paint 01 Type";
                case SelectKindType.BodyPaint02:
                    return "Paint 02 Type";
                case SelectKindType.BodyPaint01Layout:
                    return "Paint 01 Position";
                case SelectKindType.BodyPaint02Layout:
                    return "Paint 02 Position";
                case SelectKindType.HairBack:
                    return "Back Hair Type";
                case SelectKindType.HairFront:
                    return "Front Hair Type";
                case SelectKindType.HairSide:
                    return "Side Hair Type";
                case SelectKindType.HairExtension:
                    return "Extension Type";
                case SelectKindType.CosTop:
                    return "Top Type";
                case SelectKindType.CosSailor01:
                    return "Body Type";
                case SelectKindType.CosSailor02:
                    return "Collar Type";
                case SelectKindType.CosSailor03:
                    return "Decoration Type";
                case SelectKindType.CosJacket01:
                    return "Innerwear Type";
                case SelectKindType.CosJacket02:
                    return "Outerwear Type";
                case SelectKindType.CosJacket03:
                    return "Decoration Type";
                case SelectKindType.CosTopPtn01:
                case SelectKindType.CosBotPtn01:
                case SelectKindType.CosBraPtn01:
                case SelectKindType.CosShortsPtn01:
                case SelectKindType.CosGlovesPtn01:
                case SelectKindType.CosPanstPtn01:
                case SelectKindType.CosSocksPtn01:
                case SelectKindType.CosInnerShoesPtn01:
                case SelectKindType.CosOuterShoesPtn01:
                    return "Cloth Pattern ①";
                case SelectKindType.CosTopPtn02:
                case SelectKindType.CosBotPtn02:
                case SelectKindType.CosBraPtn02:
                case SelectKindType.CosShortsPtn02:
                case SelectKindType.CosGlovesPtn02:
                case SelectKindType.CosPanstPtn02:
                case SelectKindType.CosSocksPtn02:
                case SelectKindType.CosInnerShoesPtn02:
                case SelectKindType.CosOuterShoesPtn02:
                    return "Cloth Pattern ②";
                case SelectKindType.CosTopPtn03:
                case SelectKindType.CosBotPtn03:
                case SelectKindType.CosBraPtn03:
                case SelectKindType.CosShortsPtn03:
                case SelectKindType.CosGlovesPtn03:
                case SelectKindType.CosPanstPtn03:
                case SelectKindType.CosSocksPtn03:
                case SelectKindType.CosInnerShoesPtn03:
                case SelectKindType.CosOuterShoesPtn03:
                    return "Cloth Pattern ③";
                case SelectKindType.CosTopPtn04:
                case SelectKindType.CosBotPtn04:
                case SelectKindType.CosBraPtn04:
                case SelectKindType.CosShortsPtn04:
                case SelectKindType.CosGlovesPtn04:
                case SelectKindType.CosPanstPtn04:
                case SelectKindType.CosSocksPtn04:
                case SelectKindType.CosInnerShoesPtn04:
                case SelectKindType.CosOuterShoesPtn04:
                    return "Cloth Pattern ④";
                case SelectKindType.CosTopEmblem:
                case SelectKindType.CosBotEmblem:
                case SelectKindType.CosBraEmblem:
                case SelectKindType.CosShortsEmblem:
                case SelectKindType.CosGlovesEmblem:
                case SelectKindType.CosPanstEmblem:
                case SelectKindType.CosSocksEmblem:
                case SelectKindType.CosInnerShoesEmblem:
                case SelectKindType.CosOuterShoesEmblem:
                    return "Emblem 02 Type";
                case SelectKindType.CosBot:
                    return "Bottom Type";
                case SelectKindType.CosBra:
                    return "Bra Type";
                case SelectKindType.CosShorts:
                    return "Underwear Type";
                case SelectKindType.CosGloves:
                    return "Gloves Type";
                case SelectKindType.CosPanst:
                    return "Pantyhose Type";
                case SelectKindType.CosSocks:
                    return "Legwear Type";
                case SelectKindType.CosInnerShoes:
                    return "Inner Shoe Type";
                case SelectKindType.CosOuterShoes:
#if KK
                    return "Outer Shoe Type";
#elif KKS
                    return "Shoe Type";
#endif
                case SelectKindType.HairGloss:
                    return "Hihglight Type";
                case SelectKindType.HeadType:
                    return "Face Type";
                case SelectKindType.CosTopEmblem2:
                case SelectKindType.CosBotEmblem2:
                case SelectKindType.CosBraEmblem2:
                case SelectKindType.CosShortsEmblem2:
                case SelectKindType.CosGlovesEmblem2:
                case SelectKindType.CosPanstEmblem2:
                case SelectKindType.CosSocksEmblem2:
                case SelectKindType.CosInnerShoesEmblem2:
                case SelectKindType.CosOuterShoesEmblem2:
                    return "Emblem 02 Type";
            }
            return "Undefined";
        }
    }
}
