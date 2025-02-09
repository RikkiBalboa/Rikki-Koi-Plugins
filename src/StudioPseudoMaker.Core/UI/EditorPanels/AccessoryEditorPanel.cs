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
    }
}
