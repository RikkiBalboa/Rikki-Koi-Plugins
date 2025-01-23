using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Utilities;
using Studio;
using System.Xml;
using UnityEngine;
using static Plugins.StudioSkinColor;

namespace Plugins
{
    internal static class TimelineCompatibilityHelper
    {
        internal static void PopulateTimeline()
        {
            if (!TimelineCompatibility.IsTimelineAvailable()) return;

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "mainSkin",
                name: "Main Skin",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateTextureColor(Color.LerpUnclamped(leftValue, rightValue, factor), TextureColor.SkinMain),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileBody.skinMainColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "subSkin",
                name: "Sub Skin",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateTextureColor(Color.LerpUnclamped(leftValue, rightValue, factor), TextureColor.SkinSub),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileBody.skinSubColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "tan",
                name: "Tan Color",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateTextureColor(Color.LerpUnclamped(leftValue, rightValue, factor), TextureColor.Tan),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileBody.sunburnColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "bustSoftness",
                name: "Bust Softness",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateBustSoftness(Mathf.LerpUnclamped(leftValue, rightValue, factor), Bust.Softness),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileBody.bustSoftness,
                readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "bustWeight",
                name: "Bust Weight",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateBustSoftness(Mathf.LerpUnclamped(leftValue, rightValue, factor), Bust.Weight),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileBody.bustWeight,
                readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "hairColor1",
                name: "Hair Color 1",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateHairColor(Color.LerpUnclamped(leftValue, rightValue, factor), HairColor.Base),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileHair.parts[0].baseColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "hairColor2",
                name: "Hair Color 2",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateHairColor(Color.LerpUnclamped(leftValue, rightValue, factor), HairColor.Start),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileHair.parts[0].startColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "hairColor3",
                name: "Hair Color 3",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateHairColor(Color.LerpUnclamped(leftValue, rightValue, factor), HairColor.End),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileHair.parts[0].endColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

#if KKS
            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "hairGloss",
                name: "Hair Gloss Color",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateHairColor(Color.LerpUnclamped(leftValue, rightValue, factor), HairColor.Gloss),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileHair.parts[0].glossColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );
#endif

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "eyebrow",
                name: "Eyebrow Color",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateHairColor(Color.LerpUnclamped(leftValue, rightValue, factor), HairColor.Eyebrow),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileFace.eyebrowColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );
        }

        private static void WriteColorXML(XmlTextWriter writer, Color value)
        {
            writer.WriteAttributeString("R", XmlConvert.ToString(value.r));
            writer.WriteAttributeString("G", XmlConvert.ToString(value.g));
            writer.WriteAttributeString("B", XmlConvert.ToString(value.b));
            writer.WriteAttributeString("A", XmlConvert.ToString(value.a));
        }

        private static Color ReadColorXML(XmlNode node)
        {
            return new Color(
                XmlConvert.ToSingle(node.Attributes["R"].Value),
                XmlConvert.ToSingle(node.Attributes["G"].Value),
                XmlConvert.ToSingle(node.Attributes["B"].Value),
                XmlConvert.ToSingle(node.Attributes["A"].Value)
            );
        }

        private static StudioSkinColorCharaController GetParameter(ObjectCtrlInfo oci)
        {
            return StudioSkinColorCharaController.GetController(((OCIChar)oci).GetChaControl());
        }
    }
}
