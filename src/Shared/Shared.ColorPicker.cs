# if KK || KKS
using ChaCustom;
using HarmonyLib;
using KKAPI;
using System;
using UnityEngine;

namespace Shared
{
    internal class ColorPicker
    {
        public static void OpenColorPicker(Color col, Action<Color> act, string name = null)
        {
            name = name == null ? "ColorPicker" : name;

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                var studio = Singleton<global::Studio.Studio>.Instance;
                if (studio.colorPalette.visible && name != null && studio.colorPalette.Check(name))
                {
                    studio.colorPalette.visible = false;
                    return;
                }
                studio.colorPalette.Setup(name, col, act, true);
                studio.colorPalette.visible = true;
            }
            else if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
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
                        setup.Invoke(component, new object[] { name, CvsColor.ConnectColorKind.None, col, act, true });
                    else
                        setup.Invoke(component, new object[] { name, CvsColor.ConnectColorKind.None, col, act, null, true });
                }
            }
        }
    }
}
#endif