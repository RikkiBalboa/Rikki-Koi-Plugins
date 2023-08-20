using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using KK_Plugins.MaterialEditor;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.Utilities;
using Studio;
using System;
using System.Linq;
using UnityEngine;
using static KK_Plugins.MaterialEditor.MaterialEditorCharaController;
using static MaterialEditorAPI.MaterialAPI;

namespace Plugins
{
    [BepInDependency(MaterialEditorPlugin.PluginGUID, MaterialEditorPlugin.PluginVersion)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ShadowColorSwapper : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.shadowcolorswapper";
        public const string PluginName = "ShadowColorSwapper";
        public const string PluginNameInternal = Constants.Prefix + "_ShadowColorSwapper";
        public const string PluginVersion = "1.2";
        internal static new ManualLogSource Logger;

        private readonly string[] shadowColorNames = new string[3]
        {
            "ShadowColor", "shadowcolor", "shadowColor"
        };

        public static ConfigEntry<KeyboardShortcut> KeySwapShadowColors { get; private set; }
        public static ConfigEntry<KeyboardShortcut> KeySwapShadowColorsAlways { get; private set; }
        public static ConfigEntry<float> ShadowColorRed { get; private set; }
        public static ConfigEntry<float> ShadowColorGreen { get; private set; }
        public static ConfigEntry<float> ShadowColorBlue { get; private set; }
        public static ConfigEntry<float> ShadowColorAlpha { get; private set; }

        private Color shadowColor = Color.white;
        private bool updateAll = false;

        private void Awake()
        {
            Logger = base.Logger;

            KeySwapShadowColors = Config.Bind(
                "Keyboard Shortcuts", "Swap shadow colors",
                new KeyboardShortcut(KeyCode.O, KeyCode.RightControl),
                new ConfigDescription("Swap all shadow colors not changed already to the chosen color")
            );
            KeySwapShadowColorsAlways = Config.Bind(
                "Keyboard Shortcuts", "Swap ALL shadow colors",
                new KeyboardShortcut(KeyCode.I, KeyCode.RightControl),
                new ConfigDescription("Swap ALL shadow colors to the chosen color")
            );

            ShadowColorRed = Config.Bind("Shadow Color", "Red", 0.9f);
            ShadowColorGreen = Config.Bind("Shadow Color", "Green", 0.9f);
            ShadowColorBlue = Config.Bind("Shadow Color", "Blue", 0.9f);
            ShadowColorAlpha = Config.Bind("Shadow Color", "Alpha", 1f);
            shadowColor = new Color(ShadowColorRed.Value, ShadowColorGreen.Value, ShadowColorBlue.Value, ShadowColorAlpha.Value);
        }

        private void Update()
        {
            if (KeySwapShadowColors.Value.IsDown())
            {
                Logger.LogDebug("Key pressed");
                shadowColor = new Color(ShadowColorRed.Value, ShadowColorGreen.Value, ShadowColorBlue.Value, ShadowColorAlpha.Value);
                UpdateShadowColors();
            }
            else if (KeySwapShadowColorsAlways.Value.IsDown())
            {
                Logger.LogDebug("Key pressed");
                shadowColor = new Color(ShadowColorRed.Value, ShadowColorGreen.Value, ShadowColorBlue.Value, ShadowColorAlpha.Value);
                updateAll = true;
                UpdateShadowColors();
                updateAll = false;
            }
        }

        private void UpdateShadowColorsRecursive(
            TreeNodeObject node,
            SceneController sceneController,
            Action<SceneController, int, Material> objUpdateFunc,
            Action<ChaControl> characterUpdateFunc
        )
        {
            if (Studio.Studio.Instance.dicInfo.TryGetValue(node, out ObjectCtrlInfo objectCtrlInfo))
            {
                if (objectCtrlInfo is OCIItem ociItem)
                    foreach (var rend in GetRendererList(ociItem.objectItem))
                        foreach (var mat in GetMaterials(ociItem.objectItem, rend))
                            objUpdateFunc(sceneController, ociItem.objectInfo.dicKey, mat);
                else if (objectCtrlInfo is OCIChar ociChar)
                    characterUpdateFunc(ociChar.GetChaControl());
            }
            foreach (var child in node.child)
                UpdateShadowColorsRecursive(child, sceneController, objUpdateFunc, characterUpdateFunc);
        }

        private void UpdateShadowColors()
        {
            if (StudioAPI.InsideStudio)
            {
                Logger.LogDebug("Updating shadow colors in Studio");
                var objects = StudioAPI.GetSelectedObjects();
                var sceneController = MEStudio.GetSceneController();
                TreeNodeObject[] selectNodes = Singleton<Studio.Studio>.Instance.treeNodeCtrl.selectNodes;
                for (int i = 0; i < selectNodes.Length; i++)
                    UpdateShadowColorsRecursive(selectNodes[i], sceneController, UpdateShadowColorsObjects, UpdateCharaShadowColors);
            }
            else if (MakerAPI.InsideAndLoaded)
            {
                Logger.LogDebug("Updating shadow colors in Maker");
                UpdateCharaShadowColors(MakerAPI.GetCharacterControl());
            }
        }

        private void UpdateCharaShadowColors(ChaControl chaControl)
        {
            var controller = GetController(chaControl);
            for (var i = 0; i < controller.ChaControl.objClothes.Length; i++)
                UpdateShadowColorsClothes(controller, i);
            for (var i = 0; i < controller.ChaControl.objHair.Length; i++)
                UpdateShadowColorsHair(controller, i);
            for (var i = 0; i < controller.ChaControl.GetAccessoryObjects().Length; i++)
                UpdateShadowColorsAccessory(controller, i);
            UpdateShadowColorsBody(controller);
        }

        private void UpdateShadowColorsClothes(MaterialEditorCharaController controller, int slot)
        {
            var go = controller.ChaControl.objClothes[slot];
            foreach (var renderer in GetRendererList(go))
                foreach (var material in GetMaterials(go, renderer))
                    UpdateShadowColorValues(controller, slot, ObjectType.Clothing, material, go);
        }

        private void UpdateShadowColorsHair(MaterialEditorCharaController controller, int slot)
        {
            var go = controller.ChaControl.objHair[slot];
            foreach (var renderer in GetRendererList(go))
                foreach (var material in GetMaterials(go, renderer))
                    UpdateShadowColorValues(controller, slot, ObjectType.Hair, material, go);
        }
        private void UpdateShadowColorsAccessory(MaterialEditorCharaController controller, int slot)
        {
            var go = controller.ChaControl.GetAccessoryObject(slot);
            if (go != null)
                foreach (var renderer in GetRendererList(go))
                    foreach (var material in GetMaterials(go, renderer))
                        UpdateShadowColorValues(controller, slot, ObjectType.Accessory, material, go);
        }

        private void UpdateShadowColorsBody(MaterialEditorCharaController controller)
        {
            foreach (var renderer in GetRendererList(controller.ChaControl.gameObject))
                foreach (var material in GetMaterials(controller.ChaControl.gameObject, renderer))
                    UpdateShadowColorValues(controller, 0, ObjectType.Character, material, controller.ChaControl.gameObject);
        }

        private void UpdateShadowColorsObjects(SceneController controller, int id, Material material)
        {
            foreach (string name in shadowColorNames)
                if (material.HasProperty($"_{name}"))
                    if ((controller.GetMaterialColorPropertyValue(id, material, name) == null && !updateAll) | updateAll)
                     controller.SetMaterialColorProperty(id, material, name, shadowColor);
        }

        private void UpdateShadowColorValues(MaterialEditorCharaController controller, int slot, ObjectType objectType, Material mat, GameObject go)
        {
            foreach (string name in shadowColorNames)
                if (mat.HasProperty($"_{name}") && ((controller.GetMaterialColorPropertyValue(slot, objectType, mat, name, go) == null && !updateAll) | updateAll))
                    controller.SetMaterialColorProperty(slot, objectType, mat, name, shadowColor, go);
        }

        public static MaterialEditorCharaController GetController(ChaControl chaControl)
        {
            if (chaControl == null || chaControl.gameObject == null)
                return null;
            return chaControl.gameObject.GetComponent<MaterialEditorCharaController>();
        }
    }
}
