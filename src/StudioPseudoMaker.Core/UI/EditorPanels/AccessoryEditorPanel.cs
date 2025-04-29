using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class AccessoryEditorPanel : BaseEditorPanel
    {
        private static Vector3[] CopiedTransforms;
        internal static int currentAccessoryNr = -1;
        private bool currentAccessoryExists;
        private ChaListDefine.CategoryNo currentAccessoryType;

        private List<GameObject> colorRows;
        private GameObject hairAccessorySplitter;
        private GameObject matchHairColorToggle;
        private GameObject useHairGlossToggle;
        private GameObject hairLengthSlider;
        private GameObject hairAccessoryColor;
        private GameObject colorSplitter;
        private Toggle transformHeader1;
        private List<GameObject> tranformRows1;
        private ToggleComponent transform1GuideObjectToggle;
        private Toggle transformHeader2;
        private List<GameObject> tranformRows2;
        private ToggleComponent transform2GuideObjectToggle;
        private PickerComponent accessoryPicker;
        private DropdownComponent parentDropdown;
        private ToggleGroup guideObjectToggleGroup;
        private GameObject noShake;

        private GameObject a12UpdateButton;
        private GameObject MaterialEditorSplitter;
        private GameObject MaterialEditorButton;

        private bool isRefreshing = false;
        private bool[] showingGuideObject = { false, false };
        private bool keepParent = false;
        private bool keepColor = true;

        protected override void Initialize()
        {
            base.Initialize();

            #region Accessory Selection
            AddToggleRow(
                "Use Previous Parent When Changing Accessory Type",
                value => keepParent = value,
                () => keepParent
            );

            AddDropdownRow(
                "Type",
                UIMappings.AccessoryTypes.Select(x => UIMappings.GetAccessoryTypeName(x)).ToList(),
                () => UIMappings.GetAccessoryTypeIndex(PseudoMaker.selectedCharacterController.GetCurrentAccessoryType(currentAccessoryNr)),
                index => {
                    if (index == UIMappings.AccessoryTypes.Count - 1) return;
                    PseudoMaker.selectedCharacterController.SetAccessory(
                        currentAccessoryNr,
                        (int)UIMappings.AccessoryTypes[index],
                        0,
                        keepParent
                    );
                    ChangeSelectedAccessory(currentAccessoryNr, UIMappings.AccessoryTypes[index]);
                    AccessoryPanel.UpdateSlotName(currentAccessoryNr);
                }
            );

            accessoryPicker = AddPickerRow(ChaListDefine.CategoryNo.ao_none);

            parentDropdown = AddDropdownRow(
                "Parent",
                UIMappings.AccessoryParents.Values.ToList(),
                () => {
                    var parent = PseudoMaker.selectedCharacterController.GetCurrentAccessoryParent(currentAccessoryNr);
                    if (parentDropdown.dropdown.options.Count > 0)
                    {
                        if (parent == "A12")
                            parentDropdown.dropdown.options.First(x => x.text.StartsWith("A12")).text = $"A12: {Compatibility.A12.GetBoneName(currentAccessoryNr)}";
                        else if (!UIMappings.AccessoryParents.Keys.Contains(parent))
                            parentDropdown.dropdown.options.First(x => x.text.StartsWith("Other")).text = $"Other: {parent}";
                    }

                    return UIMappings.GetAccessoryParentIndex(parent);
                },
                index => {
                    a12UpdateButton.SetActive(UIMappings.AccessoryParents.ElementAt(index).Key == "A12");
                    if (index == UIMappings.AccessoryParents.Count - 1) return;
                    PseudoMaker.selectedCharacterController.SetAccessoryParent(currentAccessoryNr, UIMappings.AccessoryParents.ElementAt(index).Key);
                }
            );

            a12UpdateButton = AddButtonRow("Update A12 Parent", () => {
                Compatibility.A12.RegisterParent(currentAccessoryNr);
                RefreshPanel();
            }).gameObject;

            AddButtonRow("Swap Sides", () => {
                PseudoMaker.selectedCharacterController.AccessorySwapParent(currentAccessoryNr);
                RefreshPanel();
            });

            noShake = AddToggleRow(
                "Don't Move",
                value => PseudoMaker.selectedCharacterController.SetAccessoryNoShake(currentAccessoryNr, value),
                () => PseudoMaker.selectedCharacterController.GetAccessoryNoShake(currentAccessoryNr)
            ).gameObject;

            AddSplitter();
            #endregion

            #region Accessory Colors
            colorRows = new List<GameObject>() {
                AddColorRow("Color 1", 0).gameObject,
                AddColorRow("Color 2", 1).gameObject,
                AddColorRow("Color 3", 2).gameObject,
                AddColorRow("Color 4", 3).gameObject,
                AddColorRow("Outline Color", (int)HairColor.OutlineColor).gameObject,
                AddColorRow("Accessory Color", (int)HairColor.AccessoryColor).gameObject,
#if KKS
                AddColorRow("Gloss Color", (int)HairColor.GlossColor).gameObject,
#endif
            };

            colorSplitter = AddSplitter();
#endregion

            #region Accessory Transforms
            guideObjectToggleGroup = gameObject.AddComponent<ToggleGroup>();
            guideObjectToggleGroup.allowSwitchOff = true;

            transformHeader1 = AddHeaderToggle("Adjustment 1 ▶", value => tranformRows1.ForEach(x => x.SetActive(value)));
            var input = AddInputRow("X Location", 0, AccessoryTransform.Location, TransformVector.X);
            input.IncrementValue *= -1;
            tranformRows1 = new List<GameObject>() {
                input.gameObject,
                AddInputRow("Y Location", 0, AccessoryTransform.Location, TransformVector.Y).gameObject,
                AddInputRow("Z Location", 0, AccessoryTransform.Location, TransformVector.Z).gameObject,
                AddSplitter().gameObject,

                AddInputRow("X Rotation", 0, AccessoryTransform.Rotation, TransformVector.X).gameObject,
                AddInputRow("Y Rotation", 0, AccessoryTransform.Rotation, TransformVector.Y).gameObject,
                AddInputRow("Z Rotation", 0, AccessoryTransform.Rotation, TransformVector.Z).gameObject,
                AddSplitter().gameObject,

                AddInputRow("X Scale", 0, AccessoryTransform.Scale, TransformVector.X).gameObject,
                AddInputRow("Y Scale", 0, AccessoryTransform.Scale, TransformVector.Y).gameObject,
                AddInputRow("Z Scale", 0, AccessoryTransform.Scale, TransformVector.Z).gameObject,
                AddButtonGroupRow(new Dictionary<string, Action>
                {
                    { "Copy Values", () => CopyValues(0) },
                    { "Paste Values", () => PasteValues(0) },
                    { "Mirror", () => Mirror(0) },
                    { "Reset All", () => ResetAll(0) },
                }).gameObject,
            };
            transform1GuideObjectToggle = AddToggleRow(0);

            transformHeader2 = AddHeaderToggle("Adjustment 2 ▶", value => tranformRows2.ForEach(x => x.SetActive(value)));
            var input2 = AddInputRow("X Location", 1, AccessoryTransform.Location, TransformVector.X);
            input2.IncrementValue *= -1;
            tranformRows2 = new List<GameObject>() {
                input2.gameObject,
                AddInputRow("Y Location", 1, AccessoryTransform.Location, TransformVector.Y).gameObject,
                AddInputRow("Z Location", 1, AccessoryTransform.Location, TransformVector.Z).gameObject,
                AddSplitter().gameObject,

                AddInputRow("X Rotation", 1, AccessoryTransform.Rotation, TransformVector.X).gameObject,
                AddInputRow("Y Rotation", 1, AccessoryTransform.Rotation, TransformVector.Y).gameObject,
                AddInputRow("Z Rotation", 1, AccessoryTransform.Rotation, TransformVector.Z).gameObject,
                AddSplitter().gameObject,

                AddInputRow("X Scale", 1, AccessoryTransform.Scale, TransformVector.X).gameObject,
                AddInputRow("Y Scale", 1, AccessoryTransform.Scale, TransformVector.Y).gameObject,
                AddInputRow("Z Scale", 1, AccessoryTransform.Scale, TransformVector.Z).gameObject,
                AddButtonGroupRow(new Dictionary<string, Action>
                {
                    { "Copy Values", () => CopyValues(1) },
                    { "Paste Values", () => PasteValues(1) },
                    { "Mirror", () => Mirror(1) },
                    { "Reset All", () => ResetAll(1) },
                }).gameObject,
            };
            transform2GuideObjectToggle = AddToggleRow(1);
            #endregion

            MaterialEditorSplitter = AddSplitter();
            MaterialEditorButton = AddButtonRow(
                "Material Editor",
                () => Compatibility.MaterialEditor.SetItemType(options => options.FindIndex(o => o.text.StartsWith($"Accessory {currentAccessoryNr + 1:00} ")))
            ).gameObject;

            #region HairAccessoryCustomizer
            hairAccessorySplitter = AddSplitter();

            matchHairColorToggle = AddToggleRow(
                "Match Color With Hair",
                value =>
                {
                    PseudoMaker.selectedCharacterController.SetAccessoryColorMatchHair(currentAccessoryNr, value);
                    RefreshPanel();
                },
                () => PseudoMaker.selectedCharacterController.GetAccessoryColorMatchHair(currentAccessoryNr)
            ).gameObject;

            useHairGlossToggle = AddToggleRow(
                "Use Hair Gloss",
                value =>
                {
                    PseudoMaker.selectedCharacterController.SetAccessoryUseGloss(currentAccessoryNr, value);
                    RefreshPanel();
                },
                () => PseudoMaker.selectedCharacterController.GetAccessoryUseGloss(currentAccessoryNr)
            ).gameObject;

            hairLengthSlider = AddSliderRow(
                "Hair Length",
                () => PseudoMaker.selectedCharacterController.GetAccessoryHairLength(currentAccessoryNr),
                () => 0f,
                value => PseudoMaker.selectedCharacterController.SetAccessoryHairLength(currentAccessoryNr, value),
                () => PseudoMaker.selectedCharacterController.SetAccessoryHairLength(currentAccessoryNr, 0f),
                0,
                1
            ).gameObject;
            #endregion
        }

        private void OnEnable()
        {
            if (currentAccessoryNr > PseudoMaker.selectedCharacter.infoAccessory.Length)
                currentAccessoryNr = 0;

            accessoryPicker.gameObject.SetActive(currentAccessoryType != ChaListDefine.CategoryNo.ao_none);
            parentDropdown.gameObject.SetActive(currentAccessoryType != ChaListDefine.CategoryNo.ao_none);
            noShake.SetActive(currentAccessoryType != ChaListDefine.CategoryNo.ao_none);

            if (currentAccessoryExists)
            {
                var useCols = PseudoMaker.selectedCharacterController.CheckAccessoryUseColor(currentAccessoryNr);
                var matchHair = PseudoMaker.selectedCharacterController.GetAccessoryColorMatchHair(currentAccessoryNr);
                var isHair = PseudoMaker.selectedCharacterController.GetAccessoryIsHair(currentAccessoryNr);
                var hasLength = PseudoMaker.selectedCharacterController.CheckAccessoryUsesHairLength(currentAccessoryNr);
                var usesGloss = PseudoMaker.selectedCharacterController.GetAccessoryUseGloss(currentAccessoryNr);
                var hasAccessoryPart = PseudoMaker.selectedCharacterController.CheckAccessoryHasAccessoryPart(currentAccessoryNr);

                // Color Stuff
                for (int i = 0; i < useCols.Length; i++)
                    colorRows[i].gameObject.SetActive(useCols[i] && !(matchHair && isHair));
#if KKS
                colorRows[6].SetActive(!matchHair && isHair && usesGloss);
#endif
                colorRows[4].SetActive(!matchHair && isHair);
                colorRows[5].SetActive(isHair && hasAccessoryPart);
                colorSplitter.SetActive(useCols.Any(x => x == true && !(matchHair && isHair)));

                // Transform stuff
                transformHeader1.transform.parent.gameObject.SetActive(true);
                tranformRows1.ForEach(x => x.SetActive(transformHeader1.isOn));
                transform1GuideObjectToggle.gameObject.SetActive(true);
                var hasSecondTransform = PseudoMaker.selectedCharacterController.CheckAccessoryUsesSecondTransform(currentAccessoryNr);
                transformHeader2.transform.parent.gameObject.SetActive(hasSecondTransform);
                tranformRows2.ForEach(x => x.SetActive(hasSecondTransform && transformHeader2.isOn));
                transform2GuideObjectToggle.gameObject.SetActive(hasSecondTransform);

                // HairAccessoryCustomizer
                hairAccessorySplitter.SetActive(isHair);
                matchHairColorToggle.SetActive(isHair);
                useHairGlossToggle.SetActive(isHair);
                hairLengthSlider.SetActive(isHair && hasLength);

                a12UpdateButton.SetActive(PseudoMaker.selectedCharacterController.GetCurrentAccessoryParent(currentAccessoryNr) == "A12");
                MaterialEditorSplitter.SetActive(true);
                MaterialEditorButton.SetActive(true);
            }
            else
            {
                colorRows.ForEach(x => x.SetActive(false));
                hairAccessorySplitter.SetActive(false);
                matchHairColorToggle.SetActive(false);
                useHairGlossToggle.SetActive(false);
                hairLengthSlider.SetActive(false);
                colorSplitter.SetActive(false);
                transformHeader1.transform.parent.gameObject.SetActive(false);
                tranformRows1.ForEach(x => x.SetActive(false));
                transform1GuideObjectToggle.gameObject.SetActive(false);
                transformHeader2.transform.parent.gameObject.SetActive(false);
                tranformRows2.ForEach(x => x.SetActive(false));
                transform2GuideObjectToggle.gameObject.SetActive(false);
                a12UpdateButton.SetActive(false);
                parentDropdown.gameObject.SetActive(false);
                noShake.SetActive(false);
                MaterialEditorSplitter.SetActive(false);
                MaterialEditorButton.SetActive(false);
            }
            TimelineCompatibilityHelper.SelectedAccessory = currentAccessoryNr;
        }

        private void OnDisable()
        {
            //if (PseudoMakerUI.CurrentCategory != Category.Accessories)
            if (!isRefreshing)
                AccessoryGuideObjectManager.DestroyGuideObject();
            TimelineCompatibilityHelper.SelectedAccessory = null;
        }

        public void ChangeSelectedAccessory(int slotNr, bool exists)
        {
            if (parentDropdown?.dropdown?.options?.Count > 0)
            {
                if (Compatibility.HasA12)
                    parentDropdown.dropdown.options.First(x => x.text.StartsWith("A12")).text = "A12";
                parentDropdown.dropdown.options.First(x => x.text.StartsWith("Other")).text = "Other";
            }

            currentAccessoryNr = slotNr;
            currentAccessoryExists = exists;
            currentAccessoryType = (ChaListDefine.CategoryNo)PseudoMaker.selectedCharacterController.GetCurrentAccessoryType(slotNr);
            accessoryPicker.GetId = () => $"{currentAccessoryType}_{currentAccessoryNr}";
            accessoryPicker.CategoryNo = currentAccessoryType;
            Compatibility.SelectedSlotNr = slotNr;
            transform1GuideObjectToggle.toggle.isOn = false;
            transform2GuideObjectToggle.toggle.isOn = false;
            RefreshPanel();
            PseudoMaker.RefreshCharacterstatusPanel();
        }

        public void ChangeSelectedAccessory(int slotNr, ChaListDefine.CategoryNo categoryNr)
        {
            ChangeSelectedAccessory(slotNr, categoryNr != ChaListDefine.CategoryNo.ao_none);
        }

        private ToggleComponent AddToggleRow(int correctNr)
        {
            var toggle = AddToggleRow(
                $"Show Control Axis {correctNr + 1}",
                value =>
                {
                    showingGuideObject[correctNr] = value;
                    if (value) AccessoryGuideObjectManager.CreateGuideObject(currentAccessoryNr, correctNr, RefreshPanel);
                    else AccessoryGuideObjectManager.DestroyGuideObject();
                },
                () => showingGuideObject[correctNr]
            );
            toggle.toggle.group = guideObjectToggleGroup;
            return toggle;
        }

        private ColorComponent AddColorRow(string name, int colorNr)
        {
            return AddColorRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetAccessoryColor(currentAccessoryNr, colorNr),
                () => PseudoMaker.selectedCharacterController.GetOriginalAccessoryColor(currentAccessoryNr, colorNr),
                c => PseudoMaker.selectedCharacterController.SetAccessoryColor(currentAccessoryNr, colorNr, c),
                () => PseudoMaker.selectedCharacterController.ResetAcessoryColor(currentAccessoryNr, colorNr, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed())
            );
        }

        private InputFieldComponent AddInputRow(string name, int correctNo, AccessoryTransform transform, TransformVector vector)
        {
            float minValue = -100;
            float maxValue = 100;
            float incrementValue = 0.1f;
            bool repeat = false;
            if (transform == AccessoryTransform.Rotation)
            {
                minValue = -360;
                maxValue = 360;
                incrementValue = 1f;
                repeat = true;
            }
            else if (transform == AccessoryTransform.Scale)
            {
                minValue = -100;
                maxValue = 100;
            }

            var input = AddInputRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetAccessoryTransformValue(currentAccessoryNr, correctNo, transform, vector),
                () => PseudoMaker.selectedCharacterController.GetOriginalAccessoryTransform(currentAccessoryNr, correctNo, transform, vector),
                value => PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNo, value, transform, vector),
                () => PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNo, transform, vector, getDefault: Input.GetKey(KeyCode.LeftShift)),
                minValue,
                maxValue
            );
            input.Repeat = repeat;
            input.IncrementValue = incrementValue;
            return input;
        }

        public PickerComponent AddPickerRow(ChaListDefine.CategoryNo categoryNr)
        {
            var picker = Instantiate(PickerTemplate, PickerTemplate.transform.parent);
            picker.name = "CategoryPickerAccessories";

            var pickerComponent = picker.AddComponent<PickerComponent>();
            pickerComponent.Name = "Type";
            pickerComponent.CategoryNo = categoryNr;
            pickerComponent.GetId = () => $"{categoryNr}_{currentAccessoryNr}";
            pickerComponent.GetCurrentValue = () => PseudoMaker.selectedCharacterController.GetCurrentAccessoryId(currentAccessoryNr);
            pickerComponent.SetCurrentValue = (value) =>
            {
                PseudoMaker.selectedCharacterController.SetAccessory(
                    currentAccessoryNr,
                    (int)currentAccessoryType,
                    value.index,
                    keepParent
                );
                AccessoryPanel.UpdateSlotName(currentAccessoryNr);
                RefreshPanel();
            };

            return pickerComponent;
        }

        private void CopyValues(int correctNr)
        {
            CopiedTransforms = new Vector3[] {
                PseudoMaker.selectedCharacter.nowCoordinate.accessory.parts[currentAccessoryNr].addMove[correctNr, 0],
                PseudoMaker.selectedCharacter.nowCoordinate.accessory.parts[currentAccessoryNr].addMove[correctNr, 1],
                PseudoMaker.selectedCharacter.nowCoordinate.accessory.parts[currentAccessoryNr].addMove[correctNr, 2],
            };
        }

        private void PasteValues(int correctNr)
        {
            if (CopiedTransforms == null) return;

            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, CopiedTransforms[0], AccessoryTransform.Location);
            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, CopiedTransforms[1], AccessoryTransform.Rotation);
            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, CopiedTransforms[2], AccessoryTransform.Scale);

            RefreshPanel();
        }

        private void Mirror(int correctNr)
        {
            var currentXPos = PseudoMaker.selectedCharacterController.GetAccessoryTransformValue(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.X);
            var currentYRot = PseudoMaker.selectedCharacterController.GetAccessoryTransformValue(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Y);
            var currentZRot = PseudoMaker.selectedCharacterController.GetAccessoryTransformValue(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Z);

            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, currentXPos * -1, AccessoryTransform.Location, TransformVector.X);
            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, 360 - currentYRot, AccessoryTransform.Rotation, TransformVector.Y);
            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, 360 - currentZRot, AccessoryTransform.Rotation, TransformVector.Z);
        }

        private void ResetAll(int correctNr)
        {
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.X, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.Y, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.Z, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.X, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Y, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Z, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Scale, TransformVector.X, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Scale, TransformVector.Y, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Scale, TransformVector.Z, getDefault: PseudoMaker.KeyAltReset.Value.IsPressed());
            RefreshPanel();
        }

        protected override void RefreshPanel()
        {
            isRefreshing = true;
            base.RefreshPanel();
            isRefreshing = false;
        }
    }
}
