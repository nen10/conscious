using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem.HexCoordinateSystem;
using tr = MapSystem.HexCoordinateSystem.Transform;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    Grid oneFloor;
    [SerializeField]
    Tilemap layerOnGround;
    [SerializeField]
    Tilemap layerGround;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
