using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapSystem.SpriteManager;
using MapSystem.HexCoordinateSystem;
using gen = MapSystem.HexCoordinateSystem.HexGenerator;

namespace MapSystem.ResourceManager
{
    public struct Tilemap
    {
        public enum DECORATETYPE
        {
            template
        }
        public enum GROUNDTYPE 
        {
            wall,
            a,
            b,
            c,
        }
        public static readonly Dictionary<DECORATETYPE, Dictionary<GROUNDTYPE, string>> DECORATE2GROUND 
        = new Dictionary<DECORATETYPE, Dictionary<GROUNDTYPE, string>>()
        {
            {DECORATETYPE.template, new Dictionary<GROUNDTYPE, string>()
                {
                    {GROUNDTYPE.a, "bmapchip_ground_template_0"}, 
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

        public static readonly Dictionary<DECORATETYPE, string> DECORATE2WALLSET 
        = new Dictionary<DECORATETYPE, string>()
        {
            {DECORATETYPE.template, "Tilemap/mapchip_wall_template_"}, 
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

        public static int GetWALLTYPE(HexTile h)
        {
            int top = (int)around.iso;
            top *= walltypeGenerator(h, gen.unit.a);
            top *= walltypeGenerator(h, gen.unit.ab);
            top *= walltypeGenerator(h, gen.unit.ca);
            top %= 6;

            int bottom = (int)around.iso;
            bottom *= walltypeGenerator(h, gen.unit.bc);
            bottom *= walltypeGenerator(h, gen.unit.b);
            bottom *= walltypeGenerator(h, gen.unit.c);
            bottom %= 5;

            return top + (bottom * 4);
        }
        private static int walltypeGenerator(HexTile h, gen.unit u)
        {
            return (int)(h.hasWall(gen.unit2Hex[u]) ? unit2around[u] : around.iso);
        }
        public static string GetTileNameWall(DECORATETYPE d, HexTile h)
        {
            return DECORATE2WALLSET[d] + GetWALLTYPE(h).ToString();
        }


    }
}
