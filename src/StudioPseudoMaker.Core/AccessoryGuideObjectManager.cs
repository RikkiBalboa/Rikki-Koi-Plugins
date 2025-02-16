using KKAPI.Studio;
using Studio;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Plugins
{
    public static class AccessoryGuideObjectManager
    {
        private static GuideObject guideObject;

        public static void CreateGuideObject(int slotNr, int correctNr, Action onChangeAction = null)
        {
            if (guideObject != null)
                GuideObjectManager.Instance.Delete(guideObject);
            var target = PseudoMaker.selectedCharacterController.GetAccessoryTransform(slotNr, correctNr);
            if (target == null) return;

            guideObject = GuideObjectManager.Instance.Add(target, 7000 + correctNr);
            guideObject.mode = GuideObject.Mode.Local;
            guideObject.enablePos = true;
            guideObject.enableRot = true;
            guideObject.enableScale = true;
            guideObject.enableMaluti = false;
            guideObject.calcScale = false;
            guideObject.scaleRate = 0.5f;
            guideObject.scaleRot = 0.025f;
            guideObject.scaleSelect = 0.05f;
            guideObject.parentGuide = PseudoMaker.selectedCharacter.GetOCIChar().guideObject;
            guideObject.SetActive(true);

            guideObject.changeAmount = new ChangeAmount();
            guideObject.changeAmount.pos = target.localPosition;
            guideObject.changeAmount.rot = target.localEulerAngles;
            guideObject.changeAmount.scale = target.localScale;
            ChangeAmount changeAmount = guideObject.changeAmount;
            changeAmount.onChangePos += () =>
            {
                PseudoMaker.selectedCharacterController.SetAccessoryTransform(slotNr, correctNr, changeAmount.pos * 100, AccessoryTransform.Location);
                onChangeAction?.Invoke();
            };
            changeAmount.onChangeRot += () =>
            {
                PseudoMaker.selectedCharacterController.SetAccessoryTransform(slotNr, correctNr, changeAmount.rot, AccessoryTransform.Rotation);
                onChangeAction?.Invoke();
            };
            changeAmount.onChangeScale += scale =>
            {
                PseudoMaker.selectedCharacterController.SetAccessoryTransform(slotNr, correctNr, scale, AccessoryTransform.Scale);
                onChangeAction?.Invoke();
            };
            GuideObjectManager.Instance.selectObject = guideObject;
        }

        public static void DestroyGuideObject()
        {
            if (guideObject != null)
                GuideObjectManager.Instance.Delete(guideObject);
        }
    }
}
