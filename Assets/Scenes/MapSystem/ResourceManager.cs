using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapSystem.SpriteManager;

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
        public enum WALLTOP
        {
            iso = 1,
            ab = 2,
            ca = 3,
            a = 0,
        }
        public enum WALLBOTTOM
        {
            bc = 0,
            iso = 1,
            b = 2,
            b_c = 3,
            c = 4,
        }

        public static int wallAround2WALLTYPE(HexTile h)
        {
            int top = 1;
            top *= h.ex_a ? (int)WALLTOP.a : 1;
            top *= h.ex_ab ? (int)WALLTOP.ab : 1;
            top *= h.ex_ca ? (int)WALLTOP.ca : 1;
            top %= 6;

            int bottom = 1;
            bottom *= h.ex_bc ? (int)WALLBOTTOM.bc : 1;
            bottom *= h.ex_b ? (int)WALLBOTTOM.b : 1;
            bottom *= h.ex_c ? (int)WALLBOTTOM.c : 1;
            bottom %= 5;

            return top + (bottom * 4);
        }
        public static string DECORATE2WALL(DECORATETYPE d, int walltype )
        {
            return DECORATE2WALLSET[d] + walltype.ToString();
        }

    }
}
