using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KK_Plugins.MaterialEditor;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Studio;
using KKAPI.Utilities;
using Shared;
using Studio;
using System;
using System.Linq;
using UnityEngine;

namespace Plugins
{
    internal class ControlGUI
    {
        internal static void DrawWindow(int id)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                for(int clothingIndex = 0; clothingIndex < StudioSkinColor.selectedCharacter.nowCoordinate.clothes.parts.Length; clothingIndex++)
                {
                    if (StudioSkinColor.selectedCharacter.ctCreateClothes[clothingIndex, 0] == null)
                        StudioSkinColor.selectedCharacter.InitBaseCustomTextureClothes(true, clothingIndex);

                    var infoClothes = StudioSkinColor.selectedCharacter.infoClothes[clothingIndex];
                    var clothingPart = StudioSkinColor.selectedCharacter.nowCoordinate.clothes.parts[clothingIndex];
                    var clothesComponent = StudioSkinColor.selectedCharacter.GetCustomClothesComponent(clothingIndex);

                    GUILayout.Label(infoClothes.Name, new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState
                        {
                            textColor = Color.white
                        }
                    });

                    if (clothesComponent != null) {
                        if (clothesComponent.useColorN01)
                            DrawColorRow(clothingPart.colorInfo[0], "Color 1", clothingIndex);
                        if (clothesComponent.useColorN02)
                            DrawColorRow(clothingPart.colorInfo[1], "Color 2", clothingIndex);
                        if (clothesComponent.useColorN03)
                            DrawColorRow(clothingPart.colorInfo[2], "Color 3", clothingIndex);

                    }
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private static void DrawColorRow(ChaFileClothes.PartsInfo.ColorInfo colorInfo, string name, int kind)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(name, new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(2, 4, 4, 2)
                });

                bool colorOpened = GUILayout.Button("", Colorbutton(colorInfo.baseColor));
                if (colorOpened)
                {
                    void ChangeColorAction(Color c)
                    {
                        if (c != colorInfo.baseColor)
                        {
                            var MEController = StudioSkinColor.selectedCharacter.GetComponent<MaterialEditorCharaController>();
                            if (MEController != null)
                                MEController.RefreshClothesMainTex();
                            colorInfo.baseColor = c;
                            StudioSkinColor.selectedCharacter.ChangeCustomClothes(true, kind, true, true, true, true, true);
                        }
                    }
                    ColorPicker.OpenColorPicker(colorInfo.baseColor, ChangeColorAction);
                }

                //if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                //    KKPRimColor = KKPRimColorDefault.Value;
            }
            GUILayout.EndHorizontal();
        }

        private static GUIStyle Colorbutton(Color col)
        {
            GUIStyle guistyle = new GUIStyle();
            Texture2D texture2D = new Texture2D(1, 1, (TextureFormat)20, false);
            texture2D.SetPixel(0, 0, col);
            texture2D.Apply();
            guistyle.normal.background = texture2D;
            guistyle.fixedWidth = 50;
            return guistyle;
        }
    }
}
