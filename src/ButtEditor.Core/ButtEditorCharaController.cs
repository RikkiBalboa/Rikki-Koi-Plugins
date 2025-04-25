using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using MessagePack;
using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace ButtEditor
{
    internal class ButtEditorCharaController : CharaCustomFunctionController
    {
        public static Dictionary<SliderType, float> defaultValues = new Dictionary<SliderType, float>()
        {
            {SliderType.Stiffness, 0.04f},
            {SliderType.Elasticity, 0.2f},
            {SliderType.Dampening, 0.12f},
            {SliderType.Weight, 0f},
        };

        public static Dictionary<SliderType, float> SavedValues = new Dictionary<SliderType, float>();

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (maintainState) return;

            SavedValues.Clear();
            var data = GetExtendedData();

            if (data == null)
                return;

            if (data.data.TryGetValue(nameof(SavedValues), out var savedValues))
            {
                var values = MessagePackSerializer.Deserialize<Dictionary<SliderType, float>>((byte[])savedValues);
                foreach (var value in values)
                    SetButtValue(value.Key, value.Value);
            }
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (SavedValues.Count == 0)
            {
                SetExtendedData(null);
            }
            else
            {
                var data = new PluginData();
                data.data.Add(nameof(SavedValues), MessagePackSerializer.Serialize(SavedValues));
            }
        }

        public void SetButtValue(SliderType type, float value)
        {
            //ChaControl.reSetupDynamicBoneBust = true;

            SavedValues[type] = value;

            foreach (var _type in new ChaInfo.DynamicBoneKind[] { ChaInfo.DynamicBoneKind.HipL, ChaInfo.DynamicBoneKind.HipR })
            {
                if (ChaControl.dictDynamicBoneBust.TryGetValue(_type, out var _value))
                    _value.ResetPosition();

                if (type != SliderType.Weight)
                    ChaControl.dictDynamicBoneBust[_type].setSoftParams(
                        0,
                        -1,
                        SavedValues.GetValueOrDefault(SliderType.Dampening, defaultValues[SliderType.Dampening]),
                        SavedValues.GetValueOrDefault(SliderType.Elasticity, defaultValues[SliderType.Elasticity]),
                        SavedValues.GetValueOrDefault(SliderType.Stiffness, defaultValues[SliderType.Stiffness])
                    );
                else
                    ChaControl.dictDynamicBoneBust[_type].setGravity(0, new Vector3(0, value / -100, 0));
            }
        }
    }
}
