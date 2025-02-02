using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class UI : MonoBehaviour
    {
        public static GameObject MainWindow;
        public static RectTransform MainCanvas;
        public static RectTransform DragPanel;

        public static GameObject CategoryPanel;
        public static GameObject SubCategoryPanel;

        public static GameObject CategoryButtonTemplate;
        public static GameObject SubCategoryButtonTemplate;

        public static void Initialize()
        {
            if (MainWindow != null) return;
            //var data = ResourceUtils.GetEmbeddedResource("pseudo_maker_interface");
            //var ab = AssetBundle.LoadFromMemory(data);
            var ab = AssetBundle.LoadFromFile(Path.Combine(Paths.BepInExRootPath, @"scripts\Assets\pseudo_maker_interface.unity3d"));

            var canvasObj = ab.LoadAsset<GameObject>("StudioPseudoMakerCanvas.prefab");
            if (canvasObj == null) throw new ArgumentException("Could not find QuickAccessBoxCanvas.prefab in loaded AB");

            MainWindow = Instantiate(canvasObj);
            //copy.SetActive(false);

            Destroy(canvasObj);
            ab.Unload(false);

            MainCanvas = (RectTransform)MainWindow.transform.Find("MainCanvas").transform;
            DragPanel = (RectTransform)MainCanvas.transform.Find("DragPanel").transform;
            MovableWindow.MakeObjectDraggable(DragPanel, MainCanvas, false);

            CategoryPanel = MainCanvas.transform.Find("CategoryPanel").gameObject;
            CategoryButtonTemplate = CategoryPanel.transform.Find("ButtonTemplate").gameObject;
            CategoryButtonTemplate.SetActive(false);

            SubCategoryPanel = MainCanvas.transform.Find("SubCategoryPanel").gameObject;
            SubCategoryButtonTemplate = SubCategoryPanel.transform.Find("ButtonTemplate").gameObject; 
            foreach (var category in new string[] { "Body", "Face", "Hair", "Clothing" })
            {
                var go = Instantiate(CategoryButtonTemplate);
                go.SetActive(true);
                go.transform.SetParent(CategoryPanel.transform, false);
                var toggle = go.GetComponent<Toggle>();
                var text = go.GetComponentInChildren<Text>();
                text.text = category;
            }

            foreach (var item in UIMappings.ShapeBodyValueMap)
            {

                var go = Instantiate(SubCategoryButtonTemplate);
                go.SetActive(true);
                go.transform.SetParent(SubCategoryPanel.transform, false);
                var toggle = go.GetComponent<Toggle>();
                var text = go.GetComponentInChildren<Text>();
                text.text = item.Key;
            }
        }
    }
}
