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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plugins
{
    internal class ControlGUI
    {
        private static readonly Dictionary<string, int> clothingKinds = new Dictionary<string, int>
        {
            { "Top", 0 },
            { "Bottom", 1 },
            { "Bra", 2 },
            { "Underwear", 3 },
            { "Gloves", 4 },
            { "Pantyhose", 5 },
            { "Legwear", 6 },
#if KK
            { "Shoes", 7 },
            { "Shoes (Outdoors)", 8 }
#elif KKS
            { "Shoes", 8 }
#endif
        };
        private static int selectedKind = 0;

        internal static void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    foreach (var kind in clothingKinds)
                    {
                        var kindNotExists = StudioSkinColor.selectedCharacter.infoClothes[kind.Value].Name == "None";

                        Color c = GUI.color;
                        if (selectedKind == kind.Value)
                            GUI.color = Color.cyan;
                        if (kindNotExists)
                            GUI.enabled = false;
                        if (GUILayout.Button(kind.Key))
                            selectedKind = kind.Value;
                        GUI.color = c;
                        GUI.enabled = true;
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUI.skin.box);
                {
                    var infoClothes = StudioSkinColor.selectedCharacter.infoClothes[selectedKind];
                    var clothingPart = StudioSkinColor.selectedCharacter.nowCoordinate.clothes.parts[selectedKind];
                    var clothesComponent = StudioSkinColor.selectedCharacter.GetCustomClothesComponent(selectedKind);

                    if (StudioSkinColor.selectedCharacter.ctCreateClothes[selectedKind, 0] == null)
                        StudioSkinColor.selectedCharacter.InitBaseCustomTextureClothes(true, selectedKind);

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.Label(infoClothes.Name, new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true,
                        fontStyle = FontStyle.Bold,
                    }, GUILayout.Width(150));
                    GUILayout.EndVertical();

                    if (clothesComponent != null)
                    {
                        if (clothesComponent.useColorN01)
                            DrawColorRow(clothingPart.colorInfo[0], "Color 1:", selectedKind);
                        if (clothesComponent.useColorN02)
                            DrawColorRow(clothingPart.colorInfo[1], "Color 2:", selectedKind);
                        if (clothesComponent.useColorN03)
                            DrawColorRow(clothingPart.colorInfo[2], "Color 3:", selectedKind);

                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private static void DrawColorRow(ChaFileClothes.PartsInfo.ColorInfo colorInfo, string name, int kind)
        {
            GUILayout.Label(name, new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState
                {
                    textColor = Color.white,
                },
                fontStyle = FontStyle.Bold,
            }, GUILayout.ExpandWidth(false));
            GUILayout.Space(2);

            bool colorOpened = GUILayout.Button("", Colorbutton(colorInfo.baseColor));
            if (colorOpened)
            {
                void ChangeColorAction(Color c)
                {
                    if (c != colorInfo.baseColor)
                    {
                        var MEController = MaterialEditorPlugin.GetCharaController(StudioSkinColor.selectedCharacter);
                        if (MEController != null)
                        {
                            MEController.CustomClothesOverride = true;
                            MEController.RefreshClothesMainTex();
                        }
                        colorInfo.baseColor = c;
                        StudioSkinColor.selectedCharacter.ChangeCustomClothes(true, kind, true, true, true, true, true);
                    }
                }
                ColorPicker.OpenColorPicker(colorInfo.baseColor, ChangeColorAction);
            }
            GUILayout.Space(5);

                //if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                //    KKPRimColor = KKPRimColorDefault.Value;
        }

        private static GUIStyle Colorbutton(Color col)
        {
            GUIStyle guistyle = new GUIStyle();
            Texture2D texture2D = new Texture2D(1, 1, (TextureFormat)20, false);
            texture2D.SetPixel(0, 0, col);
            texture2D.Apply();
            guistyle.normal.background = texture2D;
            return guistyle;
        }
    }
}
