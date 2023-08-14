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
    public class KKPRimController : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.kkprimcontroller";
        public const string PluginName = "KKPRim Controller";
        public const string PluginNameInternal = Constants.Prefix + "_KKPRimController";
        public const string PluginVersion = "1.6";
        internal static new ManualLogSource Logger;
        private Studio.Studio studio;

        public static ConfigEntry<KeyboardShortcut> KeyToggleGui { get; private set; }
        public static ConfigEntry<float> KKPRimAsDiffuseDefault { get; private set; }
        public static ConfigEntry<float> KKPRimIntensityDefault { get; private set; }
        public static ConfigEntry<float> KKPRimRotateXDefault { get; private set; }
        public static ConfigEntry<float> KKPRimRotateYDefault { get; private set; }
        public static ConfigEntry<float> KKPRimSoftDefault { get; private set; }
        public static ConfigEntry<float> UseKKPRimDefault { get; private set; }
        public static ConfigEntry<Color> KKPRimColorDefault { get; private set; }
        public static ConfigEntry<float> KKPRimSoftHairMaxValue { get; private set; }


        private readonly int uiWindowHash = ('K' << 24) | ('K' << 16) | ('P' << 8) | ('R' << 4) | ('i' << 4) | 'm';
        private Rect uiRect = new Rect(20, Screen.height / 2 - 150, 160, 223);
        private bool uiShow = false;

        private float KKPRimAsDiffuse = 0;
        private float KKPRimIntensity = 0;
        private float KKPRimRotateX = 0;
        private float KKPRimRotateY = 0;
        private float KKPRimSoft = 0;
        private float UseKKPRim = 0;
        private Color KKPRimColor = Color.white;

        private bool updateBody = true;
        private bool updateHair = true;
        private bool updateClothes = true;
        private bool updateAccessories = true;
        private bool updateObjects = true;


        private void InitSettings()
        {
            KeyToggleGui = Config.Bind(
                "Keyboard Shortcuts", "Open settings window",
                new KeyboardShortcut(KeyCode.K, KeyCode.RightControl),
                new ConfigDescription("Open a window to control KKPRim values on selected characters/objects")
            );

            KKPRimAsDiffuseDefault = Config.Bind("Default Values", "Diffuse", 0f);
            KKPRimIntensityDefault = Config.Bind("Default Values", "Intensity", 0.75f);
            KKPRimRotateXDefault = Config.Bind("Default Values", "Rotate X", 0f);
            KKPRimRotateYDefault = Config.Bind("Default Values", "Rotate Y", 0f);
            KKPRimSoftDefault = Config.Bind("Default Values", "Soft", 1.5f);
            UseKKPRimDefault = Config.Bind("Default Values", "Use", 0f);
            KKPRimColorDefault = Config.Bind("Default Values", "Color", Color.white);
            KKPRimSoftHairMaxValue= Config.Bind("Misc.", "Max soft value for hair", 20f, new ConfigDescription("Hair shaders react differently to higher soft values. This limits the max value to be used on hair shaders"));
        }

        private void Awake()
        {
            Logger = base.Logger;

            InitSettings();
            KKAPI.Studio.StudioAPI.StudioLoadedChanged += OnSceneLoaded;

            KKPRimAsDiffuse = KKPRimAsDiffuseDefault.Value;
            KKPRimAsDiffuseBuffer = KKPRimAsDiffuseDefault.Value.ToString();
            KKPRimIntensity = KKPRimIntensityDefault.Value;
            KKPRimIntensityBuffer = KKPRimIntensityDefault.Value.ToString();
            KKPRimRotateX = KKPRimRotateXDefault.Value;
            KKPRimRotateXBuffer = KKPRimRotateXDefault.Value.ToString();
            KKPRimRotateY = KKPRimRotateYDefault.Value;
            KKPRimRotateYBuffer = KKPRimRotateYDefault.Value.ToString();
            KKPRimSoft = KKPRimSoftDefault.Value;
            KKPRimSoftBuffer = KKPRimSoftDefault.Value.ToString();
            UseKKPRim = UseKKPRimDefault.Value;
            UseKKPRimBuffer = UseKKPRimDefault.Value.ToString();
            KKPRimColor = KKPRimColorDefault.Value;
        }

        private void OnSceneLoaded(object sender, EventArgs e)
        {
            bool isStudio = KoikatuAPI.GetCurrentGameMode() == GameMode.Studio;
            if (isStudio)
            {
                studio = Singleton<Studio.Studio>.Instance;
            }
        }

        private void Update()
        {
            if (KeyToggleGui.Value.IsDown() && StudioAPI.InsideStudio)
            {
                uiShow = !uiShow;
            }
        }

        private void UpdateKKPRimRecursive(
            TreeNodeObject node,
            SceneController sceneController,
            Action<SceneController, int, Material> objUpdateFunc,
            Action<OCIChar> characterUpdateFunc
        )
        {
            if (Studio.Studio.Instance.dicInfo.TryGetValue(node, out ObjectCtrlInfo objectCtrlInfo))
            {
                if (objectCtrlInfo is OCIItem ociItem)
                    foreach (var rend in GetRendererList(ociItem.objectItem))
                        foreach (var mat in GetMaterials(ociItem.objectItem, rend))
                            objUpdateFunc(sceneController, ociItem.objectInfo.dicKey, mat);
                else if (objectCtrlInfo is OCIChar ociChar)
                    characterUpdateFunc(ociChar);
            }
            foreach (var child in node.child)
                UpdateKKPRimRecursive(child, sceneController, objUpdateFunc, characterUpdateFunc);
        }

        private void UpdateKKPRim()
        {
            if (StudioAPI.InsideStudio)
            {
                var sceneController = MEStudio.GetSceneController();
                TreeNodeObject[] selectNodes = Singleton<Studio.Studio>.Instance.treeNodeCtrl.selectNodes;
                for (int i = 0; i < selectNodes.Length; i++)
                {
                    UpdateKKPRimRecursive(selectNodes[i], sceneController, UpdateKKPRimObjects, UpdateKKPRimCharacter);
                }
            }
        }

        private void LoadKKPRimValues()
        {
            if (StudioAPI.InsideStudio)
            {
                var ociChar = StudioAPI.GetSelectedCharacters().FirstOrDefault();
                OCIItem ociItem = StudioAPI.GetSelectedObjects().OfType<OCIItem>().FirstOrDefault();

                void update()
                {
                    KKPRimAsDiffuseBuffer = KKPRimAsDiffuse.ToString();
                    KKPRimIntensityBuffer = KKPRimIntensity.ToString();
                    KKPRimRotateXBuffer = KKPRimRotateX.ToString();
                    KKPRimRotateYBuffer = KKPRimRotateY.ToString();
                    KKPRimSoftBuffer = KKPRimSoft.ToString();
                    UseKKPRimBuffer = UseKKPRim.ToString();
                }

                if (ociChar != null)
                {
                    var controller = GetController(ociChar.GetChaControl());
                    var renderer = GetRendererList(controller.ChaControl.gameObject).First();
                    var material = GetMaterials(controller.ChaControl.gameObject, renderer).First();

                    KKPRimAsDiffuse = controller.GetMaterialFloatPropertyValue(0, ObjectType.Character, material, "KKPRimAsDiffuse", controller.ChaControl.gameObject) ?? KKPRimAsDiffuseDefault.Value;
                    KKPRimIntensity = controller.GetMaterialFloatPropertyValue(0, ObjectType.Character, material, "KKPRimIntensity", controller.ChaControl.gameObject) ?? KKPRimIntensityDefault.Value;
                    KKPRimRotateX = controller.GetMaterialFloatPropertyValue(0, ObjectType.Character, material, "KKPRimRotateX", controller.ChaControl.gameObject) ?? KKPRimRotateXDefault.Value;
                    KKPRimRotateY = controller.GetMaterialFloatPropertyValue(0, ObjectType.Character, material, "KKPRimRotateY", controller.ChaControl.gameObject) ?? KKPRimRotateYDefault.Value;
                    KKPRimSoft = controller.GetMaterialFloatPropertyValue(0, ObjectType.Character, material, "KKPRimSoft", controller.ChaControl.gameObject) ?? KKPRimSoftDefault.Value;
                    UseKKPRim = controller.GetMaterialFloatPropertyValue(0, ObjectType.Character, material, "UseKKPRim", controller.ChaControl.gameObject) ?? UseKKPRimDefault.Value;
                    KKPRimColor = controller.GetMaterialColorPropertyValue(0, ObjectType.Character, material, "KKPRimColor", controller.ChaControl.gameObject) ?? KKPRimColorDefault.Value;
                    update();
                }
                else if (ociItem != null)
                {
                    var controller = MEStudio.GetSceneController();
                    var renderer = GetRendererList(ociItem.objectItem).First();
                    var material = GetMaterials(ociItem.objectItem, renderer).First();

                    int objectId = MEStudio.GetObjectID(ociItem);
                    KKPRimAsDiffuse = controller.GetMaterialFloatPropertyValue(objectId, material, "KKPRimAsDiffuse") ?? KKPRimAsDiffuseDefault.Value;
                    KKPRimIntensity = controller.GetMaterialFloatPropertyValue(objectId, material, "KKPRimIntensity") ?? KKPRimIntensityDefault.Value;
                    KKPRimRotateX = controller.GetMaterialFloatPropertyValue(objectId, material, "KKPRimRotateX") ?? KKPRimRotateXDefault.Value;
                    KKPRimRotateY = controller.GetMaterialFloatPropertyValue(objectId, material, "KKPRimRotateY") ?? KKPRimRotateYDefault.Value;
                    KKPRimSoft = controller.GetMaterialFloatPropertyValue(objectId, material, "KKPRimSoft") ?? KKPRimSoftDefault.Value;
                    UseKKPRim = controller.GetMaterialFloatPropertyValue(objectId, material, "UseKKPRim") ?? UseKKPRimDefault.Value;
                    KKPRimColor = controller.GetMaterialColorPropertyValue(objectId, material, "KKPRimColor") ?? KKPRimColorDefault.Value;
                    update();
                }
            }
        }

        private void ResetKKPRimValues()
        {
            if (StudioAPI.InsideStudio)
            {

                KKPRimAsDiffuse = KKPRimAsDiffuseDefault.Value;
                KKPRimIntensity = KKPRimIntensityDefault.Value;
                KKPRimRotateX = KKPRimRotateXDefault.Value;
                KKPRimRotateY = KKPRimRotateYDefault.Value;
                KKPRimSoft = KKPRimSoftDefault.Value;
                UseKKPRim = UseKKPRimDefault.Value;
                KKPRimColor = KKPRimColorDefault.Value;

                KKPRimAsDiffuseBuffer = KKPRimAsDiffuse.ToString();
                KKPRimIntensityBuffer = KKPRimIntensity.ToString();
                KKPRimRotateXBuffer = KKPRimRotateX.ToString();
                KKPRimRotateYBuffer = KKPRimRotateY.ToString();
                KKPRimSoftBuffer = KKPRimSoft.ToString();
                UseKKPRimBuffer = UseKKPRim.ToString();

                UpdateKKPRim();
            }
        }

        private void SaveAsDefaults()
        {
            KKPRimAsDiffuseDefault.Value = KKPRimAsDiffuse;
            KKPRimIntensityDefault.Value = KKPRimIntensity;
            KKPRimRotateXDefault.Value = KKPRimRotateX;
            KKPRimRotateYDefault.Value = KKPRimRotateY;
            KKPRimSoftDefault.Value = KKPRimSoft;
            UseKKPRimDefault.Value = UseKKPRim;
            KKPRimColorDefault.Value = KKPRimColor;
        }

        private void UpdateKKPRimValues(MaterialEditorCharaController controller, int slot, ObjectType objectType, Material mat, GameObject go)
        {
            if (mat.HasProperty("_UseKKPRim"))
            {
                controller.SetMaterialFloatProperty(slot, objectType, mat, "KKPRimAsDiffuse", KKPRimAsDiffuse, go);
                controller.SetMaterialFloatProperty(slot, objectType, mat, "KKPRimIntensity", KKPRimIntensity, go);
                controller.SetMaterialFloatProperty(slot, objectType, mat, "KKPRimRotateX", KKPRimRotateX, go);
                controller.SetMaterialFloatProperty(slot, objectType, mat, "KKPRimRotateY", KKPRimRotateY, go);
                controller.SetMaterialFloatProperty(
                    slot,
                    objectType,
                    mat,
                    "KKPRimSoft",
                    (mat.shader.name.ToLower().Contains("hair") && KKPRimSoft > KKPRimSoftHairMaxValue.Value) ? KKPRimSoftHairMaxValue.Value : KKPRimSoft,
                    go
                );
                controller.SetMaterialFloatProperty(slot, objectType, mat, "UseKKPRim", UseKKPRim, go);
                controller.SetMaterialColorProperty(slot, objectType, mat, "KKPRimColor", KKPRimColor, go);
            }
        }

        private void UpdateKKPRimClothes(MaterialEditorCharaController controller, int slot)
        {
            var go = controller.ChaControl.objClothes[slot];
            foreach (var renderer in GetRendererList(go))
                foreach (var material in GetMaterials(go, renderer))
                    UpdateKKPRimValues(controller, slot, ObjectType.Clothing, material, go);
        }

        private void UpdateKKPRimHair(MaterialEditorCharaController controller, int slot)
        {
            var go = controller.ChaControl.objHair[slot];
            foreach (var renderer in GetRendererList(go))
                foreach (var material in GetMaterials(go, renderer))
                    UpdateKKPRimValues(controller, slot, ObjectType.Hair, material, go);
        }
        private void UpdateKKPRimAccessory(MaterialEditorCharaController controller, int slot)
        {
            var go = controller.ChaControl.GetAccessoryObject(slot);
            if (go != null)
                foreach (var renderer in GetRendererList(go))
                    foreach (var material in GetMaterials(go, renderer))
                        if ((updateAccessories && !material.shader.name.ToLower().Contains("hair")) || updateHair && material.shader.name.ToLower().Contains("hair"))
                            UpdateKKPRimValues(controller, slot, ObjectType.Accessory, material, go);
        }

        private void UpdateKKPRimBody(MaterialEditorCharaController controller)
        {
            foreach (var renderer in GetRendererList(controller.ChaControl.gameObject))
                foreach (var material in GetMaterials(controller.ChaControl.gameObject, renderer))
                    UpdateKKPRimValues(controller, 0, ObjectType.Character, material, controller.ChaControl.gameObject);
        }

        private void UpdateKKPRimCharacter(OCIChar ociChar)
        {
            var controller = GetController(ociChar.GetChaControl());
            if (updateClothes)
                for (var i = 0; i < controller.ChaControl.objClothes.Length; i++)
                    UpdateKKPRimClothes(controller, i);
            if (updateHair)
                for (var i = 0; i < controller.ChaControl.objHair.Length; i++)
                    UpdateKKPRimHair(controller, i);
            if (updateAccessories || updateHair)
                for (var i = 0; i < controller.ChaControl.GetAccessoryObjects().Length; i++)
                    UpdateKKPRimAccessory(controller, i);
            if (updateBody)
                UpdateKKPRimBody(controller);
        }

        private void UpdateKKPRimObjects(SceneController controller, int id, Material material)
        {
            if (updateObjects)
                if (material.HasProperty("_UseKKPRim"))
                {
                    controller.SetMaterialFloatProperty(id, material, "KKPRimAsDiffuse", KKPRimAsDiffuse);
                    controller.SetMaterialFloatProperty(id, material, "KKPRimIntensity", KKPRimIntensity);
                    controller.SetMaterialFloatProperty(id, material, "KKPRimRotateX", KKPRimRotateX);
                    controller.SetMaterialFloatProperty(id, material, "KKPRimRotateY", KKPRimRotateY);
                    controller.SetMaterialFloatProperty(id, material, "KKPRimSoft", KKPRimSoft);
                    controller.SetMaterialFloatProperty(id, material, "UseKKPRim", UseKKPRim);
                    controller.SetMaterialColorProperty(id, material, "KKPRimColor", KKPRimColor);
                }
        }

        public static MaterialEditorCharaController GetController(ChaControl chaControl)
        {
            if (chaControl == null || chaControl.gameObject == null)
                return null;
            return chaControl.gameObject.GetComponent<MaterialEditorCharaController>();
        }

        #region UI
        private string KKPRimAsDiffuseBuffer = "";
        private string KKPRimIntensityBuffer = "";
        private string KKPRimRotateXBuffer = "";
        private string KKPRimRotateYBuffer = "";
        private string KKPRimSoftBuffer = "";
        private string UseKKPRimBuffer = "";

        protected void OnGUI()
        {
            if (uiShow)
            {
                IMGUIUtils.DrawSolidBox(uiRect);
                uiRect = GUILayout.Window(uiWindowHash, uiRect, DrawWindow, "Global KKPRim settings");
                IMGUIUtils.EatInputInRect(uiRect);
            }
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                DrawSlider(ref KKPRimAsDiffuse, 0f, 1f, KKPRimAsDiffuseDefault.Value, ref KKPRimAsDiffuseBuffer, "As diffuse");
                DrawSlider(ref KKPRimIntensity, 0f, 10f, KKPRimIntensityDefault.Value, ref KKPRimIntensityBuffer, "Intensity");
                DrawSlider(ref KKPRimRotateX, -2f, 2f, KKPRimRotateXDefault.Value, ref KKPRimRotateXBuffer, "Rotate X");
                DrawSlider(ref KKPRimRotateY, -2f, 2f, KKPRimRotateYDefault.Value, ref KKPRimRotateYBuffer, "Rotate Y");
                DrawSlider(ref KKPRimSoft, 0, 10f, KKPRimSoftDefault.Value, ref KKPRimSoftBuffer, "Soft");
                DrawSlider(ref UseKKPRim, 0, 1f, UseKKPRimDefault.Value, ref UseKKPRimBuffer, "Use KKPRim");

                GUILayout.Label("Color", new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = new GUIStyleState
                    {
                        textColor = Color.white
                    }
                });

                GUILayout.BeginHorizontal();
                {
                    bool flag35 = GUILayout.Button("", Colorbutton(KKPRimColor), GUILayout.ExpandWidth(true)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker);
                    if (flag35)
                    {
                        void act3(Color c)
                        {
                            if (c != KKPRimColor)
                            {
                                KKPRimColor = c;
                                UpdateKKPRim();
                            }
                        }
                        ColorPicker(KKPRimColor, act3);
                    }

                    if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                        KKPRimColor = KKPRimColorDefault.Value;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();


            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("Apply to what?", new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = new GUIStyleState
                    {
                        textColor = Color.white
                    }
                });

                GUILayout.BeginHorizontal();
                {
                    updateBody = GUILayout.Toggle(updateBody, "Body");
                    updateHair = GUILayout.Toggle(updateHair, "Hair");
                    updateClothes = GUILayout.Toggle(updateClothes, "Clothes");
                    updateAccessories = GUILayout.Toggle(updateAccessories, "Accessories");
                    updateObjects = GUILayout.Toggle(updateObjects, "Objects");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button("Set Values"))
                        UpdateKKPRim();
                    if (GUILayout.Button("Reset All"))
                        ResetKKPRimValues();
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button("Load Values"))
                        LoadKKPRimValues();
                    if (GUILayout.Button("Save As Defaults"))
                        SaveAsDefaults();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void DrawSlider(ref float value, float min, float max, float defaultValue, ref string buffer, string label)
        {
            GUILayout.Label(label, new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });

            GUILayout.BeginHorizontal();
            {
                float newValue = value;
                float sliderBuffer = GUILayout.HorizontalSlider(value, min, max, GUILayout.MinWidth(300));


                var focused = GUI.GetNameOfFocusedControl();
                if (focused == label && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
                    GUI.FocusControl(null);

                GUI.SetNextControlName(label);
                buffer = GUILayout.TextField(buffer.ToString(), GUILayout.Width(50));

                if (focused != label)
                {
                    if (!float.TryParse(buffer, out float x))
                        x = value;
                    float valueBufferFloat = x;
                    if (valueBufferFloat != value)
                        newValue = valueBufferFloat;
                    else if (sliderBuffer != value)
                    {
                        newValue = sliderBuffer;
                        buffer = newValue.ToString();
                    }
                }

                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                {
                    newValue = defaultValue;
                    buffer = defaultValue.ToString();
                }

                if (value != newValue)
                {
                    value = newValue;
                    UpdateKKPRim();
                }
            }
            GUILayout.EndHorizontal();
        }

        private GUIStyle Colorbutton(Color col)
        {
            GUIStyle guistyle = new GUIStyle();
            Texture2D texture2D = new Texture2D(1, 1, (TextureFormat)20, false);
            texture2D.SetPixel(0, 0, col);
            texture2D.Apply();
            guistyle.normal.background = texture2D;
            return guistyle;
        }

        public void ColorPicker(Color col, Action<Color> act)
        {
            bool flag = KoikatuAPI.GetCurrentGameMode() == GameMode.Studio;
            if (flag)
            {
                bool visible = this.studio.colorPalette.visible;
                if (visible)
                {
                    this.studio.colorPalette.visible = false;
                }
                else
                {
                    this.studio.colorPalette.Setup("ColorPicker", col, act, true);
                    this.studio.colorPalette.visible = true;
                }
            }
            bool flag2 = KoikatuAPI.GetCurrentGameMode() == GameMode.Maker;
            if (flag2)
            {
                CvsColor component = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsColor/Top").GetComponent<CvsColor>();
                bool isOpen = component.isOpen;
                if (isOpen)
                {
                    component.Close();
                }
                else
                {
                    component.Setup("ColorPicker", 0, col, act, true);
                }
            }
        }
        #endregion UI
    }
}
