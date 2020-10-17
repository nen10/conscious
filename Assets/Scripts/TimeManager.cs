using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem;
using MapSystem.SpriteManager;
using MapSystem.ResourceManager;
using MapSystem.HexCoordinateSystem;
using CharacterSystem;
using System;

public class TimeManager : MonoBehaviour
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
    private CharacterManager.ControlMode controllMode;
    private MapGenerator observer;


    // Start is called before the first frame update
    void Start()
    {

        this.map = new MapData(oneFloor, layerGround, layerOnGround, layerBlaind, new Tiling(Naming.DECORATETYPE.template));
        this.controllMode = CharacterManager.ControlMode.body;
        this.observer = new MapGenerator(map, 8);

        observer.GenerateTileInit();
        observer.generatedCenter.Clone();
        CharacterManager.Init(observer.generatedCenter.Clone());
        new Character(CharacterManager.playable, "", "Moth", "C");

        Character ch1 = new Character(new Hex( 4, 2, 0), "", "Slime", "C");
        Character ch2 = new Character(new Hex( 0,-3, 0), "", "EyeBall", "B");
        Character ch3 = new Character(new Hex(-2, 3, 0), "", "Moth", "D");
        Character ch4 = new Character(new Hex( 0,-2, 2), "", "Potion", "J");
        Character ch5 = new Character(new Hex(-2, 0, 3), "", "WaterSmall", "A");
        Character ch6 = new Character(new Hex( 2, 4, 0), "", "Bug", "C");

        CharacterManager.playable.members[CharacterManager.playable.pReaderId].ani.Inverse();
        CharacterManager.playable.members[CharacterManager.playable.pReaderId].SetPointer("Circle_3");
        ch1.SetPointer("Circle_39_1");
        ch2.SetPointer("Circle_6");
        ch3.SetPointer("Circle_7");
        ch4.SetPointer("Circle_38_1");
        ch5.SetPointer("Circle_39");
        ch6.SetPointer("Circle_38");
        // CharacterManager.playable.members[CharacterManager.playable.pReaderId].RemovePointer();
        // 移動させてみよう

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

