using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapSystem.SpriteManager;
using MapSystem.HexCoordinateSystem;
using gen = MapSystem.HexCoordinateSystem.HexGenerator;

namespace MapSystem.ResourceManager
{
    public class Tiling
    {
        public readonly Naming.DECORATETYPE deco;
        public Tiling(Naming.DECORATETYPE deco)
        {
            this.deco = deco;
        }

        public string GetTilePathWall(HexTile h)
        {
            return Naming.GetTilePathWall(deco, h);
        }
        public string GetTilePathBlaind(int blaindId)
        {
            return Naming.GetTilePathBlaind(deco, blaindId);
        }
        public string GetTilePathGround(int value)
        {
            return GetTilePathGround((Naming.GROUNDTYPE)value);
        }
        public string GetTilePathGround(Naming.GROUNDTYPE value)
        {
            return Naming.DECORATE2GROUND[deco][value];
        }

        public string GetTileNameGroundWall()
        {
            return GetTilePathGround(Naming.GROUNDTYPE.wall).Split('/')[1];
        }
    }
    public struct Naming
    {

        public enum DECORATETYPE
        {
            template
        }
        public enum TILESETTYPE
        {
            ground,
            wall,
            blaind,
        }
        public enum GROUNDTYPE 
        {
            wall,
            a,
            b,
            c,
        }
        public static readonly string TILEDIRECTORY = "Tilemap/";
        public static readonly Dictionary<DECORATETYPE, Dictionary<GROUNDTYPE, string>> DECORATE2GROUND 
        = new Dictionary<DECORATETYPE, Dictionary<GROUNDTYPE, string>>()
        {
            {DECORATETYPE.template, new Dictionary<GROUNDTYPE, string>()
                {
                    {GROUNDTYPE.a, "Tilemap/mapchip_ground_template_0"}, 
                    {GROUNDTYPE.b, "Tilemap/mapchip_ground_template_5"}, 
                    {GROUNDTYPE.c, "Tilemap/mapchip_ground_template_10"}, 
                    {GROUNDTYPE.wall, "Tilemap/mapchip_ground_template_15"},
                }
            },
        };
        public static string GroundWallName(DECORATETYPE d)
        {
            return DECORATE2GROUND[d][GROUNDTYPE.wall];
        }

        public static readonly Dictionary<DECORATETYPE, Dictionary<TILESETTYPE, string>> DECORATE2TILESET 
        = new Dictionary<DECORATETYPE, Dictionary<TILESETTYPE, string>>()
        {
            {DECORATETYPE.template, new Dictionary<TILESETTYPE, string>()
                {
                    {TILESETTYPE.ground, "Tilemap/mapchip_ground_template_"}, 
                    {TILESETTYPE.wall, "Tilemap/mapchip_wall_template_"}, 
                    {TILESETTYPE.blaind, "Tilemap/mapchip_blaind_template_"}, 
                }
            },
        };
        public enum around
        {
            iso = 1,

            // top
            ab = 2,
            ca = 3,
            a = 0,

            //bottom
            bc = 0,
            b = 2,
            c = 4,
            b_and_c = 3,
            
        }

        public static readonly Dictionary<gen.unit, around> unit2around
        = new Dictionary<gen.unit, around>()
        {
            {gen.unit.a, around.a}, 
            {gen.unit.ab, around.ab}, 
            {gen.unit.b, around.b}, 
            {gen.unit.bc, around.bc}, 
            {gen.unit.c, around.c}, 
            {gen.unit.ca, around.ca},  
        };

        public static readonly Dictionary<gen.unit, int> unit2blaindArea
        = new Dictionary<gen.unit, int>()
        {
            {gen.unit.a, 1}, 
            {gen.unit.ab, 3}, 
            {gen.unit.b, 5}, 
            {gen.unit.bc, 7}, 
            {gen.unit.c, 9}, 
            {gen.unit.ca, 11},  
        };
        public static int unit2blaindId(gen.unit u, bool isInductive = false)
        {
            return unit2blaindArea[u] + (isInductive ? 1 : 0);
        }

        public static int GetWALLTYPE(HexTile h)
        {
            int top = (int)around.iso;
            top *= walltypeDivider(h, gen.unit.a);
            top *= walltypeDivider(h, gen.unit.ab);
            top *= walltypeDivider(h, gen.unit.ca);
            top %= 6;

            int bottom = (int)around.iso;
            bottom *= walltypeDivider(h, gen.unit.bc);
            bottom *= walltypeDivider(h, gen.unit.b);
            bottom *= walltypeDivider(h, gen.unit.c);
            bottom %= 5;

            return top + (bottom * 4);
        }
        private static int walltypeDivider(HexTile h, gen.unit u)
        {
            return (int)(h.hasWall(gen.unit2Hex[u]) ? unit2around[u] : around.iso);
        }
        public static string GetTilePathWall(DECORATETYPE d, HexTile h)
        {
            return DECORATE2TILESET[d][TILESETTYPE.wall] + GetWALLTYPE(h).ToString();
        }

        public static string GetTilePathBlaind(DECORATETYPE d, int blaindId)
        {
            return DECORATE2TILESET[d][TILESETTYPE.blaind] + blaindId.ToString();
        }

    }
}
