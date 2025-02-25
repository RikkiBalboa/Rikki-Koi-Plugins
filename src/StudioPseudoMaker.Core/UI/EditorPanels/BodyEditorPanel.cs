using KoiSkinOverlayX;
using System.Linq;
using static ChaCustom.CustomSelectKind;

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
            }
            else if (SubCategory == SubCategory.BodyChest)
            {
                AddSliderRow("Breast Weight", FloatType.BustWeight);
                AddSliderRow("Breast Softness", FloatType.BustSoftness);
                AddSplitter();
                AddPickerRow(SelectKindType.Nip);
                AddColorRow("Nipple Color", ColorType.NippleColor);
                AddSliderRow("Nipple Gloss", FloatType.NippleGloss);
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
                    AddSkinOverlayRow(TexType.FaceOver, "Face Overlay Texture", onDone: RefreshPanel);
                    AddSkinOverlayRow(TexType.BodyOver, "Body Overlay Texture", true, onDone: RefreshPanel);
                    AddSkinOverlayRow(TexType.FaceUnder, "Face Underlay Texture", true, onDone: RefreshPanel);
                    AddSkinOverlayRow(TexType.BodyUnder, "Body Underlay Texture", true, onDone: RefreshPanel);
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
