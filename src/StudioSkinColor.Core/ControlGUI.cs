using Shared;
using System.Collections.Generic;
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

        private static StudioSkinColorCharaController controller => StudioSkinColorCharaController.GetController(selectedCharacter);
        private static ChaControl selectedCharacter => StudioSkinColor.selectedCharacter;
        private static ListInfoBase infoClothes => selectedCharacter.infoClothes[selectedKind];
        private static ChaClothesComponent clothesComponent => selectedCharacter.GetCustomClothesComponent(selectedKind);

        internal static void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    foreach (var kind in clothingKinds)
                    {
                        Color c = GUI.color;
                        if (selectedKind == kind.Value)
                            GUI.color = Color.cyan;
                        if (controller.ClothingKindExists(kind.Value))
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
                    controller.InitBaseCustomTextureClothesIfNotExists(selectedKind);

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
                            DrawColorRow(0, "Color 1:", selectedKind);
                        if (clothesComponent.useColorN02)
                            DrawColorRow(1, "Color 2:", selectedKind);
                        if (clothesComponent.useColorN03)
                            DrawColorRow(2, "Color 3:", selectedKind);

                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private static void DrawColorRow(int colorNr, string name, int kind)
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

            GUILayout.BeginHorizontal();
            {
                var currentColor = controller.GetClothingColor(kind, colorNr);
                bool colorOpened = GUILayout.Button("", Colorbutton(currentColor));
                if (colorOpened)
                {
                    void ChangeColorAction(Color c)
                    {
                        if (c != currentColor)
                            controller.SetClothingColor(kind, colorNr, c);
                    }
                    ColorPicker.OpenColorPicker(currentColor, ChangeColorAction);
                }
                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                {
                    controller.ResetClothingColor(kind, colorNr);
                }
            }
            GUILayout.EndHorizontal();
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
