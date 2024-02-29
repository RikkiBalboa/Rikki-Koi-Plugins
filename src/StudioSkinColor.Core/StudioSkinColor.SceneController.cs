using KKAPI.Studio.SaveLoad;
using System;
using System.Collections.Generic;
using Studio;
using KKAPI.Utilities;
using ExtensibleSaveFormat;
using MessagePack;
using System.Linq;
using static Plugins.StudioSkinColor;

namespace Plugins
{
    internal class SceneController : SceneCustomFunctionController
    {
        protected override void OnSceneSave()
        {
            Logger.LogInfo("Saving Skin data");
            var data = new PluginData();
            Dictionary<int, ObjectCtrlInfo> dicObjectCtrl = Studio.Studio.Instance.dicObjectCtrl;
            Dictionary<int, Dictionary<ModifiedValue, object>> defaultValues = new Dictionary<int, Dictionary<ModifiedValue, object>>();

            foreach (int id in dicObjectCtrl.Where(x => x.Value is OCIChar).ToDictionary(k => k.Key, v => v.Value).Keys)
            {
                Logger.LogInfo($"Saving character {id}");
                defaultValues[id] = DefaultValues[(OCIChar)dicObjectCtrl[id]];
            }

            if (defaultValues.Count > 0)
            {
                Logger.LogInfo($"Saving!");
                // TODO Fix Color saving
                data.data.Add("values", MessagePackSerializer.Serialize(defaultValues));
            }
            SetExtendedData(data);
        }
        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            Logger.LogInfo($"Loading data");
            if (operation == SceneOperationKind.Clear || operation == SceneOperationKind.Load)
            {
                Logger.LogInfo($"Clearing data");
                DefaultValues.Clear();
                if (operation == SceneOperationKind.Clear)
                    return;
            }

            var data = GetExtendedData();
            if (data?.data == null)
            {
                Logger.LogInfo($"No data to load");
                return;
            }

            data.data.TryGetValue("values", out var defaultValuesOut);
            if ((operation == SceneOperationKind.Load || operation == SceneOperationKind.Import) && defaultValuesOut != null)
            {
                Logger.LogInfo($"Loading data");
                var defaultValues = MessagePackSerializer.Deserialize<Dictionary<int, Dictionary<ModifiedValue, object>>>((byte[])defaultValuesOut);
                foreach (var entry in defaultValues)
                {
                    Logger.LogInfo($"Loading data for {entry.Key}");
                    DefaultValues[(OCIChar)loadedItems[entry.Key]] = entry.Value;
                }
            }
        }
    }
}