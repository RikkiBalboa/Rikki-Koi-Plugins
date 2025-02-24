using Illusion.Extensions;
using KoiClothesOverlayX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static ChaCustom.CustomSelectKind;
using static HandCtrl;
using static PseudoMaker.PseudoMaker;
using static PseudoMaker.PseudoMakerCharaController;

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
        private List<GameObject> pushupBraGameObjects;
        private List<GameObject> pushupTopGameObjects;
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
            if (clothingChangeAction != null) clothingChangeAction();

            if (fromDropDown != null)
            {
                fromDropDown.SetDropdownOptions(PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList());
                toDropDown.SetDropdownOptions(PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList());
            }

            TimelineCompatibilityHelper.SelectedClothingKind = PseudoMakerCharaController.SubCategoryToKind(SubCategory);
        }

        private void OnDisable()
        {
            TimelineCompatibilityHelper.SelectedClothingKind = null;
        }

        protected override void Initialize()
        {
            base.Initialize();

            if (SubCategory == SubCategory.ClothingPushup) InitializePushup();
            //else if (SubCategory == SubCategory.ClothingCopy) return;
            else InitializeClothing();
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
            };

            AddPickerRow(selectKindType, clothingChangeAction);
            clothingOptionObject = AddClothingOption(SubCategory).gameObject;

            if (SubCategory == SubCategory.ClothingTop)
            {
                clothingSailorGameObjects = new List<GameObject>()
                {
                    AddPickerRow(SelectKindType.CosSailor01).gameObject,
                    AddPickerRow(SelectKindType.CosSailor02).gameObject,
                    AddPickerRow(SelectKindType.CosSailor03).gameObject,
                };
                clothingJacketGameObjects = new List<GameObject>()
                {
                    AddPickerRow(SelectKindType.CosJacket01).gameObject,
                    AddPickerRow(SelectKindType.CosJacket02).gameObject,
                    AddPickerRow(SelectKindType.CosJacket03).gameObject,
                };
            }

            for (int i = 0; i < 3; i++)
                AddPatternRows(SubCategory, selectKindType, i);

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

                var clothesId = Compatibility.OverlayGetClothesId(SubCategory, selectedCharacterController.IsMultiPartTop(PseudoMakerCharaController.SubCategoryToKind(SubCategory)));

                if (Compatibility.OverlayVersionHasResizeSupport())
                {
                    var options = new List<string> { "original", "512", "1024", "2048", "4096", "8192" };
                    otherOverlayObjects.Add(
                        AddDropdownRow(
                            "Max Texture Size Override",
                            options,
                            () => options.FindIndex(x => x == Compatibility.OverlayGetSizeOverride(clothesId).ToString()),
                            index => Compatibility.OverlaySetSizeOverride(clothesId, index == 0 ? 0 : (int)(Math.Pow(2f, index - 1) * 512))
                        ).gameObject
                    );
                }

                mainOverlayObjects.AddRange(AddOverlayRow(clothesId, "Overlay texture", true));

                if (Compatibility.OverlayVersionHasColorMaskSupport())
                    mainOverlayObjects.AddRange(AddOverlayRow(clothesId, "Color mask", true, KoiClothesOverlayController.MakeColormaskId(clothesId)));

                if (SubCategory == SubCategory.ClothingTop)
                {
                    multiOverlayObjects = new List<GameObject>();
                    for (int i = 0; i < 3; i++)
                    {
                        var subClothesId = Compatibility.OverlayGetClothesId(i, true);
                        multiOverlayObjects.AddRange(AddOverlayRow(subClothesId, $"Overlay textures (Piece {i + 1})", true));

                        if (Compatibility.OverlayVersionHasColorMaskSupport())
                            multiOverlayObjects.AddRange(AddOverlayRow(subClothesId, $"Color mask (Piece {i + 1})", true, KoiClothesOverlayController.MakeColormaskId(subClothesId)));
                    }
                }


                if (SubCategory == SubCategory.ClothingTop)
                {
                    otherOverlayObjects.AddRange(AddOverlayRow(MaskKind.BodyMask.ToString(), "Body alpha mask", true));
                    otherOverlayObjects.AddRange(AddOverlayRow(MaskKind.InnerMask.ToString(), "Inner clothes alpha mask", true));
                    otherOverlayObjects.AddRange(AddOverlayRow(MaskKind.BraMask.ToString(), "Bra alpha mask", true));
                }

                mainOverlayObjects.ForEach(o => o.SetActive(false));
                multiOverlayObjects?.ForEach(o => o.SetActive(false));
                otherOverlayObjects.ForEach(o => o.SetActive(false));
            }
        }

        private List<GameObject> AddOverlayRow(string clothesId, string title, bool addSeperator = false, string colormaskId = null)
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
                        () => Compatibility.OverlayDumpOriginalTexture(clothesId)
                    ).gameObject
                );
                objectList.Add(
                    AddImageRow(() => Compatibility.OverlayGetOverlayTex(clothesId)?._texture).gameObject
                );

                if (!isMask && !isColorMask)
                    objectList.Add(
                        AddToggleRow(
                            "Hide base texture",
                            value => Compatibility.OverlaySetTextureOverride(clothesId, value),
                            () => Compatibility.OverlayGetOverlayTex(clothesId)?.Override ?? false
                        ).gameObject
                    );

                objectList.Add(
                    AddButtonRow(
                        "Load new " + texType,
                        () => Compatibility.OverlayImportClothesOverlay(clothesId)
                    ).gameObject
                );

                objectList.Add(
                    AddButtonRow(
                        "Clear " + texType,
                        () => Compatibility.OverlaySetTexAndUpdate(null, clothesId)
                    ).gameObject
                );

                objectList.Add(
                    AddButtonRow(
                        "Export " + texType,
                        () => Compatibility.OverlayExportOverlay(clothesId)
                    ).gameObject
                );

                return objectList;
            }
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

        private void InitializeCopy()
        {
            fromDropDown = AddDropdownRow(
                "Clothing Source",
                PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList(),
                () => fromSelected,
                value => { 
                    fromSelected = value;
                }
            );
            toDropDown = AddDropdownRow(
                "Clothing Destination",
                PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList(),
                () => toSelected,
                value => {
                    toSelected = value;
                }
            );

            //AddCopyRow(0, true,
            //    () => PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_top, PseudoMaker.selectedCharacter.chaFile.coordinate[fromSelected].clothes.parts[0].id),
            //    () => PseudoMaker.selectedCharacter.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.co_top, PseudoMaker.selectedCharacter.chaFile.coordinate[fromSelected].clothes.parts[0].id)
            //);
            //AddCopyRow(1, true);
            //AddCopyRow(2, true);
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
                AddSliderRow(SubCategory, colorNr, PatternValue.Width).gameObject,
                AddSliderRow(SubCategory, colorNr, PatternValue.Height).gameObject,
                AddColorRow(SubCategory, colorNr, true).gameObject,
            };
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
            var clothingOption = Instantiate(ClothingOptionTemplate, ClothingOptionTemplate.transform.parent);
            clothingOption.name = "ClothingOptions";

            var kind = PseudoMakerCharaController.SubCategoryToKind(SubCategory);

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

        private void RefreshPanel()
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}
