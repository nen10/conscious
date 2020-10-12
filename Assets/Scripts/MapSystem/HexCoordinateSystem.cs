using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using spr = MapSystem.SpriteManager;

namespace MapSystem.HexCoordinateSystem
{
    public struct MathHex
    {
        public static int SafeMod(int num, int devider)
        {
            return (num + devider) % devider;
        }
        public static Vector3Int Z3Hex2sqYXZHex(Hex h)
        {
            return new Vector3Int(
                h.a * 2 - h.c,
                h.c * 2,
                0);
            /*
            return new Vector3Int(
                h.a * 2 - (h.b + h.c),
                (h.c - h.b) * 2,
                h.b + h.c - h.a);
            */
        }
        public static Vector3Int sqYXZHex2UnityYXZHex(Vector3Int v)
        {
            int x = ((v.x % 2 == 0) ? v.x : v.x - 1) / 2;
            return new Vector3Int(x, v.y / 2, 0);
        }
        public static Vector3Int Z3Hex2UnityYXZHex(spr.HexSprite h)
        {
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
        public static Hex UnityYXZHex2Z3Hex(Vector3Int v, bool isGlobal = true)
        {
            return sqYXZHex2Z3Hex(UnityYXZHex2sqYXZHex(v), isGlobal);
        }
    }
    public class Hex
    {
        public int a{get; private set;}
        public int c{get; private set;}
        public bool isGlobal;

        public int norm
        {
            get { return L1Norm(); }
        }

        public Hex()
        {
            this.a = 0;
            this.c = 0;
            this.isGlobal = false;
        }
        public Hex(int a, int b, int c, bool isGlobal = false)
        {
            this.a = a - b;
            this.c = c - b;
            this.isGlobal = isGlobal;
        }
        public override bool Equals(object obj)
        {
            return obj is Hex hex &&
                   a == hex.a &&
                   c == hex.c &&
                   isGlobal == hex.isGlobal ;
        }
        public override int GetHashCode()
        {
            int hashCode = 296152123;
            hashCode = hashCode * -1521134295 + a.GetHashCode();
            hashCode = hashCode * -1521134295 + c.GetHashCode();
            hashCode = hashCode * -1521134295 + isGlobal.GetHashCode();
            return hashCode;
        }
        public static bool operator== (Hex h1, Hex h2)
        {
            return h1.Equals(h2);
        }
        public static bool operator!= (Hex h1, Hex h2)
        {
            return !(h1 == h2);
        }

        public static Hex operator+ (Hex h1, Hex h2)
        {
            return new Hex(
                h1.a + h2.a,
                0,
                h1.c + h2.c,
                h1.isGlobal ^ h2.isGlobal
                );
        }
        public static Hex operator- (Hex h1, Hex h2)
        {
            return new Hex(
                h1.a - h2.a,
                0,
                h1.c - h2.c,
                h1.isGlobal && ! h2.isGlobal
                );
        }
        public static Hex operator* (Hex h, int n)
        {
            return new Hex(
                h.a * n,
                0,
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
        public HexUnit HexUnit(Hex h)
        {
            return h.norm == 1 ? new HexUnit(h) : null;
        }

        public List<HexUnit> ResolveHex()
        {
            return (new HexReduct(this)).ResolveHex();
        }
        public static int L1Distance(Hex h1, Hex h2)
        {
            return (h1 - h2).norm;
        }

    }
    public class HexUnit : Hex
    {
        public HexGenerator.unit d
        {
            get { return Hex2unit(); }

        }
        public HexUnit(Hex h)
         : base (h.a, 0, h.c, false)
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
    public class HexReduct
    {
        (int, int, int) hex;
        (HexUnit, int) top
        {
            get { return (hex.Item1 >= 0 ? HexGenerator.A : HexGenerator.BC, Abs(hex.Item1)); }
        }
        (HexUnit, int) left
        {
            get { return (hex.Item2 >= 0 ? HexGenerator.B : HexGenerator.CA, Abs(hex.Item2)); }
        }
        (HexUnit, int) right
        {
            get { return (hex.Item3 >= 0 ? HexGenerator.C : HexGenerator.AB, Abs(hex.Item3)); }
        }
        public List<(HexUnit, int)> properRepresent(List<(HexUnit, int)> ret)
        {
            Action<(HexUnit, int)> act = ((HexUnit, int) x) => { if (x.Item2 != 0) { ret.Add(x); } };
            act(this.top);
            act(this.left);
            act(this.right);
            return ret;
        }
        public HexReduct(Hex h)
        {
            this.hex = reduct(h);
        }

        public static (int, int, int) reduct(Hex h)
        {
            int a = h.a;
            int b = 0;
            int c = h.c;
            int min = Min(h.a, h.c);
            int max = Max(h.a, h.c);
            if(min * max > 0)
            {
                int sub = min > 0 ? min : max;
                a -= sub;
                b -= sub;
                c -= sub;
            }
            // 各成分の絶対値の総和を最小化する座標系であらわしたHex
            return (a, b, c);
        }
        public List<HexUnit> ResolveHex()
        {
            List<HexUnit> ret = new List<HexUnit>(){};

            List<(HexUnit, int)> proper = this.properRepresent(new List<(HexUnit, int)>(){});

            if(proper.Count == 2)
            {
                int largeKey = (proper[0].Item2 > proper[1].Item2) ? 0 : 1;
                int smallKey = (largeKey + 1 ) % 2;

                (int, int) sub = (proper[largeKey].Item2 / proper[smallKey].Item2, proper[largeKey].Item2 % proper[smallKey].Item2);
                for (int i = 0; i <= sub.Item2; i++)
                {
                    ret.Add(proper[largeKey].Item1);
                }
                for (int i = 0; i <= proper[smallKey].Item2; i++)
                {
                    ret.Add(proper[smallKey].Item1);
                    for (int j = 0; j <= sub.Item1; j++)
                    {
                        ret.Add(proper[largeKey].Item1);
                    }
                }
            }else if(proper.Count == 1){
                for (int i = 0; i <= proper[0].Item2; i++)
                {
                    ret.Add(proper[0].Item1);
                }
            }
            // HexUnitの和に分解して表したHex
            return ret;
        }
    }
    public readonly struct HexGenerator
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

        public static unit unitRotate(unit u, int rot)
        {
            return (unit)(((int)u + rot) % 6);
        }

        public static List<Hex> refractEmission(int r, unit u, int refract, List<Hex> ret)
        {
            for (int i = 1; i < r; i++)
            {
                ret.Add(liner(u, r) + rotateliner(u, i, refract));
            }
            return ret;
        }

        public static List<Hex> L1CircleAllEdgePoints(int r, List<Hex> ret)
        {
            foreach(unit u in unit2Hex.Keys)
            {
                ret.Add(liner(u, r));
                refractEmission(r, u, 2, ret);
            }
            return ret;
        }

        public static List<Hex> L1CircleNodeSideTwoEdgesPoints(int r, unit u, List<Hex> ret)
        {
            ret.Add(liner(u, r));
            refractEmission(r, u, 2, ret);
            refractEmission(r, u, 4, ret);
            return ret;
        }

        public static Hex liner(unit u, int n)
        {
            return unit2Hex[u] * n;
        }
        public static Hex rotateliner(unit u, int n, int rot)
        {
            return liner(unitRotate(u, rot), n);
        }
    }

}
