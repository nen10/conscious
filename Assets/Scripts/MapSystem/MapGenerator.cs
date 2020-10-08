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
        public Tiling til;
        public MapData map;


        public MapGenerator(HexTile center, int sightRadius, int r = 0)
        {
            this.generatedCenter = center;
            this.generatedRadius = r;
            this.sightRadius = sightRadius;
            this.til = generatedCenter.til;
            this.map = generatedCenter.map;
        }

        public MapGenerator(MapData map, int sightRadius, int r = 0)
        {
            this.map = map;
            this.til = map.til;
            this.generatedCenter = new HexTile(map, 0, 0, 0);
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
                (ht + h).SetTileGround(gen.unitRotate(u, rot2));
            }
        }
        public static void SetTileGroundInductive(HexTile ht, int r, gen.unit u, int rot1, Tile t)
        {
            foreach(Hex h in gen.refractEmission(r, u, rot1, new List<Hex>(){}))
            {
                (ht + h).SetTileGround(t);
            }
        }
        public void SetTileGroundHorizon(gen.unit step)
        {
            (generatedCenter + gen.liner(step, generatedRadius)).SetTileGroundInit();
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
                (generatedCenter +　gen.liner(u, generatedRadius)).SetTileGround(gen.unitRotate(u, 3));
            }
        }
        public void SetTileGroundCircle(int r)
        {
            if(generatedRadius == 0)
            {
                generatedCenter.SetTileGroundInit();
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
            bool properSet = false;
            for (int r = minRadius; r <= generatedRadius; r++)
            {
                int count = 0;
                foreach(Hex h in gen.L1CircleAllEdgePoints(r, new List<Hex>(){}))
                {
                    HexTile genH = ht + h;
                    properSet = genH.SetTileBlaind(gt(r < sightRadius, til.GetTilePathBlaind(r == sightRadius ? blaindId(r, count) : 0)));
                    properSet |= genH.SetTileWall(gt(r > sightRadius || !(genH).hasWall(), til.GetTilePathWall(genH)));

                    map.Hex2MoveCost.Add(genH, properSet ? MapData.MOVECOST_BLOCKED : MapData.MOVECOST_EMPTY);
                    count++;
                }
            }
        }

        public void UpdateTileByStepOverGroundHorizon(HexUnit u)
        {
            HexTile nextCenter = generatedCenter + u;
            gen.unit d = u.d;
            gen.unit bd = gen.unitRotate(d, 3);

            (nextCenter + gen.liner(d, generatedRadius)).SetTileGroundInit();

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
        
        
        public static int blaindId(int sightRadius, int count)
        {
            return ((count % sightRadius) == 0 ? 1 : 2) + ((int)(count / sightRadius) * 2);
        }

        public string GenerateTileNameGroundNotWalled()
        {
            return til.GetTilePathGround(Naming.GROUNDTYPE.a);
        }
    }

}
