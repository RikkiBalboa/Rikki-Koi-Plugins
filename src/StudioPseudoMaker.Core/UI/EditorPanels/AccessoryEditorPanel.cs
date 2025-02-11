using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class AccessoryEditorPanel : BaseEditorPanel
    {
        private static Vector3[] CopiedTransforms;
        private int currentAccessoryNr;
        private bool currentAccessoryExists;

        private List<GameObject> colorRows;
        private GameObject colorSplitter;
        private Toggle transformHeader1;
        private List<GameObject> tranformRows1;
        private Toggle transformHeader2;
        private List<GameObject> tranformRows2;

        protected override void Initialize()
        {
            base.Initialize();

            colorRows = new List<GameObject>() {
                AddColorRow("Color 1", 0).gameObject,
                AddColorRow("Color 2", 1).gameObject,
                AddColorRow("Color 3", 2).gameObject,
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

            var transform2Splitter =

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
        }

        private void OnEnable()
        {
            if (currentAccessoryExists)
            {
                var useCols = PseudoMaker.selectedCharacterController.CheckAccessoryUseColor(currentAccessoryNr);

                for (int i = 0; i < useCols.Length; i++)
                    colorRows[i].gameObject.SetActive(useCols[i]);
                colorSplitter.SetActive(useCols.Any(x => x == true));
                transformHeader1.gameObject.SetActive(true);
                tranformRows1.ForEach(x => x.SetActive(transformHeader1.isOn));
                var hasSecondTransform = PseudoMaker.selectedCharacterController.CheckAccessoryUsesSecondTransform(currentAccessoryNr);
                transformHeader2.gameObject.SetActive(hasSecondTransform);
                tranformRows2.ForEach(x => x.SetActive(hasSecondTransform && transformHeader2.isOn));
            }
            else
            {
                colorRows.ForEach(x => x.SetActive(false));
                colorSplitter.SetActive(false);
                transformHeader1.gameObject.SetActive(false);
                tranformRows1.ForEach(x => x.SetActive(false));
                transformHeader2.gameObject.SetActive(false);
                tranformRows2.ForEach(x => x.SetActive(false));
            }
        }

        public void ChangeSelectedAccessory(int slotNr, bool exists)
        {
            currentAccessoryNr = slotNr;
            currentAccessoryExists = exists;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
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

            gameObject.SetActive(false);
            gameObject.SetActive(true);
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

            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}
