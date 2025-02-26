using KoiSkinOverlayX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PseudoMaker.UI
{
    public class FaceEditorPanel : BaseEditorPanel
    {
        private int eyesToEdit = 0;
        private Dictionary<int, List<GameObject>> lists = new Dictionary<int, List<GameObject>>();

        protected override void Initialize()
        {
            base.Initialize();

            if (UIMappings.ShapeFaceValueMap.TryGetValue(SubCategory, out var values))
                foreach (var value in values)
                    AddSliderRow(
                        value.Value,
                        () => PseudoMaker.selectedCharacterController.GetCurrentFaceValue(value.Key),
                        () => PseudoMaker.selectedCharacterController.GetOriginalFaceShapeValue(value.Key),
                        f => PseudoMaker.selectedCharacterController.UpdateFaceShapeValue(value.Key, f),
                        () => PseudoMaker.selectedCharacterController.ResetFaceShapeValue(value.Key),
                        onLabelClick: () => TimelineCompatibilityHelper.SelectedFaceShape = value.Key
                    );

            if (
                UIMappings.ShapeFaceValueMap.Where(x => x.Key == SubCategory).Select(x => x.Value).Count() > 0
                && SubCategory != SubCategory.BodyChest
            )
                AddSplitter();

            if (SubCategory == SubCategory.FaceGeneral)
            {
                AddPickerRow(SelectKindType.HeadType);
                AddPickerRow(SelectKindType.FaceDetail);
                AddSliderRow("Face Overlay Strength", FloatType.FaceOverlayStrength);
                AddSplitter();
            }
            else if (SubCategory == SubCategory.FaceCheeks)
            {
                AddSliderRow("Cheek Gloss", FloatType.CheekGloss);
            }
            else if (SubCategory == SubCategory.FaceEyebrows)
            {
                AddPickerRow(SelectKindType.Eyebrow);
                AddColorRow("Eyebrow Color", ColorType.EyebrowColor);
            }
            else if (SubCategory == SubCategory.FaceEyes)
            {
                AddPickerRow(SelectKindType.EyelineUp);
                AddPickerRow(SelectKindType.EyelineDown);
                AddColorRow("Eyeliner Color", ColorType.EyelineColor);
            }
            else if (SubCategory == SubCategory.FaceIris)
            {
                AddPickerRow(SelectKindType.EyeWGrade);
                AddColorRow("Sclera Color 1", ColorType.ScleraColor1);
                AddColorRow("Sclera Color 2", ColorType.ScleraColor2);
                AddSplitter();
                AddPickerRow(SelectKindType.EyeHLUp);
                AddColorRow("Upper Highlight Color", ColorType.UpperHighlightColor);
                AddPickerRow(SelectKindType.EyeHLDown);
                AddColorRow("Lower Highlight Color", ColorType.LowerHighlightColor);
                AddSliderRow("Upper Highlight Vertical", FloatType.UpperHighlightVertical);
                AddSliderRow("Upper Highlight Horizontal", FloatType.UpperHighlightHorizontal);
                AddSliderRow("Lower Highlight Vertical", FloatType.LowerHighlightVertical);
                AddSliderRow("Lower Highlight Horizontaal", FloatType.LowerHighlightHorizontal);
                AddSplitter();
                AddSliderRow("Iris Spacing", FloatType.IrisSpacing);
                AddSliderRow("Iris Vertical Position", FloatType.IrisVerticalPosition);
                AddSliderRow("Iris Width", FloatType.IrisWidth);
                AddSliderRow("Iris Height", FloatType.IrisHeight);
                AddSplitter();

                AddDropdownRow(
                    "Eye To Edit",
                    new List<string> { "Both Eyes", "Left Eye", "Right Eye" },
                    () => eyesToEdit,
                    idx => { 
                        eyesToEdit = idx;
                        foreach (var kvp in lists)
                        {
                            if (kvp.Key != idx) kvp.Value.ForEach(o => o.SetActive(false));
                            else kvp.Value.ForEach(o => o.SetActive(true));
                        }
                        RefreshPanel();
                    });

                lists[0] = new List<GameObject>
                {
                    AddPickerRow(SelectKindType.Pupil).gameObject,
                    AddColorRow("Eye Color 1", ColorType.EyeColor1).gameObject,
                    AddColorRow("Eye Color 2", ColorType.EyeColor2).gameObject,
                    AddPickerRow(SelectKindType.PupilGrade).gameObject,
                    AddSliderRow("Eye Gradient Strength", FloatType.EyeGradientStrength).gameObject,
                    AddSliderRow("Eye Gradient Vertical", FloatType.EyeGradientVertical).gameObject,
                    AddSliderRow("Eye Gradient Size", FloatType.EyeGradientSize).gameObject,
                };
                lists[1] = new List<GameObject>
                {
                    AddPickerRow(SelectKindType.PupilLeft).gameObject,
                    AddColorRow("Eye Color 1", ColorType.EyeColor1Left).gameObject,
                    AddColorRow("Eye Color 2", ColorType.EyeColor2Left).gameObject,
                    AddPickerRow(SelectKindType.PupilGradeLeft).gameObject,
                    AddSliderRow("Eye Gradient Strength", FloatType.EyeGradientStrengthLeft).gameObject,
                    AddSliderRow("Eye Gradient Vertical", FloatType.EyeGradientVerticalLeft).gameObject,
                    AddSliderRow("Eye Gradient Size", FloatType.EyeGradientSizeLeft).gameObject,
                };
                lists[2] = new List<GameObject>
                {
                    AddPickerRow(SelectKindType.PupilRight).gameObject,
                    AddColorRow("Eye Color 1", ColorType.EyeColor1Right).gameObject,
                    AddColorRow("Eye Color 2", ColorType.EyeColor2Right).gameObject,
                    AddPickerRow(SelectKindType.PupilGradeRight).gameObject,
                    AddSliderRow("Eye Gradient Strength", FloatType.EyeGradientStrengthRight).gameObject,
                    AddSliderRow("Eye Gradient Vertical", FloatType.EyeGradientVerticalRight).gameObject,
                    AddSliderRow("Eye Gradient Size", FloatType.EyeGradientSizeRight).gameObject,
                };

                AddButtonGroupRow(new Dictionary<string, Action>
                {
                    { "Copy Left To Right", () => { PseudoMaker.selectedCharacterController.CopyPupil(0, 1); RefreshPanel();  } },
                    { "Copy Right To Left", () => { PseudoMaker.selectedCharacterController.CopyPupil(1, 0); RefreshPanel();  } }
                });

                foreach (var kvp in lists)
                    if (kvp.Key != 0)
                        kvp.Value.ForEach(o => o.SetActive(false));
            }
            else if (SubCategory == SubCategory.FaceNose)
            {
                AddPickerRow(SelectKindType.Nose);
            }
            else if (SubCategory == SubCategory.FaceMouth)
            {
                AddPickerRow(SelectKindType.Lipline);
                AddColorRow("Lip Line Color", ColorType.LipLineColor);
                AddSplitter();
                AddSliderRow("Lip Gloss", FloatType.LipGloss);
            }
            else if (SubCategory == SubCategory.FaceMakeup)
            {
                AddPickerRow(SelectKindType.Eyeshadow);
                AddColorRow("Eye Shadow Color", ColorType.EyeShadowColor);
                AddSplitter();
                AddPickerRow(SelectKindType.Cheek);
                AddColorRow("Cheek Color", ColorType.CheekColor);
                AddSplitter();
                AddPickerRow(SelectKindType.Lip);
                AddColorRow("Lip Color", ColorType.LipColor);
                AddSplitter();
            }
            else if (SubCategory == SubCategory.FaceEyeOverlays)
            {
                if (!Compatibility.HasSkinOverlayPlugin) return;

                AddRows();
                void AddRows()
                {
                    AddToggleRow(
                        "Use Different Overlays Per Outfit",
                        value => Compatibility.SkinOverlays.SetPerCoord(value),
                        () => Compatibility.SkinOverlays.IsPerCoord()
                    );

                    AddDropdownRow(
                        "Eyes To Edit",
                        new List<string> { "Both", "Left", "Right" },
                        () => eyesToEdit,
                        idx =>
                        {
                            eyesToEdit = idx;
                            RefreshPanel();
                        }
                    );

                    AddSkinOverlayRow(TexType.EyeOver, "Iris Overlay Texture", onDone: RefreshPanel, getEyeToEdit: () => eyesToEdit);
                    AddSkinOverlayRow(TexType.EyeUnder, "Iris Underlay Texture", onDone: RefreshPanel, getEyeToEdit: () => eyesToEdit);
                    AddSkinOverlayRow(TexType.EyelineUnder, "Eyelashes Override Texture", onDone: RefreshPanel);
                    AddSkinOverlayRow(TexType.EyebrowUnder, "Eyebrows Override Texture", onDone: RefreshPanel);
                }
            }
        }

        private void RefreshPanel()
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}
