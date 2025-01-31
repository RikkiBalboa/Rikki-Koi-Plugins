using BepInEx;
using ChaCustom;
using Illusion.Component.UI.ColorPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static ChaCustom.CustomSelectKind;
using static GameCursor;
using static KKAPI.Maker.MakerConstants;

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
        private SelectKindType type;

        private int selectedIndex = 0;
        private Texture2D selectedThumbnail;
        private bool scrollToSelected = false;

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
            var selected = GetSelected();

            var width = StudioSkinColor.pickerRect.width - 60;
            int columns = Mathf.Max((int)Mathf.Floor(width / 100), 3);
            var size = width / columns;

            int totalRows = (int)Mathf.Ceil(lstSelectInfo.Count() / columns);
            int firstRow = Mathf.Clamp((int)(panelScroll.y / size), 0, totalRows);
            int maxrow = Mathf.Clamp((int)Mathf.Ceil(StudioSkinColor.pickerRect.height / size) + firstRow, 0, totalRows);

            if (scrollToSelected)
            {
                scrollToSelected = false;
                if (columns > 0)
                    panelScroll.y = lstSelectInfo.FindIndex(x => x.index == selected) / columns * size;
            }

            panelScroll = GUILayout.BeginScrollView(panelScroll, true, false);
            {
                GUILayout.Space(firstRow * size);

                for (int rows = firstRow; rows < maxrow; rows++)
                {
                    GUILayout.BeginHorizontal();
                    for (int column = 0; column < columns; column++)
                    {
                        var index = rows * columns + column;

                        if (!shownThumbnails.TryGetValue(index, out GUIContent thumbnail))
                        {
                            var texture = CommonLib.LoadAsset<Texture2D>(lstSelectInfo[index].assetBundle, lstSelectInfo[index].assetName);
                            if (thumbnail != null)
                                StudioSkinColor.Logger.LogInfo(texture.width);
                            shownThumbnails[index] = new GUIContent(texture);
                        }

                        var c = GUI.color;
                        if (selected == lstSelectInfo[index].index)
                            GUI.color = Color.cyan;

                        if (GUILayout.Button(thumbnail, new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft }, new GUILayoutOption[] { GUILayout.Height(size), GUILayout.Width(size) }))
                        {
                            SetSelected(lstSelectInfo[index].index);
                        }
                        GUI.color = c;
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space((totalRows - maxrow) * size);
            }
            GUILayout.EndScrollView();
        }

        public void UpdateSelected()
        {
            int newIndex = lstSelectInfo.FindIndex(x => x.index == GetSelected());
            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                selectedThumbnail = CommonLib.LoadAsset<Texture2D>(lstSelectInfo[selectedIndex].assetBundle, lstSelectInfo[selectedIndex].assetName);
            }
        }

        public void SetSelected(int id)
        {
            void UpdateClothesPattern(int kind, int pattern)
            {
                Clothes.parts[kind].colorInfo[pattern].pattern = id;
                SetClothes.parts[kind].colorInfo[pattern].pattern = id;
                if (Controller.IsMultiPartTop(kind))
                {
                    if (pattern == 0)
                    {
                        SelectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: true, updateTex02: false, updateTex03: false, updateTex04: false);
                        SelectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: true, updateTex02: false, updateTex03: false, updateTex04: false);
                    }
                    else if (pattern == 1)
                    {
                        SelectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: false, updateTex02: true, updateTex03: false, updateTex04: false);
                        SelectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: false, updateTex02: true, updateTex03: false, updateTex04: false);
                    }
                    else if (pattern == 2)
                    {
                        SelectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: true, updateTex04: false);
                        SelectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: true, updateTex04: false);
                    }
                    else if (pattern == 3)
                    {
                        SelectedCharacter.ChangeCustomClothes(main: false, 0, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: true);
                        SelectedCharacter.ChangeCustomClothes(main: false, 1, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: true);
                    }
                }
                if (pattern == 0)
                    SelectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: true, updateTex02: false, updateTex03: false, updateTex04: false);
                if (pattern == 1)
                    SelectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: false, updateTex02: true, updateTex03: false, updateTex04: false);
                if (pattern == 2)
                    SelectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: true, updateTex04: false);
                if (pattern == 3)
                    SelectedCharacter.ChangeCustomClothes(main: true, kind, updateColor: false, updateTex01: false, updateTex02: false, updateTex03: false, updateTex04: true);
            }


            if (SelectedCharacter == null)
                return;

            switch (type)
            {
                case SelectKindType.FaceDetail:
                    SelectedCharacter.fileFace.detailId = id;
                    SelectedCharacter.ChangeSettingFaceDetail();
                    break;
                case SelectKindType.Eyebrow:
                    SelectedCharacter.fileFace.eyebrowId = id;
                    SelectedCharacter.ChangeSettingEyebrow();
                    break;
                case SelectKindType.EyelineUp:
                    SelectedCharacter.fileFace.eyelineUpId = id;
                    SelectedCharacter.ChangeSettingEyelineUp();
                    break;
                case SelectKindType.EyelineDown:
                    SelectedCharacter.fileFace.eyelineDownId = id;
                    SelectedCharacter.ChangeSettingEyelineDown();
                    break;
                case SelectKindType.EyeWGrade:
                    SelectedCharacter.fileFace.whiteId = id;
                    SelectedCharacter.ChangeSettingWhiteOfEye(true, false);
                    break;
                case SelectKindType.EyeHLUp:
                    SelectedCharacter.fileFace.hlUpId = id;
                    SelectedCharacter.ChangeSettingEyeHiUp();
                    break;
                case SelectKindType.EyeHLDown:
                    SelectedCharacter.fileFace.hlDownId = id;
                    SelectedCharacter.ChangeSettingEyeHiDown();
                    break;
                case SelectKindType.Pupil:
                    SelectedCharacter.fileFace.pupil[0].id = id;
                    SelectedCharacter.ChangeSettingEye(true, false, false);
                    break;
                case SelectKindType.PupilGrade:
                    SelectedCharacter.fileFace.pupil[0].gradMaskId = id;
                    SelectedCharacter.ChangeSettingEye(false, true, false);
                    break;
                case SelectKindType.Nose:
                    SelectedCharacter.fileFace.noseId = id;
                    SelectedCharacter.ChangeSettingNose();
                    break;
                case SelectKindType.Lipline:
                    SelectedCharacter.fileFace.lipLineId = id;
                    SelectedCharacter.AddUpdateCMFaceTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLipLine: true, inpMole: false);
                    SelectedCharacter.CreateFaceTexture();
                    SelectedCharacter.SetFaceBaseMaterial();
                    break;
                case SelectKindType.Mole:
                    SelectedCharacter.fileFace.moleId = id;
                    SelectedCharacter.AddUpdateCMFaceTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpCheek: false, inpLipLine: false, inpMole: true);
                    SelectedCharacter.CreateFaceTexture();
                    SelectedCharacter.SetFaceBaseMaterial();
                    break;
                case SelectKindType.Eyeshadow:
                    SelectedCharacter.fileFace.baseMakeup.eyeshadowId = id;
                    SelectedCharacter.ChangeSettingEyeShadow();
                    break;
                case SelectKindType.Cheek:
                    SelectedCharacter.fileFace.baseMakeup.cheekId = id;
                    SelectedCharacter.AddUpdateCMFaceTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpCheek: true, inpLipLine: false, inpMole: false);
                    SelectedCharacter.CreateFaceTexture();
                    SelectedCharacter.SetFaceBaseMaterial();
                    break;
                case SelectKindType.Lip:
                    SelectedCharacter.fileFace.baseMakeup.lipId = id;
                    SelectedCharacter.ChangeSettingLip();
                    break;
                case SelectKindType.FacePaint01:
                    SelectedCharacter.fileFace.baseMakeup.paintId[0] = id;
                    SelectedCharacter.AddUpdateCMFaceTexFlags(inpBase: false, inpSub: false, inpPaint01: true, inpPaint02: false, inpCheek: false, inpLipLine: false, inpMole: false);
                    SelectedCharacter.CreateFaceTexture();
                    SelectedCharacter.SetFaceBaseMaterial();
                    break;
                case SelectKindType.FacePaint02:
                    SelectedCharacter.fileFace.baseMakeup.paintId[1] = id;
                    SelectedCharacter.AddUpdateCMFaceTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: true, inpCheek: false, inpLipLine: false, inpMole: false);
                    SelectedCharacter.CreateFaceTexture();
                    SelectedCharacter.SetFaceBaseMaterial();
                    break;
                case SelectKindType.BodyDetail:
                    SelectedCharacter.fileBody.detailId = id;
                    SelectedCharacter.ChangeSettingBodyDetail();
                    break;
                case SelectKindType.Nip:
                    SelectedCharacter.fileBody.nipId = id;
                    SelectedCharacter.ChangeSettingNip();
                    break;
                case SelectKindType.Underhair:
                    SelectedCharacter.fileBody.underhairId = id;
                    SelectedCharacter.ChangeSettingUnderhair();
                    break;
                case SelectKindType.Sunburn:
                    SelectedCharacter.fileBody.sunburnId = id;
                    SelectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: false, inpSunburn: true);
                    SelectedCharacter.CreateBodyTexture();
                    SelectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint01:
                    SelectedCharacter.fileBody.paintId[0] = id;
                    SelectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: true, inpPaint02: false, inpSunburn: false);
                    SelectedCharacter.CreateBodyTexture();
                    SelectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint02:
                    SelectedCharacter.fileBody.paintId[1] = id;
                    SelectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: true, inpSunburn: false);
                    SelectedCharacter.CreateBodyTexture();
                    SelectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint01Layout:
                    SelectedCharacter.fileBody.paintLayoutId[0] = id;
                    SelectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: true, inpPaint02: false, inpSunburn: false);
                    SelectedCharacter.CreateBodyTexture();
                    SelectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.BodyPaint02Layout:
                    SelectedCharacter.fileBody.paintLayoutId[1] = id;
                    SelectedCharacter.AddUpdateCMBodyTexFlags(inpBase: false, inpSub: false, inpPaint01: false, inpPaint02: true, inpSunburn: false);
                    SelectedCharacter.CreateBodyTexture();
                    SelectedCharacter.SetBodyBaseMaterial();
                    break;
                case SelectKindType.HairBack:
                    SelectedCharacter.fileHair.parts[0].id = id;
                    SelectedCharacter.ChangeHairBack(true);
                    break;
                case SelectKindType.HairFront:
                    SelectedCharacter.fileHair.parts[1].id = id;
                    SelectedCharacter.ChangeHairFront(true);
                    break;
                case SelectKindType.HairSide:
                    SelectedCharacter.fileHair.parts[2].id = id;
                    SelectedCharacter.ChangeHairSide(true);
                    break;
                case SelectKindType.HairExtension:
                    SelectedCharacter.fileHair.parts[3].id = id;
                    SelectedCharacter.ChangeHairOption(true);
                    break;
                case SelectKindType.CosTop:
                    //clothes.parts[clothesType].sleevesType = 0;
                    //setClothes.parts[clothesType].sleevesType = 0;
                    Clothes.parts[0].id = id;
                    SetClothes.parts[0].id = id;
                    SelectedCharacter.ChangeClothesTop(id, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor01:
                    Clothes.subPartsId[0] = id;
                    SetClothes.subPartsId[0] = id;
                    SelectedCharacter.ChangeClothesTop(0, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor02:
                    Clothes.subPartsId[1] = id;
                    SetClothes.subPartsId[1] = id;
                    SelectedCharacter.ChangeClothesTop(0, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosSailor03:
                    Clothes.subPartsId[2] = id;
                    SetClothes.subPartsId[2] = id;
                    SelectedCharacter.ChangeClothesTop(0, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket01:
                    Clothes.subPartsId[0] = id;
                    SetClothes.subPartsId[0] = id;
                    SelectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket02:
                    Clothes.subPartsId[1] = id;
                    SetClothes.subPartsId[1] = id;
                    SelectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosJacket03:
                    Clothes.subPartsId[2] = id;
                    SetClothes.subPartsId[2] = id;
                    SelectedCharacter.ChangeClothesTop(1, SetClothes.subPartsId[0], SetClothes.subPartsId[1], SetClothes.subPartsId[2], true);
                    break;
                case SelectKindType.CosTopPtn01:
                    UpdateClothesPattern(0, 0);
                    break;
                case SelectKindType.CosTopPtn02:
                    UpdateClothesPattern(0, 1);
                    break;
                case SelectKindType.CosTopPtn03:
                    UpdateClothesPattern(0, 2);
                    break;
                case SelectKindType.CosTopPtn04:
                    UpdateClothesPattern(0, 3);
                    break;
                case SelectKindType.CosTopEmblem:
                    Clothes.parts[0].emblemeId = id;
                    SetClothes.parts[0].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(0, id);
                    break;
                case SelectKindType.CosBot:
                    Clothes.parts[1].id = id;
                    SetClothes.parts[1].id = id;
                    SelectedCharacter.ChangeClothesBot(id, true);
                    break;
                case SelectKindType.CosBotPtn01:
                    UpdateClothesPattern(1, 0);
                    break;
                case SelectKindType.CosBotPtn02:
                    UpdateClothesPattern(1, 1);
                    break;
                case SelectKindType.CosBotPtn03:
                    UpdateClothesPattern(1, 2);
                    break;
                case SelectKindType.CosBotPtn04:
                    UpdateClothesPattern(1, 3);
                    break;
                case SelectKindType.CosBotEmblem:
                    Clothes.parts[1].emblemeId = id;
                    SetClothes.parts[1].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(1, id);
                    break;
                case SelectKindType.CosBra:
                    Clothes.parts[2].id = id;
                    SetClothes.parts[2].id = id;
                    SelectedCharacter.ChangeClothesBra(id, true);
					break;
                case SelectKindType.CosBraPtn01:
                    UpdateClothesPattern(2, 0);
                    break;
                case SelectKindType.CosBraPtn02:
                    UpdateClothesPattern(2, 1);
                    break;
                case SelectKindType.CosBraPtn03:
                    UpdateClothesPattern(2, 2);
                    break;
                case SelectKindType.CosBraPtn04:
                    UpdateClothesPattern(2, 3);
                    break;
                case SelectKindType.CosBraEmblem:
                    Clothes.parts[2].emblemeId = id;
                    SetClothes.parts[2].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(2, id);
                    break;
                case SelectKindType.CosShorts:
                    Clothes.parts[3].id = id;
                    SetClothes.parts[3].id = id;
                    SelectedCharacter.ChangeClothesShorts(id, true);
                    break;
                case SelectKindType.CosShortsPtn01:
                    UpdateClothesPattern(3, 0);
                    break;
                case SelectKindType.CosShortsPtn02:
                    UpdateClothesPattern(3, 1);
                    break;
                case SelectKindType.CosShortsPtn03:
                    UpdateClothesPattern(3, 2);
                    break;
                case SelectKindType.CosShortsPtn04:
                    UpdateClothesPattern(3, 3);
                    break;
                case SelectKindType.CosShortsEmblem:
                    Clothes.parts[3].emblemeId = id;
                    SetClothes.parts[3].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(3, id);
                    break;
                case SelectKindType.CosGloves:
                    Clothes.parts[4].id = id;
                    SetClothes.parts[4].id = id;
                    SelectedCharacter.ChangeClothesGloves(id, true);
                    break;
                case SelectKindType.CosGlovesPtn01:
                    UpdateClothesPattern(4, 0);
                    break;
                case SelectKindType.CosGlovesPtn02:
                    UpdateClothesPattern(4, 1);
                    break;
                case SelectKindType.CosGlovesPtn03:
                    UpdateClothesPattern(4, 2);
                    break;
                case SelectKindType.CosGlovesPtn04:
                    UpdateClothesPattern(4, 3);
                    break;
                case SelectKindType.CosGlovesEmblem:
                    Clothes.parts[4].emblemeId = id;
                    SetClothes.parts[4].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(4, id);
                    break;
                case SelectKindType.CosPanst:
                    Clothes.parts[5].id = id;
                    SetClothes.parts[5].id = id;
                    SelectedCharacter.ChangeClothesPanst(id, true);
                    break;
                case SelectKindType.CosPanstPtn01:
                    UpdateClothesPattern(5, 0);
                    break;
                case SelectKindType.CosPanstPtn02:
                    UpdateClothesPattern(5, 1);
                    break;
                case SelectKindType.CosPanstPtn03:
                    UpdateClothesPattern(5, 2);
                    break;
                case SelectKindType.CosPanstPtn04:
                    UpdateClothesPattern(5, 3);
                    break;
                case SelectKindType.CosPanstEmblem:
                    Clothes.parts[5].emblemeId = id;
                    SetClothes.parts[5].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(5, id);
                    break;
                case SelectKindType.CosSocks:
                    Clothes.parts[6].id = id;
                    SetClothes.parts[6].id = id;
                    SelectedCharacter.ChangeClothesSocks(id, true);
                    break;
                case SelectKindType.CosSocksPtn01:
                    UpdateClothesPattern(6, 0);
                    break;
                case SelectKindType.CosSocksPtn02:
                    UpdateClothesPattern(6, 1);
                    break;
                case SelectKindType.CosSocksPtn03:
                    UpdateClothesPattern(6, 2);
                    break;
                case SelectKindType.CosSocksPtn04:
                    UpdateClothesPattern(6, 3);
                    break;
                case SelectKindType.CosSocksEmblem:
                    Clothes.parts[6].emblemeId = id;
                    SetClothes.parts[6].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(6, id);
                    break;
                case SelectKindType.CosInnerShoes:
                    Clothes.parts[7].id = id;
                    SetClothes.parts[7].id = id;
                    SelectedCharacter.ChangeClothesShoes(0, id, true);
                    break;
                case SelectKindType.CosInnerShoesPtn01:
                    UpdateClothesPattern(7, 0);
                    break;
                case SelectKindType.CosInnerShoesPtn02:
                    UpdateClothesPattern(7, 1);
                    break;
                case SelectKindType.CosInnerShoesPtn03:
                    UpdateClothesPattern(7, 2);
                    break;
                case SelectKindType.CosInnerShoesPtn04:
                    UpdateClothesPattern(7, 3);
                    break;
                case SelectKindType.CosInnerShoesEmblem:
                    Clothes.parts[7].emblemeId = id;
                    SetClothes.parts[7].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(7, id);
                    break;
                case SelectKindType.CosOuterShoes:
                    Clothes.parts[8].id = id;
                    SetClothes.parts[8].id = id;
                    SelectedCharacter.ChangeClothesShoes(1, id, true);
                    break;
                case SelectKindType.CosOuterShoesPtn01:
                    UpdateClothesPattern(8, 0);
                    break;
                case SelectKindType.CosOuterShoesPtn02:
                    UpdateClothesPattern(8, 1);
                    break;
                case SelectKindType.CosOuterShoesPtn03:
                    UpdateClothesPattern(8, 2);
                    break;
                case SelectKindType.CosOuterShoesPtn04:
                    UpdateClothesPattern(8, 3);
                    break;
                case SelectKindType.CosOuterShoesEmblem:
                    Clothes.parts[8].emblemeId = id;
                    SetClothes.parts[8].emblemeId = id;
                    SelectedCharacter.ChangeCustomEmblem(8, id);
                    break;
                case SelectKindType.HairGloss:
                    SelectedCharacter.fileHair.glossId = id;
                    SelectedCharacter.LoadHairGlossMask();
                    SelectedCharacter.ChangeSettingHairGlossMaskAll();
                    break;
                case SelectKindType.HeadType:
                    SelectedCharacter.fileFace.headId = id;
                    SelectedCharacter.ChangeHead();
					break;
                case SelectKindType.CosTopEmblem2:
                    Clothes.parts[0].emblemeId2 = id;
					break;
                case SelectKindType.CosBotEmblem2:
                    Clothes.parts[1].emblemeId2 = id;
					break;
                case SelectKindType.CosBraEmblem2:
                    Clothes.parts[2].emblemeId2 = id;
					break;
                case SelectKindType.CosShortsEmblem2:
                    Clothes.parts[3].emblemeId2 = id;
					break;
                case SelectKindType.CosGlovesEmblem2:
                    Clothes.parts[4].emblemeId2 = id;
					break;
                case SelectKindType.CosPanstEmblem2:
                    Clothes.parts[5].emblemeId2 = id;
					break;
                case SelectKindType.CosSocksEmblem2:
                    Clothes.parts[6].emblemeId2 = id;
					break;
                case SelectKindType.CosInnerShoesEmblem2:
                    Clothes.parts[7].emblemeId2 = id;
					break;
                case SelectKindType.CosOuterShoesEmblem2:
                    Clothes.parts[8].emblemeId2 = id;
					break;
            }
        }

        public int GetSelected()
        {
            if (SelectedCharacter == null)
                return 0;

            switch (type)
            {
                case SelectKindType.FaceDetail:
                    return SelectedCharacter.fileFace.detailId;
                case SelectKindType.Eyebrow:
                    return SelectedCharacter.fileFace.eyebrowId;
                case SelectKindType.EyelineUp:
                    return SelectedCharacter.fileFace.eyelineUpId;
                case SelectKindType.EyelineDown:
                    return SelectedCharacter.fileFace.eyelineDownId;
                case SelectKindType.EyeWGrade:
                    return SelectedCharacter.fileFace.whiteId;
                case SelectKindType.EyeHLUp:
                    return SelectedCharacter.fileFace.hlUpId;
                case SelectKindType.EyeHLDown:
                    return SelectedCharacter.fileFace.hlDownId;
                case SelectKindType.Pupil:
                    return SelectedCharacter.fileFace.pupil[0].id;
                case SelectKindType.PupilGrade:
                    return SelectedCharacter.fileFace.pupil[0].gradMaskId;
                case SelectKindType.Nose:
                    return SelectedCharacter.fileFace.noseId;
                case SelectKindType.Lipline:
                    return SelectedCharacter.fileFace.lipLineId;
                case SelectKindType.Mole:
                    return SelectedCharacter.fileFace.moleId;
                case SelectKindType.Eyeshadow:
                    return SelectedCharacter.fileFace.baseMakeup.eyeshadowId;
                case SelectKindType.Cheek:
                    return SelectedCharacter.fileFace.baseMakeup.cheekId;
                case SelectKindType.Lip:
                    return SelectedCharacter.fileFace.baseMakeup.lipId;
                case SelectKindType.FacePaint01:
                    return SelectedCharacter.fileFace.baseMakeup.paintId[0];
                case SelectKindType.FacePaint02:
                    return SelectedCharacter.fileFace.baseMakeup.paintId[1];
                case SelectKindType.BodyDetail:
                    return SelectedCharacter.fileBody.detailId;
                case SelectKindType.Nip:
                    return SelectedCharacter.fileBody.nipId;
                case SelectKindType.Underhair:
                    return SelectedCharacter.fileBody.underhairId;
                case SelectKindType.Sunburn:
                    return SelectedCharacter.fileBody.sunburnId;
                case SelectKindType.BodyPaint01:
                    return SelectedCharacter.fileBody.paintId[0];
                case SelectKindType.BodyPaint02:
                    return SelectedCharacter.fileBody.paintId[1];
                case SelectKindType.BodyPaint01Layout:
                    return SelectedCharacter.fileBody.paintLayoutId[0];
                case SelectKindType.BodyPaint02Layout:
                    return SelectedCharacter.fileBody.paintLayoutId[1];
                case SelectKindType.HairBack:
                    return SelectedCharacter.fileHair.parts[0].id;
                case SelectKindType.HairFront:
                    return SelectedCharacter.fileHair.parts[1].id;
                case SelectKindType.HairSide:
                    return SelectedCharacter.fileHair.parts[2].id;
                case SelectKindType.HairExtension:
                    return SelectedCharacter.fileHair.parts[3].id;
                case SelectKindType.CosTop:
                    return Clothes.parts[0].id;
                case SelectKindType.CosSailor01:
                    return Clothes.subPartsId[0];
                case SelectKindType.CosSailor02:
                    return Clothes.subPartsId[1];
                case SelectKindType.CosSailor03:
                    return Clothes.subPartsId[2];
                case SelectKindType.CosJacket01:
                    return Clothes.subPartsId[0];
                case SelectKindType.CosJacket02:
                    return Clothes.subPartsId[1];
                case SelectKindType.CosJacket03:
                    return Clothes.subPartsId[2];
                case SelectKindType.CosTopPtn01:
                    return Clothes.parts[0].colorInfo[0].pattern;
                case SelectKindType.CosTopPtn02:
                    return Clothes.parts[0].colorInfo[1].pattern;
                case SelectKindType.CosTopPtn03:
                    return Clothes.parts[0].colorInfo[2].pattern;
                case SelectKindType.CosTopPtn04:
                    return Clothes.parts[0].colorInfo[3].pattern;
                case SelectKindType.CosTopEmblem:
                    return Clothes.parts[0].emblemeId;
                case SelectKindType.CosBot:
                    return Clothes.parts[1].id;
                case SelectKindType.CosBotPtn01:
                    return Clothes.parts[1].colorInfo[0].pattern;
                case SelectKindType.CosBotPtn02:
                    return Clothes.parts[1].colorInfo[1].pattern;
                case SelectKindType.CosBotPtn03:
                    return Clothes.parts[1].colorInfo[2].pattern;
                case SelectKindType.CosBotPtn04:
                    return Clothes.parts[1].colorInfo[3].pattern;
                case SelectKindType.CosBotEmblem:
                    return Clothes.parts[1].emblemeId;
                case SelectKindType.CosBra:
                    return Clothes.parts[2].id;
                case SelectKindType.CosBraPtn01:
                    return Clothes.parts[2].colorInfo[0].pattern;
                case SelectKindType.CosBraPtn02:
                    return Clothes.parts[2].colorInfo[1].pattern;
                case SelectKindType.CosBraPtn03:
                    return Clothes.parts[2].colorInfo[2].pattern;
                case SelectKindType.CosBraPtn04:
                    return Clothes.parts[2].colorInfo[3].pattern;
                case SelectKindType.CosBraEmblem:
                    return Clothes.parts[2].emblemeId;
                case SelectKindType.CosShorts:
                    return Clothes.parts[3].id;
                case SelectKindType.CosShortsPtn01:
                    return Clothes.parts[3].colorInfo[0].pattern;
                case SelectKindType.CosShortsPtn02:
                    return Clothes.parts[3].colorInfo[1].pattern;
                case SelectKindType.CosShortsPtn03:
                    return Clothes.parts[3].colorInfo[2].pattern;
                case SelectKindType.CosShortsPtn04:
                    return Clothes.parts[3].colorInfo[3].pattern;
                case SelectKindType.CosShortsEmblem:
                    return Clothes.parts[3].emblemeId;
                case SelectKindType.CosGloves:
                    return Clothes.parts[4].id;
                case SelectKindType.CosGlovesPtn01:
                    return Clothes.parts[4].colorInfo[0].pattern;
                case SelectKindType.CosGlovesPtn02:
                    return Clothes.parts[4].colorInfo[1].pattern;
                case SelectKindType.CosGlovesPtn03:
                    return Clothes.parts[4].colorInfo[2].pattern;
                case SelectKindType.CosGlovesPtn04:
                    return Clothes.parts[4].colorInfo[3].pattern;
                case SelectKindType.CosGlovesEmblem:
                    return Clothes.parts[4].emblemeId;
                case SelectKindType.CosPanst:
                    return Clothes.parts[5].id;
                case SelectKindType.CosPanstPtn01:
                    return Clothes.parts[5].colorInfo[0].pattern;
                case SelectKindType.CosPanstPtn02:
                    return Clothes.parts[5].colorInfo[1].pattern;
                case SelectKindType.CosPanstPtn03:
                    return Clothes.parts[5].colorInfo[2].pattern;
                case SelectKindType.CosPanstPtn04:
                    return Clothes.parts[5].colorInfo[3].pattern;
                case SelectKindType.CosPanstEmblem:
                    return Clothes.parts[5].emblemeId;
                case SelectKindType.CosSocks:
                    return Clothes.parts[6].id;
                case SelectKindType.CosSocksPtn01:
                    return Clothes.parts[6].colorInfo[0].pattern;
                case SelectKindType.CosSocksPtn02:
                    return Clothes.parts[6].colorInfo[1].pattern;
                case SelectKindType.CosSocksPtn03:
                    return Clothes.parts[6].colorInfo[2].pattern;
                case SelectKindType.CosSocksPtn04:
                    return Clothes.parts[6].colorInfo[3].pattern;
                case SelectKindType.CosSocksEmblem:
                    return Clothes.parts[6].emblemeId;
                case SelectKindType.CosInnerShoes:
                    return Clothes.parts[7].id;
                case SelectKindType.CosInnerShoesPtn01:
                    return Clothes.parts[7].colorInfo[0].pattern;
                case SelectKindType.CosInnerShoesPtn02:
                    return Clothes.parts[7].colorInfo[1].pattern;
                case SelectKindType.CosInnerShoesPtn03:
                    return Clothes.parts[7].colorInfo[2].pattern;
                case SelectKindType.CosInnerShoesPtn04:
                    return Clothes.parts[7].colorInfo[3].pattern;
                case SelectKindType.CosInnerShoesEmblem:
                    return Clothes.parts[7].emblemeId;
                case SelectKindType.CosOuterShoes:
                    return Clothes.parts[8].id;
                case SelectKindType.CosOuterShoesPtn01:
                    return Clothes.parts[8].colorInfo[0].pattern;
                case SelectKindType.CosOuterShoesPtn02:
                    return Clothes.parts[8].colorInfo[1].pattern;
                case SelectKindType.CosOuterShoesPtn03:
                    return Clothes.parts[8].colorInfo[2].pattern;
                case SelectKindType.CosOuterShoesPtn04:
                    return Clothes.parts[8].colorInfo[3].pattern;
                case SelectKindType.CosOuterShoesEmblem:
                    return Clothes.parts[8].emblemeId;
                case SelectKindType.HairGloss:
                    return SelectedCharacter.fileHair.glossId;
                case SelectKindType.HeadType:
                    return SelectedCharacter.fileFace.headId;
                case SelectKindType.CosTopEmblem2:
                    return Clothes.parts[0].emblemeId2;
                case SelectKindType.CosBotEmblem2:
                    return Clothes.parts[1].emblemeId2;
                case SelectKindType.CosBraEmblem2:
                    return Clothes.parts[2].emblemeId2;
                case SelectKindType.CosShortsEmblem2:
                    return Clothes.parts[3].emblemeId2;
                case SelectKindType.CosGlovesEmblem2:
                    return Clothes.parts[4].emblemeId2;
                case SelectKindType.CosPanstEmblem2:
                    return Clothes.parts[5].emblemeId2;
                case SelectKindType.CosSocksEmblem2:
                    return Clothes.parts[6].emblemeId2;
                case SelectKindType.CosInnerShoesEmblem2:
                    return Clothes.parts[7].emblemeId2;
                case SelectKindType.CosOuterShoesEmblem2:
                    return Clothes.parts[8].emblemeId2;
                default:
                    return 0;
            }
        }
    }
}
