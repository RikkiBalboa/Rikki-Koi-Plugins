using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System.Collections.Generic;
using UniRx;

namespace ButtEditor
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public partial class ButtEditor : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.buttEditor";
        public const string PluginName = "ButtEditor";
        public const string PluginNameInternal = Plugins.Constants.Prefix + "_ButtEditor";
        public const string PluginVersion = "0.1";
        internal static new ManualLogSource Logger;
        private static Harmony harmony;

        private ButtEditorCharaController buttEditorCharaController => MakerAPI.GetCharacterControl().gameObject.GetComponent<ButtEditorCharaController>();
        
        private void Awake()
        {
            Logger = base.Logger;
            //harmony = Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        private void Start()
        {
            CharacterApi.RegisterExtraBehaviour<ButtEditorCharaController>(PluginGUID);
            MakerAPI.MakerBaseLoaded += OnEarlyMakerFinishedLoading;
        }

        private void OnEarlyMakerFinishedLoading(object sender, RegisterCustomControlsEvent e)
        {
            List<MakerSlider> sliderList = new List<MakerSlider>();
            var category = MakerConstants.GetBuiltInCategory("01_BodyTop", "tglLower");

            for (int i = 0; i < 4; i++)
                sliderList.Add(AddSlider(e, category, (SliderType)i));
        }

        private MakerSlider AddSlider(RegisterCustomControlsEvent e, MakerCategory category, SliderType type)
        {
            var control = e.AddControl(new MakerSlider(category, $"Butt {type}", 0f, 3f, ButtEditorCharaController.defaultValues[type] * 10, this));
            control.ValueChanged.Subscribe(Observer.Create<float>(value => buttEditorCharaController.SetButtValue(type, value / 10)));
            control.Visible.OnNext(true);
            return control;
        }
    }
    public enum SliderType
    {
        Stiffness = 0,
        Elasticity = 1,
        Dampening = 2,
        Weight = 3,
    }
}