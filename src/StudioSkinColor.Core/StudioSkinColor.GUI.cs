using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx;

namespace Plugins
{
    public partial class StudioSkinColor : BaseUnityPlugin
    {
        private static readonly float leftPanelWidth = 140;
        private Vector2 leftPanelScroll = Vector2.zero;
        private Vector2 rightPanelScroll = Vector2.zero;

        #region Static Readonly References
        internal static readonly Dictionary<string, int> clothingKinds = new Dictionary<string, int>
        {
            { "Top", 0 },
            { "Bottom", 1 },
            { "Bra", 2 },
            { "Underwear", 3 },
            { "Gloves", 4 },
            { "Pantyhose", 5 },
            { "Legwear", 6 },
#if KK
            { "Shoes (Indoors)", 7 },
            { "Shoes (Outdoors)", 8 }
#elif KKS
            { "Shoes", 8 }
#endif
        };

        private static Dictionary<string, Dictionary<int, string>> shapeBodyValueMap = new Dictionary<string, Dictionary<int, string>>()
        {
            {
                "General", new Dictionary<int, string>()
                {
                    {0,  "Body Height"},
                    {1,  "Head Size"},
                }
            },
            {
                "Chest", new Dictionary<int, string>()
                {
                    {4, "Breast Size"},
                    {5, "Breast Vertical Position"},
                    {6, "Breast Spacing"},
                    {7, "Breast Horizontal Position"},
                    {8, "Breast Vertical Angle"},
                    {9, "Breast Depth"},
                    {10, "Breast Roundess"},
                    {11, "Areola Depth"},
                    {12, "Nipple Thickness"},
                    {13, "Nipple Depth"},
                }
            },
            {
                "Upper Body", new Dictionary<int, string>()
                {
                    {2, "Neck Width"},
                    {3, "Neck Thickness"},
                    {14, "Shoulder Width"},
                    {15, "Shoulder Thickness"},
                    {16, "Upper Torso Width"},
                    {17, "Upper Torso Thickness"},
                    {18, "Lower Torso Width"},
                    {19, "Lower Torso Thickness"},
                }
            },
            {
                "Lower Body", new Dictionary<int, string>()
                {
                    {20, "Waist Position"},
                    {21, "Belly Thickness"},
                    {22, "Waist Width"},
                    {23, "Waist Thickness"},
                    {24, "Hip Width"},
                    {25, "Hip Thickness"},
                    {26, "Butt Size"},
                    {27, "Butt Angle"},
                }
            },
            {
                "Arms", new Dictionary<int, string>()
                {
                    {37, "Shoulder Width"},
                    {38, "Shoulder Thickness"},
                    {39, "Upper Arm Width"},
                    {40, "Upper Arm Thickness"},
                    {41, "Elbow Width"},
                    {42, "Elbow Thickness"},
                    {43, "Forearm Thickness"},
                }
            },
            {
                "Legs", new Dictionary<int, string>()
                {
                    {28, "Upper Thigh Width"},
                    {29, "Upper Thigh Thickness"},
                    {30, "Lower Thigh Width"},
                    {31, "Lower Thigh Thickness"},
                    {32, "Knee Width"},
                    {33, "Knee Thickness"},
                    {34, "Calves"},
                    {35, "Ankle Width"},
                    {36, "Ankle Thickness"},
                }
            },
        };
        private static Dictionary<string, Dictionary<int, string>> shapeFaceValueMap = new Dictionary<string, Dictionary<int, string>>()
        {
            {
                "General", new Dictionary<int, string>()
                {
                    {0, "Face Width"},
                    {1, "Upper Face Depth"},
                    {2, "Upper Face Height"},
                    {3, "Upper Face Size"},
                    {4, "Lower Face Depth"},
                    {5, "Lower Face Width"},
                }
            },
            {
                "Ears", new Dictionary<int, string>()
                {
                    {47, "Ear Size"},
                    {48, "Ear Angle Y Axis"},
                    {49, "Ear Angle Z Axis"},
                    {50, "Upper Ear Shape"},
                    {51, "Lower Ear Shape"},
                }
            },
            {
                "Jaw", new Dictionary<int, string>()
                {
                    {6, "Lower Jaw Vertical Position"},
                    {7, "Lower Jaw Depth"},
                    {8, "Jaw Vertical Position"},
                    {9, "Jaw Width"},
                    {10, "Jaw Depth"},
                    {11, "Chin Vertical Position"},
                    {12, "Chin Depth"},
                    {13, "Chin Width"},
                }
            },
            {
                "Cheeks", new Dictionary<int, string>()
                {
                    {14, "Cheekbone Width"},
                    {15, "Cheekbone Depth"},
                    {16, "Cheek Width"},
                    {17, "Cheek Depth"},
                    {18, "Cheek Vertical Position"},
                }
            },
            {
                "Eyebrows", new Dictionary<int, string>()
                {
                    {19, "Eyebrow Vertical Position"},
                    {20, "Eyebrow Spacing"},
                    {21, "Eyebrow Angle"},
                    {22, "Inner Eyebrow Shape"},
                    {23, "Outer Eyebrow Shape"},
                }
            },
            {
                "Eyes", new Dictionary<int, string>()
                {
                    {24, "Upper Eyelid Shape 1"},
                    {25, "Upper Eyelid Shape 2"},
                    {26, "Upper Eyelid Shape 3"},
                    {27, "Lower Eyelid Shape 1"},
                    {28, "Lower Eyelid Shape 2"},
                    {29, "Lower Eyelid Shape 3"},
                    {30, "Eye Vertical Position"},
                    {31, "Eye Spacing"},
                    {32, "Eye Depth"},
                    {33, "Eye Rotation"},
                    {34, "Eye Height"},
                    {35, "Eye Width"},
                    {36, "Inner Eye Corner Height"},
                    {37, "Outer Eye Corner Height"},
                }
            },
            {
                "Nose", new Dictionary<int, string>()
                {
                    {38, "Nose Tip Height"},
                    {39, "Nose Vertical Position"},
                    {40, "Nose Ridge Height"},
                }
            },
            {
                "Mouth", new Dictionary<int, string>()
                {
                    {41, "Mouse Vertical Position"},
                    {42, "Mouth Width"},
                    {43, "Mouth Depth"},
                    {44, "Upper Lip Depth"},
                    {45, "Lower Lip Depth"},
                    {46, "Mouth Corner Shape"},
                }
            },
        };
        #endregion

        private SelectedTab selectedTab = SelectedTab.Clothes;
        private string selectedBodyTab = "General";
        private string selectedFaceTab = "General";
        private int selectedKind = 0;
        private readonly Dictionary<string, InputBuffer> inputBuffers = new Dictionary<string, InputBuffer>();

        private StudioSkinColorCharaController controller => StudioSkinColorCharaController.GetController(selectedCharacter);
        private ListInfoBase infoClothes => selectedCharacter.infoClothes[selectedKind];
        private ChaClothesComponent clothesComponent => selectedCharacter.GetCustomClothesComponent(selectedKind);

        internal void DrawWindow(int id)
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
            else if (selectedTab == SelectedTab.Face)
                DrawFaceWindow();
            else if (selectedTab == SelectedTab.Hair)
                DrawHairWindow();

            uiRect = KKAPI.Utilities.IMGUIUtils.DragResizeEatWindow(id, uiRect);
        }

        private void DrawClothesWindow()
        {
            if (!selectedCharacterClothing.ContainsKey(selectedCharacter))
            {
                controller.ChangeCoordinateEvent();
                return;
            }
            GUILayout.BeginHorizontal();
            {

                leftPanelScroll = GUILayout.BeginScrollView(leftPanelScroll, GUI.skin.box, GUILayout.Width(leftPanelWidth));
                {
                    foreach (var kind in clothingKinds)
                    {
                        Color c = GUI.color;
                        if (selectedKind == kind.Value)
                            GUI.color = Color.cyan;
                        if (!controller.ClothingKindExists(kind.Value))
                            GUI.enabled = false;
                        if (GUILayout.Button(kind.Key))
                            selectedKind = kind.Value;
                        GUI.color = c;
                        GUI.enabled = true;
                    }
                }
                GUILayout.EndScrollView();

                rightPanelScroll = GUILayout.BeginScrollView(rightPanelScroll, GUI.skin.box);
                {
                    controller.InitBaseCustomTextureClothesIfNotExists(selectedKind);
                    var clothingList = selectedCharacterClothing[selectedCharacter]?.Where(c => c.Kind == selectedKind);

                    foreach (var clothing in clothingList)
                    {
                        GUILayout.BeginHorizontal(GUI.skin.box);
                        if (clothing.IsC2a)
                            GUILayout.Label($"(Acc {clothing.SlotNr})", new GUIStyle(GUI.skin.label));
                        GUILayout.Label(clothing.Name, new GUIStyle(GUI.skin.label)
                        {
                            wordWrap = true,
                            fontStyle = FontStyle.Bold,
                        });
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        if (clothing.UseColors[0])
                            DrawColorRow(
                                "Color 1:",
                                controller.GetClothingColor(selectedKind, 0, clothing.SlotNr),
                                c => controller.SetClothingColor(selectedKind, 0, c, clothing.SlotNr),
                                () => controller.ResetClothingColor(selectedKind, 0, clothing.SlotNr)
                            );
                        if (clothing.UseColors[1])
                            DrawColorRow(
                                "Color 2:",
                                controller.GetClothingColor(selectedKind, 1, clothing.SlotNr),
                                c => controller.SetClothingColor(selectedKind, 1, c, clothing.SlotNr),
                                () => controller.ResetClothingColor(selectedKind, 1, clothing.SlotNr)
                            );
                        if (clothing.UseColors[2])
                            DrawColorRow(
                                "Color 3:",
                                controller.GetClothingColor(selectedKind, 2, clothing.SlotNr),
                                c => controller.SetClothingColor(selectedKind, 2, c, clothing.SlotNr),
                                () => controller.ResetClothingColor(selectedKind, 2, clothing.SlotNr)
                            );
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawBodyWindow()
        {
            GUILayout.BeginHorizontal();
            {
                leftPanelScroll = GUILayout.BeginScrollView(leftPanelScroll, GUI.skin.box, GUILayout.Width(leftPanelWidth));
                {
                    foreach (var category in shapeBodyValueMap)
                    {
                        Color c = GUI.color;
                        if (selectedBodyTab == category.Key)
                            GUI.color = Color.cyan;
                        if (GUILayout.Button(category.Key))
                            selectedBodyTab = category.Key;
                        GUI.color = c;
                    }
                }
                GUILayout.EndScrollView();

                rightPanelScroll = GUILayout.BeginScrollView(rightPanelScroll, GUI.skin.box);
                {
                    foreach (var bodyValue in shapeBodyValueMap[selectedBodyTab])
                        DrawSliderRow(
                            bodyValue.Value,
                            bodyValue.Value + bodyValue.Key,
                            controller.GetCurrentBodyValue(bodyValue.Key),
                            -1f,
                            2f,
                            (f) => controller.UpdateBodyShapeValue(bodyValue.Key, f),
                            () => controller.ResetBodyShapeValue(bodyValue.Key)
                        );

                    if (selectedBodyTab == "General")
                    {
                        DrawColorRow(
                            "Main Skin Color:",
                            controller.GetBodyColor(TextureColor.SkinMain),
                            c => controller.UpdateTextureColor(c, TextureColor.SkinMain),
                            () => controller.ResetBodyColor(TextureColor.SkinMain)
                        );
                        DrawColorRow(
                            "Sub Skin Color:",
                            controller.GetBodyColor(TextureColor.SkinSub),
                            c => controller.UpdateTextureColor(c, TextureColor.SkinSub),
                            () => controller.ResetBodyColor(TextureColor.SkinSub)
                        );
                        DrawColorRow(
                            "Tan Color:",
                            controller.GetBodyColor(TextureColor.Tan),
                            c => controller.UpdateTextureColor(c, TextureColor.Tan),
                            () => controller.ResetBodyColor(TextureColor.Tan)
                        );
                    }
                    else if (selectedBodyTab == "Chest")
                    {
                        DrawSliderRow(
                            "Softness",
                            "BustSoftness",
                            controller.GetBustValue(Bust.Softness),
                            0,
                            1,
                            f => controller.SetBustValue(f, Bust.Softness),
                            () => controller.ResetBustValue(Bust.Softness)
                        );
                        DrawSliderRow(
                            "Weight",
                            "BustWeight",
                            controller.GetBustValue(Bust.Weight),
                            0,
                            1,
                            f => controller.SetBustValue(f, Bust.Weight),
                            () => controller.ResetBustValue(Bust.Weight)
                        );
                    }

                    foreach (var category in shapeBodyValueMap)
                    {
                        
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawFaceWindow()
        {
            GUILayout.BeginHorizontal();
            {
                leftPanelScroll = GUILayout.BeginScrollView(leftPanelScroll, GUI.skin.box, GUILayout.Width(leftPanelWidth));
                {
                    foreach (var category in shapeFaceValueMap)
                    {
                        Color c = GUI.color;
                        if (selectedFaceTab == category.Key)
                            GUI.color = Color.cyan;
                        if (GUILayout.Button(category.Key))
                            selectedFaceTab = category.Key;
                        GUI.color = c;
                    }
                }
                GUILayout.EndScrollView();

                rightPanelScroll = GUILayout.BeginScrollView(rightPanelScroll, GUI.skin.box);
                {
                    foreach (var bodyValue in shapeFaceValueMap[selectedFaceTab])
                        DrawSliderRow(
                            bodyValue.Value,
                            bodyValue.Value + bodyValue.Key,
                            controller.GetCurrentFaceValue(bodyValue.Key),
                            -1f,
                            2f,
                            (f) => controller.UpdateFaceShapeValue(bodyValue.Key, f),
                            () => controller.ResetFaceShapeValue(bodyValue.Key)
                        );
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawHairWindow()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                DrawColorRow(
                    "Color 1:",
                    controller.GetHairColor(HairColor.Base),
                    c => controller.UpdateHairColor(c, HairColor.Base),
                    () => controller.ResetHairColor(HairColor.Base)
                );
                DrawColorRow(
                    "Color 2:",
                    controller.GetHairColor(HairColor.Start),
                    c => controller.UpdateHairColor(c, HairColor.Start),
                    () => controller.ResetHairColor(HairColor.Start)
                );
                DrawColorRow(
                    "Color 3:",
                    controller.GetHairColor(HairColor.End),
                    c => controller.UpdateHairColor(c, HairColor.End),
                    () => controller.ResetHairColor(HairColor.End)
                );
#if KKS
                DrawColorRow(
                    "Gloss color:",
                    controller.GetHairColor(HairColor.Gloss),
                    c => controller.UpdateHairColor(c, HairColor.Gloss),
                    () => controller.ResetHairColor(HairColor.Gloss)
                );
#endif
                DrawColorRow(
                    "Eyebrow color:",
                    controller.GetHairColor(HairColor.Eyebrow),
                    c => controller.UpdateHairColor(c, HairColor.Eyebrow),
                    () => controller.ResetHairColor(HairColor.Eyebrow)
                );
            }
            GUILayout.EndVertical();
        }

        private void DrawColorRow(string name, Color currentColor, Action<Color> setColorAction, Action resetColorAction)
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
                    ColorPicker.OpenColorPicker(currentColor, (c) =>
                    {
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

        private void DrawSliderRow(string name, string key, float currentValue, float min, float max, Action<float> setValueAction, Action resetValueAction)
        {
            inputBuffers.TryGetValue(key, out var buffer);
            if (buffer == null)
            {
                buffer = new InputBuffer(currentValue);
                inputBuffers[key] = buffer;
            }
            else if (buffer.SliderValue != currentValue)
                buffer.SliderValue = currentValue;

            GUILayout.Label(name, GUI.skin.label);
            GUILayout.BeginHorizontal();
            {
                buffer.SliderValue = GUILayout.HorizontalSlider(buffer.SliderValue, min, max, GUILayout.ExpandWidth(true));
                buffer.InputValue = GUILayout.TextField(buffer.InputValue, GUILayout.Width(40));
                if (GUILayout.Button("Reset"))
                    resetValueAction();
            }

            if (buffer.SliderValue != currentValue)
                setValueAction(buffer.SliderValue);
            GUILayout.EndHorizontal();
        }

        private GUIStyle Colorbutton(Color col)
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
            Face,
            Hair,
        }

        internal void ClearBuffers()
        {
            inputBuffers.Clear();
        }

        private class InputBuffer
        {
            private float sliderValue;
            public float SliderValue
            {
                get { return sliderValue; }
                set
                {
                    if (sliderValue != value)
                    {
                        sliderValue = value;
                        inputValue = value.ToString("0.000");
                    }
                }
            }

            private string inputValue;
            public string InputValue
            {
                get { return inputValue; }
                set
                {
                    if (float.TryParse(value, out float inputValueFloat))
                        inputValue = value;
                    sliderValue = inputValueFloat;
                }
            }

            public InputBuffer(float value)
            {
                SliderValue = value;
                InputValue = value.ToString("0.000");
            }
        }
    }
}
