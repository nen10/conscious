using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem.HexCoordinateSystem;
using gen = MapSystem.HexCoordinateSystem.HexGenerator;
using rt = MapSystem.ResourceManager.Tilemap;

namespace MapSystem.SpriteManager 
{
    
    public class MapData
    {
        public Tilemap layerGround;
        public Tilemap layerOnGround;

        public rt.DECORATETYPE deco;
        public MapData(Tilemap ground, Tilemap onGround, rt.DECORATETYPE d)
        {
            this.layerGround = ground;
            this.layerOnGround = onGround;
            this.deco = d;
        }

        public HexTile HexTile(Hex h)
        {
            return new HexTile(this, h);
        }

        public bool generated(Hex h)
        {
            return layerGround.GetTile((Vector3Int)h) != null;
        }

        public bool hasWall(Hex h)
        {
            return rt.GroundWallName(deco) == layerGround.GetTile((Vector3Int)h).name;
        }

        public void SetTileWall(HexTile h)
        {
            if ( !hasWall(h) ){ return; }

            Tile wall = Resources.Load<Tile>(rt.GetTileNameWall(deco, h));
            layerOnGround.SetTile((Vector3Int)h, wall);
        }

        public void SetTileGround(HexTile h)
        {
            if ( ! h.isGlobal ){ return; }
            if ( generated(h) ){ return; }
            layerGround.SetTile((Vector3Int)h, GetTileGround(GetRandomValue(h)));
        } 
        public void SetTileGroundInit(HexTile h,int count)
        {
            if ( ! h.isGlobal ){ return; }
            if ( generated(h) ){ return; }

            layerGround.SetTile((Vector3Int)h, GetTileGround(GetRandomValue(count)));
        } 
        public Tile GetTileGround(rt.GROUNDTYPE randomvalue)
        {
            return Resources.Load<Tile>(rt.DECORATE2GROUND[deco][randomvalue]);
        } 

        public rt.GROUNDTYPE GetRandomValue(int count)
        {
            rt.GROUNDTYPE randomvalue = 0;
            return randomvalue;
        }
        public rt.GROUNDTYPE GetRandomValue(HexTile h)
        {
            rt.GROUNDTYPE randomvalue = 0;
            return randomvalue;
        }

    }

    public class HexTile : Hex
    {
        private MapData map;
        public HexTile(MapData map, int a, int b, int c)
         : base(a, b , c, true)
        {
            this.map = map;
        }
        public HexTile(MapData map, Hex h)
         : base(h.a, h.b , h.c, h.isGlobal)
        {
            this.map = h.isGlobal ? map : null;
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

        public bool hasWall(HexUnit h){
            return this.map.hasWall(this + h);
        }
        public bool generated(HexUnit h){
            return this.map.generated(this + h);
        }

    }


}
