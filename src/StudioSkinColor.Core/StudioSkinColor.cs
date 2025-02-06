using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using KK_Plugins.MaterialEditor;
using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("ClothesToAccessories", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MaterialEditorPlugin.PluginGUID, MaterialEditorPlugin.PluginVersion)]
    [BepInProcess(Constants.StudioProcessName)]
    public partial class StudioSkinColor : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.studioSkinColorControl";
        public const string PluginName = "StudioSkinColorControl";
        public const string PluginNameInternal = Constants.Prefix + "_StudioSkinColorControl";
        public const string PluginVersion = "1.1.1";
        internal static new ManualLogSource Logger;
        private static Harmony harmony;

        internal static ChaControl selectedCharacter;
        internal static StudioSkinColorCharaController selectedCharacterController;
        internal static Dictionary<ChaControl, List<CharacterClothing>> selectedCharacterClothing = new Dictionary<ChaControl, List<CharacterClothing>>();
        public static ConfigEntry<KeyboardShortcut> KeyToggleGui { get; private set; }
        public static ConfigEntry<bool> UseWideLayout { get; private set; }
        public static ConfigEntry<float> WindowWidth { get; private set; }
        public static ConfigEntry<float> WindowHeight { get; private set; }

        internal static Dictionary<CustomSelectKind.SelectKindType, CategoryPicker> categoryPickers = new Dictionary<CustomSelectKind.SelectKindType, CategoryPicker>();

        internal static IDictionary c2aAIlnstances = null;
        internal static Type c2aAdapterType = null;
        internal static FieldInfo c2aClothingKindField = null;

        private readonly int uiWindowHash = ('S' << 24) | ('S' << 16) | ('C' << 8) | ('C' << 4);
        private readonly int pickerUiWindowHash = ('S' << 24) | ('S' << 16) | ('C' << 8) | ('C' << 4) | ('P' << 2) | 'I';
        internal Rect uiRect;
        internal static Rect pickerRect;
        internal Action pickerWindowFunc;
        internal string pickerWindowName = "Picker";
        private bool uiShow = false;
        private static StudioSkinColor instance;
        private static PseudoMakerUI MainWindow;
        private static Button PseudoMakerStudioButton;

        private void Awake()
        {
            instance = this;
            Logger = base.Logger;
            harmony = Harmony.CreateAndPatchAll(typeof(Hooks));

            KeyToggleGui = Config.Bind(
                "Keyboard Shortcuts", "Open editor window",
                new KeyboardShortcut(KeyCode.Q, KeyCode.RightControl),
                new ConfigDescription("Open a window to control KKPRim values on selected characters/objects")
            );
            WindowWidth = Config.Bind(
                "UI", "Window Width",
                400f,
                new ConfigDescription("", new AcceptableValueRange<float>(200f, 800f))
            );
            WindowHeight = Config.Bind(
                "UI", "Window Height",
                260f,
                new ConfigDescription("", new AcceptableValueRange<float>(200f, 800f))
            );
            UseWideLayout = Config.Bind(
                "UI", "Use wide layout",
                true,
                new ConfigDescription("Labels are next to sliders/color pickers, instead of above them")
            );
            CharacterApi.RegisterExtraBehaviour<StudioSkinColorCharaController>(PluginGUID);

            uiRect = new Rect(20, Screen.height / 2 - 150, WindowWidth.Value, WindowHeight.Value);
            WindowWidth.SettingChanged += (e, a) => uiRect = new Rect(uiRect.x, uiRect.y, WindowWidth.Value, WindowHeight.Value);
            WindowHeight.SettingChanged += (e, a) => uiRect = new Rect(uiRect.x, uiRect.y, WindowWidth.Value, WindowHeight.Value);

            pickerRect = new Rect(20, Screen.height / 2 - 150, WindowWidth.Value, WindowHeight.Value);

            c2aAdapterType = Type.GetType("KK_Plugins.ClothesToAccessoriesAdapter, KKS_ClothesToAccessories", throwOnError: false);
            if (c2aAdapterType != null)
            {
                var field = c2aAdapterType.GetField("AllInstances", AccessTools.all);
                c2aAIlnstances = field.GetValue(c2aAdapterType) as IDictionary;
                c2aClothingKindField = c2aAdapterType.GetField("_clothingKind", AccessTools.all);
            }

            StudioAPI.StudioLoadedChanged += (sender, e) => PickerPanel.InitializeCategories();
            MainWindow = PseudoMakerUI.Initialize().AddComponent<PseudoMakerUI>();

#if DEBUG
            PickerPanel.InitializeCategories();
            foreach (var item in Studio.Studio.Instance.dicObjectCtrl.Values)
            {
                if (item is OCIChar oCIChar)
                    oCIChar.GetChaControl().GetOrAddComponent<StudioSkinColorCharaController>();
            }
#endif
        }

        private void Start()
        {
            if (StudioAPI.InsideStudio)
            {
                RegisterStudioControls();
                SceneManager.sceneLoaded += (s, lsm) => AddStudioButton(s.name);
#if DEBUG
                AddStudioButton("Studio");
#endif
                TimelineCompatibilityHelper.PopulateTimeline();
            }
        }

        protected void OnGUI()
        {
            //var skin = GUI.skin;
            //GUI.skin = IMGUIUtils.SolidBackgroundGuiSkin;
            //GUI.skin.label.normal.textColor = Color.white;

            //if (uiShow && selectedCharacter != null)
            //{
            //    InitializeStyles();
            //    uiRect = GUILayout.Window(uiWindowHash, uiRect, DrawWindow, "Studio Pseudo Maker");
            //    IMGUIUtils.EatInputInRect(uiRect);

            //    if (pickerWindowFunc != null)
            //    {
            //        pickerRect = GUILayout.Window(pickerUiWindowHash, pickerRect, DrawPickerWindow, pickerWindowName);
            //        IMGUIUtils.EatInputInRect(pickerRect);
            //    }
            //}
            //GUI.skin = skin;
        }

        //private void InitializeCategories()
        //{
        //    CategoryPicker.InitializeCategories();
        //    foreach (var category in Enum.GetValues(typeof(CustomSelectKind.SelectKindType)).Cast<CustomSelectKind.SelectKindType>())
        //    {
        //        var cat = new CategoryPicker(category);
        //        cat.OnActivateAction = () =>
        //        {
        //            if (pickerWindowFunc == null || pickerWindowFunc != cat.DrawWindow)
        //            {
        //                pickerWindowName = cat.name;
        //                pickerWindowFunc = cat.DrawWindow;
        //            }
        //            else
        //                cat.OnCloseAction();
        //        };
        //        cat.OnCloseAction = () => pickerWindowFunc = null;
        //        categoryPickers[category] = cat;
        //    }
        //}

        private void DrawPickerWindow(int id)
        {
            int visibleAreaSize = GUI.skin.window.border.top - 4;
            if (GUI.Button(new Rect(pickerRect.width - visibleAreaSize - 2, 2, visibleAreaSize, visibleAreaSize), "X"))
            {
                pickerWindowFunc = null;
                return;
            }

            pickerWindowFunc();

            pickerRect = IMGUIUtils.DragResizeEatWindow(id, pickerRect);
        }

        private void Update()
        {
            var newChar = StudioAPI.GetSelectedCharacters().FirstOrDefault()?.GetChaControl();
            if (newChar != selectedCharacter && newChar != null)
            {
                ClearBuffers();
                selectedCharacter = newChar;
                selectedCharacterController = StudioSkinColorCharaController.GetController(selectedCharacter);
            }

            if (KeyToggleGui.Value.IsDown())
            {
                MainWindow.gameObject.SetActive(!MainWindow.gameObject.activeSelf);
                uiShow = !uiShow;
            }

            if (uiRect.width != WindowWidth.Value)
                WindowWidth.Value = uiRect.width;
            if (uiRect.height != WindowHeight.Value)
                WindowHeight.Value = uiRect.height;
        }

        private void RegisterStudioControls()
        {
            var catBody = StudioAPI.GetOrCreateCurrentStateCategory("Body");
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => c.GetChaControl().fileBody.skinMainColor, c => UpdateTextureColor(c, ColorType.SkinMain)));
            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => c.GetChaControl().fileBody.skinSubColor, c => UpdateTextureColor(c, ColorType.SkinSub)));
            catBody.AddControl(new CurrentStateColorSlider("Tan", c => c.GetChaControl().fileBody.sunburnColor, c => UpdateTextureColor(c, ColorType.SkinTan)));

            var catBust = StudioAPI.GetOrCreateCurrentStateCategory("Bust");
            catBust.AddControl(new CurrentStateCategorySlider("Softness", c => c.GetChaControl().fileBody.bustSoftness, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, FloatType.Softness));
            catBust.AddControl(new CurrentStateCategorySlider("Weight", c => c.GetChaControl().fileBody.bustWeight, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, FloatType.Weight));

            var catHair = StudioAPI.GetOrCreateCurrentStateCategory("Hair");
            catHair.AddControl(new CurrentStateColorSlider("Color 1", c => c.GetChaControl().fileHair.parts[0].baseColor, color => UpdateColorProperty(color, ColorType.HairBase)));
            catHair.AddControl(new CurrentStateColorSlider("Color 2", c => c.GetChaControl().fileHair.parts[0].startColor, color => UpdateColorProperty(color, ColorType.HairStart)));
            catHair.AddControl(new CurrentStateColorSlider("Color 3", c => c.GetChaControl().fileHair.parts[0].endColor, color => UpdateColorProperty(color, ColorType.HairEnd)));
#if KKS
            catHair.AddControl(new CurrentStateColorSlider("Gloss", c => c.GetChaControl().fileHair.parts[0].glossColor, color => UpdateColorProperty(color, ColorType.HairGloss)));
#endif
            catHair.AddControl(new CurrentStateColorSlider("Eyebrow", c => c.GetChaControl().fileFace.eyebrowColor, color => UpdateColorProperty(color, ColorType.Eyebrow)));
        }

        internal static void UpdateTextureColor(Color color, ColorType textureColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                StudioSkinColorCharaController.GetController(cha.GetChaControl())?.UpdateColorProperty(color, textureColor);
        }

        internal static void UpdateBustSoftness(float value, FloatType bust)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                StudioSkinColorCharaController.GetController(cha.GetChaControl())?.SetFloatTypeValue(value, bust);
        }

        internal static void UpdateColorProperty(Color color, ColorType hairColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                StudioSkinColorCharaController.GetController(cha.GetChaControl())?.UpdateColorProperty(color, hairColor);
        }

        private static void AddStudioButton(string sceneName)
        {
            if (sceneName != "Studio") return;
            SceneManager.sceneLoaded -= (s, lsm) => AddStudioButton(s.name);

            RectTransform original = GameObject.Find("StudioScene").transform.Find("Canvas Object List/Image Bar/Button Route").GetComponent<RectTransform>();

            PseudoMakerStudioButton = Instantiate(original.gameObject, original.parent, false).GetComponent<Button>();
            PseudoMakerStudioButton.name = "Button Pseudo Maker";

            RectTransform transform = PseudoMakerStudioButton.transform as RectTransform;
            PseudoMakerStudioButton.transform.SetParent(original.parent, true);
            PseudoMakerStudioButton.transform.localScale = original.localScale;

            transform.anchoredPosition = original.anchoredPosition + new Vector2(-48f * 3 + 4, 44f);

            Texture2D texture2D = new Texture2D(32, 32);
            texture2D.LoadImage(ResourceUtils.GetEmbeddedResource("StudioIcon.png"));
            Image DBDEIcon = PseudoMakerStudioButton.targetGraphic as Image;
            DBDEIcon.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 32, 32), new Vector2(40, 40));
            DBDEIcon.color = Color.white;

            PseudoMakerStudioButton.onClick = new Button.ButtonClickedEvent();
            PseudoMakerStudioButton.onClick.AddListener(() => { 
                if (StudioAPI.GetSelectedCharacters().Count() > 0)
                    MainWindow.gameObject.SetActive(true);
            });
        }

#if DEBUG
        private void OnDestroy()
        {
            StudioSkinColorCharaController.allControllers.Clear();
            if (PseudoMakerUI.MainWindow != null) Destroy(PseudoMakerUI.MainWindow);
            if (PseudoMakerStudioButton.gameObject != null) Destroy(PseudoMakerStudioButton);
            harmony.UnpatchSelf();
        }
#endif

        internal static object GetC2aAdapter(ChaControl chaControl, int kind, int index)
        {
            var kindArray = c2aAIlnstances[chaControl] as object[];
            var adapterList = kindArray[kind] as IList;
            return adapterList[index];
        }
    }

    internal enum ColorType
    {
        SkinMain,
        SkinSub,
        SkinTan,
        NippleColor,
        NailColor,
        PubicHairColor,
        HairBase,
        HairStart,
        HairEnd,
        HairGloss,
        Eyebrow,
        EyebrowColor,
        EyelineColor,
        ScleraColor1,
        ScleraColor2,
        UpperHighlightColor,
        LowerHightlightColor,
        EyeColor1Left,
        EyeColor2Left,
        EyeColor1Right,
        EyeColor2Right,
        LipLineColor,
        EyeShadowColor,
        CheekColor,
        LipColor,
    }

    internal enum FloatType
    {
        SkinTypeStrenth,
        SkinGloss,
        DisplaySkinDetailLines,
        Softness,
        Weight,
        NippleGloss,
        NailGloss,
        FaceOverlayStrenth,
        CheekGloss,
        UpperHighlightVertical,
        UpperHighlightHorizontal,
        LowerHightlightVertical,
        LowerHightlightHorizontal,
        IrisSpacing,
        IrisVerticalPosition,
        IrisWidth,
        IrisHeight,
        EyeGradientStrenth,
        EyeGradientVertical,
        EyeGradientSize,
        LipGloss,
    }

    internal enum PatternValue
    {
        Horizontal,
        Vertical,
        Rotation,
        Width,
        Height,
    }
}