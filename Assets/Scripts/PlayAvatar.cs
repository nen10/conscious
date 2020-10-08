using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem;
using MapSystem.SpriteManager;
using MapSystem.ResourceManager;
using MapSystem.HexCoordinateSystem;
using CharacterSystem;

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

    private MapGenerator observer;

    // Start is called before the first frame update
    void Start()
    {

        map = new MapData(oneFloor, layerGround, layerOnGround, layerBlaind, new Tiling(Naming.DECORATETYPE.template));
        observer = new MapGenerator(map, 8);
        observer.GenerateTileInit();
        observer.generatedCenter.Clone();
        CharacterManager.Init(observer.generatedCenter.Clone());
        new Character(CharacterManager.playable, "Moth", "C");

        new Character(new Hex( 4, 2, 0), "Slime", "C");
        new Character(new Hex( 0,-3, 0), "EyeBall", "B");
        new Character(new Hex(-2, 3, 0), "Moth", "D");
        new Character(new Hex( 0,-2, 2), "Potion", "J");
        new Character(new Hex(-2, 0, 3), "WaterSmall", "A");
        new Character(new Hex( 2, 4, 0), "Bug", "C");

        CharacterManager.playable.members[CharacterManager.playable.pReaderId].ani.Inverse();


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
