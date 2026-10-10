using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

using MTM101BaldAPI;
using PlusStudioLevelLoader;
using MTM101BaldAPI.AssetTools;
using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;

using PlusStudioLevelFormat;

namespace FarmComeback.Editor
{
    public static class EditorSupport
    {
        public static void AddEditorContent()
        {
            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("FarmFlag", LevelLoaderPlugin.Instance.basicObjects["FarmFlag"], 3, 26, 1, Vector3.up * 13f);
            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("SoonSign", LevelLoaderPlugin.Instance.basicObjects["SoonSign"], 5, 10, 1, Vector3.up * 5f);
            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("HayObject", LevelLoaderPlugin.Instance.basicObjects["HayObject"], 4, 10, 1, Vector3.up * 4.5f);
            EditorInterface.AddObjectVisual("Barn", LevelLoaderPlugin.Instance.basicObjects["Barn"], true);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        public static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            Dictionary<string, Sprite> icons = FarmComebackPlugin.EditorIcons;

            EditorInterfaceModes.AddToolsToCategory(mode, "objects", new EditorTool[]
            {
                new ObjectTool("Barn", icons["Editor_Barn"]),
                new ObjectTool("HayObject", icons["Editor_Hay"]),
                new ObjectToolNoRotation("FarmFlag", icons["Editor_FarmFlag"]),
                new ObjectTool("SoonSign", icons["Editor_Sign"])
            });
        }
    }
}