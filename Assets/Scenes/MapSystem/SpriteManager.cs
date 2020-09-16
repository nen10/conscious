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

        public bool hasWall(Hex h)
        {
            return rt.GroundWallName(deco) == layerGround.GetTile((Vector3Int)h).name;
        }

        public HexTile HexTile(Hex h)
        {
            return new HexTile(this, h);
        }

        public void SetTileWall(HexTile h)
        {
            if ( !h.ex_here ){ return; }

            Tile groundWall = Resources.Load<Tile>(GetTileNameWall(h));
            layerGround.SetTile((Vector3Int)h, groundWall);
        }
        public string GetTileNameWall(HexTile h)
        {
            return rt.DECORATE2WALL(deco, rt.wallAround2WALLTYPE(h));
        }

        public void SetTileWalls(HexTile h, int r)
        {
            if ( !h.ex_here ){ return; }

            Tile groundWall = Resources.Load<Tile>(GetTileNameWall(h));
            layerGround.SetTile((Vector3Int)h, groundWall);
        }

    }

    public class HexTile : Hex
    {
        private MapData map;
        public bool ex_here
        {
            get { return map.hasWall(this); }
        }
        public bool ex_a
        {
            get { return map.hasWall(this + gen.A); }
        }
        public bool ex_ab
        {
            get { return map.hasWall(this + gen.AB); }
        }
        public bool ex_b
        {
            get { return map.hasWall(this + gen.B); }
        }
        public bool ex_bc
        {
            get { return map.hasWall(this + gen.BC); }
        }
        public bool ex_c
        {
            get { return map.hasWall(this + gen.C); }
        }
        public bool ex_ca
        {
            get { return map.hasWall(this + gen.CA); }
        }
        public HexTile(MapData map, int a, int b, int c, bool isGlobal = false)
         : base(a, b , c, isGlobal)
        {
            this.map = map;
        }
        public HexTile(MapData map, Hex h)
         : base(h.a, h.b , h.c, h.isGlobal)
        {
            this.map = map;
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



    }


}
