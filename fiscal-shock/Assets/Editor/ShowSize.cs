using UnityEngine;
using UnityEditor;

class ShowSize : EditorWindow {
    [MenuItem("Window/Show Prefab Size")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        ShowSize sizeWindow = new ShowSize();
        sizeWindow.autoRepaintOnSceneChange = true;
        sizeWindow.Show();
    }

    void OnGUI () {
        GameObject thisObject = Selection.activeObject as GameObject;
        if (thisObject == null) {
            return;
        }

        MeshFilter mf = thisObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (mf == null) {
            return;
        }

        Mesh mesh = mf.sharedMesh;
        if (mesh == null) {
            return;
        }

        Vector3 size = mesh.bounds.size;
        Vector3 scale = thisObject.transform.localScale;
        GUILayout.Label($"{thisObject.name}\nX: {size.x*scale.x}\nY: {size.y*scale.y}\nZ: {size.z*scale.z}");
    }
}
