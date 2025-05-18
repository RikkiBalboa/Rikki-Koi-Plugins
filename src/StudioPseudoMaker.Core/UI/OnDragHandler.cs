using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PseudoMaker.UI
{
    public class OnDragHandler : MonoBehaviour, IDragHandler
    {
        public Action<float> UpdateAction = null;
        public Action<float> PairedUpdateAction = null;

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (UpdateAction == null) return;

            //float delta = eventData.delta.x / Screen.dpi * (Input.GetKey(KeyCode.LeftShift) ? 10f : 1f) / (Input.GetKey(KeyCode.LeftControl) ? 10f : 1f) * (MaterialEditorPluginBase.DragSensitivity.Value / 100f);
            float delta = eventData.delta.x / Screen.dpi * (Input.GetKey(KeyCode.LeftShift) ? 10f : 1f) / (Input.GetKey(KeyCode.LeftControl) ? 10f : 1f) * (30 / 100f);
            UpdateAction(delta);

            if (PairedUpdateAction != null && Input.GetKey(KeyCode.LeftAlt))
                PairedUpdateAction(delta);
        }
    }
}
