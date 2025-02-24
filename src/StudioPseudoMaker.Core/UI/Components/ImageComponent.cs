using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PseudoMaker.UI
{
    public class ImageComponent : MonoBehaviour
    {
        private RawImage image;
        public Func<Texture> GetCurrentValue;

        private void Awake()
        {
            image = GetComponentInChildren<RawImage>(true);
        }

        private void OnEnable()
        {
            if (GetCurrentValue != null)
            {
                image.texture = GetCurrentValue();
            }
        }
    }
}
