#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Map helpers for the open scene. Every change goes through Undo, so Ctrl+Z reverts it.
public static class UAndIMapTools
{
    static readonly string[] Interactables = { "ClockOut", "SecurityTerminal", "Clone Zone", "OperatingRoomSwitcher", "Operating Table" };

    // MapManager is scaled down (e.g. 0.6). This scales the things that shouldn't have shrunk back up:
    // interactables, doors, wall thickness (not length), and zooms the minimap camera in to match.
    [MenuItem("Tools/U and I/Map: Undo Shrink On Props, Doors, Wall Thickness, Minimap")]
    public static void RestoreSizes()
    {
        Scene scene = SceneManager.GetActiveScene();
        Transform map = FindInScene(scene, "MapManager");
        if (!map) { EditorUtility.DisplayDialog("Map tools", "No MapManager in the open scene. Open PT-Game first.", "OK"); return; }

        float s = map.localScale.x;
        if (Mathf.Approximately(s, 1f)) { EditorUtility.DisplayDialog("Map tools", "MapManager isn't scaled, nothing to undo.", "OK"); return; }

        string key = "UAndI.MapRestore." + scene.path + "." + s.ToString("0.###");
        if (EditorPrefs.GetBool(key) && !EditorUtility.DisplayDialog("Map tools",
            $"This already ran for MapManager scale {s}. Running it again would grow things twice.", "Run anyway", "Cancel")) return;

        float c = 1f / s;
        var report = new StringBuilder();
        Undo.SetCurrentGroupName("Restore map prop sizes");
        int group = Undo.GetCurrentGroup();

        // 1. interactables back to full size
        foreach (string name in Interactables)
        {
            foreach (Transform t in FindAll(map, name))
            {
                Undo.RecordObject(t, "scale");
                t.localScale *= c;
                report.AppendLine($"{name}: scale x{c:0.##}");
            }
        }

        // 2. doors back to full size
        Transform doors = FindDeep(map, "Doors");
        if (doors)
        {
            foreach (Transform door in doors)
            {
                Undo.RecordObject(door, "scale");
                door.localScale *= c;
            }
            report.AppendLine($"Doors: {doors.childCount} doors scaled x{c:0.##} (check they don't overlap the shortened walls)");
        }

        // 3. wall thickness: grow only each wall's thin axis, so length stays shortened
        Transform level = FindDeep(map, "Level");
        int walls = 0;
        if (level)
        {
            foreach (SpriteRenderer sr in level.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (!sr.sprite || !sr.GetComponent<BoxCollider2D>()) continue; // the floor has no collider
                Transform t = sr.transform;
                Vector2 size = sr.sprite.bounds.size;
                float w = Mathf.Abs(size.x * t.localScale.x), h = Mathf.Abs(size.y * t.localScale.y);
                if (Mathf.Approximately(w, h)) continue; // square, no thin axis

                Undo.RecordObject(t, "wall thickness");
                Vector3 scale = t.localScale;
                if (w < h) scale.x *= c; else scale.y *= c;
                t.localScale = scale;
                walls++;
            }
        }
        report.AppendLine($"Walls: thickened {walls} walls x{c:0.##} on their thin side");

        // 4. minimap camera: a smaller map needs a smaller view to fill the minimap
        foreach (Camera cam in map.GetComponentsInChildren<Camera>(true))
        {
            if (!cam.orthographic) continue;
            Undo.RecordObject(cam, "minimap zoom");
            cam.orthographicSize *= s;
            report.AppendLine($"{cam.name}: orthographic size -> {cam.orthographicSize:0.#}");
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorPrefs.SetBool(key, true);

        report.AppendLine();
        report.AppendLine("Next: rebake the NavMesh (FloorManager/NavMesh > Bake), then save the scene.");
        Debug.Log("[Map tools]\n" + report);
        EditorUtility.DisplayDialog("Map tools", report.ToString(), "OK");
    }

    // Adds the SafeZone component to every object named SafeZone and removes their NavMeshObstacle:
    // the NavMeshModifier (Not Walkable) already keeps patients out, and a non-carving obstacle only shoves agents around.
    [MenuItem("Tools/U and I/Map: Set Up Safe Zones")]
    public static void SetUpSafeZones()
    {
        Scene scene = SceneManager.GetActiveScene();
        var report = new StringBuilder();
        Undo.SetCurrentGroupName("Set up safe zones");
        int group = Undo.GetCurrentGroup();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name != "SafeZone" || !t.GetComponent<SpriteRenderer>()) continue;

                if (!t.GetComponent<SafeZone>())
                {
                    Undo.AddComponent<SafeZone>(t.gameObject);
                    report.AppendLine($"Added SafeZone to {Path(t)}");
                }
                NavMeshObstacle obstacle = t.GetComponent<NavMeshObstacle>();
                if (obstacle)
                {
                    Undo.DestroyObjectImmediate(obstacle);
                    report.AppendLine($"Removed NavMeshObstacle from {Path(t)}");
                }
            }
        }

        Undo.CollapseUndoOperations(group);
        if (report.Length == 0) report.AppendLine("No SafeZone objects needed changes.");
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[Map tools]\n" + report);
        EditorUtility.DisplayDialog("Map tools", report.ToString(), "OK");
    }

    // ───────── helpers ─────────

    static Transform FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root.transform;
            Transform t = FindDeep(root.transform, name);
            if (t) return t;
        }
        return null;
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != root && t.name == name) return t;
        }
        return null;
    }

    static List<Transform> FindAll(Transform root, string name)
    {
        var list = new List<Transform>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != root && t.name == name) list.Add(t);
        }
        return list;
    }

    static string Path(Transform t) => t.parent ? Path(t.parent) + "/" + t.name : t.name;
}
#endif
