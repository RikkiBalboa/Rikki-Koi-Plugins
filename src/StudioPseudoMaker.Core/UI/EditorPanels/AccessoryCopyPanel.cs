using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using KKAPI.Maker;
using MessagePack;
using UnityEngine;

namespace PseudoMaker.UI
{
    public class AccessoryCopyPanel : BaseEditorPanel
    {
        private int fromSelected;
        private int toSelected;
        
        private DropdownComponent fromDropDown;
        private DropdownComponent toDropDown;
        
        private Dictionary<int, CopyComponent> _copyComponents = new Dictionary<int, CopyComponent>();
        
        private GameObject copyRowTemplate;

        private bool _initialized;
        
        protected override void Initialize()
        {
            base.Initialize();

            copyRowTemplate = Instantiate(CopyRowTemplate, CopyRowTemplate.transform.parent);
            copyRowTemplate.SetActive(false);
            
            fromDropDown = AddDropdownRow(
                "Source Outfit",
                PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList(),
                () => fromSelected,
                value => { 
                    fromSelected = value;
                    _copyComponents.Values.ToList().ForEach(c => c.Refresh());
                },
                transform
            );
            fromDropDown.transform.SetAsFirstSibling();
            toDropDown = AddDropdownRow(
                "Target Outfit",
                PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) => KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList(),
                () => toSelected,
                value => {
                    toSelected = value;
                    _copyComponents.Values.ToList().ForEach(c => c.Refresh());
                },
                transform
            );
            toDropDown.transform.SetSiblingIndex(1);
            
            
            AddButtonGroupRow(new Dictionary<string, Action>()
            {
                { "Toggle  All", () => _copyComponents.Values.ToList().ForEach(c => c.Toggled = true) },
                { "Toggle  None", () => _copyComponents.Values.ToList().ForEach(c => c.Toggled = false) },
                { "Copy", CopyMethod}
            }, transform);
            
            _initialized = true;
        }

        private void OnEnable()
        {
            RefreshRows();
            RefreshDropdowns();
        }

        internal void RefreshRows()
        {
            if (!_initialized || !isActiveAndEnabled) return;
            if (PseudoMaker.selectedCharacter.infoAccessory.Length != _copyComponents.Count)
            {
                Dictionary<int, bool> toggleStates = new Dictionary<int, bool>();
                foreach (KeyValuePair<int,CopyComponent> kvp in _copyComponents)
                {
                    toggleStates.Add(kvp.Key, kvp.Value.Toggled);
                    Destroy(kvp.Value.gameObject);
                }
                _copyComponents.Clear();
                
                for (var i = 0; i < PseudoMaker.selectedCharacter.infoAccessory.Length; i++)
                {
                    int slotNum = i;
                    _copyComponents.Add(slotNum, AddCopyRow($"Slot {slotNum+1}", () =>
                    {
                        ChaFileAccessory fromAccessory = PseudoMaker.selectedCharacter.chaFile.coordinate[fromSelected].accessory;
                        if (slotNum >= fromAccessory.parts.Length) return "None";
                        ListInfoBase listInfoFrom = PseudoMaker.selectedCharacter.lstCtrl.GetListInfo((ChaListDefine.CategoryNo)fromAccessory.parts[slotNum].type, fromAccessory.parts[slotNum].id);
                        return listInfoFrom != null ? listInfoFrom.Name : "None";
                    }, () =>
                    {
                        ChaFileAccessory toAccessory = PseudoMaker.selectedCharacter.chaFile.coordinate[toSelected].accessory;
                        if (slotNum >= toAccessory.parts.Length) return "None";
                        ListInfoBase listInfoTo = PseudoMaker.selectedCharacter.lstCtrl.GetListInfo((ChaListDefine.CategoryNo)toAccessory.parts[slotNum].type, toAccessory.parts[slotNum].id);
                        return listInfoTo != null ? listInfoTo.Name : "None";
                    }, toggleStates.TryGetValue(i, out bool toggled) && toggled, copyRowTemplate));
                }
            }
            _copyComponents.Values.ToList().ForEach(c => c.Refresh());
        }

        public void RefreshDropdowns()
        {
            if (!fromDropDown || !toDropDown) return;
            List<string> options = PseudoMaker.selectedCharacter.chaFile.coordinate.Select((coordinate, index) =>
                KK_Plugins.MoreOutfits.Plugin.GetCoodinateName(PseudoMaker.selectedCharacter, index)).ToList();
            fromDropDown.SetDropdownOptions(options);
            toDropDown.SetDropdownOptions(options);
        }
        

        private void CopyMethod()
        {
            ChaFileAccessory toAccessory = PseudoMaker.selectedCharacter.chaFile.coordinate[toSelected].accessory;
            ChaFileAccessory fromAccessory = PseudoMaker.selectedCharacter.chaFile.coordinate[fromSelected].accessory;
            var copiedSlots = new List<int>();
            for (int i = 0; i < PseudoMaker.selectedCharacter.infoAccessory.Length; i++)
            {
                if (_copyComponents[i].Toggled)
                {
                    if (toAccessory.parts.Length <= i)
                    {
                        PseudoMaker.selectedCharacterController.AddAccessorySlot((i+1) - toAccessory.parts.Length, toSelected);
                    }
                    byte[] array = MessagePackSerializer.Serialize(fromAccessory.parts[i]);
                    toAccessory.parts[i] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(array);
                    copiedSlots.Add(i);
                }
            }
            PseudoMaker.selectedCharacter.ChangeCoordinateType(true);
            PseudoMaker.selectedCharacter.Reload(false, true, true, true);
            Studio.Studio.instance?.manipulatePanelCtrl?.charaPanelInfo.mpCharCtrl.UpdateInfo();
            
            // trigger KKAPI event
            
            FieldInfo eventInfo = typeof(AccessoriesApi).GetField(nameof(AccessoriesApi.AccessoriesCopied), BindingFlags.NonPublic | BindingFlags.Static);
            if (eventInfo == null) return;
            object eventValue = eventInfo.GetValue(null);
            AccessoryCopyEventArgs args = new AccessoryCopyEventArgs(
                from kvp in _copyComponents where kvp.Value.Toggled select kvp.Key,
                (ChaFileDefine.CoordinateType)fromSelected,
                (ChaFileDefine.CoordinateType)toSelected
            );
            eventValue?.GetType().GetMethod("Invoke")?.Invoke(eventValue, new object[] { this, args });
            
            // TODO: someone rework A12
            // Compatibility.A12.CopyAccessoryAfter(fromSelected, toSelected, copiedSlots);
        }
    }
}