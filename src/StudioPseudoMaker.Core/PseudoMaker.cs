using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KK_Plugins.MaterialEditor;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;
using Studio;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInIncompatibility("com.rikkibalboa.bepinex.studioSkinColorControl")]
    [BepInDependency(MaterialEditorPlugin.PluginGUID, MaterialEditorPlugin.PluginVersion)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(KK_Plugins.HairAccessoryCustomizer.GUID, KK_Plugins.HairAccessoryCustomizer.Version)]
    [BepInProcess(Constants.StudioProcessName)]
    public partial class PseudoMaker : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.studioPseudoMaker";
        public const string PluginName = "StudioPseudoMaker";
        public const string PluginNameInternal = Constants.Prefix + "_StudioPseudoMaker";
        public const string PluginVersion = "0.2";
        internal static new ManualLogSource Logger;
        private static Harmony harmony;

        internal static ChaControl selectedCharacter;
        internal static PseudoMakerCharaController selectedCharacterController;
        internal static KK_Plugins.HairAccessoryCustomizer.HairAccessoryController selectedHairAccessoryController;

        public static ConfigEntry<KeyboardShortcut> KeyToggleGui { get; private set; }
        public static ConfigEntry<float> MainWindowWidth { get; private set; }
        public static ConfigEntry<float> MainWindowHeight { get; private set; }
        public static ConfigEntry<float> PickerWindowWidth { get; private set; }
        public static ConfigEntry<float> PickerWindowHeight { get; private set; }
        public static ConfigEntry<float> UIScale { get; set; }
        public static ConfigEntry<int> PickerThumbnailSize { get; set; }

        internal static PseudoMaker instance;
        internal static PseudoMakerUI MainWindow;
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
            UIScale = Config.Bind(
                "UI", "UI Scale",
                1f,
                new ConfigDescription("Controls the size of the window.", new AcceptableValueRange<float>(0.1f, 3f), null, new ConfigurationManagerAttributes { Order = 100 })
            );
            MainWindowWidth = Config.Bind(
                "UI", "Main Window Width",
                600f,
                new ConfigDescription("", new AcceptableValueRange<float>(400f, 2000f), null, new ConfigurationManagerAttributes { Order = 90 })
            );
            MainWindowHeight = Config.Bind(
                "UI", "Main Window Height",
                260f,
                new ConfigDescription("", new AcceptableValueRange<float>(200f, 2000f), null, new ConfigurationManagerAttributes { Order = 80 })
            );
            PickerWindowWidth = Config.Bind(
                "UI", "Picker Window Width",
                430f,
                new ConfigDescription("", new AcceptableValueRange<float>(120f, 2000f), null, new ConfigurationManagerAttributes { Order = 70 })
            );
            PickerWindowHeight = Config.Bind(
                "UI", "Picker Window Height",
                375f,
                new ConfigDescription("", new AcceptableValueRange<float>(200f, 2000f), null, new ConfigurationManagerAttributes { Order = 60 })
            );
            PickerThumbnailSize = Config.Bind(
                "UI", "Picker Thumbnail Size",
                100,
                new ConfigDescription(
                    "Controls the size of the thumbnails in picker windows (Choosing clothes, skin types, etc)",
                    new AcceptableValueRange<int>(30, 200), null, new ConfigurationManagerAttributes { Order = 95 }
                )
            );
            CharacterApi.RegisterExtraBehaviour<PseudoMakerCharaController>(PluginGUID);

            UIMappings.AddOtherPluginMappings();

            UIScale.SettingChanged += (e, a) => SetUIScale();

#if DEBUG
            InitUI("Studio");
            foreach (var item in Studio.Studio.Instance.dicObjectCtrl.Values)
            {
                if (item is OCIChar oCIChar)
                    oCIChar.GetChaControl().GetOrAddComponent<PseudoMakerCharaController>();
            }
#endif
        }

        private void Start()
        {
            if (StudioAPI.InsideStudio)
            {
                RegisterStudioControls();
                SceneManager.sceneLoaded += (s, lsm) => InitUI(s.name);
                TimelineCompatibilityHelper.PopulateTimeline();
            }
        }

        private void Update()
        {
            var newChar = StudioAPI.GetSelectedCharacters().FirstOrDefault()?.GetChaControl();
            if (newChar != selectedCharacter && newChar != null)
            {
                selectedCharacter = newChar;
                selectedCharacterController = PseudoMakerCharaController.GetController(selectedCharacter);
                selectedHairAccessoryController = selectedCharacter.gameObject.GetComponent<KK_Plugins.HairAccessoryCustomizer.HairAccessoryController>();
                MainWindow.RefreshValues();
            }
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
                PseudoMakerCharaController.GetController(cha.GetChaControl())?.UpdateColorProperty(color, textureColor);
        }

        internal static void UpdateBustSoftness(float value, FloatType bust)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                PseudoMakerCharaController.GetController(cha.GetChaControl())?.SetFloatTypeValue(value, bust);
        }

        internal static void UpdateColorProperty(Color color, ColorType hairColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                PseudoMakerCharaController.GetController(cha.GetChaControl())?.UpdateColorProperty(color, hairColor);
        }

        private static void SetUIScale()
        {
            MainWindow.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920 / UIScale.Value, 1080 / UIScale.Value);
        }

        private static void InitUI(string sceneName)
        {
            if (sceneName != "Studio") return;
            SceneManager.sceneLoaded -= (s, lsm) => InitUI(s.name);


            PickerPanel.InitializeCategories();
            MainWindow = PseudoMakerUI.Initialize().AddComponent<PseudoMakerUI>();
            AddStudioButton();
        }

        private static void AddStudioButton()
        {
            RectTransform original = GameObject.Find("StudioScene").transform.Find("Canvas Object List/Image Bar/Button Route").GetComponent<RectTransform>();

            PseudoMakerStudioButton = Instantiate(original.gameObject, original.parent, false).GetComponent<Button>();
            PseudoMakerStudioButton.name = "Button Pseudo Maker";

            RectTransform transform = PseudoMakerStudioButton.transform as RectTransform;
            PseudoMakerStudioButton.transform.SetParent(original.parent, true);
            PseudoMakerStudioButton.transform.localScale = original.localScale;

            transform.anchoredPosition = original.anchoredPosition + new Vector2(-48f * 3 + 4, 44f);

            Texture2D texture2D = new Texture2D(32, 32);
            texture2D.LoadImage(ResourceUtils.GetEmbeddedResource("StudioIcon.png"));
            Image icon = PseudoMakerStudioButton.targetGraphic as Image;
            icon.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 32, 32), new Vector2(40, 40));
            icon.color = Color.white;

            PseudoMakerStudioButton.onClick = new Button.ButtonClickedEvent();
            PseudoMakerStudioButton.onClick.AddListener(() => {
                if (StudioAPI.GetSelectedCharacters().Count() > 0)
                {
                    SetUIScale();
                    MainWindow.gameObject.SetActive(true);
                }
            });
        }

#if DEBUG
        private void OnDestroy()
        {
            if (PseudoMakerStudioButton.gameObject != null) Destroy(PseudoMakerStudioButton.gameObject);
            if (PseudoMakerUI.MainWindow != null) Destroy(PseudoMakerUI.MainWindow.gameObject);
            foreach (var controller in PseudoMakerCharaController.allControllers)
                DestroyImmediate(controller.Value);
            harmony.UnpatchSelf();
        }
#endif
    }

    public enum ColorType
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
        HairOutline,
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

    public enum FloatType
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
        HairFrontLenght,
    }

    public enum PatternValue
    {
        Horizontal,
        Vertical,
        Rotation,
        Width,
        Height,
    }
}