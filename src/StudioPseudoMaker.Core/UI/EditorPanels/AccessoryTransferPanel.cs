using HarmonyLib;
using KKAPI.Maker;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class AccessoryTransferPanel : BaseEditorPanel
    {
        private List<TransferComponent> transferComponents = new List<TransferComponent>();
        private GameObject rowTemplate;

        private ToggleGroup fromToggleGroup;
        private ToggleGroup toToggleGroup;

        private int fromSlotNr;
        private int toSlotNr;

        protected override void Initialize()
        {
            base.Initialize();

            var rectTransform = (RectTransform)transform;

            rowTemplate = Instantiate(TransferRowTemplate, TransferRowTemplate.transform.parent);
            rowTemplate.SetActive(false);

            var go = new GameObject("FromToggleGroup");
            fromToggleGroup = go.AddComponent<ToggleGroup>();
            fromToggleGroup.allowSwitchOff = false;

            go = new GameObject("ToToggleGroup");
            toToggleGroup = go.AddComponent<ToggleGroup>();
            toToggleGroup.allowSwitchOff = false;

            var buttonRow = AddButtonGroupRow(new Dictionary<string, Action>()
            {
                { "Copy", () => {
                    Compatibility.A12TransferAccessoryBefore(toSlotNr);
                    var bytes = MessagePackSerializer.Serialize(PseudoMaker.selectedCharacter.nowCoordinate.accessory.parts[fromSlotNr]);
                    PseudoMaker.selectedCharacter.nowCoordinate.accessory.parts[toSlotNr] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);
                    PseudoMaker.selectedCharacter.AssignCoordinate((ChaFileDefine.CoordinateType)PseudoMaker.selectedCharacter.fileStatus.coordinateType);
                    PseudoMaker.selectedCharacter.Reload(noChangeClothes: false, noChangeHead: true, noChangeHair: true, noChangeBody: true);
                    typeof(AccessoriesApi).GetMethod("OnChangeAcs", AccessTools.all).Invoke(null, new object[] { this, fromSlotNr, toSlotNr });
                    EditTransferRow(transferComponents[toSlotNr], toSlotNr);
                    Compatibility.A12TransferAccessoryAfter();
                }}
            }, transform);
        }

        private void OnEnable()
        {
            int processedEntries = 0;

            for (int i = 0; i < PseudoMaker.selectedCharacter.infoAccessory.Length && i < transferComponents.Count; i++)
            {
                EditTransferRow(transferComponents[i], i);
                transferComponents[i].RefreshText();
                processedEntries++;
            }
            for (int i = processedEntries; i < PseudoMaker.selectedCharacter.infoAccessory.Length; i++)
            {
                transferComponents.Add(AddTransferRow(i));
                processedEntries++;
            }
            for (var i = transferComponents.Count - 1; i >= processedEntries; i--)
            {
                Destroy(transferComponents[i].gameObject);
                transferComponents.RemoveAt(i);
            }
        }

        private TransferComponent AddTransferRow(int slotNr)
        {
            var transferRow = Instantiate(rowTemplate, rowTemplate.transform.parent);
            transferRow.SetActive(true);
            transferRow.name = $"TransferRow{slotNr}";

            var transferRowComponent = transferRow.AddComponent<TransferComponent>();
            transferRowComponent.FromToggleGroup = fromToggleGroup;
            transferRowComponent.ToToggleGroup = toToggleGroup;
            transferRowComponent.fromEnabledAction = value => fromSlotNr = value;
            transferRowComponent.toEnabledAction = value => toSlotNr = value;
            EditTransferRow(transferRowComponent, slotNr );

            return transferRowComponent;
        }

        private void EditTransferRow(TransferComponent transferRow, int slotNr)
        {
            transferRow.SlotNr = slotNr;
            if (PseudoMaker.selectedCharacter.infoAccessory[slotNr] != null)
                transferRow.AccessoryName = PseudoMaker.selectedCharacter.infoAccessory[slotNr].Name;
            else transferRow.AccessoryName = $"Slot {slotNr + 1}";
            transferRow.RefreshText();
        }
    }
}
