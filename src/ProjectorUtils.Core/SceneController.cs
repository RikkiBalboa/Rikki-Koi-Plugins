using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using Studio;
using UnityEngine;

namespace Plugins
{
    internal class SceneController : SceneCustomFunctionController
    {
        protected override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
        {
            //Turn projectors off if the item (or one of its parents) is toggled off
            foreach (var objectCtrlInfo in loadedItems.Values)
                if (objectCtrlInfo is OCIItem item)
                    foreach (var projector in item.objectItem.GetComponentsInChildren<Projector>())
                        projector.enabled = item.visible;
        }

        protected override void OnSceneSave() { }

        protected override void OnObjectVisibilityToggled(ObjectCtrlInfo objectCtrlInfo, bool visible)
        {
            if (objectCtrlInfo is OCIItem item)
                //Turn projector off if the item (or one of its parents) is toggled off
                foreach (var projector in item.objectItem.GetComponentsInChildren<Projector>())
                    projector.enabled = item.visible;


            base.OnObjectVisibilityToggled(objectCtrlInfo, visible);
        }
    }
}
