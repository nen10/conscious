using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem.HexCoordinateSystem;
using gen = MapSystem.HexCoordinateSystem.HexGenerator;
using MapSystem.ResourceManager;

namespace MapSystem.SpriteManager 
{
    
    public class MapData
    {
        public Tilemap layerGround;
        public Tilemap layerOnGround;
        public Tilemap layerBlaind;

        public Tiling til;
        public MapData(Tilemap ground, Tilemap onGround, Tilemap blaind, Tiling til)
        {
            this.layerGround = ground;
            this.layerOnGround = onGround;
            this.layerBlaind = blaind;
            this.til = til;
        }

        public HexTile HexTile(Hex h)
        {
            return new HexTile(this, h);
        }
        public static string GetTileName(TileBase t)
        {
            return t == null ? "" : t.name;
        }
        public static string GetTileName(Tilemap tm, HexTile h)
        {
            return GetTileName(tm.GetTile((Vector3Int)h));
        }
        public static bool isNull(Tilemap tm, HexTile h)
        {
            return tm.GetTile((Vector3Int)h) != null;
        }

        public bool hasGround(HexTile h)
        {
            return ! isNull(layerGround, h);
        }
        public string GetGroundTileName(HexTile h)
        {
            return GetTileName(layerGround, h);
        }

        public bool hasWall(HexTile h)
        {
            return til.GetTileNameGroundWall() == GetTileName(layerGround,h);
        }
    }

    public class HexTile : Hex
    {
        private MapData map;
        public Tiling til{ get { return this.map.til; } }
        public HexTile(MapData map, int a, int b, int c)
         : base(a, b, c, true)
        {
            this.map = map;
        }
        public HexTile(MapData map, Hex h)
         : base(h.a, 0, h.c, h.isGlobal)
        {
            this.map = h.isGlobal ? map : null;
        }

        public Vector3Int unityYXZHex
        {
            get { return HexCoordinateSystem.Transform.Z3Hex2UnityYXZHex(this); }
        }

        public static HexTile operator+ (HexTile h1, Hex h2)
        {
            return new HexTile(h1.map, (Hex)h1 + h2);
        }

        public static HexTile operator- (HexTile h1, Hex h2)
        {
            return new HexTile(h1.map, (Hex)h1 - h2);
        }

        public static HexTile operator* (HexTile h, int n)
        {
            return new HexTile(h.map, (Hex)h * n);
        }
        public static HexTile operator- (HexTile h)
        {
            return h * -1;
        }

        public static explicit operator Vector3Int(HexTile h)
        {
            return h.unityYXZHex;
        }

        public bool hasWall(HexUnit h){
            return this.map.hasWall(this + h);
        }
        public bool hasWall(){
            return this.map.hasWall(this);
        }
        public string groundName
        {
            get { return this.map.GetGroundTileName(this); }
        }

        public void SetTileGroundInit(Hex h)
        {
            map.layerGround.SetTile((Vector3Int)(this + h), Resources.Load<Tile>(til.GetTilePathGround(GetRandomValue())));
        } 

        public void SetTileWall(Hex h, Tile t)
        {
            if (!(this + h).hasWall()) { return; }
            map.layerOnGround.SetTile((Vector3Int)(this + h), t);
        }
        public void SetTileGround(Hex h, gen.unit generatePoint)
        {
            map.layerGround.SetTile((Vector3Int)(this + h), GetTileGround(h, generatePoint));
        } 
        public void SetTileBlaind(Hex h, Tile t)
        {
            map.layerBlaind.SetTile((Vector3Int)(this + h), t);
        }
        public string GetTilePathWall()
        {
            return this.hasWall() ? Naming.GetTilePathWall(til.deco, this) : "";
        }
        public string GetTilePathBlaind(int sightRadius, int count)
        {
            return Naming.GetTilePathBlaind(til.deco, blaindId(sightRadius,count));
        }
        public string GetTilePathBlaind()
        {
            return Naming.GetTilePathBlaind(til.deco, 0);
        }
        public static int blaindId(int sightRadius, int count)
        {
            return ((count % sightRadius) == 0 ? 1 : 2) + ((int)(count / sightRadius) * 2);
        }

        public Tile GetTileGround(Hex h, gen.unit generatePoint)
        {
            string n = (this + h).groundName;
            return Resources.Load<Tile>(n == "" ? til.GetTilePathGround(GetRandomValue(h, generatePoint)) : Naming.TILEDIRECTORY + n);
        } 

        public int GetRandomValue()
        {
            int randomvalue = (int)Naming.GROUNDTYPE.a;
            return randomvalue;
        }
        public int GetRandomValue(Hex h, gen.unit generatePoint)
        {
            int randomvalue = (int)Naming.GROUNDTYPE.wall;
            return randomvalue;
        }

    }
}
