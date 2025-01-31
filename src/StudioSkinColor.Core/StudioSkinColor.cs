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
        internal static Dictionary<ChaControl, List<CharacterClothing>> selectedCharacterClothing = new Dictionary<ChaControl, List<CharacterClothing>>();
        public static ConfigEntry<KeyboardShortcut> KeyToggleGui { get; private set; }
        public static ConfigEntry<bool> UseWideLayout { get; private set; }
        public static ConfigEntry<float> WindowWidth { get; private set; }
        public static ConfigEntry<float> WindowHeight { get; private set; }

        internal static IDictionary c2aAIlnstances = null;
        internal static Type c2aAdapterType = null;
        internal static FieldInfo c2aClothingKindField = null;

        private readonly int uiWindowHash = ('S' << 24) | ('S' << 16) | ('C' << 8) | ('C' << 4);
        internal Rect uiRect;
        private bool uiShow = false;
        private static StudioSkinColor instance;

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

            c2aAdapterType = Type.GetType("KK_Plugins.ClothesToAccessoriesAdapter, KKS_ClothesToAccessories", throwOnError: false);
            if (c2aAdapterType != null)
            {
                var field = c2aAdapterType.GetField("AllInstances", AccessTools.all);
                c2aAIlnstances = field.GetValue(c2aAdapterType) as IDictionary;
                c2aClothingKindField = c2aAdapterType.GetField("_clothingKind", AccessTools.all);
            }
            InitializeCategories();
            ChangeSelection(CustomSelectKind.SelectKindType.HeadType);

#if DEBUG
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
                TimelineCompatibilityHelper.PopulateTimeline();
            }
        }

        protected void OnGUI()
        {
            var skin = GUI.skin;
            GUI.skin = IMGUIUtils.SolidBackgroundGuiSkin;
            GUI.skin.label.normal.textColor = Color.white;

            if (uiShow && selectedCharacter != null)
            {
                uiRect = GUILayout.Window(uiWindowHash, uiRect, DrawWindow, "Studio Pseudo Maker");
                IMGUIUtils.EatInputInRect(uiRect);
            }
            GUI.skin = skin;
        }

        private void Update()
        {
            var newChar = StudioAPI.GetSelectedCharacters().FirstOrDefault()?.GetChaControl();
            if (newChar != selectedCharacter && newChar != null)
            {
                ClearBuffers();
                selectedCharacter = newChar;
            }

            if (KeyToggleGui.Value.IsDown())
            {
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
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => c.GetChaControl().fileBody.skinMainColor, c => UpdateTextureColor(c, BodyColor.SkinMain)));
            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => c.GetChaControl().fileBody.skinSubColor, c => UpdateTextureColor(c, BodyColor.SkinSub)));
            catBody.AddControl(new CurrentStateColorSlider("Tan", c => c.GetChaControl().fileBody.sunburnColor, c => UpdateTextureColor(c, BodyColor.SkinTan)));

            var catBust = StudioAPI.GetOrCreateCurrentStateCategory("Bust");
            catBust.AddControl(new CurrentStateCategorySlider("Softness", c => c.GetChaControl().fileBody.bustSoftness, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Softness));
            catBust.AddControl(new CurrentStateCategorySlider("Weight", c => c.GetChaControl().fileBody.bustWeight, 0, 1)).Value.Subscribe(f => UpdateBustSoftness(f, Bust.Weight));

            var catHair = StudioAPI.GetOrCreateCurrentStateCategory("Hair");
            catHair.AddControl(new CurrentStateColorSlider("Color 1", c => c.GetChaControl().fileHair.parts[0].baseColor, color => UpdateHairColor(color, HairColor.Base)));
            catHair.AddControl(new CurrentStateColorSlider("Color 2", c => c.GetChaControl().fileHair.parts[0].startColor, color => UpdateHairColor(color, HairColor.Start)));
            catHair.AddControl(new CurrentStateColorSlider("Color 3", c => c.GetChaControl().fileHair.parts[0].endColor, color => UpdateHairColor(color, HairColor.End)));
#if KKS
            catHair.AddControl(new CurrentStateColorSlider("Gloss", c => c.GetChaControl().fileHair.parts[0].glossColor, color => UpdateHairColor(color, HairColor.Gloss)));
#endif
            catHair.AddControl(new CurrentStateColorSlider("Eyebrow", c => c.GetChaControl().fileFace.eyebrowColor, color => UpdateHairColor(color, HairColor.Eyebrow)));
        }

        internal static void UpdateTextureColor(Color color, BodyColor textureColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                StudioSkinColorCharaController.GetController(cha.GetChaControl())?.UpdateBodyColor(color, textureColor);
        }

        internal static void UpdateBustSoftness(float value, Bust bust)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                StudioSkinColorCharaController.GetController(cha.GetChaControl())?.SetBustValue(value, bust);
        }

        internal static void UpdateHairColor(Color color, HairColor hairColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                StudioSkinColorCharaController.GetController(cha.GetChaControl())?.UpdateHairColor(color, hairColor);
        }

#if DEBUG
        private void OnDestroy()
        {
            StudioSkinColorCharaController.allControllers.Clear();
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

    internal enum BodyColor
    {
        SkinMain,
        SkinSub,
        SkinTan,
        NippleColor,
        NailColor,
        PubicHairColor,
    }

    internal enum Bust
    {
        Softness,
        Weight,
    }

    internal enum HairColor
    {
        Base,
        Start,
        End,
        Gloss,
        Eyebrow,
    }

    internal enum FaceColor
    {
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
}