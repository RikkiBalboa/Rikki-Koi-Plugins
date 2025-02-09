using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    public class FaceEditorPanel : BaseEditorPanel
    {
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
                        () => PseudoMaker.selectedCharacterController.ResetFaceShapeValue(value.Key)
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
                AddSliderRow("Face Overlay Strength", FloatType.FaceOverlayStrenth);
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
                AddColorRow("Lower Highlight Color", ColorType.LowerHightlightColor);
                AddSliderRow("Upper Highlight Vertical", FloatType.UpperHighlightVertical);
                AddSliderRow("Upper Highlight Horizontal", FloatType.UpperHighlightHorizontal);
                AddSliderRow("Lower Highlight Vertical", FloatType.LowerHightlightVertical);
                AddSliderRow("Lower Highlight Horizontaal", FloatType.LowerHightlightHorizontal);
                AddSplitter();
                AddSliderRow("Iris Spacing", FloatType.IrisSpacing);
                AddSliderRow("Iris Vertical Position", FloatType.IrisVerticalPosition);
                AddSliderRow("Iris Width", FloatType.IrisWidth);
                AddSliderRow("Iris Height", FloatType.IrisHeight);
                AddSplitter();
                AddPickerRow(SelectKindType.Pupil);
                AddColorRow("Eye Color 1 (Left)", ColorType.EyeColor1Left);
                AddColorRow("Eye Color 2 (Left)", ColorType.EyeColor2Left);
                AddColorRow("Eye Color 1 (Right)", ColorType.EyeColor1Right);
                AddColorRow("Eye Color 2 (Right)", ColorType.EyeColor2Right);
                AddPickerRow(SelectKindType.PupilGrade);
                AddSliderRow("Eye Gradient Strength", FloatType.EyeGradientStrenth);
                AddSliderRow("Eye Gradient Vertical", FloatType.EyeGradientSize);
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
                AddPickerRow(SelectKindType.Eyebrow);
                AddColorRow("Eye Shadow Color", ColorType.EyeShadowColor);
                AddSplitter();
                AddPickerRow(SelectKindType.Cheek);
                AddColorRow("Eye Shadow Color", ColorType.CheekColor);
                AddSplitter();
                AddPickerRow(SelectKindType.Lip);
                AddColorRow("Lip Color", ColorType.LipColor);
                AddSplitter();
            }
        }
    }
}
