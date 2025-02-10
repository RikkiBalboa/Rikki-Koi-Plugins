using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Plugins
{
    public class AccessoryEditorPanel : BaseEditorPanel
    {
        private int currentAccessoryNr;

        private ColorComponent color1;
        private ColorComponent color2;
        private ColorComponent color3;
        private ColorComponent colorAccessory;

        protected override void Initialize()
        {
            base.Initialize();

            color1 = AddColorRow("Color 1", 0);
            color2 = AddColorRow("Color 2", 1);
            color3 = AddColorRow("Color 3", 2);

            AddSplitter();

            var input = AddInputRow("X Location", 0, AccessoryTransform.Location, TransformVector.X);
            input.IncrementValue *= -1;
            AddInputRow("Y Location", 0, AccessoryTransform.Location, TransformVector.Y);
            AddInputRow("Z Location", 0, AccessoryTransform.Location, TransformVector.Z);
            AddSplitter();

            AddInputRow("X Rotation", 0, AccessoryTransform.Rotation, TransformVector.X);
            AddInputRow("Y Rotation", 0, AccessoryTransform.Rotation, TransformVector.Y);
            AddInputRow("Z Rotation", 0, AccessoryTransform.Rotation, TransformVector.Z);
            AddSplitter();

            AddInputRow("X Scale", 0, AccessoryTransform.Scale, TransformVector.X);
            AddInputRow("Y Scale", 0, AccessoryTransform.Scale, TransformVector.Y);
            AddInputRow("Z Scale", 0, AccessoryTransform.Scale, TransformVector.Z);
        }

        private void OnEnable()
        {
            var useCols = PseudoMaker.selectedCharacterController.CheckAccessoryUseColor(currentAccessoryNr);
            color1.gameObject.SetActive(useCols[0]);
            color2.gameObject.SetActive(useCols[1]);
            color3.gameObject.SetActive(useCols[2]);
        }

        public void ChangeSelectedAccessory(int slotNr)
        {
            currentAccessoryNr = slotNr;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        private ColorComponent AddColorRow(string name, int colorNr)
        {
            return AddColorRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetAccessoryColor(currentAccessoryNr, colorNr),
                () => Color.white,
                c => PseudoMaker.selectedCharacterController.SetAccessoryColor(currentAccessoryNr, colorNr, c),
                () => { }
            );
        }

        private InputFieldComponent AddInputRow(string name, int correctNo, AccessoryTransform transform, TransformVector vector)
        {
            float minValue = -100;
            float maxValue = 100;
            float incrementValue = 0.1f;
            bool repeat = false;
            bool isInt = false;
            if (transform == AccessoryTransform.Rotation)
            {
                minValue = -360;
                maxValue = 360;
                incrementValue = 1f;
                repeat = true;
                isInt = true;
            }
            else if (transform == AccessoryTransform.Scale)
            {
                minValue = 0.01f;
                maxValue = 100;
            }

            var input = AddInputRow(
                name,
                () => PseudoMaker.selectedCharacterController.GetAccessoryTransform(currentAccessoryNr, correctNo, transform, vector),
                () => 0,
                value => PseudoMaker.selectedCharacterController.SetAccessoryTransform(currentAccessoryNr, correctNo, value, transform, vector),
                () => { },
                minValue,
                maxValue
            );
            input.Repeat = repeat;
            input.IncrementValue = incrementValue;
            input.IsInt = isInt;
            return input;
        }
    }
}
