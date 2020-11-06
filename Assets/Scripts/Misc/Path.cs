using System.Collections.Generic;
using UnityEngine;

//This is used to determine if skeletons are coming from more than 2 directions.
public enum PathOrigin
{
    North,
    West,
    East
}

public class Path : MonoBehaviour
{
    private List<Vector2> _nodes = new List<Vector2>();
    private GameManager _gameManagerReference;

    [SerializeField]
    private PathOrigin _origin;

    [SerializeField]
    private Pumpkin pumpkin;

    private void Awake()
    {
        _gameManagerReference = FindObjectOfType<GameManager>();


        foreach (Transform child in transform)
        {
            _nodes.Add(new Vector2(child.position.x, child.position.y));
        }
    }

    public void SetOriginValue(bool value)
    {
        _gameManagerReference.SetSkeletonDirection(_origin, value);
    }

    public PathOrigin GetPathOrigin()
    {
        return _origin;
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