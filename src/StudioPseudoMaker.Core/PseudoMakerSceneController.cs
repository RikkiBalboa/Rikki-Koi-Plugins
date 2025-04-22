using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using KK_Plugins;
using KKAPI.Studio;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using PseudoMaker.UI;
using Studio;

namespace PseudoMaker
{
    public class PseudoMakerSceneController : SceneCustomFunctionController
    {
        public static PseudoMakerSceneController Instance;
        public ChaControl SelectedCharacter;
        public HairAccessoryCustomizer.HairAccessoryController SelectedHairAccessoryController;
        public Pushup.PushupController SelectedPushupController;

        private void Awake()
        {
            Instance = this;
        }

        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
        }

        protected override void OnSceneSave()
        {
        }

        protected override void OnObjectsSelected(List<ObjectCtrlInfo> objectCtrlInfo)
        {
            OCIChar ociChar = objectCtrlInfo
                .Where(o => o is OCIChar chara)
                .Select(o => o as OCIChar)
                .FirstOrDefault();
            if (ociChar == null) return;
            SelectedCharacter = ociChar.GetChaControl();
            SelectedHairAccessoryController = SelectedCharacter.gameObject.GetComponent<HairAccessoryCustomizer.HairAccessoryController>();
            SelectedPushupController = SelectedCharacter.gameObject.GetComponent<Pushup.PushupController>();
            
            if (PseudoMakerUI.Instance.CategoryPanels.TryGetValue(Category.Clothing, out CategoryPanel clothingPanel) && clothingPanel.SubCategoryPanels.TryGetValue(SubCategory.ClothingCopy, out BaseEditorPanel clothingCopyPanel))
                ((ClothingEditorPanel)clothingCopyPanel)?.RefreshDropdowns();
            if (PseudoMakerUI.Instance.CategoryPanels.TryGetValue(Category.Accessories, out CategoryPanel accessoryPanel) && accessoryPanel.SubCategoryPanels.TryGetValue(SubCategory.AccessoryCopy, out BaseEditorPanel accessoryCopyPanel))
                ((AccessoryCopyPanel)accessoryCopyPanel).RefreshDropdowns();
            
            PseudoMaker.MainWindow.RefreshValues();
        }

        protected override void OnObjectDeleted(ObjectCtrlInfo objectCtrlInfo)
        {
            if (objectCtrlInfo is OCIChar ociChar && ociChar.GetChaControl() == SelectedCharacter)
            {
                PseudoMaker.MainWindow.gameObject.SetActive(false);
            }
        }
    }
}