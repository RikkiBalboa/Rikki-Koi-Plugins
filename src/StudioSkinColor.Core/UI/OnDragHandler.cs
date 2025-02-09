using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins
{
    public class OnDragHandler : MonoBehaviour, IDragHandler
    {
        public Action<float> UpdateAction = null;

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (UpdateAction == null) return;

            float multiplier = 0f;
            //float delta = eventData.delta.x / Screen.dpi * (Input.GetKey(KeyCode.LeftShift) ? 10f : 1f) / (Input.GetKey(KeyCode.LeftControl) ? 10f : 1f) * (MaterialEditorPluginBase.DragSensitivity.Value / 100f);
            float delta = eventData.delta.x / Screen.dpi * (Input.GetKey(KeyCode.LeftShift) ? 10f : 1f) / (Input.GetKey(KeyCode.LeftControl) ? 10f : 1f) * (30 / 100f);
            UpdateAction(delta);
            //if (float.TryParse(InputField.text, out float input))
            //{
            //    multiplier = delta / input + 1;
            //    InputField.onEndEdit.Invoke((input + delta).ToString());
            //}
            //if (PairedInputFields?.Length > 0 && Input.GetKey(KeyCode.LeftAlt))
            //    foreach (var pairedInputField in PairedInputFields)
            //        if (float.TryParse(pairedInputField.text, out float pairedInput))
            //        {
            //            if (Input.GetKey(KeyCode.Mouse1) && !float.IsInfinity(multiplier) && !float.IsNaN(multiplier))
            //                pairedInputField.onEndEdit.Invoke((pairedInput * multiplier).ToString());
            //            else
            //                pairedInputField.onEndEdit.Invoke((pairedInput + delta).ToString());
            //        }
        }
    }
}
