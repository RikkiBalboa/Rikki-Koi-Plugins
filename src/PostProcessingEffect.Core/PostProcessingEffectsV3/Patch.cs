using HarmonyLib;
using UnityEngine;

namespace PostProcessingEffectsV3
{
    public class Patch
    {
        [HarmonyPatch(typeof(ChaControl), "LoadCharaFbxData")]
        [HarmonyPostfix]
        private static void Postfix(ChaControl __instance)
        {
            Renderer[] componentsInChildren = __instance.GetComponentsInChildren<Renderer>();
            Renderer[] array = componentsInChildren;
            foreach (Renderer renderer in array)
            {
                if (renderer.sharedMaterial.shader.name.Contains("hair") && renderer.sharedMaterial.GetTag("RenderType", false) == "Transparent")
                {
                    renderer.sharedMaterial.SetOverrideTag("RenderType", "TransparentCutout");
                }
            }
        }
    }
}
