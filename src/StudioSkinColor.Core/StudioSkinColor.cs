using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KK_Plugins.MaterialEditor;
using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(MaterialEditorPlugin.PluginGUID, MaterialEditorPlugin.PluginVersion)]
    [BepInProcess(Constants.StudioProcessName)]
    public class StudioSkinColor : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.studioSkinColorControl";
        public const string PluginName = "StudioSkinColorControl";
        public const string PluginNameInternal = Constants.Prefix + "_StudioSkinColorControl";
        public const string PluginVersion = "1.1.1";
        internal static new ManualLogSource Logger;

        internal static ChaControl selectedCharacter;
        public static ConfigEntry<KeyboardShortcut> KeyToggleGui { get; private set; }
        public static ConfigEntry<float> WindowWidth { get; private set; }
        public static ConfigEntry<float> WindowHeight { get; private set; }

        private readonly int uiWindowHash = ('S' << 24) | ('S' << 16) | ('C' << 8) | ('C' << 4);
        private Rect uiRect;
        private bool uiShow = false;

        private void Awake()
        {
            Logger = base.Logger;

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
            CharacterApi.RegisterExtraBehaviour<StudioSkinColorCharaController>(PluginGUID);

            WindowWidth.SettingChanged += (e, a) => uiRect = new Rect(20, Screen.height / 2 - 150, WindowWidth.Value, WindowHeight.Value);
            WindowHeight.SettingChanged += (e, a) => uiRect = new Rect(20, Screen.height / 2 - 150, WindowWidth.Value, WindowHeight.Value);

            uiRect = new Rect(20, Screen.height / 2 - 150, WindowWidth.Value, WindowHeight.Value);
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

            if (uiShow)
            {
                uiRect = GUILayout.Window(uiWindowHash, uiRect, ControlGUI.DrawWindow, "Studio Pseudo Maker");
                IMGUIUtils.EatInputInRect(uiRect);
            }
            GUI.skin = skin;
        }

        private void Update()
        {
            var newChar = StudioAPI.GetSelectedCharacters().FirstOrDefault()?.GetChaControl();
            if (newChar != selectedCharacter && newChar != null)
                ControlGUI.ClearBuffers();
            selectedCharacter = newChar;

            if (KeyToggleGui.Value.IsDown() && selectedCharacter != null)
            {
                uiShow = !uiShow;
            }
            if (selectedCharacter == null)
                uiShow = false;
        }

        private void RegisterStudioControls()
        {
            var catBody = StudioAPI.GetOrCreateCurrentStateCategory("Body");
            catBody.AddControl(new CurrentStateColorSlider("Main Skin", c => c.GetChaControl().fileBody.skinMainColor, c => UpdateTextureColor(c, TextureColor.SkinMain)));
            catBody.AddControl(new CurrentStateColorSlider("Sub Skin", c => c.GetChaControl().fileBody.skinSubColor, c => UpdateTextureColor(c, TextureColor.SkinSub)));
            catBody.AddControl(new CurrentStateColorSlider("Tan", c => c.GetChaControl().fileBody.sunburnColor, c => UpdateTextureColor(c, TextureColor.Tan)));

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

        internal static void UpdateTextureColor(Color color, TextureColor textureColor)
        {
            foreach (var cha in StudioAPI.GetSelectedCharacters())
                StudioSkinColorCharaController.GetController(cha.GetChaControl())?.UpdateTextureColor(color, textureColor);
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
        private void OnDestroy()
        {
            StudioSkinColorCharaController.allControllers.Clear();
        }
    }

    internal enum TextureColor
    {
        SkinMain,
        SkinSub,
        Tan,
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
}