using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
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
        private Toggle transformHeader2;
        private List<GameObject> tranformRows2;
        private PickerComponent accessoryPicker;

        protected override void Initialize()
        {
            base.Initialize();

            AddDropdownRow(
                "Type",
                UIMappings.AccessoryTypes.Select(x => UIMappings.GetAccessoryTypeName(x)).ToList(),
                () => UIMappings.GetAccessoryTypeIndex(PseudoMaker.selectedCharacterController.GetCurrentAccessoryType(currentAccessoryNr)),
                index => {
                    PseudoMaker.selectedCharacterController.SetAccessory(
                        currentAccessoryNr,
                        (int)UIMappings.AccessoryTypes[index],
                        0,
                        ""
                    );
                    ChangeSelectedAccessory(currentAccessoryNr, UIMappings.AccessoryTypes[index]);
                }
            );

            accessoryPicker = AddPickerRow(ChaListDefine.CategoryNo.ao_none);

            AddSplitter();

            colorRows = new List<GameObject>() {
                AddColorRow("Color 1", 0).gameObject,
                AddColorRow("Color 2", 1).gameObject,
                AddColorRow("Color 3", 2).gameObject,
                AddColorRow("Color 4", 3).gameObject,
                AddColorRow("Outline Color", (int)HairColor.OutlineColor).gameObject,
                AddColorRow("Accessory Color", (int)HairColor.AccessoryColor).gameObject,
                AddColorRow("Gloss Color", (int)HairColor.GlossColor).gameObject,
            };

            colorSplitter = AddSplitter();

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
        }

        private void OnEnable()
        {
            accessoryPicker.gameObject.SetActive(currentAccessoryType != ChaListDefine.CategoryNo.ao_none);

            if (currentAccessoryExists)
            {
                var useCols = PseudoMaker.selectedCharacterController.CheckAccessoryUseColor(currentAccessoryNr);
                var matchHair = PseudoMaker.selectedCharacterController.GetAccessoryColorMatchHair(currentAccessoryNr);
                var isHair = PseudoMaker.selectedCharacterController.GetAccessoryIsHair(currentAccessoryNr);
                var hasLength = PseudoMaker.selectedCharacterController.CheckAccessoryUsesHairLength(currentAccessoryNr);
                var usesGloss = PseudoMaker.selectedCharacterController.GetAccessoryUseGloss(currentAccessoryNr);
                var hasAccessoryPart = PseudoMaker.selectedCharacterController.CheckAccessoryHasAccessoryPart(currentAccessoryNr);

                for (int i = 0; i < useCols.Length; i++)
                    colorRows[i].gameObject.SetActive(useCols[i] && !(matchHair && isHair));
#if KKS
                colorRows[6].SetActive(!(matchHair && isHair) && usesGloss);
#endif
                colorRows[4].SetActive(!(matchHair && isHair));
                colorRows[5].SetActive(isHair && hasAccessoryPart);
                hairAccessorySplitter.SetActive(isHair);
                matchHairColorToggle.SetActive(isHair);
                useHairGlossToggle.SetActive(isHair);
                hairLengthSlider.SetActive(isHair && hasLength);

                colorSplitter.SetActive(useCols.Any(x => x == true && !(matchHair && isHair)));
                transformHeader1.transform.parent.gameObject.SetActive(true);
                tranformRows1.ForEach(x => x.SetActive(transformHeader1.isOn));
                var hasSecondTransform = PseudoMaker.selectedCharacterController.CheckAccessoryUsesSecondTransform(currentAccessoryNr);
                transformHeader2.transform.parent.gameObject.SetActive(hasSecondTransform);
                tranformRows2.ForEach(x => x.SetActive(hasSecondTransform && transformHeader2.isOn));
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
                transformHeader2.transform.parent.gameObject.SetActive(false);
                tranformRows2.ForEach(x => x.SetActive(false));
            }
        }

        public void ChangeSelectedAccessory(int slotNr, bool exists)
        {
            currentAccessoryNr = slotNr;
            currentAccessoryExists = exists;
            currentAccessoryType = (ChaListDefine.CategoryNo)PseudoMaker.selectedCharacterController.GetCurrentAccessoryType(slotNr);
            accessoryPicker.CategoryNo = currentAccessoryType;
            RefreshPanel();
        }

        public void ChangeSelectedAccessory(int slotNr, ChaListDefine.CategoryNo categoryNr)
        {
            ChangeSelectedAccessory(slotNr, categoryNr != ChaListDefine.CategoryNo.ao_none);
        }

        private ColorComponent AddColorRow(string name, int colorNr)
        {
            return AddColorRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetAccessoryColor(currentAccessoryNr, colorNr),
                () => PseudoMaker.selectedCharacterController.GetOriginalAccessoryColor(currentAccessoryNr, colorNr),
                c => PseudoMaker.selectedCharacterController.SetAccessoryColor(currentAccessoryNr, colorNr, c),
                () => PseudoMaker.selectedCharacterController.ResetAcessoryColor(currentAccessoryNr, colorNr)
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
                () => PseudoMaker.selectedCharacterController.GetAccessoryTransform(currentAccessoryNr, correctNo, transform, vector),
                () => PseudoMaker.selectedCharacterController.GetOriginalAccessoryTransform(currentAccessoryNr, correctNo, transform, vector),
                value => PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNo, value, transform, vector),
                () => PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNo, transform, vector),
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
            pickerComponent.GetCurrentValue = () => PseudoMaker.selectedCharacterController.GetCurrentAccessoryId(currentAccessoryNr);
            pickerComponent.SetCurrentValue = (value) =>
            {
                PseudoMaker.selectedCharacterController.SetAccessory(currentAccessoryNr, (int)currentAccessoryType, value.index, "");
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
            var currentXPos = PseudoMaker.selectedCharacterController.GetAccessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.X);
            var currentYRot = PseudoMaker.selectedCharacterController.GetAccessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Y);
            var currentZRot = PseudoMaker.selectedCharacterController.GetAccessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Z);

            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, currentXPos * -1, AccessoryTransform.Location, TransformVector.X);
            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, 360 - currentYRot, AccessoryTransform.Rotation, TransformVector.Y);
            PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNr, 360 - currentZRot, AccessoryTransform.Rotation, TransformVector.Z);
        }

        private void ResetAll(int correctNr)
        {
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.X);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.Y);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Location, TransformVector.Z);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.X);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Y);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Rotation, TransformVector.Z);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Scale, TransformVector.X);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Scale, TransformVector.Y);
            PseudoMaker.selectedCharacterController.ResetAcessoryTransform(currentAccessoryNr, correctNr, AccessoryTransform.Scale, TransformVector.Z);
            RefreshPanel();
        }

        public void RefreshPanel()
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}
