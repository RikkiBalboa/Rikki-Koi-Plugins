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

        public static readonly Dictionary<string, Dictionary<int, string>> shapeBodyValueMap = new Dictionary<string, Dictionary<int, string>>()
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
        public static readonly Dictionary<string, Dictionary<int, string>> shapeFaceValueMap = new Dictionary<string, Dictionary<int, string>>()
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
            { "Iris", new Dictionary<int, string>() },
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
            { "Makeup", new Dictionary<int, string>() },
        };
        #endregion

        private SelectedTab selectedTab = SelectedTab.Body;
        private string selectedBodyTab = "General";
        private string selectedFaceTab = "General";
        private int selectedKind = 0;
        private readonly Dictionary<string, InputBuffer> inputBuffers = new Dictionary<string, InputBuffer>();

        private StudioSkinColorCharaController controller => StudioSkinColorCharaController.GetController(selectedCharacter);
        private ListInfoBase infoClothes => selectedCharacter.infoClothes[selectedKind];
        private ChaClothesComponent clothesComponent => selectedCharacter.GetCustomClothesComponent(selectedKind);

        internal void DrawWindow(int id)
        {
            int visibleAreaSize = GUI.skin.window.border.top - 4;
            if (GUI.Button(new Rect(uiRect.width - visibleAreaSize - 2, 2, visibleAreaSize, visibleAreaSize), "X"))
            {
                uiShow = false;
                return;
            }

            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                //selectedCharacter.chaFile
                GUILayout.Label($"{selectedCharacter.chaFile.parameter.fullname} ({selectedCharacter.name})", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                });
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                Color c = GUI.color;
                foreach (var value in Enum.GetValues(typeof(SelectedTab)).Cast<SelectedTab>())
                {
                    if (selectedTab == value)
                        GUI.color = Color.cyan;
                    else if (controller.IsCategoryEdited(value))
                        GUI.color = Color.magenta;
                    if (GUILayout.Button(value.ToString()))
                        selectedTab = value;
                    GUI.color = c;
                }
            }
            GUILayout.EndHorizontal();

            if (selectedTab == SelectedTab.Body)
                DrawBodyWindow();
            else if (selectedTab == SelectedTab.Face)
                DrawFaceWindow();
            else if (selectedTab == SelectedTab.Hair)
                DrawHairWindow();
            else if (selectedTab == SelectedTab.Clothes)
                DrawClothesWindow();

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
                        else if (controller.IsClothingKindEdited(kind.Value))
                            GUI.color = Color.magenta;
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

                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosTop].DrawSelectedItem();

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
                                controller.GetOriginalClothingColor(selectedKind, 0, clothing.SlotNr),
                                c => controller.SetClothingColor(selectedKind, 0, c, clothing.SlotNr),
                                () => controller.ResetClothingColor(selectedKind, 0, clothing.SlotNr)
                            );
                        if (clothing.UseColors[1])
                            DrawColorRow(
                                "Color 2:",
                                controller.GetClothingColor(selectedKind, 1, clothing.SlotNr),
                                controller.GetOriginalClothingColor(selectedKind, 1, clothing.SlotNr),
                                c => controller.SetClothingColor(selectedKind, 1, c, clothing.SlotNr),
                                () => controller.ResetClothingColor(selectedKind, 1, clothing.SlotNr)
                            );
                        if (clothing.UseColors[2])
                            DrawColorRow(
                                "Color 3:",
                                controller.GetClothingColor(selectedKind, 2, clothing.SlotNr),
                                controller.GetOriginalClothingColor(selectedKind, 2, clothing.SlotNr),
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
            void BodyColorRow(string name, BodyColor bodyColor)
            {
                DrawColorRow(
                    $"{name}:",
                    controller.GetBodyColor(bodyColor),
                    controller.GetOriginalBodyColor(bodyColor),
                    c => controller.UpdateBodyColor(c, bodyColor),
                    () => controller.ResetBodyColor(bodyColor)
                );
            }

            GUILayout.BeginHorizontal();
            {
                leftPanelScroll = GUILayout.BeginScrollView(leftPanelScroll, GUI.skin.box, GUILayout.Width(leftPanelWidth));
                {
                    foreach (var category in shapeBodyValueMap)
                    {
                        Color c = GUI.color;
                        if (selectedBodyTab == category.Key)
                            GUI.color = Color.cyan;
                        else if (controller.IsBodyCategoryEdited(category.Key))
                            GUI.color = Color.magenta;
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
                            controller.GetOriginalBodyShapeValue(bodyValue.Key),
                            - 1f,
                            2f,
                            (f) => controller.UpdateBodyShapeValue(bodyValue.Key, f),
                            () => controller.ResetBodyShapeValue(bodyValue.Key)
                        );

                    if (selectedBodyTab == "General")
                    {
                        BodyColorRow("Main Skin Color", BodyColor.SkinMain);
                        BodyColorRow("Sub Skin Color", BodyColor.SkinSub);
                        BodyColorRow("Tan Color", BodyColor.SkinTan);
                        BodyColorRow("Pubic Hair Color", BodyColor.PubicHairColor);
                        BodyColorRow("Nail Color", BodyColor.NailColor);
                    }
                    else if (selectedBodyTab == "Chest")
                    {
                        DrawSliderRow(
                            "Softness",
                            "BustSoftness",
                            controller.GetBustValue(Bust.Softness),
                            controller.GetOriginalBustValue(Bust.Softness),
                            0,
                            1,
                            f => controller.SetBustValue(f, Bust.Softness),
                            () => controller.ResetBustValue(Bust.Softness)
                        );
                        DrawSliderRow(
                            "Weight",
                            "BustWeight",
                            controller.GetBustValue(Bust.Weight),
                            controller.GetOriginalBustValue(Bust.Weight),
                            0,
                            1,
                            f => controller.SetBustValue(f, Bust.Weight),
                            () => controller.ResetBustValue(Bust.Weight)
                        );
                        BodyColorRow("Nipple Color", BodyColor.NippleColor);
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
            void FaceColorRow(string name, FaceColor faceColor)
            {
                DrawColorRow(
                    $"{name}:",
                    controller.GetFaceColor(faceColor),
                    controller.GetOriginalFaceColor(faceColor),
                    c => controller.UpdateFaceColor(c, faceColor),
                    () => controller.ResetFaceColor(faceColor)
                );
            }

            GUILayout.BeginHorizontal();
            {
                leftPanelScroll = GUILayout.BeginScrollView(leftPanelScroll, GUI.skin.box, GUILayout.Width(leftPanelWidth));
                {
                    foreach (var category in shapeFaceValueMap)
                    {
                        Color c = GUI.color;
                        if (selectedFaceTab == category.Key)
                            GUI.color = Color.cyan;
                        else if (controller.IsFaceEdited(category.Key))
                            GUI.color = Color.magenta;
                        if (GUILayout.Button(category.Key))
                            selectedFaceTab = category.Key;
                        GUI.color = c;
                    }
                }
                GUILayout.EndScrollView();

                rightPanelScroll = GUILayout.BeginScrollView(rightPanelScroll, GUI.skin.box);
                {
                    foreach (var faceValue in shapeFaceValueMap[selectedFaceTab])
                        DrawSliderRow(
                            faceValue.Value,
                            faceValue.Value + faceValue.Key,
                            controller.GetCurrentFaceValue(faceValue.Key),
                            controller.GetOriginalFaceShapeValue(faceValue.Key),
                            - 1f,
                            2f,
                            (f) => controller.UpdateFaceShapeValue(faceValue.Key, f),
                            () => controller.ResetFaceShapeValue(faceValue.Key)
                        );

                    if (selectedFaceTab == "Eyebrows")
                        FaceColorRow("Eyebrow Color", FaceColor.EyebrowColor);
                    else if (selectedFaceTab == "Eyes")
                        FaceColorRow("Eyeline Color", FaceColor.EyelineColor);
                    else if (selectedFaceTab == "Iris")
                    {
                        FaceColorRow("Sclera Color 1", FaceColor.ScleraColor1);
                        FaceColorRow("Sclera Color 2", FaceColor.ScleraColor2);
                        FaceColorRow("Upper Highlight Color", FaceColor.UpperHighlightColor);
                        FaceColorRow("Lower Highlight Color", FaceColor.LowerHightlightColor);
                        FaceColorRow("Eye Color 1 (Left)", FaceColor.EyeColor1Left);
                        FaceColorRow("Eye Color 2 (Left)", FaceColor.EyeColor2Left);
                        FaceColorRow("Eye Color 1 (Right)", FaceColor.EyeColor1Right);
                        FaceColorRow("Eye Color 2 (Right)", FaceColor.EyeColor2Right);
                    }
                    else if (selectedFaceTab == "Mouth")
                    {
                        FaceColorRow("Lip Line Color", FaceColor.LipLineColor);
                    }
                    else if (selectedFaceTab == "Makeup")
                    {
                        FaceColorRow("Eye Shadow Color", FaceColor.EyeShadowColor);
                        FaceColorRow("Cheek Color", FaceColor.CheekColor);
                        FaceColorRow("Lip Color", FaceColor.LipColor);
                    }
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
                    controller.GetOriginalHairColor(HairColor.Base),
                    c => controller.UpdateHairColor(c, HairColor.Base),
                    () => controller.ResetHairColor(HairColor.Base)
                );
                DrawColorRow(
                    "Color 2:",
                    controller.GetHairColor(HairColor.Start),
                    controller.GetOriginalHairColor(HairColor.Start),
                    c => controller.UpdateHairColor(c, HairColor.Start),
                    () => controller.ResetHairColor(HairColor.Start)
                );
                DrawColorRow(
                    "Color 3:",
                    controller.GetHairColor(HairColor.End),
                    controller.GetOriginalHairColor(HairColor.End),
                    c => controller.UpdateHairColor(c, HairColor.End),
                    () => controller.ResetHairColor(HairColor.End)
                );
#if KKS
                DrawColorRow(
                    "Gloss color:",
                    controller.GetHairColor(HairColor.Gloss),
                    controller.GetOriginalHairColor(HairColor.Gloss),
                    c => controller.UpdateHairColor(c, HairColor.Gloss),
                    () => controller.ResetHairColor(HairColor.Gloss)
                );
#endif
                DrawColorRow(
                    "Eyebrow color:",
                    controller.GetHairColor(HairColor.Eyebrow),
                    controller.GetOriginalHairColor(HairColor.Eyebrow),
                    c => controller.UpdateHairColor(c, HairColor.Eyebrow),
                    () => controller.ResetHairColor(HairColor.Eyebrow)
                );
            }
            GUILayout.EndVertical();
        }

        private void DrawColorRow(string name, Color currentColor, Color originalColor, Action<Color> setColorAction, Action resetColorAction)
        {
            Color _c = GUI.color;
            if (!UseWideLayout.Value)
                GUILayout.Label(name, new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(GUI.skin.label.margin.left, GUI.skin.label.margin.right, GUI.skin.label.margin.top, 0)
                }, GUILayout.ExpandWidth(false));

            GUILayout.BeginHorizontal();
            {
                if (UseWideLayout.Value)
                    GUILayout.Label(name, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = new RectOffset(GUI.skin.label.margin.left, GUI.skin.label.margin.right, GUI.skin.label.margin.top, 0),
                        fixedWidth = 160,
                        wordWrap = false,
                    }, GUILayout.ExpandWidth(false));

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

                if (currentColor != originalColor)
                    GUI.color = Color.magenta;

                if (GUILayout.Button("Reset", new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(GUI.skin.button.margin.left, GUI.skin.button.margin.right, 0, GUI.skin.button.margin.bottom)
                }, GUILayout.ExpandWidth(false)))
                    resetColorAction();
            }
            GUI.color = _c;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private void DrawSliderRow(string name, string key, float currentValue, float originalValue, float min, float max, Action<float> setValueAction, Action resetValueAction)
        {
            var valueChanged = Mathf.Abs(currentValue - originalValue) > 0.001f;

            Color c = GUI.color;
            inputBuffers.TryGetValue(key, out var buffer);
            if (buffer == null)
            {
                buffer = new InputBuffer(currentValue);
                inputBuffers[key] = buffer;
            }
            else if (Mathf.Abs(buffer.SliderValue - currentValue) > 0.001f)
                buffer.SliderValue = currentValue;

            if (!UseWideLayout.Value)
                GUILayout.Label(name, GUI.skin.label);
            GUILayout.BeginHorizontal();
            {
                if (UseWideLayout.Value)
                    GUILayout.Label(name, new GUIStyle(GUI.skin.label)
                    {
                        fixedWidth = 160,
                        wordWrap = false,
                    });
                buffer.SliderValue = GUILayout.HorizontalSlider(buffer.SliderValue, min, max, GUILayout.ExpandWidth(true));

                if (valueChanged)
                    GUI.color = Color.magenta;

                buffer.InputValue = GUILayout.TextField(buffer.InputValue, GUILayout.Width(40));
                if (GUILayout.Button("Reset"))
                    resetValueAction();
            }

            if (Mathf.Abs(buffer.SliderValue - currentValue) > 0.001f)
                setValueAction(buffer.SliderValue);
            GUI.color = c;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
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

        public enum SelectedTab
        {
            Body,
            Face,
            Hair,
            Clothes,
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
