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
#if DEBUG
            ObjectSelectRefresh(Studio.Studio.Instance.dicObjectCtrl.Values);
#endif
        }

        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            if (operation == SceneOperationKind.Clear) ClearProperties();
        }

        internal void ClearProperties()
        {
            if (PseudoMaker.MainWindow != null && (SelectedCharacter || PseudoMaker.MainWindow.gameObject.activeInHierarchy)) 
            {
                PseudoMaker.MainWindow.gameObject.SetActive(false);
            }
                
            SelectedCharacter = null;
            SelectedHairAccessoryController = null;
            SelectedPushupController = null;
        }

        protected override void OnSceneSave()
        {
        }

        protected override void OnObjectsSelected(List<ObjectCtrlInfo> objectCtrlInfo)
        {
            ObjectSelectRefresh(objectCtrlInfo);
        }

        private void ObjectSelectRefresh(IEnumerable<ObjectCtrlInfo> objectCtrlInfo)
        {
            OCIChar ociChar = objectCtrlInfo
                .Where(o => o is OCIChar chara)
                .Select(o => o as OCIChar)
                .FirstOrDefault();
            if (ociChar == null) return;
            SelectedCharacter = ociChar.GetChaControl();
            if (!SelectedCharacter) return;
            SelectedHairAccessoryController = SelectedCharacter.gameObject.GetComponent<HairAccessoryCustomizer.HairAccessoryController>();
            SelectedPushupController = SelectedCharacter.gameObject.GetComponent<Pushup.PushupController>();

            if (!PseudoMakerUI.Instance) return;

            if (PseudoMakerUI.Instance.CategoryPanels.TryGetValue(Category.Clothing, out CategoryPanel clothingPanel) && clothingPanel.SubCategoryPanels.TryGetValue(SubCategory.ClothingCopy, out BaseEditorPanel clothingCopyPanel))
                ((ClothingEditorPanel)clothingCopyPanel)?.RefreshDropdowns();
            if (PseudoMakerUI.Instance.CategoryPanels.TryGetValue(Category.Accessories, out CategoryPanel accessoryPanel) && accessoryPanel.SubCategoryPanels.TryGetValue(SubCategory.AccessoryCopy, out BaseEditorPanel accessoryCopyPanel))
                ((AccessoryCopyPanel)accessoryCopyPanel).RefreshDropdowns();

            PseudoMaker.MainWindow?.RefreshValues();
        }

        protected override void OnObjectDeleted(ObjectCtrlInfo objectCtrlInfo)
        {
            if (SelectedCharacter && objectCtrlInfo is OCIChar ociChar && ociChar.GetChaControl() == SelectedCharacter)
            {
                PseudoMaker.MainWindow.gameObject.SetActive(false);
            }
        }
        
        
    }
}