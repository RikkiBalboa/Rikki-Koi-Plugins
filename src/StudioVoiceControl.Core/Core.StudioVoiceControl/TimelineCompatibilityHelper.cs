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
		
		/**
         * For modded voices the beginning key of the no values can be a arbitrary large number that changes with each loading of the game.
         * Its unclear why that is!
         * 
         * This function corrects that behavior by asuming the first entry in the dictionary marks the 0 position of the provided list. 
         * The corrected value is then calculated relative position in the list of voice samples by difference between the first and the current No.
         */
        private static int toRelativeVoiceInfoNo(int group, int category, int absNo)
        {
            Dictionary<int, Info.LoadCommonInfo> noDic = Singleton<Info>.Instance.dicVoiceLoadInfo[group][category];
            if(noDic.Count > 0)
            {
                return absNo - noDic.First().Key;
            }else
            {
                return 0;
            }
        }

        /**
         * The inverse function of toRelativeVoiceInfoNo.
         */
        private static int toAbsoluteVoiceInfoNo(int group, int category, int relNo)
        {
            Dictionary<int, Info.LoadCommonInfo> noDic = Singleton<Info>.Instance.dicVoiceLoadInfo[group][category];
            if (noDic.Count > 0)
            {
                return noDic.First().Key + relNo;
            }
            else
            {
                return 0;
            }
        }

        private static void WriteVoiceInfoXML(ObjectCtrlInfo oci, XmlTextWriter writer, VoiceCtrl.VoiceInfo value)
        {
            //StudioVoiceControl.Logger.LogInfo("Writing to timeline");
            writer.WriteAttributeString("group", XmlConvert.ToString(value.group));
            writer.WriteAttributeString("category", XmlConvert.ToString(value.category));

            int relativeNo = TimelineCompatibilityHelper.toRelativeVoiceInfoNo(value.group, value.category, value.no);
            writer.WriteAttributeString("no", XmlConvert.ToString( relativeNo ));

            //StudioVoiceControl.Logger.LogInfo("ToRelative No [" + value.no + "]: " + relativeNo);
        }

        private static VoiceCtrl.VoiceInfo ReadVoiceInfoXML(ObjectCtrlInfo oci, XmlNode node)
        {
            //StudioVoiceControl.Logger.LogInfo("Reading from timeline");
            int group = XmlConvert.ToInt32(node.Attributes["group"].Value);
            int category = XmlConvert.ToInt32(node.Attributes["category"].Value);
            int relNo = XmlConvert.ToInt32(node.Attributes["no"].Value);
            int absoNo = TimelineCompatibilityHelper.toAbsoluteVoiceInfoNo(group, category, relNo);

            //StudioVoiceControl.Logger.LogInfo("ToAbsolute No [" + relNo + "]: " + absoNo);

            return new VoiceCtrl.VoiceInfo(
                group,
                category,
                absoNo
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
