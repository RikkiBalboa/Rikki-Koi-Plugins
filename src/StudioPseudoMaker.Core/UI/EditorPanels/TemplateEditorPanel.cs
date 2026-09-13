using HarmonyLib;
using Illusion.Extensions;
using KKAPI.Maker;
using KoiSkinOverlayX;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class TemplateEditorPanel : BaseEditorPanel
    {
        internal static GameObject EditorTemplate { get; private set; }

        internal static GameObject SliderTemplate { get; private set; }
        internal static GameObject InputTemplate { get; private set; }
        internal static GameObject TextInputTemplate { get; private set; }
        internal static GameObject ButtonGroupTemplate { get; private set; }
        internal static GameObject ColorTemplate { get; private set; }
        internal static GameObject PickerTemplate { get; private set; }
        internal static GameObject DropdownTemplate { get; private set; }
        internal static GameObject ClothingOptionTemplate { get; private set; }
        internal static GameObject ToggleOptionTemplate { get; private set; }
        internal static GameObject ToggleGroupTemplate { get; private set; }
        internal static GameObject SplitterTemplate { get; private set; }
        internal static GameObject HeaderTemplate { get; private set; }
        internal static GameObject TransferRowTemplate { get; private set; }
        internal static GameObject CopyRowTemplate { get; private set; }
        internal static GameObject ImageRowTemplate { get; private set; }

        protected override void Initialize()
        {
            scrollRect = GetComponentInChildren<ScrollRect>();

            SliderTemplate = scrollRect.content.Find("SliderTemplate").gameObject;
            InputTemplate = scrollRect.content.Find("InputFieldTemplate").gameObject;
            TextInputTemplate = scrollRect.content.Find("TextInputFieldTemplate").gameObject;
            ButtonGroupTemplate = scrollRect.content.Find("ButtonGroupTemplate").gameObject;
            ColorTemplate = scrollRect.content.Find("ColorTemplate").gameObject;
            PickerTemplate = scrollRect.content.Find("PickerTemplate").gameObject;
            DropdownTemplate = scrollRect.content.Find("DropdownTemplate").gameObject;
            ClothingOptionTemplate = scrollRect.content.Find("ClothingOptionTemplate").gameObject;
            ToggleOptionTemplate = scrollRect.content.Find("ToggleOptionTemplate").gameObject;
            ToggleGroupTemplate = scrollRect.content.Find("ToggleGroupTemplate").gameObject;
            SplitterTemplate = scrollRect.content.Find("SplitterTemplate").gameObject;
            HeaderTemplate = scrollRect.content.Find("HeaderTemplate").gameObject;
            TransferRowTemplate = scrollRect.content.Find("AccessoryTransferRowRemplate").gameObject;
            CopyRowTemplate = scrollRect.content.Find("CopyRowTemplate").gameObject;
            ImageRowTemplate = scrollRect.content.Find("ImageRowTemplate").gameObject;
        }

        /// <summary>
        /// Fully initializes the template. The passed GameObject will be used as the template UI template.
        /// After which it will create a template panel and destroy the passed template. 
        /// <para></para>
        /// This created template panel will act as a singleton. Any subsequent calls will just destroy the passed GameObject.
        /// </summary>
        /// <param name="go">Reference to the template game object</param>
        internal static void InitializeTemplate(GameObject go)
        {
            if (EditorTemplate == null)
            {
                EditorTemplate = go;
                CreatePanel<TemplateEditorPanel>(SubCategory.Template, go.transform.parent);
                EditorTemplate.SetActive(false);
            }
            else
                Destroy(go);
        }
    }
}
