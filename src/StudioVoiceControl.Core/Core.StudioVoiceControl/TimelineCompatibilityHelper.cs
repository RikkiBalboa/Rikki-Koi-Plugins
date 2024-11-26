using KKAPI.Utilities;
using Studio;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Plugins
{
    internal class TimelineCompatibilityHelper
    {
        private static float _lastPlaybackTime;
        private static bool stoppedPlayback;

        internal static void Update()
        {
            // Stolen code from TimelineFlowControl
            // Makes a keyframe only trigger once when it's passed in playback
            // Otherwise it would keep clearing and starting playpack every frame
            if (Timeline.Timeline.isPlaying)
            {
                stoppedPlayback = false;
                var currentTime = Timeline.Timeline.playbackTime;

                if (_lastPlaybackTime < currentTime && currentTime - _lastPlaybackTime < 0.5f)
                {
                    foreach (var keyframe in GetAllKeyframes("voiceMulti"))
                    {
                        var keyframeTime = keyframe.Key;
                        if (keyframeTime > _lastPlaybackTime && keyframeTime <= currentTime)
                            PlayVoice(keyframe.Value.parent.oci as OCIChar, keyframe.Value.value as VoiceCtrl.VoiceInfo);
                    }

                    foreach (var keyframe in GetAllKeyframes("voiceSingle"))
                    {
                        var keyframeTime = keyframe.Key;
                        if (keyframeTime > _lastPlaybackTime && keyframeTime <= currentTime)
                            PlayVoice(keyframe.Value.parent.oci as OCIChar, keyframe.Value.value as VoiceCtrl.VoiceInfo);
                    }

                    foreach (var keyframe in GetAllKeyframes("stopVoice"))
                    {
                        var keyframeTime = keyframe.Key;
                        if (keyframeTime > _lastPlaybackTime && keyframeTime <= currentTime)
                            StopVoice(keyframe.Value.parent.oci as OCIChar);
                    }
                }

                _lastPlaybackTime = currentTime;
            }
            // Stop playback once timeline is stopped (and only do this once, not every frame)
            else if (!Timeline.Timeline.isPlaying && !stoppedPlayback)
            {
                foreach (var oci in GetAllCharacters())
                    StopVoice(oci);
                stoppedPlayback = true;
            }
        }
        internal static void PopulateTimeline()
        {
            if (!TimelineCompatibility.IsTimelineAvailable()) return;

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "TimelineVoiceControl",
                id: "voiceMulti",
                name: "Play voice (Multi interoperable)",
                interpolateBefore: null,
                interpolateAfter: null,
                getValue: (oci, parameter) => parameter,
                readValueFromXml: (parameter, node) => parameter,
                writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", XmlConvert.ToString(true)),
                getParameter: oci => ((OCIChar)oci).voiceCtrl.list.LastOrDefault(),
                readParameterFromXml: ReadVoiceInfoXML,
                writeParameterToXml: WriteVoiceInfoXML,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null,
                getFinalName: (currentName, oci, parameter) => $"Voice ({Singleton<Info>.Instance.dicVoiceGroupCategory[parameter.group].name} - {Singleton<Info>.Instance.dicVoiceGroupCategory[parameter.group].dicCategory[parameter.category]} - {Singleton<Info>.Instance.dicVoiceLoadInfo[parameter.group][parameter.category][parameter.no].name})",
                useOciInHash: true
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "TimelineVoiceControl",
                id: "voiceSingle",
                name: "Play voice (Single interoperable)",
                interpolateBefore: null,
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).voiceCtrl.list.LastOrDefault(),
                readValueFromXml: ReadVoiceInfoXML,
                writeValueToXml: WriteVoiceInfoXML,
                getParameter: oci => oci,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null
            );

            TimelineCompatibility.AddInterpolableModelDynamic(
                owner: "TimelineVoiceControl",
                id: "stopVoice",
                name: "Stop voice",
                interpolateBefore: null,
                interpolateAfter: null,
                getValue: (oci, parameter) => ((OCIChar)oci).voiceCtrl.list.ElementAtOrDefault(0),
                readValueFromXml: (parameter, node) => XmlConvert.ToBoolean(node.Attributes["value"].Value),
                writeValueToXml: (parameter, writer, value) => writer.WriteAttributeString("value", XmlConvert.ToString(true)),
                getParameter: oci => oci,
                isCompatibleWithTarget: oci => oci is OCIChar,
                checkIntegrity: null,
                getFinalName: (currentName, oci, parameter) => $"{currentName} ({((OCIChar)oci).charInfo.fileParam.fullname})"
            );
        }

        private static void WriteVoiceInfoXML(ObjectCtrlInfo oci, XmlTextWriter writer, VoiceCtrl.VoiceInfo value)
        {
            writer.WriteAttributeString("group", XmlConvert.ToString(value.group));
            writer.WriteAttributeString("category", XmlConvert.ToString(value.category));
            writer.WriteAttributeString("no", XmlConvert.ToString(value.no));
        }

        private static VoiceCtrl.VoiceInfo ReadVoiceInfoXML(ObjectCtrlInfo oci, XmlNode node)
        {
            return new VoiceCtrl.VoiceInfo(
                XmlConvert.ToInt32(node.Attributes["group"].Value),
                XmlConvert.ToInt32(node.Attributes["category"].Value),
                XmlConvert.ToInt32(node.Attributes["no"].Value)
            );
        }


        private static void PlayVoice(OCIChar ociChar, VoiceCtrl.VoiceInfo voiceInfo)
        {
            VoiceCtrl voiceCtrl = ociChar.voiceCtrl;
            voiceCtrl.list.Clear();
            voiceCtrl.list.Add(voiceInfo);
            voiceCtrl.Play(0);
        }

        private static void StopVoice(OCIChar ociChar)
        {
            ociChar.StopVoice();
        }

        internal static IEnumerable<KeyValuePair<float, Timeline.Keyframe>> GetAllKeyframes(string id = "")
        {
            return Timeline.Timeline.GetAllInterpolables(true)
                           .Where(x => x.owner == "TimelineVoiceControl" && x.id == id)
                           .SelectMany(x => x.keyframes);
        }

        internal static IEnumerable<OCIChar> GetAllCharacters()
        {
            return Timeline.Timeline.GetAllInterpolables(true)
                           .Where(x => x.owner == "TimelineVoiceControl")
                           .SelectMany(x => x.keyframes)
                           .Select(x => x.Value.parent.oci as OCIChar)
                           .Distinct();
        }
    }
}
