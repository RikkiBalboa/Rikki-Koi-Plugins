using KK_PregnancyPlus;
using KoiSkinOverlayX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoMaker.UI
{
    public class BodyEditorPanel : BaseEditorPanel
    {
        protected override void Initialize()
        {
            base.Initialize();

            if (UIMappings.ShapeBodyValueMap.TryGetValue(SubCategory, out var values))
                foreach (var value in values)
                    AddSliderRow(
                        value.Value,
                        () => PseudoMaker.selectedCharacterController.GetCurrentBodyValue(value.Key),
                        () => PseudoMaker.selectedCharacterController.GetOriginalBodyShapeValue(value.Key),
                        f => PseudoMaker.selectedCharacterController.UpdateBodyShapeValue(value.Key, f),
                        () => PseudoMaker.selectedCharacterController.ResetBodyShapeValue(value.Key),
                        onLabelClick: () => TimelineCompatibilityHelper.SelectedBodyShape = value.Key
                    );

            if (
                UIMappings.ShapeBodyValueMap.Where(x => x.Key == SubCategory).Select(x => x.Value).Count() > 0
                && SubCategory != SubCategory.BodyChest
            )
                AddSplitter();

            if (SubCategory == SubCategory.BodyGeneral)
            {
                AddPickerRow(SelectKindType.BodyDetail);
                AddSliderRow("Skin Type Strength", FloatType.SkinTypeStrength);
                AddSplitter();
                AddColorRow("Main Skin Color", ColorType.SkinMain);
                AddColorRow("Sub Skin Color", ColorType.SkinSub);
                AddSliderRow("Skin Gloss", FloatType.SkinGloss);
                AddSplitter();
                AddColorRow("Nail Color", ColorType.NailColor);
                AddSliderRow("Nail Gloss", FloatType.NailGloss);

                AddSplitter();
                AddButtonRow("Material Editor (body)", () => Compatibility.MaterialEditor.SetItemType("body"));
                AddButtonRow("Material Editor (face)", () => Compatibility.MaterialEditor.SetItemType("face"));
                AddButtonRow("Material Editor (all)", () => Compatibility.MaterialEditor.SetItemType(""));
            }
            else if (SubCategory == SubCategory.BodyChest)
            {
                AddSliderRow("Areola Size", FloatType.AreolaSize);
                AddSliderRow("Breast Weight", FloatType.BustWeight);
                AddSliderRow("Breast Softness", FloatType.BustSoftness);
                AddSplitter();
                AddPickerRow(SelectKindType.Nip);
                AddColorRow("Nipple Color", ColorType.NippleColor);
                AddSliderRow("Nipple Gloss", FloatType.NippleGloss);
            }
            else if (SubCategory == SubCategory.BodyLower)
            {
                if (Compatibility.HasButtPhysicsEditorPlugin)
                    AddButtEditorRows();

                void AddButtEditorRows()
                {
                    foreach (var type in Enum.GetValues(typeof(ButtPhysicsEditor.SliderType)).Cast<ButtPhysicsEditor.SliderType>())
                    {
                        var slider = AddSliderRow($"Butt {type}", (FloatType)Compatibility.ButtPhysicsEditorPlugin.SliderTypeTofloatType(type), minValue: 0f, maxValue: 0.3f);
                        slider.displayTemplate = "0.000";
                    }
                }
            }
            else if (SubCategory == SubCategory.BodyPubicHair)
            {
                AddPickerRow(SelectKindType.Underhair);
                AddColorRow("Pubic Hair Color", ColorType.PubicHairColor);
            }
            else if (SubCategory == SubCategory.BodySuntan)
            {
                AddPickerRow(SelectKindType.Sunburn);
                AddColorRow("Suntan Color", ColorType.SkinTan);
            }
            else if (SubCategory == SubCategory.BodySkinOverlays) {
                if (!Compatibility.HasSkinOverlayPlugin) return;

                AddRows();
                void AddRows()
                {
                    AddToggleRow(
                        "Use Different Overlays Per Outfit",
                        value => Compatibility.SkinOverlays.SetPerCoord(value),
                        ()  => Compatibility.SkinOverlays.IsPerCoord()
                    );

                    AddSkinOverlayRow(TexType.FaceOver, "Face Overlay Texture", onDone: RefreshPanel);
                    AddSkinOverlayRow(TexType.BodyOver, "Body Overlay Texture", onDone: RefreshPanel);
                    AddSkinOverlayRow(TexType.FaceUnder, "Face Underlay Texture", onDone: RefreshPanel);
                    AddSkinOverlayRow(TexType.BodyUnder, "Body Underlay Texture", onDone: RefreshPanel);
                }
            }
            else if  (SubCategory == SubCategory.BodyPregnancyPlus)
            {
                if (!Compatibility.HasPregnancyPlus) return;

                AddRows();
                void AddRows()
                {
                    int selectedPreset = 0;
                    AddDropdownRow(
                        "Apply Preset Shape",
                        BellyTemplate.shapeNames.ToList(),
                        () => selectedPreset,
                        value => {
                            selectedPreset = value;
                            Compatibility.PregnancyPlus.PasteBelly(BellyTemplate.GetTemplate(value));
                            RefreshPanel();
                        }
                    );

                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusInflation, "Pregnancy+");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusMultiplier, "Multiplier");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusRoundness, "Roundness");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusMoveY, "Move Y");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusMoveZ, "Move Z");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusStretchX, "Stretch X");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusStretchY, "Stretch Y");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusShiftY, "Shift Y");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusShiftZ, "Shift Z");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusTaperY, "Taper Y");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusTaperZ, "Taper Z");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusClothOffset, "Cloth Offset");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusFatFold, "Fat Fold");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusFatFoldHeight, "Fat Fold Height");
                    AddSliderRowPregnancyPlus(FloatType.PregnancyPlusFatFoldGap, "Fat Fold Gap");

                    AddButtonGroupRow(new Dictionary<string, Action>
                    {
                        { "Copy Belly",  Compatibility.PregnancyPlus.CopyBelly },
                        { "Paste Belly", () => { Compatibility.PregnancyPlus.PasteBelly(PregnancyPlusPlugin.copiedBelly); RefreshPanel(); } },
                        { "Reset Belly", () => { Compatibility.PregnancyPlus.ResetBelly(); RefreshPanel(); } },
                    });
                    AddButtonGroupRow(new Dictionary<string, Action>
                    {
                        { "Open BlendShapes", Compatibility.PregnancyPlus.OpenBlendshapes },
#if KKS
                        { "Open Individual Offsets", Compatibility.PregnancyPlus.OpenOffsets },
#endif
                    });
                    AddButtonRow("Belly Mesh Smoothing", Compatibility.PregnancyPlus.SmoothBelly);
                    AddToggleRow(
                        "Include Cloth When Smoothing",
                        value => PregnancyPlusGui.includeClothSmoothing = value,
                        () => PregnancyPlusGui.includeClothSmoothing
                    );
                }
            }
        }

        private void AddSliderRowPregnancyPlus(FloatType type, string name)
        {
            AddSliderRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetFloatValue(type),
                () => PseudoMaker.selectedCharacterController.GetOriginalFloatValue(type),
                value => PseudoMaker.selectedCharacterController.SetFloatTypeValue(value, type),
                () => PseudoMaker.selectedCharacterController.ResetFloatTypeValue(type),
                minValue: Compatibility.PregnancyPlus.GetSliderRange(type)[0],
                maxValue: Compatibility.PregnancyPlus.GetSliderRange(type)[1]
            );
        }

        private void RefreshPanel()
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}
