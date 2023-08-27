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
using System.Collections.Generic;

namespace Plugins
{
    [HarmonyPatch]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class SaveCameraObjectFov : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.savecameraobjectfov";
        public const string PluginName = "SaveCameraObjectFov";
        public const string PluginNameInternal = Constants.Prefix + "_SaveCameraObjectFov";
        public const string PluginVersion = "1.0";
        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony(PluginGUID);


        internal static Dictionary<OCICamera, float> cameras = new Dictionary<OCICamera, float>();
        internal static float mainFov = 23;
        internal static OCICamera previousCamera;
        internal static int previousCameraIndex = 0;
        internal static int cameraIndex = 0;

        private void Awake()
        {
            Logger = base.Logger;
            _harmony.PatchAll();
        }

        private static void SetFOV(float fov)
        {
            Studio.Studio.Instance.cameraCtrl.cameraData.parse = fov;
            Studio.Studio.Instance.cameraCtrl.fieldOfView = fov;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Studio.Studio), "ChangeCamera", new Type[] { typeof(OCICamera), typeof(bool), typeof(bool) })]
        private static void ChangeCameraPostfix(OCICamera _ociCamera, bool _active)
        {
            if (previousCamera != null && cameras.ContainsKey(previousCamera))
                cameras[previousCamera] = Studio.Studio.Instance.cameraCtrl.cameraData.parse;

            SaveCameraObjectFov.Logger.LogInfo($"Switching Camera {cameraIndex}");
            if (cameras.ContainsKey(_ociCamera))
                SaveCameraObjectFov.SetFOV(cameras[_ociCamera]);
            else
                cameras[_ociCamera] = Studio.Studio.Instance.cameraCtrl.cameraData.parse;

            if (cameraIndex == 0)
                SaveCameraObjectFov.SetFOV(mainFov);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Studio.CameraSelector), "OnValueChanged")]
        private static void OnValueChangedPrefix(int _index)
        {
            previousCamera = Studio.Studio.instance.ociCamera;
            previousCameraIndex = cameraIndex;
            cameraIndex = _index;

            if (previousCameraIndex == 0)
                mainFov = Studio.Studio.Instance.cameraCtrl.cameraData.parse;
        }
    }
}
