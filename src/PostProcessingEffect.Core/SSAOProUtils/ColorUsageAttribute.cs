using UnityEngine;

namespace SSAOProUtils
{
    public class ColorUsageAttribute : PropertyAttribute
    {
        public ColorUsageAttribute(bool showAlpha)
        {
        }

        public ColorUsageAttribute(bool showAlpha, bool hdr, float minBrightness, float maxBrightness, float minExposureValue, float maxExposureValue)
        {
        }
    }
}
