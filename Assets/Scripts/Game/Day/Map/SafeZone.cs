using System.Collections.Generic;
using UnityEngine;

// Players standing inside a safe zone can't be targeted by patients (see Patient.IsUntargetable).
// Uses this object's Collider2D if it has one, otherwise its sprite's bounds.
// Keeping patients physically out is still the NavMeshModifier's job (Not Walkable area).
public class SafeZone : MonoBehaviour
{
    public static readonly List<SafeZone> All = new List<SafeZone>();

    private Collider2D col;
    private Renderer rend;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        rend = GetComponent<Renderer>();
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public bool Contains(Vector2 point)
    {
        if (col) return col.OverlapPoint(point);
        if (rend)
        {
            Bounds b = rend.bounds;
            return point.x >= b.min.x && point.x <= b.max.x && point.y >= b.min.y && point.y <= b.max.y;
        }
        return false;
    }

    public static bool IsSafe(Vector2 point)
    {
        foreach (SafeZone zone in All)
        {
            if (zone.Contains(point)) return true;
        }
        return false;
    }
}
