using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem;
using MapSystem.SpriteManager;
using MapSystem.ResourceManager;

public class PlayAvatar : MonoBehaviour
{
    [SerializeField]
    Grid OneFloor;

    [SerializeField]
    Tilemap layerGround;

    [SerializeField]
    Tilemap layerOnGround;

    [SerializeField]
    Tilemap layerBlaind;

    private MapData map;

    private MapGenerator partyPosition;

    // Start is called before the first frame update
    void Start()
    {

        map = new MapData(layerGround, layerOnGround, layerBlaind, new Tiling(Naming.DECORATETYPE.template));
        partyPosition = new MapGenerator(new HexTile(map, 0, 0, 0), 8);
        partyPosition.GenerateTileInit();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
