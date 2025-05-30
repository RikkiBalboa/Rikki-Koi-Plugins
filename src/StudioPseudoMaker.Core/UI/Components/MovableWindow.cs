﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PseudoMaker.UI
{
    internal class MovableWindow : UIBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static MovableWindow MakeObjectDraggable(RectTransform clickableDragZone, RectTransform draggableObject, bool preventCameraControl = true)
        {
            MovableWindow mv = clickableDragZone.gameObject.AddComponent<MovableWindow>();
            mv.toDrag = draggableObject;
            mv.preventCameraControl = preventCameraControl;
            return mv;
        }

        private Vector2 _cachedDragPosition;
        private Vector2 _cachedMousePosition;
        private bool _pointerDownCalled;
        private BaseCameraControl _cameraControl;
        private BaseCameraControl.NoCtrlFunc _noControlFunctionCached;

        public event Action<PointerEventData> onPointerDown;
        public event Action<PointerEventData> onDrag;
        public event Action<PointerEventData> onPointerUp;

        public RectTransform toDrag;
        public bool preventCameraControl;

        public override void Awake()
        {
            base.Awake();
            _cameraControl = FindObjectOfType<BaseCameraControl>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (preventCameraControl && _cameraControl)
            {
                _noControlFunctionCached = _cameraControl.NoCtrlCondition;
                _cameraControl.NoCtrlCondition = () => true;
            }
            _pointerDownCalled = true;
            _cachedDragPosition = toDrag.position;
            _cachedMousePosition = Input.mousePosition;
            onPointerDown?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_pointerDownCalled == false)
                return;
            toDrag.position = _cachedDragPosition + ((Vector2)Input.mousePosition - _cachedMousePosition);
            onDrag?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerDownCalled == false)
                return;
            if (preventCameraControl && _cameraControl)
                _cameraControl.NoCtrlCondition = _noControlFunctionCached;
            _pointerDownCalled = false;
            onPointerUp?.Invoke(eventData);
        }
    }
}