using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Plugins
{
    public class AccessoryEditorPanel : BaseEditorPanel
    {
        private int currentAccessoryNr;

        protected override void Initialize()
        {
            base.Initialize();

            AddColorRow("Color 1", 0);
            AddColorRow("Color 2", 1);
            AddColorRow("Color 3", 2);
            AddColorRow("Accessory Color", ColorType.CheekColor);
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
