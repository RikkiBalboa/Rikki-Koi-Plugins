using BepInEx.Configuration;
using KKAPI.Utilities;
using Studio;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace PostProcessingEffectsV3
{
    internal class TimelineCompatibilityHelper
    {

        internal static void PopulateTimeline()
        {
            var variables = GetConfigProperties();
            foreach (var variable in variables)
            {
                var config = variable.GetValue(PostProcessingEffectsV3.ppe, null);
                if (config is ConfigEntry<bool> boolConfig)
                {
                    TimelineCompatibility.AddInterpolableModelStatic(
                        owner: "PostProcessingEffectsV3",
                        id: $"{boolConfig.Definition.Section} - {boolConfig.Definition.Key}",
                        name: $"{boolConfig.Definition.Section} - {boolConfig.Definition.Key}",
                        interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Value = leftValue,
                        interpolateAfter: null,
                        getValue: (oci, parameter) => boolConfig.Value,
                        readValueFromXml: (parameter, node) => XmlConvert.ToBoolean(node.Attributes["value"].Value),
                        writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", XmlConvert.ToString(value)),
                        isCompatibleWithTarget: (oci) => true,
                        useOciInHash: false,
                        parameter: boolConfig,
                        readParameterFromXml: GetBoolParameter,
                        writeParameterToXml: WriteParameter
                    );
                }
                else if (config is ConfigEntry<float> floatConfig)
                {
                    TimelineCompatibility.AddInterpolableModelStatic(
                        owner: "PostProcessingEffectsV3",
                        id: $"{floatConfig.Definition.Section} - {floatConfig.Definition.Key}",
                        name: $"{floatConfig.Definition.Section} - {floatConfig.Definition.Key}",
                        interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Value = Mathf.LerpUnclamped(leftValue, rightValue, factor),
                        interpolateAfter: null,
                        getValue: (oci, parameter) => floatConfig.Value,
                        readValueFromXml: (parameter, node) => XmlConvert.ToSingle(node.Attributes["value"].Value),
                        writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", XmlConvert.ToString(value)),
                        isCompatibleWithTarget: (oci) => true,
                        useOciInHash: false,
                        parameter: floatConfig,
                        readParameterFromXml: GetFloatParameter,
                        writeParameterToXml: WriteParameter
                    );
                }
                else if (config is ConfigEntry<int> intConfig)
                {
                    TimelineCompatibility.AddInterpolableModelStatic(
                        owner: "PostProcessingEffectsV3",
                        id: $"{intConfig.Definition.Section} - {intConfig.Definition.Key}",
                        name: $"{intConfig.Definition.Section} - {intConfig.Definition.Key}",
                        interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Value = (int)Mathf.LerpUnclamped(leftValue, rightValue, factor),
                        interpolateAfter: null,
                        getValue: (oci, parameter) => intConfig.Value,
                        readValueFromXml: (parameter, node) => XmlConvert.ToInt32(node.Attributes["value"].Value),
                        writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", XmlConvert.ToString(value)),
                        isCompatibleWithTarget: (oci) => true,
                        useOciInHash: false,
                        parameter: intConfig,
                        readParameterFromXml: GetIntParameter,
                        writeParameterToXml: WriteParameter
                    );
                }
                else if (config is ConfigEntry<Color> colorConfig)
                {
                    TimelineCompatibility.AddInterpolableModelStatic(
                        owner: "PostProcessingEffectsV3",
                        id: $"{colorConfig.Definition.Section} - {colorConfig.Definition.Key}",
                        name: $"{colorConfig.Definition.Section} - {colorConfig.Definition.Key}",
                        interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Value = Color.LerpUnclamped(leftValue, rightValue, factor),
                        interpolateAfter: null,
                        getValue: (oci, parameter) => colorConfig.Value,
                        readValueFromXml: (parameter, node) =>
                        {
                            return new Color(
                                XmlConvert.ToSingle(node.Attributes["R"].Value),
                                XmlConvert.ToSingle(node.Attributes["G"].Value),
                                XmlConvert.ToSingle(node.Attributes["B"].Value),
                                XmlConvert.ToSingle(node.Attributes["A"].Value)
                            );
                        },
                        writeValueToXml: (parameter, writer, value) =>
                        {
                            writer.WriteAttributeString("R", XmlConvert.ToString(value.r));
                            writer.WriteAttributeString("G", XmlConvert.ToString(value.g));
                            writer.WriteAttributeString("B", XmlConvert.ToString(value.b));
                            writer.WriteAttributeString("A", XmlConvert.ToString(value.a));
                        },
                        isCompatibleWithTarget: (oci) => true,
                        useOciInHash: false,
                        parameter: colorConfig,
                        readParameterFromXml: GetColorParameter,
                        writeParameterToXml: WriteParameter
                    );
                }
                else if (config is ConfigEntry<Vector2> vector2Config)
                {
                    TimelineCompatibility.AddInterpolableModelStatic(
                        owner: "PostProcessingEffectsV3",
                        id: $"{vector2Config.Definition.Section} - {vector2Config.Definition.Key}",
                        name: $"{vector2Config.Definition.Section} - {vector2Config.Definition.Key}",
                        interpolateBefore: (oci, parameter, leftValue, rightValue, factor) => parameter.Value = Vector2.LerpUnclamped(leftValue, rightValue, factor),
                        interpolateAfter: null,
                        getValue: (oci, parameter) => vector2Config.Value,
                        readValueFromXml: (parameter, node) =>
                        {
                            return new Vector2(
                                XmlConvert.ToSingle(node.Attributes["X"].Value),
                                XmlConvert.ToSingle(node.Attributes["Y"].Value)
                            );
                        },
                        writeValueToXml: (parameter, writer, value) =>
                        {
                            writer.WriteAttributeString("X", XmlConvert.ToString(value.x));
                            writer.WriteAttributeString("Y", XmlConvert.ToString(value.y));
                        },
                        isCompatibleWithTarget: (oci) => true,
                        useOciInHash: false,
                        parameter: vector2Config,
                        readParameterFromXml: GetVector2Parameter,
                        writeParameterToXml: WriteParameter
                    );
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetConfigProperties()
        {
            return PostProcessingEffectsV3.ppe.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(
                    x => x.PropertyType.IsGenericType
                    && x.PropertyType.GetGenericTypeDefinition() == typeof(ConfigEntry<>)
                );
        }

        private static void WriteParameter<T>(ObjectCtrlInfo oci, XmlTextWriter writer, ConfigEntry<T> configEntry)
        {
            writer.WriteAttributeString("section", configEntry.Definition.Section);
            writer.WriteAttributeString("key", configEntry.Definition.Key);
        }

        private static ConfigEntry<bool> GetBoolParameter(ObjectCtrlInfo oci, XmlNode node)
        {
            foreach (var variable in GetConfigProperties())
            {
                if (variable.GetValue(PostProcessingEffectsV3.ppe, null) is ConfigEntry<bool> config)
                    return config;
            }
            return null;
        }

        private static ConfigEntry<float> GetFloatParameter(ObjectCtrlInfo oci, XmlNode node)
        {
            foreach (var variable in GetConfigProperties())
            {
                if (variable.GetValue(PostProcessingEffectsV3.ppe, null) is ConfigEntry<float> config)
                    return config;
            }
            return null;
        }

        private static ConfigEntry<int> GetIntParameter(ObjectCtrlInfo oci, XmlNode node)
        {
            foreach (var variable in GetConfigProperties())
            {
                if (variable.GetValue(PostProcessingEffectsV3.ppe, null) is ConfigEntry<int> config)
                    return config;
            }
            return null;
        }

        private static ConfigEntry<Color> GetColorParameter(ObjectCtrlInfo oci, XmlNode node)
        {
            foreach (var variable in GetConfigProperties())
            {
                if (variable.GetValue(PostProcessingEffectsV3.ppe, null) is ConfigEntry<Color> config)
                    return config;
            }
            return null;
        }

        private static ConfigEntry<Vector2> GetVector2Parameter(ObjectCtrlInfo oci, XmlNode node)
        {
            foreach (var variable in GetConfigProperties())
            {
                if (variable.GetValue(PostProcessingEffectsV3.ppe, null) is ConfigEntry<Vector2> config)
                    return config;
            }
            return null;
        }
    }
}
