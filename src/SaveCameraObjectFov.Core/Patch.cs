using HarmonyLib;
using Illusion.Extensions;
using Studio;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Plugins
{
    [HarmonyPatch]
    internal class Patch
    {
        internal static Dictionary<OCICamera, float> cameras = new Dictionary<OCICamera, float>();
        internal static float mainFov = 23;
        internal static OCICamera previousCamera;
        internal static int previousCameraIndex = 0;
        internal static int cameraIndex = 0;

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
