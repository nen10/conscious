using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapSystem.HexCoordinateSystem;
using MapSystem.SpriteManager;
using gen = MapSystem.HexCoordinateSystem.HexGenerator;
using tr = MapSystem.HexCoordinateSystem.Transform;
using rt = MapSystem.ResourceManager.Tilemap;

namespace MapSystem
{
    public class MapGenerator
    {
        HexTile generatedCenter;
        int generatedRadius;
        int sightRadius;


        public MapGenerator(HexTile center, int sightRadius, int r = 0)
        {
            this.generatedCenter = center;
            this.generatedRadius = r;
            this.sightRadius = sightRadius;
        }

        public void SetTileGroundHorizon(gen.unit step)
        {
            generatedCenter.SetTileGroundInit(gen.liner(step, generatedRadius + 1));

            foreach(Hex h in gen.refractEmission(generatedRadius, step, 2, new List<Hex>(){}))
            {
                generatedCenter.SetTileGround(h, gen.unitRotate(step, 4));
            }
            foreach(Hex h in gen.refractEmission(generatedRadius, step, 4, new List<Hex>(){}))
            {
                generatedCenter.SetTileGround(h, gen.unitRotate(step, 2));
            }
        }
        public void SetTileGroundHorizons()
        {
            List<gen.unit> endPoints = new List<gen.unit>(){};

            foreach(gen.unit u in gen.unit2Hex.Keys)
            {
                if((int)u % 2 == generatedRadius % 2)
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
                generatedCenter.SetTileGround(gen.liner(u, generatedRadius + 1), gen.unitRotate(u, 3));
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
                SetTileGroundHorizons();
                generatedRadius += 1;
            }
        }

        public void SetTileOverGround()
        {
            for (int r = 1; r <= generatedRadius; r++)
            {
                SetTileOverGround(r);
            }
        }
        public void SetTileOverGround(int radius)
        {
            foreach(Hex h in gen.L1CircleAllEdgePoints(radius, new List<Hex>(){}))
            {
                if (radius <= sightRadius)
                { 
                    generatedCenter.SetTileWall(h);
                }
                if (radius >= sightRadius)
                {
                    generatedCenter.SetTileBlaind(h);
                }
            }
        }

        public void UpdateTileByStepOverGroundHorizon(HexUnit u)
        {
            HexTile nextCenter = generatedCenter + u;
            gen.unit d = u.d;
            gen.unit bd = gen.unitRotate(d, 3);

            nextCenter.SetTileGroundInit(gen.liner(d, generatedRadius));
            nextCenter.SetTileBlaind(gen.liner(d, generatedRadius));
            foreach(Hex h in gen.refractEmission(generatedRadius, d, 2, new List<Hex>(){}))
            {
                nextCenter.SetTileGround(h, gen.unitRotate(d, 4));
                nextCenter.SetTileBlaind(h);
            }
            foreach(Hex h in gen.refractEmission(generatedRadius, d, 4, new List<Hex>(){}))
            {
                nextCenter.SetTileGround(h, gen.unitRotate(d, 2));
                nextCenter.SetTileBlaind(h);
            }

            foreach(Hex h in gen.L1CircleNodeSideTwoEdgesPoints(generatedRadius, d, new List<Hex>(){}))
            {
                nextCenter.RemoveTileWall(h);
                generatedCenter.RemoveTileGround(h);
            }

            foreach(Hex h in gen.L1CircleNodeSideTwoEdgesPoints(sightRadius - 1, d, new List<Hex>(){}))
            {
                nextCenter.SetTileWall(h);
                nextCenter.RemoveTileBlaind(h);
            }
            foreach(Hex h in gen.L1CircleNodeSideTwoEdgesPoints(sightRadius - 1, bd, new List<Hex>(){}))
            {
                generatedCenter.SetTileBlaind(h);
            }
            foreach(Hex h in gen.L1CircleNodeSideTwoEdgesPoints(generatedRadius, bd, new List<Hex>(){}))
            {
                nextCenter.RemoveTileWall(h);
                generatedCenter.RemoveTileGround(h);
                generatedCenter.RemoveTileBlaind(h);
            }
        }


        public void GenerateTileInit(int generateRadius)
        {
            // generateRadius >= sightRadius +1  であること！！！
            SetTileGroundCircle(generateRadius);
            SetTileOverGround();
        }
        public void UpdateTileByStep(HexUnit u)
        {
            UpdateTileByStepOverGroundHorizon(u);
            generatedCenter += u;
        }
        public void UpdateTileByTeleport(HexTile nextCenter)
        {
            Hex teleportPath = nextCenter - generatedCenter;
            foreach(HexUnit u in teleportPath.ResolveHex())
            {
                UpdateTileByStepOverGroundHorizon(u);
            }
        }
        
    }

}
