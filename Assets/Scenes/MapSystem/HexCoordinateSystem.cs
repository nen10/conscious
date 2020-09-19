using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

namespace MapSystem.HexCoordinateSystem
{
    public struct Transform
    {
        public static Vector3Int Z3Hex2sqYXZHex(Hex h)
        {
            return new Vector3Int(
                h.a * 2 - (h.b + h.c),
                (h.c - h.b) * 2,
                h.b + h.c - h.a);
        }
        public static Vector3Int sqYXZHex2UnityYXZHex(Vector3Int v)
        {
            int x = ((v.x % 2 == 0) ? v.x : v.x - 1) / 2;
            return new Vector3Int(x, v.y / 2, 0);
        }
        public static Vector3Int Z3Hex2UnityYXZHex(Hex h)
        {
            if(!h.isGlobal)
            {
                return new Vector3Int();
            }
            return sqYXZHex2UnityYXZHex(Z3Hex2sqYXZHex(h));
        }
        public static Vector3Int UnityYXZHex2sqYXZHex(Vector3Int v)
        {
            int x = ((v.y % 2) == 0 ? v.x * 2 : v.x * 2 + 1);
            return new Vector3Int(x, v.y * 2, 0);
        }
        public static Hex sqYXZHex2Z3Hex(Vector3Int v, bool isGlobal = false)
        {
            return new Hex((v.x + v.y / 2) / 2, 0, v.y / 2, isGlobal);
        }
        public static Hex UnityYXZHex2Z3Hex(Vector3Int v)
        {
            return sqYXZHex2Z3Hex(UnityYXZHex2sqYXZHex(v), true);
        }
        public static Vector3 sqYXZHex2FlatYXZHex(Vector3Int v)
        {
            float x = (float)v.x / 2;
            return new Vector3(x, (float)v.y / 2, 0);
        }
        public static Vector3Int flatYXZHex2sqYXZHex(Vector3 v)
        {
            return new Vector3Int((int)(v.x * 2), (int)(v.y * 2), 0);
        }
        public static Vector3 Z3Hex2FlatYXZHex(Hex h)
        {
            return sqYXZHex2FlatYXZHex(Z3Hex2sqYXZHex(h));
        }
    }
    public class Hex
    {
        public int a
        {
            set { a = value; }
            get { return a - b; }
        }
        public int b
        {
            set { a -= value; c -= value; }
            get { return b - b; }
        }
        public int c
        {
            set { c = value; }
            get { return c - b; }
        }
        public bool isGlobal;

        public int norm
        {
            get { return L1Norm(); }
        }

        public Vector3 flatYXZHex
        {
            get { return Transform.Z3Hex2FlatYXZHex(this); }
        }
        public Vector3Int unityYXZHex
        {
            get { return Transform.Z3Hex2UnityYXZHex(this); }
        }
        public Hex()
        {
            this.a = 0;
            this.b = 0;
            this.c = 0;
            this.isGlobal = false;
        }
        public Hex(int a, int b, int c, bool isGlobal = false)
        {
            this.a = a - b;
            this.b = b - b;
            this.c = c - b;
            this.isGlobal = isGlobal;
        }
        public static Hex operator+ (Hex h1, Hex h2)
        {
            return new Hex(
                h1.a + h2.a,
                h1.b + h2.b,
                h1.c + h2.c,
                h1.isGlobal ^ h2.isGlobal
                );
        }
        public Hex add (Hex h)
        {
            this.a += h.a;
            this.b += h.b;
            this.c += h.c;
            this.isGlobal = this.isGlobal ^ h.isGlobal;
            return this;
        }
        public static Hex operator- (Hex h1, Hex h2)
        {
            return new Hex(
                h1.a - h2.a,
                h1.b - h2.b,
                h1.c - h2.c,
                h1.isGlobal && ! h2.isGlobal
                );
        }
        public Hex minus (Hex h)
        {
            this.a -= h.a;
            this.b -= h.b;
            this.c -= h.c;
            this.isGlobal = this.isGlobal && !h.isGlobal;
            return this;
        }
        public static Hex operator* (Hex h, int n)
        {
            return new Hex(
                h.a * n,
                h.b * n,
                h.c * n,
                h.isGlobal
                );
        }
        public static Hex operator- (Hex h)
        {
            return h * -1;
        }
        public int L1Norm()
        {
            return Max(Abs(this.a), Abs(this.c));
        }
        public static explicit operator Vector3Int(Hex h)
        {
            return h.unityYXZHex;
        }
        public static explicit operator Vector3(Hex h)
        {
            return h.flatYXZHex;
        }
        public HexUnit HexUnit(Hex h)
        {
            return h.norm == 1 ? new HexUnit(h) : null;
        }


    }
    public class HexUnit : Hex
    {
        public HexGenerator.unit d
        {
            get { return Hex2unit(); }

        }
        public HexUnit(Hex h)
         : base (h.a, h.b, h.c, false)
        {
        }
        public HexUnit(int a, int b, int c)
         : base (a, b, c, false)
        {
        }
        public static HexUnit operator- (HexUnit h)
        {
            return new HexUnit((Hex)h * -1);
        }
        public HexGenerator.unit Hex2unit()
        {
            int max = Max(this.a, this.c);
            int min = Min(this.a, this.c);
            if (min == 0)
            {
                return this.a == 0 ? HexGenerator.unit.c : HexGenerator.unit.a; 
            }
            if (max == 0)
            {
                return this.a == 0 ? HexGenerator.unit.ab : HexGenerator.unit.bc; 
            }

            return min > 0 ? HexGenerator.unit.ca : HexGenerator.unit.b; 

        }

    }
    public struct HexGenerator
    {
        public static readonly Hex O = new Hex(0, 0, 0);
        public static readonly HexUnit A = new HexUnit(1, 0, 0);
        public static readonly HexUnit B = new HexUnit(0, 1, 0);
        public static readonly HexUnit C = new HexUnit(0, 0, 1);
        public static readonly HexUnit BC = -A;
        public static readonly HexUnit CA = -B;
        public static readonly HexUnit AB = -C;

        public enum unit
        {
            a = 0,
            ab = 1,
            b = 2,
            bc = 3,
            c = 4,
            ca = 5,

        }

        public static readonly Dictionary<unit, HexUnit> unit2Hex
        = new Dictionary<unit, HexUnit>()
        { 
            {unit.a, A}, 
            {unit.ab, AB}, 
            {unit.b, B}, 
            {unit.bc, BC}, 
            {unit.c, C}, 
            {unit.ca, CA}, 
        };

        public static HexUnit unit2HexRotate(unit u, int rot)
        {
            return unit2Hex[(unit)(((int)u + rot) % 6)];
        }
        public static List<Hex> L1CircleOneEdgePointsLeft(int r, unit u, List<Hex> ret)
        {
            for (int i = 1; i < r; i++)
            {
                ret.Add((unit2Hex[u] * r) + (unit2HexRotate(u, 2) * i));
            }
            return ret;
        }
        public static List<Hex> L1CircleOneEdgePointsRight(int r, unit u, List<Hex> ret)
        {
            for (int i = 1; i < r; i++)
            {
                ret.Add((unit2Hex[u] * r) + (unit2HexRotate(u, 4) * i));
            }
            return ret;
        }

        public static List<Hex> L1CircleNodeSideTwoEdgesPoints(int r, unit u, List<Hex> ret)
        {
            L1CircleOneEdgePointsLeft(r, u, ret);
            L1CircleOneEdgePointsRight(r, u, ret);

            for (int i = 1; i < r; i++)
            {
                ret.Add((unit2Hex[u] * r) + (unit2HexRotate(u, 2) * i));
            }
            return ret;
        }
        public static List<Hex> L1CircleAllEdgePoints(int r, List<Hex> ret)
        {
            foreach(unit u in unit2Hex.Keys)
            {
                ret.Add(unit2Hex[u] * r);
                L1CircleOneEdgePointsLeft(r, u, ret);
            }
            return ret;
        }

        public static List<Hex> L1OddOrEvenNodes(int r, List<Hex> ret)
        {
            foreach(unit u in unit2Hex.Keys)
            {
                if((int)u % 2 == r % 2){
                    ret.Add(unit2Hex[u] * r);
                }
            }
            return ret;
        }

    }

}
