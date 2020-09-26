using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using UnityEngine.Tilemaps;
using MapSystem.HexCoordinateSystem;
using MapSystem.SpriteManager;
using MapSystem.ResourceManager;
using gen = MapSystem.HexCoordinateSystem.HexGenerator;

namespace MapSystem
{
    public class MapGenerator
    {
        public HexTile generatedCenter;
        public int generatedRadius;
        public int sightRadius;


        public MapGenerator(HexTile center, int sightRadius, int r = 0)
        {
            this.generatedCenter = center;
            this.generatedRadius = r;
            this.sightRadius = sightRadius;
        }


        public void SetTileGroundInductive(gen.unit u, int rot1, int rot2)
        {
            SetTileGroundInductive(generatedCenter, generatedRadius, u, rot1, rot2);
        }
        public static void SetTileGroundInductive(HexTile ht, int r, gen.unit u, int rot1, int rot2)
        {
            foreach(Hex h in gen.refractEmission(r, u, rot1, new List<Hex>(){}))
            {
                ht.SetTileGround(h, gen.unitRotate(u, rot2));
            }
        }
        public static void SetTileGroundInductive(HexTile ht, int r, gen.unit u, int rot1, Tile t)
        {
            foreach(Hex h in gen.refractEmission(r, u, rot1, new List<Hex>(){}))
            {
                ht.SetTileGround(h, t);
            }
        }
        public void SetTileGroundHorizon(gen.unit step)
        {
            generatedCenter.SetTileGroundInit(gen.liner(step, generatedRadius));
            SetTileGroundInductive(step, 2, 4);
            SetTileGroundInductive(step, 4, 2);
        }
        public void SetTileGroundHorizons()
        {
            List<gen.unit> endPoints = new List<gen.unit>(){};

            foreach(gen.unit u in gen.unit2Hex.Keys)
            {
                if((int)u % 2 != generatedRadius % 2)
                {
                    SetTileGroundHorizon(u);
                }
                else
                {
                    endPoints.Add(u);
                }
            }
            foreach(gen.unit u in endPoints)
            {
                generatedCenter.SetTileGround(gen.liner(u, generatedRadius), gen.unitRotate(u, 3));
            }
        }
        public void SetTileGroundCircle(int r)
        {
            if(generatedRadius == 0)
            {
                generatedCenter.SetTileGroundInit(gen.O);
            }
            while(generatedRadius < r)
            {
                generatedRadius++;
                SetTileGroundHorizons();
            }
        }

        public void SetTileOverGround(HexTile ht, int minRadius)
        {
            Func<bool, string, Tile> gt = (bool b, string s) => { return b ? null : Resources.Load<Tile>(s); };
            for (int r = minRadius; r <= generatedRadius; r++)
            {
                int count = 0;
                foreach(Hex h in gen.L1CircleAllEdgePoints(r, new List<Hex>(){}))
                {
                    ht.SetTileBlaind(h, gt(r < sightRadius, r == sightRadius ? ht.GetTilePathBlaind(r, count): ht.GetTilePathBlaind()));
                    ht.SetTileWall(h, gt(r > sightRadius, (ht + h).GetTilePathWall()));
                    count++;
                }
            }
        }

        public void UpdateTileByStepOverGroundHorizon(HexUnit u)
        {
            HexTile nextCenter = generatedCenter + u;
            gen.unit d = u.d;
            gen.unit bd = gen.unitRotate(d, 3);

            nextCenter.SetTileGroundInit(gen.liner(d, generatedRadius));

            SetTileGroundInductive(nextCenter,generatedRadius,d, 2, 4);
            SetTileGroundInductive(nextCenter,generatedRadius,d, 4, 2);
        }


        public void GenerateTileInit()
        {
            SetTileGroundCircle(sightRadius + 2);
            SetTileOverGround(generatedCenter, 1);
        }
        public void UpdateTileByStep(HexUnit u)
        {
            UpdateTileByStepOverGroundHorizon(u);
            generatedCenter += u;
            SetTileOverGround(generatedCenter, sightRadius - 1);
        }
        public void UpdateTileByTeleport(Hex teleportPath)
        {
            foreach(HexUnit u in teleportPath.ResolveHex())
            {
                UpdateTileByStepOverGroundHorizon(u);
                generatedCenter += u;
            }
            SetTileOverGround(generatedCenter, Max(1, sightRadius - teleportPath.norm));
        }
        public void UpdateTileBySceneBrake(HexTile nextCenter)
        {
            //
        }


        public static MapGenerator operator+ (MapGenerator g, HexUnit u)
        {
            g.UpdateTileByStep(u);
            return g;
        }
        public static MapGenerator operator+ (MapGenerator g, Hex h)
        {
            g.UpdateTileByTeleport(h);
            return g;
        }
        
        public static MapGenerator operator+ (MapGenerator g, HexTile h)
        {
            g.UpdateTileBySceneBrake(h);
            return g;
        }
        
        
    }

}
