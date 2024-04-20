using BepInEx;
using BepInEx.Logging;
using Studio;
using HarmonyLib;
using KKAPI.Studio;
using UnityEngine;
using KKAPI.Studio.SaveLoad;
using BepInEx.Configuration;

namespace Plugins
{
    [HarmonyPatch]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class ProjectorUtils : BaseUnityPlugin
    {
        public const string PluginGUID = "com.rikkibalboa.bepinex.projectorutils";
        public const string PluginName = "ProjectorUtils";
        public const string PluginNameInternal = Constants.Prefix + "_ProjectorUtils";
        public const string PluginVersion = "1.0";
        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new Harmony(PluginGUID);

        public static ConfigEntry<Color> LineColor { get; private set; }

        private void Awake()
        {
            Logger = base.Logger;
            _harmony.PatchAll();
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneController>(PluginGUID);
            LineColor = Config.Bind("Settings", "Line Color", new Color(1,1,1,0.75f));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DrawLightLine), nameof(DrawLightLine.OnPostRender))]
        private static void OnPostRenderPostFix()
        {
            foreach (var objectCtrlInfo in StudioAPI.GetSelectedObjects())
                if (objectCtrlInfo is OCIItem item)
                {
                    var projector = item.objectItem.GetComponent<Projector>();
                    if (projector != null)
                        DrawProjectionWireframe(item.objectItem.transform.rotation, item.objectItem.transform.position, projector.orthographic ? projector.orthographicSize : projector.fieldOfView, projector.nearClipPlane, projector.farClipPlane, projector.aspectRatio, projector.orthographic);
                }
        }

        private static void DrawProjectionWireframe(Quaternion rotation, Vector3 position, float angle, float nearClipPlane, float farClipPlane, float aspectRatio, bool orthographic)
        {
            Material material = LightLine.material;

            float startRange, endRange;
            if (orthographic)
            {
                startRange = angle;
                endRange = angle;
            }
            else
            {
                startRange = nearClipPlane * Mathf.Tan(Mathf.PI / 180f * angle / 2f);
                endRange = farClipPlane * Mathf.Tan(Mathf.PI / 180f * angle / 2f);
            }

            Vector3 forward = rotation * Vector3.forward;
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;

            Vector3 startTopRight, startBottomRight, startTopLeft, startBottomLeft, endTopRight, endBottomRight, endTopLeft, endBottomLeft;
            startTopRight = position + forward * nearClipPlane + (up + right * aspectRatio) * startRange;
            startBottomRight = position + forward * nearClipPlane + (-up + right * aspectRatio) * startRange;
            startTopLeft = position + forward * nearClipPlane + (up - right * aspectRatio) * startRange;
            startBottomLeft = position + forward * nearClipPlane + (-up - right * aspectRatio) * startRange;

            endTopRight = position + forward * farClipPlane + (up + right * aspectRatio) * endRange;
            endBottomRight = position + forward * farClipPlane + (-up + right * aspectRatio) * endRange;
            endTopLeft = position + forward * farClipPlane + (up - right * aspectRatio) * endRange;
            endBottomLeft = position + forward * farClipPlane + (-up - right * aspectRatio) * endRange;

            //Draw near clip plane
            DrawLine(startTopRight, startBottomRight);
            DrawLine(startBottomRight, startBottomLeft);
            DrawLine(startBottomLeft, startTopLeft);
            DrawLine(startTopLeft, startTopRight);

            //Draw far clip plane
            DrawLine(endTopRight, endBottomRight);
            DrawLine(endBottomRight, endBottomLeft);
            DrawLine(endBottomLeft, endTopLeft);
            DrawLine(endTopLeft, endTopRight);

            //Draw connection between near and far clip planes
            DrawLine(startTopRight, endTopRight);
            DrawLine(startBottomRight, endBottomRight);
            DrawLine(startTopLeft, endTopLeft);
            DrawLine(startBottomLeft, endBottomLeft);

            void DrawLine(Vector3 p1, Vector3 p2, Color? color = null)
            {
                if (BeginLineDrawing(Matrix4x4.identity, color))
                {
                    GL.Vertex(p1);
                    GL.Vertex(p2);
                    EndLineDrawing();
                }
            }

            bool BeginLineDrawing(Matrix4x4 matrix, Color? color = null)
            {
                if (color == null)
                    color = LineColor.Value;

                if (material == null)
                {
                    return false;
                }
                material.SetPass(0);
                material.SetColor("_Color", (Color)color);
                GL.PushMatrix();
                GL.MultMatrix(matrix);
                GL.Begin(1);
                return true;
            }

            void EndLineDrawing()
            {
                GL.End();
                GL.PopMatrix();
            }
        }
    }
}
