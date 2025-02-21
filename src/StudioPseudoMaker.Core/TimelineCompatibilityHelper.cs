using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Utilities;
using Studio;
using System;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace Plugins
{
    internal static class TimelineCompatibilityHelper
    {
        private static FloatType? _selectedFloatType;
        public static FloatType? SelectedFloatType
        {
            get {  return _selectedFloatType; }
            set { 
                _selectedFloatType = value;

                _selectedColorType = null;
                _selectedBodyShape = null;
                selectedFaceShape = null;

                TimelineCompatibility.RefreshInterpolablesList();
            }
        }

        private static ColorType? _selectedColorType;
        public static ColorType? SelectedColorType
        {
            get { return _selectedColorType; }
            set
            {
                _selectedColorType = value;

                _selectedFloatType = null;
                _selectedBodyShape = null;
                selectedFaceShape = null;

                TimelineCompatibility.RefreshInterpolablesList();
            }
        }

        private static int? _selectedBodyShape;
        public static int? SelectedBodyShape
        {
            get { return _selectedBodyShape; }
            set
            {
                _selectedBodyShape = value;

                _selectedFloatType = null;
                _selectedColorType = null;
                selectedFaceShape = null;

                TimelineCompatibility.RefreshInterpolablesList();
            }
        }

        private static int? selectedFaceShape;
        public static int? SelectedFaceShape
        {
            get { return selectedFaceShape; }
            set
            {
                selectedFaceShape = value;

                _selectedFloatType = null;
                _selectedColorType = null;
                _selectedBodyShape = null;

                TimelineCompatibility.RefreshInterpolablesList();
            }
        }

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

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "Pseudo Maker",
                id: "BodyShapeValue",
                name: "Body Shape Value",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Controller.UpdateBodyShapeValue(parameter.Type, Mathf.LerpUnclamped(leftValue, rightValue, factor)),
                interpolateAfter: null,
                getValue: (oci, parameter) => parameter.Controller.GetCurrentBodyValue(parameter.Type),
                readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                getParameter: oci => new SimpleParameter<int>(oci, (int)SelectedBodyShape),
                isCompatibleWithTarget: oci => oci is OCIChar && SelectedBodyShape != null,
                readParameterFromXml: SimpleParameter<int>.ReadXml,
                writeParameterToXml: (oci, writer, parameter) => parameter.WriteToXml(writer),
                checkIntegrity: null,
                getFinalName: (currentName, oci, parameter) => UIMappings.ShapeBodyValueMap.First(x => x.Value.ContainsKey(parameter.Type)).Value[parameter.Type]
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "Pseudo Maker",
                id: "FaceShapeValue",
                name: "Face Shape Value",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Controller.UpdateFaceShapeValue(parameter.Type, Mathf.LerpUnclamped(leftValue, rightValue, factor)),
                interpolateAfter: null,
                getValue: (oci, parameter) => parameter.Controller.GetCurrentFaceValue(parameter.Type),
                readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                getParameter: oci => new SimpleParameter<int>(oci, (int)SelectedFaceShape),
                isCompatibleWithTarget: oci => oci is OCIChar && SelectedFaceShape != null,
                readParameterFromXml: SimpleParameter<int>.ReadXml,
                writeParameterToXml: (oci, writer, parameter) => parameter.WriteToXml(writer),
                checkIntegrity: null,
                getFinalName: (currentName, oci, parameter) => UIMappings.ShapeFaceValueMap.First(x => x.Value.ContainsKey(parameter.Type)).Value[parameter.Type]
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "Pseudo Maker",
                id: "FloatValue",
                name: "Float Value",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Controller.SetFloatTypeValue(Mathf.LerpUnclamped(leftValue, rightValue, factor), parameter.Type),
                interpolateAfter: null,
                getValue: (oci, parameter) => parameter.Controller.GetFloatValue(parameter.Type),
                readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", value.ToString()),
                getParameter: oci => new SimpleParameter<FloatType>(oci, (FloatType)SelectedFloatType),
                isCompatibleWithTarget: oci => oci is OCIChar && SelectedFloatType != null,
                readParameterFromXml: SimpleParameter<FloatType>.ReadXml,
                writeParameterToXml: (oci, writer, parameter) => parameter.WriteToXml(writer),
                checkIntegrity: null,
                getFinalName: (currentName, oci, parameter) => AddSpacesToSentence(parameter.Type)
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "Pseudo Maker",
                id: "ColorValue",
                name: "Color Value",
                interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Controller.UpdateColorProperty(Color.LerpUnclamped(leftValue, rightValue, factor), parameter.Type),
                interpolateAfter: null,
                getValue: (oci, parameter) => parameter.Controller.GetColorPropertyValue(parameter.Type),
                readValueFromXml: (parameter, node) => ReadColorXML(node),
                writeValueToXml: (parameter, writer, value) => WriteColorXML(writer, value),
                getParameter: oci => new SimpleParameter<ColorType>(oci, (ColorType)SelectedColorType),
                isCompatibleWithTarget: oci => oci is OCIChar && SelectedColorType != null,
                readParameterFromXml: SimpleParameter<ColorType>.ReadXml,
                writeParameterToXml: (oci, writer, parameter) => parameter.WriteToXml(writer),
                checkIntegrity: null,
                getFinalName: (currentName, oci, parameter) => AddSpacesToSentence(parameter.Type)
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

        private class BaseParameter
        {
            protected int _hashCode;
            protected ObjectCtrlInfo oci;
            public PseudoMakerCharaController Controller
            {
                get { return GetParameter(oci); }
            }

            public override int GetHashCode()
            {
                return this._hashCode;
            }
        }

        private class SimpleParameter<T> : BaseParameter
        {
            public T Type { get; set; }

            public SimpleParameter(ObjectCtrlInfo oci, T type)
            {
                this.oci = oci;
                Type = type;

                unchecked
                {
                    int hash = 17;
                    this._hashCode = hash * 31 + type.GetHashCode();
                }
            }

            public void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteAttributeString("Type", Type.ToString());
            }

            public static SimpleParameter<T> ReadXml(ObjectCtrlInfo oci, XmlNode node)
            {
                var value = typeof(T).IsEnum ? Enum.Parse(typeof(T), node.Attributes["Type"].Value) : Int32.Parse(node.Attributes["Type"].Value);
                return new SimpleParameter<T>(oci, (T)value);
            }
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
