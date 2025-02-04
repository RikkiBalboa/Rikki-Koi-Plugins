using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins
{
    public class SubCategoryEditorPanel : MonoBehaviour
    {
        public SubCategory SubCategory;

        public ScrollRect scrollRect;

        public GameObject SliderTemplate;
        public GameObject ColorTemplate;
        public GameObject PickerTemplate;

        public void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();

            SliderTemplate = scrollRect.content.Find("SliderTemplate").gameObject;
            ColorTemplate = scrollRect.content.Find("ColorTemplate").gameObject;
            PickerTemplate = scrollRect.content.Find("PickerTemplate").gameObject;



            SliderTemplate.SetActive(false);
            ColorTemplate.SetActive(false);
            PickerTemplate.SetActive(false);
        }
    }
}
