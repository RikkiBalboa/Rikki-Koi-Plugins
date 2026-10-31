namespace PseudoMaker.UI
{
    public class SaveEditorPanel : BaseEditorPanel
    {
        private string _coordianteNameText;
        
        protected override void Initialize()
        {
            base.Initialize();

            if (SubCategory == SubCategory.SaveCharacter)
            {
                AddHeader("Save Character Card");
                AddButtonRow("Save Character (New card)", CardSaver.SaveCharacter);
            }
            else if (SubCategory == SubCategory.SaveClothes)
            {
                AddHeader("Save Coordinate Card");
                AddTextInputRow("Name:", () => _coordianteNameText, s => _coordianteNameText = s,
                    placeholder: Compatibility.MoreOutfits.GetCurrentOutfitName());
                AddButtonRow("Save Clothes (New card)", () => CardSaver.SaveCoordinate(_coordianteNameText));
            }
        }
    }
}
