using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    private List<Vector2> _nodes = new List<Vector2>();
    public Pumpkin pumpkin;

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            _nodes.Add(new Vector2(child.position.x, child.position.y));
        }
    }

    public Pumpkin GetPumpkin()
    {
        return pumpkin;
    }

    public Vector2[] GetNodes()
    {
        return _nodes.ToArray();
    }
}