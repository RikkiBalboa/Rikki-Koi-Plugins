using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KK_Plugins.MaterialEditor;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Utilities;
using PseudoMaker.UI;
using Studio;
using System.Linq;
using ChaCustom;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PseudoMaker
{
    internal class PseudoMakerApi
    {
        /// <summary>
        /// Fires after the pseudo maker UI is fully initialized and thus UI elements can be added. Attach your own init code here.
        /// Fired only once.
        /// </summary>
        public static event Action PseudoMakerInitialized;

        /// <summary>
        /// Occurs when the selected accessory in the UI changes.
        /// Send what slotNr is selected and whether the accessory exists (true) or is None (false).
        /// </summary>
        public static event Action<int, bool> SelectedAccessoryChanged;

        public static ChaControl SelectedCharacter => PseudoMaker.selectedCharacter;

        internal static void AnnounceInit()
        {
            PseudoMakerInitialized?.Invoke();
        }

        internal static void ChangeSelectedAccessory(int slotNr, bool exists)
        {
            SelectedAccessoryChanged?.Invoke(slotNr, exists);
        }

        public static IEditorPanelApiInterface GetPanel(SubCategory subCategory)
        {
            return PseudoMaker.MainWindow.CategoryPanels.SelectMany(x => x.Value.SubCategoryPanels).FirstOrDefault(x => x.Value.SubCategory == subCategory).Value;
        }
    }
}
