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
            return new Vector3Int(x, v.y / 2, -x);
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
            return new Vector3(x, (float)v.y / 2, -x);
        }
        public static Vector3Int flatYXZHex2sqYXZHex(Vector3 v)
        {
            return new Vector3Int((int)(v.x * 2), (int)(v.y * 2), 0);
        }
        public static Vector3 Z3Hex2FlatYXZHex(Hex h)
        {
            if(!h.isGlobal)
            {
                return new Vector3Int();
            }
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


    }
    public struct HexGenerator
    {
        public static readonly Hex A = new Hex(1, 0, 0);
        public static readonly Hex B = new Hex(0, 1, 0);
        public static readonly Hex C = new Hex(0, 0, 1);
        public static readonly Hex BC = -A;
        public static readonly Hex CA = -B;
        public static readonly Hex AB = -C;

        public enum area
        {
            ab = 0,
            ba = 1,
            bc = 2,
            cb = 3,
            ca = 4,
            ac = 5,
        }

        public static area Hex2area(Hex h)
        {
            int mi = Min(h.a, h.c);
            int ma = Max(h.a, h.c);

            int a = h.a - mi;

            if (mi == 0 && ma == 0){
                return area.ab;
            }
            if (mi >= 0){
                return a >= 0 ? area.ac : area.ca;
            }
            if (ma < 0){
                return a == 0 ? area.bc : area.ba;
            }
            return a == 0 ? area.cb : area.ab;
        }
        public static readonly Dictionary<area, (Hex,Hex)> area2PositiveBasis
        = new Dictionary<area, (Hex, Hex)>()
        {
            {area.ab, (A, B)}, 
            {area.ba, (B, A)}, 
            {area.bc, (B, C)}, 
            {area.cb, (C, B)}, 
            {area.ca, (C, A)}, 
            {area.ac, (A, C)}, 
        };
        public static List<Hex> L1CircumferenceEdgePoints(int r, area d, List<Hex> ret)
        {

            Hex initiative = area2PositiveBasis[d].Item1;
            Hex sub = area2PositiveBasis[d].Item2;

            for (int i = 1; i < r; i++)
            {
                ret.Add((initiative * r) + (sub * i));
            }
            return ret;
        }

        public static List<Hex> L1CircumferencePoints(int r, List<Hex> ret)
        {
            for (int d = 0; d <= 5 * Min(1,r); d++)
            {
                ret.Add(area2PositiveBasis[(area)d].Item1 * r);
                L1CircumferenceEdgePoints(r, (area)d, ret);

            }
            return ret;
        }

    }

}
