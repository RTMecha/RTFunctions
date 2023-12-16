﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;

namespace RTFunctions.Functions.Components
{
    public class Clickable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> onClick;
        public Action<PointerEventData> onDown;
        public Action<PointerEventData> onEnter;
        public Action<PointerEventData> onExit;
        public Action<PointerEventData> onUp;

        public Action<PointerEventData> onBeginDrag;
        public Action<PointerEventData> onDrag;
        public Action<PointerEventData> onEndDrag;

        public void OnBeginDrag(PointerEventData pointerEventData) => onBeginDrag?.Invoke(pointerEventData);
        public void OnDrag(PointerEventData pointerEventData) => onDrag?.Invoke(pointerEventData);
        public void OnEndDrag(PointerEventData pointerEventData) => onEndDrag?.Invoke(pointerEventData);

        public void OnPointerClick(PointerEventData pointerEventData) => onClick?.Invoke(pointerEventData);

        public void OnPointerDown(PointerEventData pointerEventData) => onDown?.Invoke(pointerEventData);

        public void OnPointerEnter(PointerEventData pointerEventData) => onEnter?.Invoke(pointerEventData);

        public void OnPointerExit(PointerEventData pointerEventData) => onExit?.Invoke(pointerEventData);

        public void OnPointerUp(PointerEventData pointerEventData) => onUp?.Invoke(pointerEventData);
    }
}
