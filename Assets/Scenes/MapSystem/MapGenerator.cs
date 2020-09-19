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
        MapData map;
        HexTile generatedCenter;
        int generatedRadius;



        public void SetTileGround(HexUnit h)
        {

        }
        public List<Hex> GetGeneratePointsOneUnitSides(gen.unit u)
        {
            List<Hex> ret = new List<Hex>() { };
            gen.L1CircleOneEdgePointsLeft(generatedRadius + 1, u, ret);
            gen.L1CircleOneEdgePointsRight(generatedRadius + 1, u, ret);
            return ret;
        }
        public List<Hex> GetGenerateNodes(int r)
        {
            List<Hex> ret = new List<Hex>() { };
            
            for (int n = generatedRadius + 1; n <= r; n++)
            {
                gen.L1OddOrEvenNodes(n, ret);
            }
            return ret;
        }
        public Hex GetGenerateNode(gen.unit u)
        {
            return gen.unit2Hex[u] * (generatedRadius + 1);
        }


        public void InitialTile(int r)
        {
            
        }
        
    }

}
