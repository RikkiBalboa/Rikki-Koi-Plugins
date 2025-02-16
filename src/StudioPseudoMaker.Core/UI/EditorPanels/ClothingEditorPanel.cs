using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    public class ClothingEditorPanel : BaseEditorPanel
    {
        private Action clothingChangeAction;
        private Action<int> patternChangeAction;

        private Dictionary<int, List<GameObject>> clothingColorGameobjects;
        private Dictionary<int, List<GameObject>> clothingPatternGameobjects;
        private List<GameObject> clothingSailorGameObjects;
        private List<GameObject> clothingJacketGameObjects;
        private GameObject clothingOptionObject;

        private void OnEnable()
        {
            if (clothingChangeAction != null) clothingChangeAction();
        }

        protected override void Initialize()
        {
            base.Initialize();

            clothingColorGameobjects = new Dictionary<int, List<GameObject>>();
            clothingPatternGameobjects = new Dictionary<int, List<GameObject>>();

            SelectKindType selectKindType = SelectKindType.CosTop;
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
                AddSliderRow(SubCategory, colorNr, PatternValue.Horizontal).gameObject,
                AddSliderRow(SubCategory, colorNr, PatternValue.Vertical).gameObject,
#if KKS
                AddSliderRow(SubCategory, colorNr, PatternValue.Rotation).gameObject,
                AddSliderRow(SubCategory, colorNr, PatternValue.Width).gameObject,
                AddSliderRow(SubCategory, colorNr, PatternValue.Height).gameObject,
#endif
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
                () => PseudoMaker.selectedCharacterController.ResetClothingColor(clothingKind, colorNr, isPattern: isPattern, getDefault: PseudoMaker.KeyToggleGui.Value.IsPressed())
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
    }
}
