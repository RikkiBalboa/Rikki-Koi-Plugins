using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Utilities;
using Studio;
using System;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using static RootMotion.FinalIK.GrounderQuadruped;

namespace Plugins
{
    internal static class TimelineCompatibilityHelper
    {
        internal static void PopulateTimeline()
        {
            if (!TimelineCompatibility.IsTimelineAvailable()) return;

            #region Legacy StudioSkinColor
            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "StudioSkinColor",
                id: "mainSkin",
                name: "Main Skin",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.SkinMain),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.SkinSub),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.SkinTan),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.SetFloatTypeValue(Mathf.LerpUnclamped(leftValue, rightValue, factor), FloatType.BustSoftness),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.SetFloatTypeValue(Mathf.LerpUnclamped(leftValue, rightValue, factor), FloatType.BustWeight),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.HairBase),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.HairStart),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.HairEnd),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.HairGloss),
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
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), ColorType.Eyebrow),
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).GetChaControl().fileFace.eyebrowColor,
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: GetParameter,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );
            #endregion

            foreach (var category in UIMappings.ShapeBodyValueMap)
                foreach (var shape in category.Value)
                    TimelineCompatibility.AddInterpolableModelDynamic(
                        owner: "Pseudo Maker (Body Shapes)",
                        id: $"{category.Key}-{shape.Key}",
                        name: $"{AddSpacesToSentence(category.Key)} - {AddSpacesToSentence(shape.Value)}",
                        interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateBodyShapeValue(shape.Key, Mathf.LerpUnclamped(leftValue, rightValue, factor)),
                        interpolateAfter: null,
                        getValue: (oci, parameter) => parameter.GetCurrentBodyValue(shape.Key),
                        readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                        writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                        getParameter: GetParameter,
                        isCompatibleWithTarget: oci => oci is OCIChar,
                        checkIntegrity: null
                    );

            foreach (var category in UIMappings.ShapeBodyValueMap)
                foreach (var shape in category.Value)
                    TimelineCompatibility.AddInterpolableModelDynamic(
                        owner: "Pseudo Maker (Face Shapes)",
                        id: $"{category.Key}-{shape.Key}",
                        name: $"Face {AddSpacesToSentence(category.Key)} - {AddSpacesToSentence(shape.Value)}",
                        interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateBodyShapeValue(shape.Key, Mathf.LerpUnclamped(leftValue, rightValue, factor)),
                        interpolateAfter: null,
                        getValue: (oci, parameter) => parameter.GetCurrentBodyValue(shape.Key),
                        readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                        writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                        getParameter: GetParameter,
                        isCompatibleWithTarget: oci => oci is OCIChar,
                    checkIntegrity: null
                    );

            foreach (var floatType in Enum.GetValues(typeof(FloatType)).Cast<FloatType>())
                TimelineCompatibility.AddInterpolableModelDynamic(
                    owner: "Pseudo Maker (Float Values)",
                    id: floatType.ToString(),
                    name: AddSpacesToSentence(floatType),
                    interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.SetFloatTypeValue(Mathf.LerpUnclamped(leftValue, rightValue, factor), floatType),
                    interpolateAfter: null,
                    getValue: (oci, parameter) => parameter.GetFloatValue(floatType),
                    readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                    writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                    getParameter: GetParameter,
                    isCompatibleWithTarget: oci => oci is OCIChar,
                    checkIntegrity: null
                );

            foreach (var colorType in Enum.GetValues(typeof(ColorType)).Cast<ColorType>())
                TimelineCompatibility.AddInterpolableModelDynamic(
                    owner: "Pseudo Maker (Color Values)",
                    id: colorType.ToString(),
                    name: AddSpacesToSentence(colorType),
                    interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), colorType),
                    interpolateAfter: null,
                    getValue: (oci, parameter) => parameter.GetColorPropertyValue(colorType),
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


        private static string AddSpacesToSentence(Enum text)
        {
            return AddSpacesToSentence(text.ToString());
        }
        private static string AddSpacesToSentence(string text)
        {
            if (text.IsNullOrWhiteSpace())
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        private static PseudoMakerCharaController GetParameter(ObjectCtrlInfo oci)
        {
            return PseudoMakerCharaController.GetController(((OCIChar)oci).GetChaControl());
        }
    }
}
