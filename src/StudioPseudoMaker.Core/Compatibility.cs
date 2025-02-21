using BepInEx;
using BepInEx.Bootstrap;
using ChaCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PseudoMaker
{
    public static class Compatibility
    {
        public static CvsAccessory CvsAccessory;
        public static int SelectedSlotNr;

        public static bool HasA12 { get; private set; }

        static Compatibility()
        {
            C2AType = Type.GetType("KK_Plugins.ClothesToAccessoriesPlugin, KKS_ClothesToAccessories", throwOnError: false);

            var plugins = Chainloader.PluginInfos.Values.Select(x => x.Instance)
                .Where(plugin => plugin != null)
                .Union(UnityEngine.Object.FindObjectsOfType(typeof(BaseUnityPlugin)).Cast<BaseUnityPlugin>())
                .ToArray();
            foreach (var plugin in plugins)
                switch (plugin.Info.Metadata.GUID)
                {
                    case "starstorm.aaaaaaaaaaaa": HasA12 = true; break;
                }
        }

        #region A12
        public static Dictionary<int, Dictionary<int, string>> GetA12A12CustomAccParents()
        {
            if (!HasA12) return null;

            return GetValue();
            Dictionary<int, Dictionary<int, string>> GetValue()
            {
                return PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().customAccParents;
            }
        }

        public static void A12ChangeAccessoryParent(int slotNr)
        {
            if (!HasA12) return;

            RemoveData();
            void RemoveData()
            {
                var coord = PseudoMaker.selectedCharacter.fileStatus.coordinateType;
                var customAccParents = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().customAccParents;
                if (customAccParents.ContainsKey(coord))
                    customAccParents[coord].Remove(slotNr);
            }
        }

        public static string A12GetBoneName(int slotNr)
        {
            if (!HasA12) return "";

            return A12GetBoneTransform(slotNr)?.name ?? "Unknown";
        }

        public static Transform A12GetBoneTransform(int slotNr)
        {
            if (!HasA12) return null;

            return GetBone();
            Transform GetBone()
            {
                var coord = PseudoMaker.selectedCharacter.fileStatus.coordinateType;
                var customAccParents = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().customAccParents;
                var dicHashBones = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>().dicHashBones;
                if (customAccParents.ContainsKey(coord))
                    return dicHashBones[customAccParents[coord][slotNr]].bone;
                return null;
            }
        }

        public static void A12RegisterParent(int slotNr)
        {
            if (!HasA12) return;

            RegisterBone();
            void RegisterBone()
            {
                Transform selected = KKABMX.GUI.KKABMX_AdvancedGUI._selectedTransform.Value;
                if (selected == null)
                {
                    PseudoMaker.Logger.LogMessage("[AAAAAAAAAAAA] Please select a bone in ABMX!");
                    return;
                }
                var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();
                var accTransform = PseudoMaker.selectedCharacter.objAccessory[slotNr]?.transform;

                if (accTransform != null && controller.dicTfBones.TryGetValue(accTransform, out var accBone) && controller.dicTfBones.TryGetValue(selected, out var parentBone))
                {
                    // Make sure not to parent anything to itself or its children
                    if (parentBone.IsChildOf(accBone))
                    {
                        PseudoMaker.Logger.LogMessage("[AAAAAAAAAAAA] Can't parent accessory to itself or its children!");
                        return;
                    }

                    accBone.SetParent(parentBone);
                    accBone.PerformBoneUpdate();

                    var coord = PseudoMaker.selectedCharacter.fileStatus.coordinateType;
                    // Save parentage to dictionary
                    if (!controller.customAccParents.ContainsKey(coord))
                    {
                        controller.customAccParents[coord] = new Dictionary<int, string>();
                    }
                    controller.customAccParents[coord][slotNr] = parentBone.Hash;
                }
            }
        }

        public static void A12TransferAccessoryBefore(int toSlotnNr)
        {
            if (!HasA12) return;

            TransferAccessory();
            void TransferAccessory()
            {
                var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();

                if (
                    controller.customAccParents.TryGetValue(PseudoMaker.selectedCharacter.fileStatus.coordinateType, out var dicCoord)
                    && AAAAAAAAAAAA.AAAAAAAAAAAA.TryGetStudioAccBone(controller, toSlotnNr, out var accBone)
                )
                {
                    var toDelete = new List<int>();
                    foreach (var hash in dicCoord)
                        if (controller.dicHashBones.TryGetValue(hash.Value, out var bone) && bone.IsChildOf(accBone))
                            toDelete.Add(hash.Key);
                    foreach(var key in toDelete)
                        dicCoord.Remove(key);
                }
            }
        }

        public static void A12TransferAccessoryAfter()
        {
            if (!HasA12) return;

            TransferAccessory();
            void TransferAccessory()
            {
                var controller = PseudoMaker.selectedCharacter.GetComponent<AAAAAAAAAAAA.CardDataController>();

                controller.LoadData();
            }
        }
        #endregion

        #region C2AA
        public static Type C2AType;

        public static bool CheckC2AInstalled()
        {
            return C2AType != null;
        }
        #endregion
    }
}
