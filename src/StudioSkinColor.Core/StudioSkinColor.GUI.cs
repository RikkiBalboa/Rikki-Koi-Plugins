﻿using Shared;
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
            { "Pubic Hair", new Dictionary<int, string>() },
            { "Suntan", new Dictionary<int, string>() },
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

        private bool isInitialized = false;
        Texture2D colorTexture;
        private RectOffset colorButtonRectOffset;
        private RectOffset labelRectOffset;
        private RectOffset resetButtonRectOffset;

        private SelectedTab selectedTab = SelectedTab.Body;
        private string selectedBodyTab = "General";
        private string selectedFaceTab = "General";
        private int selectedKind = 0;
        private int selectedAccessory = 0;
        private readonly Dictionary<string, InputBuffer> inputBuffers = new Dictionary<string, InputBuffer>();

        private StudioSkinColorCharaController controller => StudioSkinColorCharaController.GetController(selectedCharacter);
        private ListInfoBase InfoClothes => selectedCharacter.infoClothes[selectedKind];
        private ListInfoBase[] InfoAccessories => selectedCharacter.infoAccessory;
        private ChaFileAccessory.PartsInfo[] Accessories => selectedCharacter.nowCoordinate.accessory.parts;
        private ChaClothesComponent ClothesComponent => selectedCharacter.GetCustomClothesComponent(selectedKind);

        internal void InitializeStyles()
        {
            if (isInitialized) return;
            colorTexture = new Texture2D(1, 1, (TextureFormat)20, false);
            colorButtonRectOffset = new RectOffset(GUI.skin.button.margin.left, GUI.skin.button.margin.right, 0, GUI.skin.button.margin.bottom);
            labelRectOffset = new RectOffset(GUI.skin.button.margin.left, GUI.skin.button.margin.right, 0, GUI.skin.button.margin.bottom);
            resetButtonRectOffset = new RectOffset(GUI.skin.button.margin.left, GUI.skin.button.margin.right, 0, GUI.skin.button.margin.bottom);
            isInitialized = true;
        }

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
                    //else if (controller.IsCategoryEdited(value))
                    //    GUI.color = Color.magenta;
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
            else if (selectedTab == SelectedTab.Accessories)
                DrawAccessoriesWindow();

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
                        if (!clothing.IsC2a)
                        {
                            if (selectedKind == 0) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosTop].DrawSelectedItem();
                            else if (selectedKind == 1) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosBot].DrawSelectedItem();
                            else if (selectedKind == 2) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosBra].DrawSelectedItem();
                            else if (selectedKind == 3) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosShorts].DrawSelectedItem();
                            else if (selectedKind == 4) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosGloves].DrawSelectedItem();
                            else if (selectedKind == 5) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosPanst].DrawSelectedItem();
                            else if (selectedKind == 6) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosSocks].DrawSelectedItem();
                            else if (selectedKind == 7) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosInnerShoes].DrawSelectedItem();
                            else if (selectedKind == 8) categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosOuterShoes].DrawSelectedItem();

                            var optParts = controller.GetClothingUsesOptParts(ClothesComponent);
                            if (optParts > 0)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(160);
                                    if (optParts > 0)
                                        controller.SetHideOpt(clothing.Kind, 0, !GUILayout.Toggle(!controller.GetHideOpt(clothing.Kind, 0), "Option 1"));
                                    if (optParts > 1)
                                        controller.SetHideOpt(clothing.Kind, 1, !GUILayout.Toggle(!controller.GetHideOpt(clothing.Kind, 1), "Option 2"));
                                }
                                GUILayout.EndHorizontal();
                            }

                            if (selectedKind == 0)
                            {
                                var kind = selectedCharacter.infoClothes[clothing.Kind]?.Kind;
                                if (kind == 1)
                                {
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosSailor01].DrawSelectedItem();
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosSailor02].DrawSelectedItem();
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosSailor03].DrawSelectedItem();
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosTopEmblem].DrawSelectedItem();
                                }
                                else if (kind == 2)
                                {
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosJacket01].DrawSelectedItem();
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosJacket02].DrawSelectedItem();
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosJacket03].DrawSelectedItem();
                                    categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.CosTopEmblem].DrawSelectedItem();
                                }
                            }
                        }

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

                        for (int i = 0; i < 3; i++)
                        {
                            var _i = i;
                            if (clothing.UseColors[i])
                            {
                                DrawColorRow(
                                    $"Color {i + 1}:",
                                    controller.GetClothingColor(selectedKind, i, clothing.SlotNr),
                                    controller.GetOriginalClothingColor(selectedKind, i, clothing.SlotNr),
                                    c => controller.SetClothingColor(selectedKind, _i, c, clothing.SlotNr),
                                    () => controller.ResetClothingColor(selectedKind, _i, clothing.SlotNr)
                                );
                                if (!clothing.IsC2a)
                                {
                                    categoryPickers[StudioSkinColorCharaController.KindToSelectKind(clothing.Kind, i + 1)].DrawSelectedItem();
                                    if (controller.ClothingUsesPattern(clothing.Kind, i + 1))
                                    {
#if KKS
                                        DrawSliderRow(
                                            $"Pattern {i + 1} Horizontal:",
                                            $"Pattern{i}Horizontal",
                                            controller.GetPatternValue(selectedKind, i, PatternValue.Horizontal),
                                            0.5f,
                                            -2f,
                                            2f,
                                            f => controller.SetPatternValue(selectedKind, _i, PatternValue.Horizontal, f),
                                            () => controller.SetPatternValue(selectedKind, _i, PatternValue.Horizontal, 0.5f)
                                        );
                                        DrawSliderRow(
                                            $"Pattern {i} Vertical:",
                                            $"Pattern{i}Vertical",
                                            controller.GetPatternValue(selectedKind, i, PatternValue.Vertical),
                                            0.5f,
                                            -2f,
                                            2f,
                                            f => controller.SetPatternValue(selectedKind, _i, PatternValue.Vertical, f),
                                            () => controller.SetPatternValue(selectedKind, _i, PatternValue.Vertical, 0.5f)
                                        );
                                        DrawSliderRow(
                                            $"Pattern {i} Rotation:",
                                            $"Pattern{i}Rotation",
                                            controller.GetPatternValue(selectedKind, i, PatternValue.Rotation),
                                            0.5f,
                                            -2f,
                                            2f,
                                            f => controller.SetPatternValue(selectedKind, _i, PatternValue.Rotation, f),
                                            () => controller.SetPatternValue(selectedKind, _i, PatternValue.Rotation, 0.5f)
                                        );
#endif
                                        DrawSliderRow(
                                            $"Pattern {i} Width:",
                                            $"Pattern{i}Width",
                                            controller.GetPatternValue(selectedKind, i, PatternValue.Width),
                                            0.5f,
                                            -2f,
                                            2f,
                                            f => controller.SetPatternValue(selectedKind, _i, PatternValue.Width, f),
                                            () => controller.SetPatternValue(selectedKind, _i, PatternValue.Width, 0.5f)
                                        );
                                        DrawSliderRow(
                                            $"Pattern {i} Height:",
                                            $"Pattern{i}Height",
                                            controller.GetPatternValue(selectedKind, i, PatternValue.Height),
                                            0.5f,
                                            -2f,
                                            2f,
                                            f => controller.SetPatternValue(selectedKind, _i, PatternValue.Height, f),
                                            () => controller.SetPatternValue(selectedKind, _i, PatternValue.Height, 0.5f)
                                        );
                                        DrawColorRow(
                                            $"Pattern Color {i + 1}:",
                                            controller.GetClothingColor(selectedKind, i, clothing.SlotNr, true),
                                            controller.GetOriginalClothingColor(selectedKind, i, clothing.SlotNr, true),
                                            c => controller.SetClothingColor(selectedKind, _i, c, clothing.SlotNr, true),
                                            () => controller.ResetClothingColor(selectedKind, _i, clothing.SlotNr, true)
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAccessoriesWindow()
        {
            GUILayout.BeginHorizontal();
            {
                leftPanelScroll = GUILayout.BeginScrollView(leftPanelScroll, GUI.skin.box, GUILayout.Width(165));
                {
                    for (int i = 0; i < InfoAccessories.Count(); i++)
                    {
                        Color c = GUI.color;
                        if (selectedAccessory == i)
                            GUI.color = Color.cyan;
                        //else if (controller.IsClothingKindEdited(kind.Value))
                        //    GUI.color = Color.magenta;
                        if (GUILayout.Button(InfoAccessories[i] == null ? i.ToString() : InfoAccessories[i].Name, new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleLeft,
                        }, GUILayout.Width(140)))
                            selectedAccessory = i;
                        GUI.color = c;
                        GUI.enabled = true;
                    }
                }
                GUILayout.EndScrollView();

                rightPanelScroll = GUILayout.BeginScrollView(rightPanelScroll, GUI.skin.box);
                {
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label(InfoAccessories[selectedAccessory].Name, new GUIStyle(GUI.skin.label)
                    {
                        fontStyle = FontStyle.Bold,
                    });
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    var useCols = controller.CheckAccessoryUseColor(selectedAccessory);
                    for (int i = 0; i < 3; i++)
                    {
                        var _i = i;
                        if (useCols[_i])
                        {
                            DrawColorRow(
                                $"Color {i + 1}:",
                                controller.GetAccessoryColor(selectedAccessory, _i),
                                controller.GetOriginalAccessoryColor(selectedAccessory, _i),
                                c => controller.SetAccessoryColor(selectedAccessory, _i, c),
                                () => controller.ResetAccessoryColor(selectedAccessory, _i)
                            );
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawBodyWindow()
        {
            void BodyColorRow(string name, ColorType bodyColor)
            {
                DrawColorRow(
                    name,
                    controller.GetColorPropertyValue(bodyColor),
                    controller.GetOriginalColorPropertyValue(bodyColor),
                    c => controller.UpdateColorProperty(c, bodyColor),
                    () => controller.ResetColorProperty(bodyColor)
                );
            }

            void BodyFloatRow(string name, FloatType floatType)
            {
                DrawSliderRow(
                    name,
                    name,
                    controller.GetFloatValue(floatType),
                    controller.GetOriginalFloatValue(floatType),
                    -1,
                    2,
                    f => controller.SetFloatTypeValue(f, floatType),
                    () => controller.ResetFloatTypeValue(floatType)
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
                        //else if (controller.IsBodyCategoryEdited(category.Key))
                        //    GUI.color = Color.magenta;
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
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.BodyDetail].DrawSelectedItem();
                        BodyFloatRow("Skin Type Strenth:", FloatType.SkinTypeStrenth);
                        BodyColorRow("Main Skin Color:", ColorType.SkinMain);
                        BodyColorRow("Sub Skin Color:", ColorType.SkinSub);
                        BodyFloatRow("Skin Gloss:", FloatType.SkinGloss);
                        BodyColorRow("Nail Color:", ColorType.NailColor);
                        BodyFloatRow("Nail Gloss:", FloatType.NailGloss);
                    }
                    else if (selectedBodyTab == "Chest")
                    {
                        DrawSliderRow(
                            "Softness",
                            "BustSoftness",
                            controller.GetFloatValue(FloatType.Softness),
                            controller.GetOriginalFloatValue(FloatType.Softness),
                            0,
                            1,
                            f => controller.SetFloatTypeValue(f, FloatType.Softness),
                            () => controller.ResetFloatTypeValue(FloatType.Softness)
                        );
                        DrawSliderRow(
                            "Weight",
                            "BustWeight",
                            controller.GetFloatValue(FloatType.Weight),
                            controller.GetOriginalFloatValue(FloatType.Weight),
                            0,
                            1,
                            f => controller.SetFloatTypeValue(f, FloatType.Weight),
                            () => controller.ResetFloatTypeValue(FloatType.Weight)
                        );
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Nip].DrawSelectedItem();
                        BodyColorRow("Nipple Color", ColorType.NippleColor);
                        BodyFloatRow("Nipple Gloss", FloatType.NippleGloss);
                    }
                    else if (selectedBodyTab == "Pubic Hair")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Underhair].DrawSelectedItem();
                        BodyColorRow("Pubic Hair Color", ColorType.PubicHairColor);
                    }
                    else if (selectedBodyTab == "Suntan")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Sunburn].DrawSelectedItem();
                        BodyColorRow("Tan Color", ColorType.SkinTan);
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
            void FaceColorRow(string name, ColorType faceColor)
            {
                DrawColorRow(
                    $"{name}:",
                    controller.GetColorPropertyValue(faceColor),
                    controller.GetOriginalColorPropertyValue(faceColor),
                    c => controller.UpdateColorProperty(c, faceColor),
                    () => controller.ResetColorProperty(faceColor)
                );
            }

            void FaceFloatRow(string name, FloatType floatType)
            {
                DrawSliderRow(
                    name,
                    name,
                    controller.GetFloatValue(floatType),
                    controller.GetOriginalFloatValue(floatType),
                    -1,
                    2,
                    f => controller.SetFloatTypeValue(f, floatType),
                    () => controller.ResetFloatTypeValue(floatType)
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

                    if (selectedFaceTab == "General")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.HeadType].DrawSelectedItem();
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.FaceDetail].DrawSelectedItem();
                        FaceFloatRow("Face Overlay Strenth", FloatType.FaceOverlayStrenth);
                    }
                    else if (selectedFaceTab == "Cheeks")
                    {
                        FaceFloatRow("Cheek Gloss", FloatType.CheekGloss);
                    }
                    else if (selectedFaceTab == "Eyebrows")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Eyebrow].DrawSelectedItem();
                        FaceColorRow("Eyebrow Color", ColorType.EyebrowColor);
                    }
                    else if (selectedFaceTab == "Eyes")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.EyelineUp].DrawSelectedItem();
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.EyelineDown].DrawSelectedItem();
                        FaceColorRow("Eyeline Color", ColorType.EyelineColor);
                    }
                    else if (selectedFaceTab == "Iris")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.EyeWGrade].DrawSelectedItem();
                        FaceColorRow("Sclera Color 1", ColorType.ScleraColor1);
                        FaceColorRow("Sclera Color 2", ColorType.ScleraColor2);
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.EyeHLUp].DrawSelectedItem();
                        FaceColorRow("Upper Highlight Color", ColorType.UpperHighlightColor);
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.EyeHLDown].DrawSelectedItem();
                        FaceColorRow("Lower Highlight Color", ColorType.LowerHightlightColor);
                        FaceFloatRow("Upper Highlight Vertical", FloatType.UpperHighlightVertical);
                        FaceFloatRow("Upper Highlight Horizontal", FloatType.UpperHighlightHorizontal);
                        FaceFloatRow("Lower Highlight Vertical", FloatType.LowerHightlightVertical);
                        FaceFloatRow("Lower Highlight Horizontal", FloatType.LowerHightlightHorizontal);
                        FaceFloatRow("Iris Spacing", FloatType.IrisSpacing);
                        FaceFloatRow("Iris Vertical Position", FloatType.IrisVerticalPosition);
                        FaceFloatRow("Iris Width", FloatType.IrisWidth);
                        FaceFloatRow("Iris Height", FloatType.IrisHeight);
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Pupil].DrawSelectedItem();
                        FaceColorRow("Eye Color 1 (Left)", ColorType.EyeColor1Left);
                        FaceColorRow("Eye Color 2 (Left)", ColorType.EyeColor2Left);
                        FaceColorRow("Eye Color 1 (Right)", ColorType.EyeColor1Right);
                        FaceColorRow("Eye Color 2 (Right)", ColorType.EyeColor2Right);
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.PupilGrade].DrawSelectedItem();
                        FaceFloatRow("Eye Gradient Strenth", FloatType.EyeGradientStrenth);
                        FaceFloatRow("Eye Gradient Vertical", FloatType.EyeGradientVertical);
                        FaceFloatRow("Eye Gradient Size", FloatType.EyeGradientSize);
                    }
                    else if (selectedFaceTab == "Nose")
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Nose].DrawSelectedItem();
                    else if (selectedFaceTab == "Mouth")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Lipline].DrawSelectedItem();
                        FaceColorRow("Lip Line Color", ColorType.LipLineColor);
                        FaceFloatRow("Lip Gloss", FloatType.LipGloss);
                    }
                    else if (selectedFaceTab == "Makeup")
                    {
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Eyeshadow].DrawSelectedItem();
                        FaceColorRow("Eye Shadow Color", ColorType.EyeShadowColor);
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Cheek].DrawSelectedItem();
                        FaceColorRow("Cheek Color", ColorType.CheekColor);
                        categoryPickers[ChaCustom.CustomSelectKind.SelectKindType.Lip].DrawSelectedItem();
                        FaceColorRow("Lip Color", ColorType.LipColor);
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
                    controller.GetColorPropertyValue(ColorType.HairBase),
                    controller.GetOriginalColorPropertyValue(ColorType.HairBase),
                    c => controller.UpdateColorProperty(c, ColorType.HairBase),
                    () => controller.ResetColorProperty(ColorType.HairBase)
                );
                DrawColorRow(
                    "Color 2:",
                    controller.GetColorPropertyValue(ColorType.HairStart),
                    controller.GetOriginalColorPropertyValue(ColorType.HairStart),
                    c => controller.UpdateColorProperty(c, ColorType.HairStart),
                    () => controller.ResetColorProperty(ColorType.HairStart)
                );
                DrawColorRow(
                    "Color 3:",
                    controller.GetColorPropertyValue(ColorType.HairEnd),
                    controller.GetOriginalColorPropertyValue(ColorType.HairEnd),
                    c => controller.UpdateColorProperty(c, ColorType.HairEnd),
                    () => controller.ResetColorProperty(ColorType.HairEnd)
                );
#if KKS
                DrawColorRow(
                    "Gloss color:",
                    controller.GetColorPropertyValue(ColorType.HairGloss),
                    controller.GetOriginalColorPropertyValue(ColorType.HairGloss),
                    c => controller.UpdateColorProperty(c, ColorType.HairGloss),
                    () => controller.ResetColorProperty(ColorType.HairGloss)
                );
#endif
                DrawColorRow(
                    "Eyebrow color:",
                    controller.GetColorPropertyValue(ColorType.Eyebrow),
                    controller.GetOriginalColorPropertyValue(ColorType.Eyebrow),
                    c => controller.UpdateColorProperty(c, ColorType.Eyebrow),
                    () => controller.ResetColorProperty(ColorType.Eyebrow)
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
                    margin = labelRectOffset
                }, GUILayout.ExpandWidth(false));

            GUILayout.BeginHorizontal();
            {
                if (UseWideLayout.Value)
                    GUILayout.Label(name, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = labelRectOffset,
                        fixedWidth = 160,
                        wordWrap = false,
                    }, GUILayout.ExpandWidth(false));

                if (GUILayout.Button("", Colorbutton(currentColor)))
                    ColorPicker.OpenColorPicker(currentColor, (c) =>
                    {
                        if (c != currentColor)
                            setColorAction(c);
                    });

                if (currentColor != originalColor)
                    GUI.color = Color.magenta;

                if (GUILayout.Button("Reset", new GUIStyle(GUI.skin.button)
                {
                    margin = resetButtonRectOffset
                }, GUILayout.ExpandWidth(false)))
                    resetColorAction();
            }
            GUI.color = _c;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private void DrawSliderRow(string name, string key, float currentValue, float originalValue, float min, float max, Action<float> setValueAction, Action resetValueAction)
        {
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

                if (Mathf.Abs(currentValue - originalValue) > 0.001f)
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
            colorTexture.SetPixel(0, 0, col);
            colorTexture.Apply();
            GUIStyle guistyle = new GUIStyle(GUI.skin.button);
            guistyle.normal.background = colorTexture;
            guistyle.hover = guistyle.normal;
            guistyle.onHover = guistyle.normal;
            guistyle.onActive = guistyle.normal;
            guistyle.onFocused = guistyle.normal;
            guistyle.active = guistyle.normal;
            guistyle.margin = colorButtonRectOffset;
            return guistyle;
        }

        public enum SelectedTab
        {
            Body,
            Face,
            Hair,
            Clothes,
            Accessories,
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
