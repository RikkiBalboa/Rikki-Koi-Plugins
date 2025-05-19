using KoiClothesOverlayX;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;
using UnityEngine.UI;
using static PseudoMaker.PseudoMakerCharaController;
using static PseudoMaker.Compatibility;
using KKAPI;

namespace PseudoMaker.UI
{
    public class ClothingEditorPanel : BaseEditorPanel
    {
        private SelectKindType selectKindType = SelectKindType.CosTop;

        private Action clothingChangeAction;
        private Action<int> patternChangeAction;

        private Dictionary<int, List<GameObject>> clothingColorGameobjects;
        private Dictionary<int, List<GameObject>> clothingPatternGameobjects;
        private List<GameObject> clothingSailorGameObjects;
        private List<GameObject> clothingJacketGameObjects;
        private GameObject clothingOptionObject;
        private GameObject sleeveTypeObject;
        private List<GameObject> pushupBraGameObjects;
        private List<GameObject> pushupTopGameObjects;
        private GameObject MaterialEditorSplitter;
        private GameObject MaterialEditorButton;
        private GameObject overlaySplitter;
        private Toggle overlayHeader;
        private List<GameObject> otherOverlayObjects;
        private List<GameObject> mainOverlayObjects;
        private List<GameObject> multiOverlayObjects;

        private DropdownComponent fromDropDown;
        private int fromSelected = 0;
        private DropdownComponent toDropDown;
        private int toSelected = 0;

        private void OnEnable()
        {
            clothingChangeAction?.Invoke();

            RefreshDropdowns();

            TimelineCompatibilityHelper.SelectedClothingKind = PseudoMakerCharaController.SubCategoryToKind(SubCategory);
            _coordianteNameText = MoreOutfits.GetCurrentOutfitName();
        }

        public void RefreshDropdowns()
        {
            if (!fromDropDown || !toDropDown) return;
            fromDropDown.SetDropdownOptions(PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList());
            toDropDown.SetDropdownOptions(PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList());
        }

        private void OnDisable()
        {
            TimelineCompatibilityHelper.SelectedClothingKind = null;
        }

        protected override void Initialize()
        {
            base.Initialize();

            switch (SubCategory)
            {
                case SubCategory.ClothingPushup:
                    InitializePushup();
                    break;
                case SubCategory.ClothingSettings:
                    InitializeSettings();
                    break;
                case SubCategory.ClothingCopy:
                    InitializeCopy();
                    break;
                default:
                    InitializeClothing();
                    break;
            }
        }

        private void InitializeClothing()
        {
            clothingColorGameobjects = new Dictionary<int, List<GameObject>>();
            clothingPatternGameobjects = new Dictionary<int, List<GameObject>>();

            if (SubCategory == SubCategory.ClothingTop) selectKindType = SelectKindType.CosTop;
            else if (SubCategory == SubCategory.ClothingBottom) selectKindType = SelectKindType.CosBot;
            else if (SubCategory == SubCategory.ClothingBra) selectKindType = SelectKindType.CosBra;
            else if (SubCategory == SubCategory.ClothingUnderwear) selectKindType = SelectKindType.CosShorts;
            else if (SubCategory == SubCategory.ClothingGloves) selectKindType = SelectKindType.CosGloves;
            else if (SubCategory == SubCategory.ClothingPantyhose) selectKindType = SelectKindType.CosPanst;
            else if (SubCategory == SubCategory.ClothingLegwear) selectKindType = SelectKindType.CosSocks;
#if KK
            else if (SubCategory == SubCategory.ClothingShoesInDoors) selectKindType = SelectKindType.CosInnerShoes;
            else if (SubCategory == SubCategory.ClothingShoesOutdoors) selectKindType = SelectKindType.CosOuterShoes;
#else
            else if (SubCategory == SubCategory.ClothingShoes) selectKindType = SelectKindType.CosOuterShoes;
#endif
            patternChangeAction = pattern =>
            {
                if (clothingPatternGameobjects != null && clothingPatternGameobjects.TryGetValue(pattern, out var _gameObjects))
                {
                    var usePattern = PseudoMaker.selectedCharacterController.ClothingUsesPattern(PseudoMakerCharaController.SubCategoryToKind(SubCategory), pattern + 1);
                    foreach (var _gameObject in _gameObjects)
                        _gameObject.SetActive(usePattern);
                }
            };
            clothingChangeAction = () =>
            {
                var kind = PseudoMakerCharaController.SelectKindToIntKind(selectKindType);
                var useCols = PseudoMaker.selectedCharacterController.CheckClothingUseColor(kind);
                for (int colorNr = 0; colorNr < useCols.Length; colorNr++)
                {
                    if (clothingColorGameobjects != null && clothingColorGameobjects.TryGetValue(colorNr, out var _gameObjects))
                        foreach (var _gameObject in _gameObjects)
                            _gameObject.SetActive(useCols[colorNr]);
                    patternChangeAction(colorNr);
                }
                var current = PseudoMaker.selectedCharacterController.GetSelected(selectKindType);
                clothingSailorGameObjects?.ForEach(x => x.SetActive(false));
                clothingJacketGameObjects?.ForEach(x => x.SetActive(false));

                if (SubCategory == SubCategory.ClothingTop && current == 1)
                    clothingSailorGameObjects?.ForEach(x => x.SetActive(true));
                if (SubCategory == SubCategory.ClothingTop && current == 2)
                    clothingJacketGameObjects?.ForEach(x => x.SetActive(true));

                if (PseudoMaker.selectedCharacterController.GetClothingUsesOptPart(kind, 0) || PseudoMaker.selectedCharacterController.GetClothingUsesOptPart(kind, 1))
                    clothingOptionObject?.SetActive(true);
                else clothingOptionObject?.SetActive(false);

                sleeveTypeObject?.SetActive(false);
                sleeveTypeObject?.SetActive(PseudoMaker.selectedCharacterController.GetSleeveTypeCount(kind) > 0);

                MaterialEditorSplitter.SetActive(current != 0);
                MaterialEditorButton.SetActive(current != 0);
                overlaySplitter?.SetActive(current != 0);
                overlayHeader?.gameObject.SetActive(current != 0);
                otherOverlayObjects?.ForEach(o => o.SetActive(current != 0 && overlayHeader.isOn));
                if (SubCategory == SubCategory.ClothingTop && (current == 1 || current == 2))
                {
                    mainOverlayObjects?.ForEach(o => o.SetActive(false));
                    multiOverlayObjects?.ForEach(o => o.SetActive(true && overlayHeader.isOn));
                }
                else
                {
                    mainOverlayObjects?.ForEach(o => o.SetActive(true && overlayHeader.isOn));
                    multiOverlayObjects?.ForEach(o => o.SetActive(false));
                }

                PseudoMaker.RefreshCharacterstatusPanel();
            };

            AddPickerRow(selectKindType, clothingChangeAction);

            if (SubCategory == SubCategory.ClothingTop)
#if KK
                if (KoikatuAPI.IsDarkness())
#endif
                    sleeveTypeObject = AddToggleGroupRow(
                        "Sleeve Type",
                        new string[] { "Type A", "Type B", "Type C" },
                        value => PseudoMaker.selectedCharacterController.SetSleeveType(SelectKindToIntKind(selectKindType), value),
                        () => PseudoMaker.selectedCharacterController.GetSleeveType(SelectKindToIntKind(selectKindType)),
                        () => PseudoMaker.selectedCharacterController.GetSleeveTypeCount(SelectKindToIntKind(selectKindType))
                    ).gameObject;

            clothingOptionObject = AddClothingOption(SubCategory)?.gameObject;

            if (SubCategory == SubCategory.ClothingTop)
            {
                clothingSailorGameObjects = new List<GameObject>()
                {
                    AddPickerRow(SelectKindType.CosSailor01, clothingChangeAction).gameObject,
                    AddPickerRow(SelectKindType.CosSailor02, clothingChangeAction).gameObject,
                    AddPickerRow(SelectKindType.CosSailor03, clothingChangeAction).gameObject,
                };
                clothingJacketGameObjects = new List<GameObject>()
                {
                    AddPickerRow(SelectKindType.CosJacket01, clothingChangeAction).gameObject,
                    AddPickerRow(SelectKindType.CosJacket02, clothingChangeAction).gameObject,
                    AddPickerRow(SelectKindType.CosJacket03, clothingChangeAction).gameObject,
                };
            }

            for (int i = 0; i < 3; i++)
                AddPatternRows(SubCategory, selectKindType, i);

            MaterialEditorSplitter = AddSplitter();
            MaterialEditorButton = AddButtonRow(
                "Material Editor",
                () => MaterialEditor.SetItemType(options => options.FindIndex(o => o.text == $"Clothes {MaterialEditor.ClothesIndexToString(SubCategoryToKind(SubCategory))}"))
            ).gameObject;

            AddOverlayRows();
        }

        private void AddOverlayRows()
        {
            if (!Compatibility.HasClothesOverlayPlugin) return;

            BuildOverlayRows();
            void BuildOverlayRows()
            {
                overlaySplitter = AddSplitter();

                overlayHeader = AddHeaderToggle("Overlays ▶", value => {
                    var current = PseudoMaker.selectedCharacterController.GetSelected(selectKindType);
                    if (SubCategory == SubCategory.ClothingTop && (current == 1 || current == 2))
                    {
                        multiOverlayObjects?.ForEach(o => o.SetActive(value));
                        mainOverlayObjects?.ForEach(o => o.SetActive(false));
                    }
                    else
                    {
                        multiOverlayObjects?.ForEach(o => o.SetActive(false));
                        mainOverlayObjects?.ForEach(o => o.SetActive(value));
                    }
                    otherOverlayObjects.ForEach(o => o.SetActive(value));
                });
                mainOverlayObjects = new List<GameObject>();
                otherOverlayObjects = new List<GameObject>();

                var clothesId = ClothesOverlays.GetClothesId(SubCategory);

                if (ClothesOverlays.HasResizeSupport())
                    mainOverlayObjects.Add(AddResizeDropdown(clothesId));

                mainOverlayObjects.AddRange(AddClothingOverlayRow(clothesId, "Overlay texture", true));

                if (ClothesOverlays.HasColorMaskSupport())
                    AddColorMaskRow("Color mask", clothesId);
                if (ClothesOverlays.HasPatternSupport())
                    AddPatternRows(clothesId);

                if (SubCategory == SubCategory.ClothingTop)
                {
                    multiOverlayObjects = new List<GameObject>();
                    for (int i = 0; i < 3; i++)
                    {
                        var subClothesId = ClothesOverlays.GetClothesId(i, true);
                        multiOverlayObjects.Add(AddResizeDropdown(subClothesId));
                        multiOverlayObjects.AddRange(AddClothingOverlayRow(subClothesId, $"Overlay textures (Piece {i + 1})", true));

                        if (ClothesOverlays.HasColorMaskSupport())
                            multiOverlayObjects.AddRange(AddClothingOverlayRow(subClothesId, $"Color mask (Piece {i + 1})", true, KoiClothesOverlayController.MakeColormaskId(subClothesId)));
                    }
                }


                if (SubCategory == SubCategory.ClothingTop)
                {
                    otherOverlayObjects.AddRange(AddClothingOverlayRow(MaskKind.BodyMask.ToString(), "Body alpha mask", true));
                    otherOverlayObjects.AddRange(AddClothingOverlayRow(MaskKind.InnerMask.ToString(), "Inner clothes alpha mask", true));
                    otherOverlayObjects.AddRange(AddClothingOverlayRow(MaskKind.BraMask.ToString(), "Bra alpha mask", true));
                }

                mainOverlayObjects.ForEach(o => o.SetActive(false));
                multiOverlayObjects?.ForEach(o => o.SetActive(false));
                otherOverlayObjects.ForEach(o => o.SetActive(false));

                GameObject AddResizeDropdown(string _clothesId)
                {
                    var options = new List<string> { "original", "512", "1024", "2048", "4096", "8192" };

                    return AddDropdownRow(
                        "Max Texture Size Override",
                        options,
                        () => options.FindIndex(x => x == ClothesOverlays.GetSizeOverride(_clothesId).ToString()),
                        index => ClothesOverlays.SetSizeOverride(_clothesId, index == 0 ? 0 : (int)(Math.Pow(2f, index - 1) * 512))
                    ).gameObject;
                }

                void AddColorMaskRow(string name, string _clothesId)
                {
                    mainOverlayObjects.AddRange(AddClothingOverlayRow(_clothesId, name, true, KoiClothesOverlayController.MakeColormaskId(_clothesId)));
                }

                void AddPatternRows(string _clothesId)
                {
                    for (int i = 0; i < 3; i++)
                        mainOverlayObjects.AddRange(AddClothingOverlayRow(_clothesId, $"Pattern {i + 1}", true, KoiClothesOverlayController.MakePatternId(_clothesId, i)));
                }
            }
        }

        public List<GameObject> AddClothingOverlayRow(string clothesId, string title, bool addSeperator = false, string colormaskId = null)
        {
            if (!Compatibility.HasClothesOverlayPlugin) return new List<GameObject>();

            return BuildOverlayRows();
            List<GameObject> BuildOverlayRows()
            {
                var objectList = new List<GameObject>();

                var isMask = KoiClothesOverlayController.IsMaskKind(clothesId);
                var texType = isMask ? "override texture" : "overlay texture";
                var isColorMask = colormaskId != null;
                texType = isColorMask ? "override texture" : texType;

                clothesId = !isColorMask ? clothesId : colormaskId;

                if (addSeperator) objectList.Add(AddSplitter());

                objectList.Add(AddHeader(title));

                objectList.Add(
                    AddButtonRow(
                        "Dump Original Texture",
                        () => ClothesOverlays.DumpOriginalTexture(clothesId)
                    ).gameObject
                );
                objectList.Add(
                    AddImageRow(() => ClothesOverlays.GetOverlayTex(clothesId)?._texture).gameObject
                );

                if (!isMask && !isColorMask)
                    objectList.Add(
                        AddToggleRow(
                            "Hide base texture",
                            value => ClothesOverlays.SetTextureOverride(clothesId, value),
                            () => ClothesOverlays.GetOverlayTex(clothesId)?.Override ?? false
                        ).gameObject
                    );

                objectList.Add(
                    AddButtonRow(
                        "Load new " + texType,
                        () => ClothesOverlays.ImportClothesOverlay(clothesId, RefreshPanel)
                    ).gameObject
                );

                objectList.Add(
                    AddButtonRow(
                        "Clear " + texType,
                        () => ClothesOverlays.SetTexAndUpdate(null, clothesId, RefreshPanel)
                    ).gameObject
                );

                objectList.Add(
                    AddButtonRow(
                        "Export " + texType,
                        () => ClothesOverlays.ExportOverlay(clothesId)
                    ).gameObject
                );

                return objectList;
            }
        }


        private string _coordianteNameText;
        private void InitializeSettings()
        {
            AddHeader("Clothing Unlock");
            AddToggleRow(
                "Clothing Unlock",
                ClothingUnlock.ChangeClothingUnlockState,
                ClothingUnlock.GetClothingUnlockState
            );
            AddSplitter();
            AddHeader("More Outfits");
            AddButtonGroupRow(new Dictionary<string, Action> { { "Add Outfit", MoreOutfits.AddOufitSlot }, { "Remove Last Outfit", MoreOutfits.RemoveOutfitSlot } });
            AddTextInputRow("Name:", () => _coordianteNameText, s => _coordianteNameText = s, MoreOutfits.GetCurrentOutfitName());
            AddButtonRow("Rename current outfit", () => MoreOutfits.SetCurrentOutfitName(_coordianteNameText));
        }

        private void InitializePushup()
        {
            KK_Plugins.Pushup.ClothData GetClothData(bool bra)
            {
                if (bra) return PseudoMaker.selectedPushupController.CurrentBraData;
                return PseudoMaker.selectedPushupController.CurrentTopData;
            }

            foreach (var useBra in new bool[] { true, false })
            {
                if (useBra) AddHeaderToggle("Bra ▶", value => pushupBraGameObjects.ForEach(o => o.SetActive(value)));
                else AddHeaderToggle("Top ▶", value => pushupTopGameObjects.ForEach(o => o.SetActive(value)));
                var list = new List<GameObject>() {
                    AddToggleRow(
                        "Enabled",
                        value =>
                        {
                            GetClothData(useBra).EnablePushup = value;
                            PseudoMaker.selectedPushupController.RecalculateBody(false);
                        },
                        () => GetClothData(useBra).EnablePushup
                    ).gameObject,

                    AddSliderRow("Firmness", useBra, PushupValue.Firmness).gameObject,
                    AddSliderRow("Lift", useBra, PushupValue.Lift).gameObject,
                    AddSliderRow("Push Together", useBra, PushupValue.PushTogether).gameObject,
                    AddSliderRow("Squeeze", useBra, PushupValue.Squeeze).gameObject,
                    AddSliderRow("Center Nipples", useBra, PushupValue.CenterNipples).gameObject,

                    AddToggleRow(
                        "Flatten Nipples",
                        value =>
                        {
                            GetClothData(useBra).FlattenNipples = value;
                            PseudoMaker.selectedPushupController.RecalculateBody(false);
                        },
                        () => GetClothData(useBra).FlattenNipples
                    ).gameObject,

                    AddSplitter().gameObject,

                    AddToggleRow(
                        "Advanced Mode",
                        value =>
                        {
                            GetClothData(useBra).UseAdvanced = value;
                            PseudoMaker.selectedPushupController.RecalculateBody(false);
                        },
                        () => GetClothData(useBra).UseAdvanced
                    ).gameObject,

                    AddButtonGroupRow(new Dictionary<string, Action>
                    {
                        { "Copy Body to Advanced", () => {
                            PseudoMaker.selectedCharacterController.CopyPushupData(useBra, PseudoMaker.selectedPushupController.BaseData);
                            RefreshPanel();
                        }},
                        { "Copy Basic to Advanced", () => {
                            PseudoMaker.selectedCharacterController.CopyPushupData(useBra, PseudoMaker.selectedPushupController.CurrentPushupData, true);
                            RefreshPanel();
                        }},
                    }).gameObject,

                    AddSliderRow("Size", useBra, PushupValue.AdvancedSize).gameObject,
                    AddSliderRow("Vertical Position", useBra, PushupValue.AdvancedVerticalPosition).gameObject,
                    AddSliderRow("Vertical Angle", useBra, PushupValue.AdvancedVerticalAngle).gameObject,
                    AddSliderRow("Horizontal Position", useBra, PushupValue.AdvancedHorizontalPosition).gameObject,
                    AddSliderRow("Horizontal Angle", useBra, PushupValue.AdvancedHorizontalAngle).gameObject,
                    AddSliderRow("Depth", useBra, PushupValue.AdvancedDepth).gameObject,
                    AddSliderRow("Roundness", useBra, PushupValue.AdvancedRoundness).gameObject,
                    AddSliderRow("Softness", useBra, PushupValue.AdvancedSoftness).gameObject,
                    AddSliderRow("Weight", useBra, PushupValue.AdvancedWeight).gameObject,
                    AddSliderRow("Areola Depth", useBra, PushupValue.AdvancedAreolaDepth).gameObject,
                    AddSliderRow("Nipple Width", useBra, PushupValue.AdvancedNippleWidth).gameObject,
                    AddSliderRow("Nipple Depth", useBra, PushupValue.AdvancedNippleDepth).gameObject,
                };
                if (useBra)
                {
                    AddSplitter();
                    pushupBraGameObjects = list;
                }
                else pushupTopGameObjects = list;
            }
            pushupBraGameObjects.ForEach(o => o.SetActive(false));
            pushupTopGameObjects.ForEach(o => o.SetActive(false));
        }

        private Dictionary<int, CopyComponent> _copyComponents = new Dictionary<int, CopyComponent>();

        private void InitializeCopy()
        {

            fromDropDown = AddDropdownRow(
                "Source Outfit",
                PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList(),
                () => fromSelected,
                value => {
                    fromSelected = value;
                    _copyComponents.Values.ToList().ForEach(c => c.Refresh());
                }
            );
            toDropDown = AddDropdownRow(
                "Target Outfit",
                PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList(),
                () => toSelected,
                value => {
                    toSelected = value;
                    _copyComponents.Values.ToList().ForEach(c => c.Refresh());
                }
            );

            ChaListDefine.CategoryNo[] cateNo = new[]
            {
                ChaListDefine.CategoryNo.co_top,
                ChaListDefine.CategoryNo.co_bot,
                ChaListDefine.CategoryNo.co_bra,
                ChaListDefine.CategoryNo.co_shorts,
                ChaListDefine.CategoryNo.co_gloves,
                ChaListDefine.CategoryNo.co_panst,
                ChaListDefine.CategoryNo.co_socks,
                ChaListDefine.CategoryNo.co_shoes,
                ChaListDefine.CategoryNo.co_shoes
            };

            for (var i = 0; i < cateNo.Length; i++)
            {
                int cNum = i;
                _copyComponents.Add(cNum, AddCopyRow(UIMappings.GetClothingTypeName(cateNo[cNum]), () =>
                {
                    ChaFileClothes fromClothes = PseudoMaker.selectedCharacter.chaFile.coordinate[fromSelected].clothes;
                    ListInfoBase listInfoFrom = PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[cNum], fromClothes.parts[cNum].id) ?? PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[cNum], DefClothesID()[cNum]);
                    return listInfoFrom.Name;
                }, () =>
                {
                    ChaFileClothes toClothes = PseudoMaker.selectedCharacter.chaFile.coordinate[toSelected].clothes;
                    ListInfoBase listInfoTo = PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[cNum], toClothes.parts[cNum].id) ?? PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[cNum], DefClothesID()[cNum]);
                    return listInfoTo.Name;
                }));
            }

            AddButtonGroupRow(new Dictionary<string, Action>()
            {
                { "Toggle  All", () => _copyComponents.Values.ToList().ForEach(c => c.Toggled = true) },
                { "Toggle  None", () => _copyComponents.Values.ToList().ForEach(c => c.Toggled = false) },
                { "Copy", CopyMethod}
            });
            return;

            void CopyMethod()
            {
                ChaFileClothes toClothes = PseudoMaker.selectedCharacter.chaFile.coordinate[toSelected].clothes;
                ChaFileClothes fromClothes = PseudoMaker.selectedCharacter.chaFile.coordinate[fromSelected].clothes;
                ListInfoBase listInfoTo = PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[0], toClothes.parts[0].id) ??
                                        PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[0], DefClothesID()[0]);
                ListInfoBase listInfoFrom = PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[0], fromClothes.parts[0].id) ??
                                         PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(cateNo[0], DefClothesID()[0]);
                for (var i = 0; i < cateNo.Length; i++)
                {
                    if (!_copyComponents[i].Toggled) continue;

                    byte[] bytes = MessagePackSerializer.Serialize<ChaFileClothes.PartsInfo>(fromClothes.parts[i]);
                    toClothes.parts[i] = MessagePackSerializer.Deserialize<ChaFileClothes.PartsInfo>(bytes);
                    if (i == 0)
                    {
                        if ((1 == listInfoFrom.Kind && 1 == listInfoTo.Kind) || (2 == listInfoFrom.Kind && 2 == listInfoTo.Kind))
                        {
                            for (int j = 0; j < toClothes.subPartsId.Length; j++)
                            {
                                toClothes.subPartsId[j] = fromClothes.subPartsId[j];
                            }
                        }
                        else if (1 == listInfoFrom.Kind || 2 == listInfoFrom.Kind)
                        {
                            for (int k = 0; k < toClothes.subPartsId.Length; k++)
                            {
                                toClothes.subPartsId[k] = fromClothes.subPartsId[k];
                            }
                        }
                        else
                        {
                            for (int l = 0; l < toClothes.subPartsId.Length; l++)
                            {
                                toClothes.subPartsId[l] = 0;
                            }
                        }
                    }
                    else if (2 == i)
                    {
                        toClothes.hideBraOpt[0] = fromClothes.hideBraOpt[0];
                        toClothes.hideBraOpt[1] = fromClothes.hideBraOpt[1];
                    }
                    else if (3 == i)
                    {
                        toClothes.hideShortsOpt[0] = fromClothes.hideShortsOpt[0];
                        toClothes.hideShortsOpt[1] = fromClothes.hideShortsOpt[1];
                    }
                }
                PseudoMaker.selectedCharacter.ChangeCoordinateType(true);
                PseudoMaker.selectedCharacter.Reload(false, true, true, true);
                PseudoMaker.RefreshCharacterstatusPanel();

                // section to call postfixes/events of the vanilla method
                MaterialEditor.ClothingCopiedEvent(fromSelected, toSelected, (from kvp in _copyComponents where kvp.Value.Toggled select kvp.Key).ToList());
                ClothesOverlays.CLothingCopiedEvent(fromSelected, toSelected, (from kvp in _copyComponents where kvp.Value.Toggled select kvp.Key).ToList());
            }

            int[] DefClothesID()
            {
                var defClothesID = new int[9];
#if KKS
                defClothesID[2] = PseudoMaker.selectedCharacter.sex == 0 ? ChaFileDefine.DefClothesMBraID : ChaFileDefine.DefClothesFBraID;
                defClothesID[3] = PseudoMaker.selectedCharacter.sex == 0 ? ChaFileDefine.DefClothesMShortsID : ChaFileDefine.DefClothesFShortsID;
#endif
                return defClothesID;
            }
        }

        public void AddPatternRows(SubCategory subcategory, SelectKindType selectKindType, int colorNr)
        {
            var colorGameObjects = new List<GameObject>()
            {
                AddSplitter(),
                AddColorRow(SubCategory, colorNr).gameObject,
                AddPickerRow((SelectKindType)Enum.Parse(typeof(SelectKindType), $"{selectKindType}Ptn0{colorNr + 1}", true), () => patternChangeAction?.Invoke(colorNr)).gameObject,
            };
            var patternGameObjects = new List<GameObject>()
            {
#if KKS
                AddSliderRow(SubCategory, colorNr, PatternValue.Horizontal).gameObject,
                AddSliderRow(SubCategory, colorNr, PatternValue.Vertical).gameObject,
                AddSliderRow(SubCategory, colorNr, PatternValue.Rotation).gameObject,
#endif
            };

            var width = AddSliderRow(SubCategory, colorNr, PatternValue.Width);
            var height = AddSliderRow(SubCategory, colorNr, PatternValue.Height);
            width.PairedInputs = new SliderComponent[] { height };
            height.PairedInputs = new SliderComponent[] { width };

            patternGameObjects.Add(width.gameObject);
            patternGameObjects.Add(height.gameObject);
            patternGameObjects.Add(AddColorRow(SubCategory, colorNr, true).gameObject);

            colorGameObjects.AddRange(patternGameObjects);
            clothingColorGameobjects[colorNr] = colorGameObjects;
            clothingPatternGameobjects[colorNr] = patternGameObjects;
        }

        public SliderComponent AddSliderRow(SubCategory subCategory, int colorNr, PatternValue pattern)
        {
            int clothingKind = PseudoMakerCharaController.SubCategoryToKind(subCategory);

            return AddSliderRow(
                $"Pattern {colorNr + 1} {pattern}",
                () => PseudoMaker.selectedCharacterController.GetPatternValue(clothingKind, colorNr, pattern),
                () => 0.5f,
                value => PseudoMaker.selectedCharacterController.SetPatternValue(clothingKind, colorNr, pattern, value),
                () => PseudoMaker.selectedCharacterController.SetPatternValue(clothingKind, colorNr, pattern, 0.5f)
            );
        }

        public ColorComponent AddColorRow(SubCategory subCategory, int colorNr, bool isPattern = false)
        {
            int clothingKind = PseudoMakerCharaController.SubCategoryToKind(subCategory);

            return AddColorRow(
                isPattern ? $"Pattern Color {colorNr + 1}" : $"Cloth Color {colorNr + 1}",
                () => PseudoMaker.selectedCharacterController.GetClothingColor(clothingKind, colorNr, isPattern: isPattern),
                () => PseudoMaker.selectedCharacterController.GetOriginalClothingColor(clothingKind, colorNr, isPattern: isPattern),
                c => PseudoMaker.selectedCharacterController.SetClothingColor(clothingKind, colorNr, c, isPattern: isPattern),
                () => PseudoMaker.selectedCharacterController.ResetClothingColor(clothingKind, colorNr, isPattern: isPattern, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed())
            );
        }

        public ClothingOptionComponent AddClothingOption(SubCategory subCategory)
        {
            var kind = PseudoMakerCharaController.SubCategoryToKind(SubCategory);
#if KK
            if (!KoikatuAPI.IsDarkness() && kind != 2 && kind != 3)
                return null;
#endif

            var clothingOption = Instantiate(ClothingOptionTemplate, ClothingOptionTemplate.transform.parent);
            clothingOption.name = "ClothingOptions";


            var clothingOptionComponent = clothingOption.AddComponent<ClothingOptionComponent>();
            clothingOptionComponent.GetCurrentValue = option => !PseudoMaker.selectedCharacterController.GetHideOpt(kind, option);
            clothingOptionComponent.SetValueAction = (option, value) => PseudoMaker.selectedCharacterController.SetHideOpt(kind, option, !value);
            clothingOptionComponent.CheckUsePart = (option) => PseudoMaker.selectedCharacterController.GetClothingUsesOptPart(kind, option);

            return clothingOptionComponent;
        }

        private SliderComponent AddSliderRow(string name, bool bra, PushupValue pushupValue)
        {
            return AddSliderRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetPushupValue(bra, pushupValue),
                () => PseudoMaker.selectedCharacterController.GetOriginalPushupValue(bra, pushupValue),
                value => {
                    PseudoMaker.selectedCharacterController.SetPushupValue(bra, pushupValue, value);
                    PseudoMaker.selectedPushupController.RecalculateBody(false);
                },
                () => {
                    PseudoMaker.selectedCharacterController.ResetPushupValue(bra, pushupValue, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
                    PseudoMaker.selectedPushupController.RecalculateBody(false);
                },
                pushupValue.ToString().StartsWith("Advanced") ? KK_Plugins.Pushup.ConfigSliderMin.Value / 100 : 0,
                pushupValue.ToString().StartsWith("Advanced") ? KK_Plugins.Pushup.ConfigSliderMax.Value / 100 : 1
            );
        }
    }
}
