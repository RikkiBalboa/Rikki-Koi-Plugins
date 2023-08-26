using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ChaCustom;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.Utilities;
using Studio;
using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;

namespace Plugins
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class SaveCameraObjectFov : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.savecameraobjectfov";
        public const string PluginName = "SaveCameraObjectFov";
        public const string PluginNameInternal = Constants.Prefix + "_SaveCameraObjectFov";
        public const string PluginVersion = "1.0";
        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            Logger = base.Logger;
            _harmony.PatchAll();
        }

        internal static void SetFOV(float fov)
        {
            Studio.Studio.Instance.cameraCtrl.cameraData.parse = fov;
            Studio.Studio.Instance.cameraCtrl.fieldOfView = fov;
        }
    }
}
