using ChaCustom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;

namespace Plugins
{
    public static class Compatibility
    {
        public static CvsAccessory CvsAccessory;
        public static int SelectedSlotNr;

        static Compatibility()
        {
            CvsAccessory = new CvsAccessory();

            A12ControllerType = Type.GetType("AAAAAAAAAAAA.CardDataController, AAAAAAAAAAAA.KoikatsuSunshine", throwOnError: false);
            if (A12ControllerType != null)
                A12CustomAccParentsField = A12ControllerType.GetField("customAccParents", AccessTools.all);
            //A12HookPatchType = Type.GetType("AAAAAAAAAAAA.HookPatch+Maker, AAAAAAAAAAAA.KoikatsuSunshine", throwOnError: false);
            //if (A12HookPatchType != null)
            //{
            //    A12CvsAccessoryAfterUpdateSelectAccessoryParent = A12HookPatchType.GetMethod("CvsAccessoryAfterUpdateSelectAccessoryParent", AccessTools.all);
            //    A12MakerLoader = A12HookPatchType.GetField("makerLoaded", AccessTools.all);
            //    A12MakerLoader?.SetValue(null, true);
            //}

            C2AType = Type.GetType("KK_Plugins.ClothesToAccessoriesPlugin, KKS_ClothesToAccessories", throwOnError: false);
        }

        #region A12
        public static Type A12ControllerType;
        public static Type A12HookPatchType;
        public static FieldInfo A12CustomAccParentsField;
        public static FieldInfo A12MakerLoader;
        public static MethodInfo A12CvsAccessoryAfterUpdateSelectAccessoryParent;

        public static bool CheckA12Installed()
        {
            return A12ControllerType != null;
        }

        public static Dictionary<int, Dictionary<int, string>> GetA12A12CustomAccParents()
        {
            if (A12ControllerType == null) return null;

            return A12CustomAccParentsField.GetValue(PseudoMaker.selectedCharacter.GetComponent(A12ControllerType)) as Dictionary<int, Dictionary<int, string>>;
        }

        //public static void A12ChangeAccessoryParent()
        //{
        //    PseudoMaker.Logger.LogInfo($"Invoking A12 Parent Change: {A12CvsAccessoryAfterUpdateSelectAccessoryParent != null}");
        //    A12CvsAccessoryAfterUpdateSelectAccessoryParent?.Invoke(null, new object[] { CvsAccessory, (int)1 });
        //}
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
