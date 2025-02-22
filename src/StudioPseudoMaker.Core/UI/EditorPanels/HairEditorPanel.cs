using Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using static ChaCustom.CustomSelectKind;

namespace PseudoMaker.UI
{
    public class HairEditorPanel : BaseEditorPanel
    {
        protected override void Initialize()
        {
            base.Initialize();

            if (SubCategory == SubCategory.HairBack)
            {
                AddPickerRow(SelectKindType.HairBack);
                AddToggleRow(
                    "Don't Move",
                    value => PseudoMaker.selectedCharacterController.SetHairNoShake(0, value),
                    () => PseudoMaker.selectedCharacterController.GetHairNoShake(0)
                );
                DrawHairColors();
            }
            else if (SubCategory == SubCategory.HairFront)
            {
                AddPickerRow(SelectKindType.HairFront);
                AddSliderRow("Front Hair Length", FloatType.HairFrontLength);
                AddToggleRow(
                    "Don't Move",
                    value => PseudoMaker.selectedCharacterController.SetHairNoShake(1, value),
                    () => PseudoMaker.selectedCharacterController.GetHairNoShake(1)
                );
                DrawHairColors();
            }
            else if (SubCategory == SubCategory.HairSide)
            {
                AddPickerRow(SelectKindType.HairSide);
                AddToggleRow(
                    "Don't Move",
                    value => PseudoMaker.selectedCharacterController.SetHairNoShake(2, value),
                    () => PseudoMaker.selectedCharacterController.GetHairNoShake(2)
                );
                DrawHairColors();
            }
            else if (SubCategory == SubCategory.HairExtensions)
            {
                AddPickerRow(SelectKindType.HairExtension);
                AddToggleRow(
                    "Don't Move",
                    value => PseudoMaker.selectedCharacterController.SetHairNoShake(3, value),
                    () => PseudoMaker.selectedCharacterController.GetHairNoShake(3)
                );
                DrawHairColors();
            }
            else if (SubCategory == SubCategory.HairMiscellaneous)
                AddPickerRow(SelectKindType.HairGloss);
        }

        private void DrawHairColors()
        {
            AddColorRow("Base Color", ColorType.HairBase);
            AddColorRow("Root Color", ColorType.HairStart);
            AddColorRow("Tip Color", ColorType.HairEnd);
            AddColorRow("Outline Color", ColorType.HairOutline);
#if KKS
            AddColorRow("Highlight Color", ColorType.HairGloss);
#endif
        }
    }
}
