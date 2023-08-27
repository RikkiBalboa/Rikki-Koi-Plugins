using KKAPI.Studio.SaveLoad;
using System;
using System.Collections.Generic;
using Studio;
using KKAPI.Utilities;
using ExtensibleSaveFormat;
using MessagePack;
using System.Linq;

namespace Plugins
{
    internal class SceneController : SceneCustomFunctionController
    {
        protected override void OnSceneSave()
        {
            var data = new PluginData();

            Dictionary<int, ObjectCtrlInfo> dicObjectCtrl = Studio.Studio.Instance.dicObjectCtrl;
            Dictionary<int, float> savedFovs = new Dictionary<int, float>();

            foreach (int id in dicObjectCtrl.Where(x => x.Value.GetType() == typeof(OCICamera)).ToDictionary(k => k.Key, v => v.Value).Keys)
                savedFovs[id] = SaveCameraObjectFov.cameras[(OCICamera)dicObjectCtrl[id]];

            if (savedFovs.Count > 0)
                data.data.Add("FovData", MessagePackSerializer.Serialize(savedFovs));
            else
                data.data.Add("FovData", null);
            SetExtendedData(data);
        }

        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            SaveCameraObjectFov.Logger.LogInfo("Start loading");
            var data = GetExtendedData();
            if (data?.data == null) return;
            SaveCameraObjectFov.Logger.LogInfo("Found data!");

            data.data.TryGetValue("FovData", out var temp);
            if (temp == null)
            {

                SaveCameraObjectFov.Logger.LogInfo("No data!");
            }

            if (operation == SceneOperationKind.Clear)
                SaveCameraObjectFov.cameras.Clear();
            else if (operation == SceneOperationKind.Load && temp != null)
            {
                SaveCameraObjectFov.cameras.Clear();
                var savedFovs = MessagePackSerializer.Deserialize<Dictionary<int, float>>((byte[])temp);
                SaveCameraObjectFov.Logger.LogInfo(string.Join(Environment.NewLine, savedFovs));
                foreach (var entry in savedFovs)
                    SaveCameraObjectFov.cameras[(OCICamera)loadedItems[entry.Key]] = entry.Value;
            }
        }
    }
}
