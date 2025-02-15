using BepInEx;
using BepInEx.Bootstrap;
using ChaCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plugins
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
