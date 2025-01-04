using BepInEx;
using BepInEx.Logging;
using KKAPI.Studio;


namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess(Constants.StudioProcessName)]
    public class StudioVoiceControl : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.studioVoiceControl";
        public const string PluginName = "StudioVoiceControl";
        public const string PluginNameInternal = Constants.Prefix + "_StudioVoiceControl";
        public const string PluginVersion = "1.1";
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
        }

        private void Start()
        {
            if (StudioAPI.InsideStudio)
                TimelineCompatibilityHelper.PopulateTimeline();
        }

        private void Update()
        {
            TimelineCompatibilityHelper.Update();
        }
    }
}