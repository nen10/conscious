using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem;
using MapSystem.SpriteManager;
using MapSystem.ResourceManager;
using MapSystem.HexCoordinateSystem;

public class PlayAvatar : MonoBehaviour
{
    [SerializeField]
    Grid oneFloor;

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

        map = new MapData(oneFloor, layerGround, layerOnGround, layerBlaind, new Tiling(Naming.DECORATETYPE.template));
        partyPosition = new MapGenerator(new HexTile(map, 0, 0, 0), 8);
        partyPosition.GenerateTileInit();
        (partyPosition.generatedCenter + new Hex(4,2,0)).SetCharactor("Slime", "C");
        (partyPosition.generatedCenter + new Hex(1,0,0)).SetCharactor("Cloud", "C");
        (partyPosition.generatedCenter + new Hex(0,-3,0)).SetCharactor("EyeBall", "B");
        (partyPosition.generatedCenter + new Hex(-2,3,0)).SetCharactor("Moth", "D");
        (partyPosition.generatedCenter + new Hex(0,-2,2)).SetCharactor("Potion", "J");
        (partyPosition.generatedCenter + new Hex(-2,0,3)).SetCharactor("WaterSmall", "A");
        (partyPosition.generatedCenter + new Hex(2,4,0)).SetCharactor("Bug", "C");
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
