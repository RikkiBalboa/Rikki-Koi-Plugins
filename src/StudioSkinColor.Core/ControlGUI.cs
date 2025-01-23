using Illusion.Game;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RootMotion.FinalIK.GrounderQuadruped;

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

        private static SelectedTab selectedTab = SelectedTab.Clothes;
        private static int selectedKind = 0;

        private static StudioSkinColorCharaController controller => StudioSkinColorCharaController.GetController(selectedCharacter);
        private static ChaControl selectedCharacter => StudioSkinColor.selectedCharacter;
        private static ListInfoBase infoClothes => selectedCharacter.infoClothes[selectedKind];
        private static ChaClothesComponent clothesComponent => selectedCharacter.GetCustomClothesComponent(selectedKind);

        internal static void DrawWindow(int id)
        {

            GUILayout.BeginHorizontal();
            {
                Color c = GUI.color;
                foreach (var value in Enum.GetValues(typeof(SelectedTab)).Cast<SelectedTab>())
                {
                    if (selectedTab == value)
                        GUI.color = Color.cyan;
                    if (GUILayout.Button(value.ToString()))
                        selectedTab = value;
                    GUI.color = c;
                }
            }
            GUILayout.EndHorizontal();

            if (selectedTab == SelectedTab.Clothes)
                DrawClothesWindow();
            else if (selectedTab == SelectedTab.Body)
                DrawBodyWindow();
            else if (selectedTab == SelectedTab.Hair)
                DrawHairWindow();

            GUI.DragWindow();
        }

        private static void DrawClothesWindow()
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
                    GUILayout.Label(infoClothes.Name, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true,
                        fontStyle = FontStyle.Bold,
                    }, GUILayout.Width(150));
                    GUILayout.EndVertical();

                    if (clothesComponent != null)
                    {
                        if (clothesComponent.useColorN01)
                            DrawColorRow(
                                "Color 1:",
                                controller.GetClothingColor(selectedKind, 0),
                                c => controller.SetClothingColor(selectedKind, 0, c),
                                () => controller.ResetClothingColor(selectedKind, 0)
                            );
                        if (clothesComponent.useColorN02)
                            DrawColorRow(
                                "Color 2:",
                                controller.GetClothingColor(selectedKind, 1),
                                c => controller.SetClothingColor(selectedKind, 1, c),
                                () => controller.ResetClothingColor(selectedKind, 1)
                            );
                        if (clothesComponent.useColorN03)
                            DrawColorRow(
                                "Color 3:",
                                controller.GetClothingColor(selectedKind, 2),
                                c => controller.SetClothingColor(selectedKind, 2, c),
                                () => controller.ResetClothingColor(selectedKind, 2)
                            );

                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawBodyWindow()
        {

        }
        private static void DrawHairWindow()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                DrawColorRow(
                    "Color 1:",
                    controller.GetHairColor(HairColor.Base),
                    c => controller.UpdateHairColor(c, HairColor.Base),
                    () => controller.ResetClothingColor(selectedKind, 0)
                );
                DrawColorRow(
                    "Color 2:",
                    controller.GetHairColor(HairColor.Start),
                    c => controller.UpdateHairColor(c, HairColor.Start),
                    () => controller.ResetClothingColor(selectedKind, 0)
                );
                DrawColorRow(
                    "Color 3:",
                    controller.GetHairColor(HairColor.End),
                    c => controller.UpdateHairColor(c, HairColor.End),
                    () => controller.ResetClothingColor(selectedKind, 0)
                );
#if KKS
                DrawColorRow(
                    "Gloss color:",
                    controller.GetHairColor(HairColor.Gloss),
                    c => controller.UpdateHairColor(c, HairColor.Gloss),
                    () => controller.ResetClothingColor(selectedKind, 0)
                );
#endif
                DrawColorRow(
                    "Eyebrow color:",
                    controller.GetHairColor(HairColor.Eyebrow),
                    c => controller.UpdateHairColor(c, HairColor.Eyebrow),
                    () => controller.ResetClothingColor(selectedKind, 0)
                );
            }
            GUILayout.EndVertical();
        }

        private static void DrawColorRow(string name, Color currentColor, Action<Color> setColorAction, Action resetColorAction)
        {
            GUILayout.Label(name, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(GUI.skin.label.margin.left, GUI.skin.label.margin.right, GUI.skin.label.margin.top, 0)
            }, GUILayout.ExpandWidth(false));

            GUILayout.BeginHorizontal();
            {
                bool colorOpened = GUILayout.Button("", new GUIStyle(Colorbutton(currentColor))
                {
                    margin = new RectOffset(GUI.skin.button.margin.left, GUI.skin.button.margin.right, 0, GUI.skin.button.margin.bottom)
                });
                if (colorOpened)
                    ColorPicker.OpenColorPicker(currentColor, (c) => {
                        if (c != currentColor)
                            setColorAction(c);
                    });
                if (GUILayout.Button("Reset", new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(GUI.skin.button.margin.left, GUI.skin.button.margin.right, 0, GUI.skin.button.margin.bottom)
                }, GUILayout.ExpandWidth(false)))
                    resetColorAction();
            }
            GUILayout.EndHorizontal();
        }

        private static GUIStyle Colorbutton(Color col)
        {
            GUIStyle guistyle = new GUIStyle(GUI.skin.button);
            Texture2D texture2D = new Texture2D(1, 1, (TextureFormat)20, false);
            texture2D.SetPixel(0, 0, col);
            texture2D.Apply();
            guistyle.normal.background = texture2D;
            guistyle.hover = guistyle.normal;
            guistyle.onHover = guistyle.normal;
            guistyle.onActive = guistyle.normal;
            guistyle.onFocused = guistyle.normal;
            guistyle.active = guistyle.normal;
            guistyle.margin = new RectOffset(guistyle.margin.left, guistyle.margin.right, 0, guistyle.margin.bottom);
            return guistyle;
        }

        private enum SelectedTab
        {
            Clothes,
            Body,
            Hair,
        }
    }
}
