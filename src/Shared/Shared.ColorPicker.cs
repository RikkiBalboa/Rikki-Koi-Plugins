# if KK || KKS
using ChaCustom;
using HarmonyLib;
using KKAPI;
using Studio;
using System;
using UnityEngine;

namespace Shared
{
    internal class ColorPicker
    {
        public static void OpenColorPicker(Color col, Action<Color> act)
        {
            var studio = Singleton<global::Studio.Studio>.Instance;

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                var setup = AccessTools.Method(typeof(ColorPalette), nameof(ColorPalette.Setup));
                if (studio.colorPalette.visible)
                {
                    studio.colorPalette.visible = false;
                }
                else
                {
                    setup.Invoke(studio.colorPalette, new object[] { "ColorPicker", col, act, true });
                    studio.colorPalette.visible = true;
                }
            }
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                CvsColor component = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsColor/Top").GetComponent<CvsColor>();
                var setup = AccessTools.Method(typeof(CvsColor), nameof(CvsColor.Setup));
                if (component.isOpen)
                {
                    component.Close();
                }
                else
                {
                    if (setup.GetParameters().Length == 5)
                        setup.Invoke(component, new object[] { "ColorPicker", CvsColor.ConnectColorKind.None, col, act, true });
                    else
                        setup.Invoke(component, new object[] { "ColorPicker", CvsColor.ConnectColorKind.None, col, act, null, true });
                }
            }
        }
    }
}
#endif