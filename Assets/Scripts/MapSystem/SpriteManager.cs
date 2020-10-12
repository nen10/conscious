using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Mathf;
using MapSystem.HexCoordinateSystem;
using gen = MapSystem.HexCoordinateSystem.HexGenerator;
using mh = MapSystem.HexCoordinateSystem.MathHex;
using MapSystem.ResourceManager;
using CharacterSystem;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using System.ComponentModel.Design.Serialization;
using System.Linq;

namespace MapSystem.SpriteManager 
{
    
    public class MapData
    {
        public Grid oneFloor;
        public Tilemap layerGround;
        public UnityEngine.Transform layerEntities;
        public Tilemap layerOnGround;
        public Tilemap layerBlaind;
        public UnityEngine.Transform a;
        public Tiling til;

        public Dictionary<HexSprite, int> Hex2MoveCost;
        public Dictionary<HexSprite, int> Hex2Trap;
        public static readonly int MOVECOST_EMPTY = 1;
        public static readonly int MOVECOST_EXIST_CHARACTER = 2;
        public static readonly int MOVECOST_BLOCKED = 65535;
        public readonly Vector3 A;
        public readonly Vector3 B;
        public readonly Vector3 C;
        public readonly Vector3 BC;
        public readonly Vector3 CA;
        public readonly Vector3 AB;

        public readonly Dictionary<gen.unit, Vector3> unit2World;
        public MapData(Grid floor, Tilemap ground, Tilemap onGround, Tilemap blaind, Tiling til)
        {
            this.oneFloor = floor;
            this.layerGround = ground;
            this.layerOnGround = onGround;
            this.layerBlaind = blaind;
            this.til = til;
            this.Hex2MoveCost = new Dictionary<HexSprite, int>() { };
            this.Hex2Trap = new Dictionary<HexSprite, int>() { };

            this.A = HexUnit2World(gen.A);
            this.B = HexUnit2World(gen.B);
            this.C = HexUnit2World(gen.C);
            this.BC = HexUnit2World(gen.BC);
            this.CA = HexUnit2World(gen.CA);
            this.AB = HexUnit2World(gen.AB);

            this.unit2World
            = new Dictionary<gen.unit, Vector3>()
            { 
                {gen.unit.a, A},
                {gen.unit.ab, AB},
                {gen.unit.b, B},
                {gen.unit.bc, BC},
                {gen.unit.c, C},
                {gen.unit.ca, CA},
            };
        }
        public Vector3 HexUnit2World(HexUnit u)
        {
            return oneFloor.CellToWorld(mh.sqYXZHex2UnityYXZHex(mh.Z3Hex2sqYXZHex(u)));
        }

        public static string GetTileName(TileBase t)
        {
            return t is null ? "" : t.name;
        }
        public static string GetTileName(Tilemap tm, HexSprite h)
        {
            return GetTileName(tm.GetTile((Vector3Int)h));
        }
        public static bool isNull(Tilemap tm, HexSprite h)
        {
            return tm.GetTile((Vector3Int)h) is null;
        }

        public bool hasGround(HexSprite h)
        {
            return ! isNull(layerGround, h);
        }
        public bool hasOnGround(HexSprite h)
        {
            // trap, wall or something.
            return ! isNull(layerOnGround, h);
        }
        public string GetGroundTileName(HexSprite h)
        {
            return GetTileName(layerGround, h);
        }

        public bool hasWall(HexSprite h)
        {
            return til.GetTileNameGroundWall() == GetTileName(layerGround,h);
        }

        public int GetMoveCost(HexSprite h)
        {
            return Hex2MoveCost[h] + (CharacterManager.Hex2Char.ContainsKey(h) ? MOVECOST_EXIST_CHARACTER : 0);
        }
        public Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> GetMinPath(
            HexSprite s,
            HexSprite e,
            int moveRange
            )
        {
            Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> minCostPath = new Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)>() { };
            minCostPath[s] = (0, new List<HexUnit>() { }, null);
            DeeperSightRecurcive(
                e, moveRange, minCostPath,
                DeeperSight(s, e, moveRange, minCostPath)
            );
            return minCostPath;
        }

        public Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> GetMinPaths(
            HexSprite s,
            int moveRange
            )
        {
            Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> minCostPath = new Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)>() { };
            minCostPath[s] = (0, new List<HexUnit>() {}, null);
            FlatSightRecurcive(
                s, moveRange, minCostPath, 
                FlatSight(s, s, moveRange, minCostPath)
            );
            return minCostPath;
        }
        public void DeeperSightRecurcive(
            HexSprite e,
            int moveRange,
            Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> minCostPath,
            int priority, // Hex.L1Distance(e, s)
            Dictionary<int, List<HexSprite>> lookDeeper
            )
        {
            for (int i = 0; i <= moveRange - priority; i++)
            {
                int p = priority + i;
                if (lookDeeper.ContainsKey(p))
                {
                    foreach(HexSprite h in lookDeeper[p])
                    {
                        // 既知のpathの move cost が
                        // p(=現在地点 l を経由して e に到達するときの最小の move cost)以下なら、
                        // 別の経路を探索する必要はない
                        if(minCostPath.ContainsKey(e) && minCostPath[e].Item1 <= p) return;

                        DeeperSightRecurcive(
                            e, moveRange, minCostPath, p,
                            DeeperSight(h, e, moveRange, minCostPath));
                    }
                }
            }
        }

        public void DeeperSightRecurcive(
            HexSprite e,
            int moveRange,
            Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> minCostPath,
            Dictionary<int, List<HexSprite>> lookDeeper
            )
        {
            Dictionary<int, List<HexSprite>> l = new Dictionary<int, List<HexSprite>>() { };
            foreach (KeyValuePair<int, List<HexSprite>> i in lookDeeper)
            {
                foreach (HexSprite h in i.Value)
                {
                    if(minCostPath.ContainsKey(h) && minCostPath[h].Item1 < i.Key) continue;
                    foreach (KeyValuePair<int, List<HexSprite>> item in DeeperSight(h, e, moveRange, minCostPath))
                    {
                        if(!l.ContainsKey(item.Key)) l[item.Key] = new List<HexSprite>() { };
                        l[item.Key].AddRange(item.Value);
                    }
                }
            }
            DeeperSightRecurcive(e, moveRange, minCostPath, l);
        }
        public void FlatSightRecurcive(
            HexSprite s,
            int moveRange,
            Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> minCostPath,
            Dictionary<int, List<HexSprite>> lookFlat
            )
        {
            Dictionary<int, List<HexSprite>> l = new Dictionary<int, List<HexSprite>>() { };
            foreach (KeyValuePair<int, List<HexSprite>> i in lookFlat)
            {
                foreach (HexSprite h in i.Value)
                {
                    if(minCostPath.ContainsKey(h) && minCostPath[h].Item1 < i.Key) continue;
                    foreach (KeyValuePair<int, List<HexSprite>> item in FlatSight(s, h, moveRange, minCostPath))
                    {
                        if(!l.ContainsKey(item.Key)) l[item.Key] = new List<HexSprite>() { };
                        l[item.Key].AddRange(item.Value);
                    }
                }
            }
            FlatSightRecurcive(s, moveRange, minCostPath, l);
        }
        public Dictionary<int, List<HexSprite>> DeeperSight(
            HexSprite s,
            HexSprite e,
            int moveRange,
            Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> minCostPath
            )
        {
            Dictionary<int, List<HexSprite>> lookDeeper = new Dictionary<int, List<HexSprite>>() { };
            foreach(HexUnit u in gen.unit2Hex.Values)
            {
                HexSprite h = s + u;
                // マップ情報による判断
                int c = minCostPath[s].Item1 + GetMoveCost(h);
                int d = Hex.L1Distance(e, h);
                HexSprite trapHex = minCostPath[s].Item3;

                // 現在地点 h に至る既知のpathの move cost が
                // 現在のmove cost以下なら、すでにその地点の周りは探索しはじめている
                if(minCostPath.ContainsKey(h) && minCostPath[h].Item1 <= c) continue;
                if(moveRange < c) continue; //深掘りしない
                List<HexUnit> np = new List<HexUnit>(minCostPath[s].Item2);
                np.Add(u);
                this.Hex2Trap = new Dictionary<HexSprite, int>() { };
                minCostPath[h] = (c, np, (trapHex is null) && this.Hex2Trap.ContainsKey(h) ? h : trapHex);
                if(moveRange < d + c) continue; //深掘りしない
                if(!lookDeeper.ContainsKey(c)) lookDeeper[c] = new List<HexSprite>() { };
                lookDeeper[c].Add(h);
            }
            return lookDeeper;
        }
        public Dictionary<int, List<HexSprite>> FlatSight(
            HexSprite s,
            HexSprite e,
            int moveRange,
            Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> minCostPath
            )
        {
            Dictionary<int, List<HexSprite>> lookFlat = new Dictionary<int, List<HexSprite>>() { };
            foreach(HexUnit u in gen.unit2Hex.Values)
            {
                HexSprite h = e + u;
                // マップ情報による判断
                int c = minCostPath[e].Item1 + GetMoveCost(h);
                int d = Hex.L1Distance(h, s);
                HexSprite trapHex = minCostPath[s].Item3;

                // 現在地点 h に至る既知のpathの move cost が
                // 現在のmove cost以下なら、すでにその地点の周りは探索しはじめている
                if(minCostPath.ContainsKey(h) && minCostPath[h].Item1 <= c) continue;
                if(moveRange < c ) continue;
                List<HexUnit> np = new List<HexUnit>(minCostPath[e].Item2);
                np.Add(u);
                minCostPath[h] = (c, np, (trapHex is null) && this.Hex2Trap.ContainsKey(h) ? h : trapHex);
                if(!lookFlat.ContainsKey(c)) lookFlat[c] = new List<HexSprite>() { };
                lookFlat[c].Add(h);
            }
            return lookFlat;
        }

        public Hex WorldToHex(Vector3 w, bool isGlobal = false)
        {
            return MathHex.UnityYXZHex2Z3Hex(this.oneFloor.WorldToCell(w), isGlobal);
        }
        public HexSprite WorldToHexSprite(Vector3 w, bool isGlobal = true)
        {
            return new HexSprite(this, MathHex.UnityYXZHex2Z3Hex(this.oneFloor.WorldToCell(w), isGlobal));
        }
    }

    public class HexSprite : Hex
    {
        public  MapData map;
        public Tiling til { get { return this.map.til; } }
        public static readonly Vector3 CENTERING_PIXEL_MOBS = new Vector3(0,0.07f,2);
        public static readonly Vector3 CENTERING_POINTER = new Vector3(0,0,4);
        public HexSprite(MapData map, int a, int b, int c)
         : base(a, b, c, true)
        {
            this.map = map;
        }
        public HexSprite(MapData map, Hex h)
         : base(h.a, 0, h.c, h.isGlobal)
        {
            this.map = h.isGlobal ? map : null;
        }
        public HexSprite(MapData map, Vector3 w, Hex h)
         : base(h.a, 0, h.c, h.isGlobal)
        {
            this.map = h.isGlobal ? map : null;
        }
        public Vector3Int unityYXZHex
        {
            get { return mh.Z3Hex2UnityYXZHex(this); }
        }
        public Vector3 unityXYZ
        {
            get { return this.map.oneFloor.CellToWorld((Vector3Int)this); }
        }
        public Vector3 positonWorldPixelMobs()
        {
            return this.unityXYZ + CENTERING_PIXEL_MOBS;
        }
        public Vector3 positonWorldPointer()
        {
            return this.unityXYZ + CENTERING_POINTER;
        }
        public static explicit operator Vector3Int(HexSprite h)
        {
            return h.unityYXZHex;
        }
        public static explicit operator Vector3(HexSprite h)
        {
            return h.unityXYZ;
        }

        public static HexSprite operator +(HexSprite h1, Hex h2)
        {
            return new HexSprite(h1.map, (Hex)h1 + h2);
        }

        public static HexSprite operator -(HexSprite h1, Hex h2)
        {
            return new HexSprite(h1.map, (Hex)h1 - h2);
        }

        public static HexSprite operator *(HexSprite h, int n)
        {
            return new HexSprite(h.map, (Hex)h * n);
        }
        public static HexSprite operator -(HexSprite h)
        {
            return h * -1;
        }

        public bool hasWall(HexUnit h)
        {
            return this.map.hasWall(this + h);
        }
        public bool hasWall()
        {
            return this.map.hasWall(this);
        }
        public string groundName
        {
            get { return this.map.GetGroundTileName(this); }
        }
        // public bool CameCharacterOnGround()
        // {
        //     if ((this).hasWall()) { return false; }
        //     map.layerGround.SetTile((Vector3Int)(this), Resources.Load<Tile>(til.GetTilePathGround((int)Naming.GROUNDTYPE.wall)));
        //     return true;
        // }
        // public void LeaveCharacterFromGround()
        // {
        //     map.layerGround.SetTile((Vector3Int)(this), Resources.Load<Tile>(til.GetTilePathGround((int)Naming.GROUNDTYPE.wall)));
        // }
        public void SetGridAsParent(GameObject ch)
        {
            ch.transform.SetParent(map.oneFloor.transform);
        }
        public HexSprite Clone()
        {
            return (HexSprite)MemberwiseClone();
        }
        public Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> GetMinPath(HexSprite e, int moveRange)
        {
            return map.GetMinPath(this, e, moveRange);
        }

        public Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> GetMinPaths(int moveRange)
        {

            return map.GetMinPaths(this,moveRange);
        }

    }
    public class HexTile : HexSprite
    {
        public HexTile(MapData map, int a, int b, int c)
         : base(map, a, b, c)
        {
        }
        public HexTile(MapData map, Hex h)
         : base(map, h)
        {
        }

        public static HexTile operator +(HexTile h1, Hex h2)
        {
            return new HexTile(h1.map, (Hex)h1 + h2);
        }

        public static HexTile operator -(HexTile h1, Hex h2)
        {
            return new HexTile(h1.map, (Hex)h1 - h2);
        }

        public static HexTile operator *(HexTile h, int n)
        {
            return new HexTile(h.map, (Hex)h * n);
        }
        public static HexTile operator -(HexTile h)
        {
            return h * -1;
        }


        public bool SetTileGroundInit()
        {
            return this.SetTileGround(Resources.Load<Tile>(GenerateTileNameGroundNotWalled()));
        }
        public bool SetTileGround(gen.unit generatePoint)
        {
            return this.SetTileGround(Resources.Load<Tile>(this.GenerateTileNameGround(generatePoint)));
        }
        public bool SetTileGround(Tile t)
        {
            this.map.layerGround.SetTile((Vector3Int)this, t);
            return !(t is null);
        }
        public bool SetTileWall(Tile t)
        {
            map.layerOnGround.SetTile((Vector3Int)this, t);
            return !(t is null);
        }
        public bool SetTileBlaind(Tile t)
        {
            map.layerBlaind.SetTile((Vector3Int)this, t);
            return !(t is null);
        }
        
        public string GenerateTileNameGround(gen.unit generatePoint)
        {
            string name = this.groundName;
            if(name != "") return Naming.TILEDIRECTORY + name;
            if(this.GetWallThreadshold(generatePoint) < Random.Range(0f, 8f)) return til.GetTilePathGround(Naming.GROUNDTYPE.wall);
            return this.GenerateTileNameGroundNotWalled();
        }
        public string GenerateTileNameGroundNotWalled()
        {
            return til.GetTilePathGround(Naming.GROUNDTYPE.a);
        }

        private int hasWallAround(gen.unit generatePoint)
        {
            int state = 0;
            state += hasWall(gen.unit2Hex[gen.unitRotate(generatePoint, 4)]) ? 1 : 0;
            state += hasWall(gen.unit2Hex[generatePoint]) ? 2 : 0;
            state += hasWall(gen.unit2Hex[gen.unitRotate(generatePoint, 2)]) ? 4 : 0;
            return state;
        }

        private float GetWallThreadshold(gen.unit generatePoint)
        {
            switch (hasWallAround(generatePoint))
            {
                case 7: return 1f;
                case 6: return 5f;
                case 5: return 7f;
                case 4: return 3f;
                case 3: return 5f;
                case 2: return 3f;
                case 1: return 3f;
                case 0: return 5f;
                default: return 8f;
            }
        }
    }
}
