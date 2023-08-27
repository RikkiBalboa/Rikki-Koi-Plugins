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
                data.data.Add("cameras", MessagePackSerializer.Serialize(savedFovs));
            else
                data.data.Add("cameras", null);

            data.data.Add("mainFov", SaveCameraObjectFov.mainFov);
            data.data.Add("previousCameraIndex", SaveCameraObjectFov.previousCameraIndex);
            data.data.Add("cameraIndex", SaveCameraObjectFov.cameraIndex);

            SetExtendedData(data);
        }

        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            var data = GetExtendedData();
            if (data?.data == null)
            {
                if (operation == SceneOperationKind.Load)
                    SaveCameraObjectFov.ResetValues();
                return;
            }

            data.data.TryGetValue("cameras", out var cameras);
            data.data.TryGetValue("mainFov", out var mainFov);
            data.data.TryGetValue("previousCameraIndex", out var previousCameraIndex);
            data.data.TryGetValue("cameraIndex", out var cameraIndex);

            if (operation == SceneOperationKind.Clear)
                SaveCameraObjectFov.ResetValues();
            else if (operation == SceneOperationKind.Load && cameras != null)
            {
                SaveCameraObjectFov.ResetValues();

                var savedFovs = MessagePackSerializer.Deserialize<Dictionary<int, float>>((byte[])cameras);
                foreach (var entry in savedFovs)
                    SaveCameraObjectFov.cameras[(OCICamera)loadedItems[entry.Key]] = entry.Value;

                if (mainFov != null)
                    SaveCameraObjectFov.mainFov = (float)mainFov;
                if (previousCameraIndex != null)
                    SaveCameraObjectFov.previousCameraIndex = (int)previousCameraIndex;
                if (cameraIndex != null)
                    SaveCameraObjectFov.cameraIndex = (int)cameraIndex;
            }
            else if (operation == SceneOperationKind.Import && cameras != null)
            {
                var savedFovs = MessagePackSerializer.Deserialize<Dictionary<int, float>>((byte[])cameras);
                foreach (var entry in savedFovs)
                    SaveCameraObjectFov.cameras[(OCICamera)loadedItems[entry.Key]] = entry.Value;
            }
        }
    }
}
