using Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using static ChaCustom.CustomSelectKind;

namespace Plugins
{
    public class HairEditorPanel : BaseEditorPanel
    {
        protected override void Initialize()
        {
            base.Initialize();

            if (SubCategory == SubCategory.HairBack)
            {
                AddPickerRow(SelectKindType.HairBack);
                DrawHairColors();
            }
            else if (SubCategory == SubCategory.HairFront)
            {
                AddPickerRow(SelectKindType.HairFront);
                AddSliderRow("Front Hair Length", FloatType.HairFrontLenght);
                DrawHairColors();
            }
            else if (SubCategory == SubCategory.HairSide)
            {
                AddPickerRow(SelectKindType.HairSide);
                DrawHairColors();
            }
            else if (SubCategory == SubCategory.HairExtensions)
            {
                AddPickerRow(SelectKindType.HairExtension);
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
