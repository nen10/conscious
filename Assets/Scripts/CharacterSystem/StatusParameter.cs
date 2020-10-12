using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using MapSystem.HexCoordinateSystem;

namespace CharacterSystem.StatusParameter
{
    public readonly struct growparam
    {
        public struct rate
        {
            public static readonly float mult0000By1Lv = 0f;
            public static readonly float mult0001By1Lv = 1f;
            public static readonly float mult0500By100Lv = 1.0641f;
            public static readonly float mult0600By100Lv = 1.0660f;
            public static readonly float mult0700By100Lv = 1.0677f;
            public static readonly float mult0800By100Lv = 1.0692f;
            public static readonly float mult0900By100Lv = 1.0704f;
            public static readonly float mult1000By100Lv = 1.0716f;
            public static readonly float mult1100By100Lv = 1.0726f;
            public static readonly float mult1200By100Lv = 1.0735f;
            public static readonly float mult1300By100Lv = 1.0744f;
            public static readonly float mult1400By100Lv = 1.0752f;
            public static readonly float mult1500By100Lv = 1.0759f;
            public static readonly float mult1000By1000Lv = 1.006932f;
            public static readonly float mult1100By1000Lv = 1.007028f;
            public static readonly float mult1200By1000Lv = 1.007116f;
            public static readonly float mult1300By1000Lv = 1.007196f;

            // キャラクター成長率
            public static readonly float Up0 = mult1100By1000Lv;
            public static readonly float Up1 = mult1000By1000Lv;
            public static readonly float Up2 = mult1100By1000Lv;
            public static readonly float Up3 = mult1200By1000Lv;
            public static readonly float Up4 = mult1300By1000Lv;
            
            // 装備のRNG,SIG成長率
            public static readonly float scale0 = mult0001By1Lv;
            public static readonly float scale1 = mult0001By1Lv;
            public static readonly float scale2 = mult0001By1Lv;
            // 装備の成長率
            public static readonly float equip0 = mult1000By100Lv;
            public static readonly float equipH1 = mult1100By100Lv;
            public static readonly float equipH2 = mult1200By100Lv;
            public static readonly float equipH3 = mult1300By100Lv;
            public static readonly float equipH4 = mult1400By100Lv;
            public static readonly float equipH5 = mult1500By100Lv;
            public static readonly float equipL1 = mult0900By100Lv;
            public static readonly float equipL2 = mult0800By100Lv;
            public static readonly float equipL3 = mult0700By100Lv;
            public static readonly float equipL4 = mult0600By100Lv;
            public static readonly float equipL5 = mult0500By100Lv;
        }

        public static readonly Dictionary<int, float> rateDict
        = new Dictionary<int, float>()
        {
            {-2, rate.mult0000By1Lv},
            {-1, rate.mult0001By1Lv},
            { 0, rate.equip0},
            { 1, rate.equipH1},
            { 2, rate.equipH2},
            { 3, rate.equipH3},
            { 4, rate.equipH4},
            { 5, rate.equipH5},
            {11, rate.equipL1},
            {12, rate.equipL2},
            {13, rate.equipL3},
            {14, rate.equipL4},
            {15, rate.equipL5},
        };
        public static readonly Dictionary<string, Dictionary<characteristic, float>> role2rate 
        = new Dictionary<string, Dictionary<characteristic, float>>()
        {
            {"HP", new Dictionary<characteristic, float>()
                {
                    {characteristic.will, rate.Up0},
                    {characteristic.hostile, rate.Up4},
                    {characteristic.mercy, rate.Up1},
                    {characteristic.friendship, rate.Up2},
                    {characteristic.insight, rate.Up3},
                }
            },
            {"MOV", new Dictionary<characteristic, float>()
                {
                    {characteristic.will, rate.Up0},
                    {characteristic.hostile, rate.Up1},
                    {characteristic.mercy, rate.Up4},
                    {characteristic.friendship, rate.Up3},
                    {characteristic.insight, rate.Up2},
                }
            },
            {"CP", new Dictionary<characteristic, float>()
                {
                    {characteristic.will, rate.Up0},
                    {characteristic.hostile, rate.Up2},
                    {characteristic.mercy, rate.Up3},
                    {characteristic.friendship, rate.Up4},
                    {characteristic.insight, rate.Up1},
                }
            },
            {"DEX", new Dictionary<characteristic, float>()
                {
                    {characteristic.will, rate.Up0},
                    {characteristic.hostile, rate.Up3},
                    {characteristic.mercy, rate.Up2},
                    {characteristic.friendship, rate.Up1},
                    {characteristic.insight, rate.Up4},
                }
            },
        };

        public enum characteristic 
        {
            will = -1,
            hostile,
            mercy,
            friendship,
            insight,

            free = will,
            attacker = hostile,
            healer = mercy,
            reader = friendship,
            defender = insight,
        }
        public enum target 
        {
            none = characteristic.will,
            inoperative = characteristic.hostile,
            operative = characteristic.mercy,
            space = characteristic.friendship,
            self = characteristic.insight,
        }
        public enum socialSkill
        {
            w_menu = -2,
            w_create = -1,
            h_attack = 0,
            m_aura = 4,
            f_close = 8,
            i_misdirection = 12,
            h_pride = 1,
            m_heal = 5,
            f_cover = 9,
            i_intercept = 13,
            h_trap = 2,
            m_pray = 6,
            f_call = 10,
            i_search = 14,
            h_hide = 3,
            m_recursive = 7,
            f_return = 11,
            i_counter = 15,
        }
        public enum physicalSkill
        {
            w_menu = -2,
            w_equip = -1,
            m_teleport = 6,
            f_apport = 10,
            i_float = 14,
            h_repeat = 3,
            m_global = 7,
            f_double = 11,
            i_convergence = 15,
        }
    }
    public abstract class ParamMeta
    {
        public float p;
        public ParamMeta(float v)
        {
            p = v;
        }
    }

    // 成長に伴い値が自明でない変化をするパラメータ
    public class Param : ParamMeta
    {
        public readonly float b; // value @ LV.0
        public readonly float growrate;
        public Param(float v, float b, float g) : base(v) 
        {
            this.b = b; 
            this.growrate = g; 
        }
        public Param(float b, float g) : base(b*g) 
        {
            this.b = b; 
            this.growrate = g; 
        }
        public float recalculate(LV lv)
        {
            return recalculate((int)lv.p);
        }
        public float recalculate(int LV = 1)
        {
            p = b;
            return grow(LV);
        }
        public virtual float grow(int LV = 1)
        {
            for (int i = 0; i < LV; i++)
            {
                p *= growrate;
            }
            return p;
        }
    }

    // 成長と独立に値が変化するパラメータ
    public class ParamVariable : Param
    {
        public float c; // current value
        public float m; // current max value
        public ParamVariable(float b, float g) : base(b, g) { m = b * g; c = m; }

        public override float grow(int LV = 1)
        {
            for (int i = 0; i < LV; i++)
            {
                p *= growrate;
                m = p;
            }
            return p;
        }
    }

    public class EXP : ParamMeta
    {
        // experience = comprehend 理解
        public EXP(float v) : base(v) { }
    }
    public class LV : ParamMeta
    {
        public LV(float v) : base(v) { }

        public float recalculate(Param para)
        {
            return para.recalculate(this);
        }
    }
    public class LVparty : LV
    {
        // LV = Linkage Value 連携値
        public EXP exp;
        public ParamVariable nextLVEXP;
        public LVparty(float v) : base(v) 
        {
            nextLVEXP = new ParamVariable(100, growparam.rate.mult1000By100Lv);
            exp = new EXP(0);
        }
        public int grow(EXP gotEXP)
        {
            exp.p += gotEXP.p;
            int lvup = 0;
            while(gotEXP.p >= nextLVEXP.c)
            {
                gotEXP.p -= nextLVEXP.c;
                nextLVEXP.grow();
                lvup++;
            }
            this.p += lvup;
            return lvup;
        }
    }
    public class INT : ParamMeta
    {
        // integration 統合
        public INT(float v) : base(v) { }
    }

    public class HP : ParamVariable
    {
        public HP(float b, growparam.characteristic role) : base(b, growparam.role2rate["HP"][role]) { }
    }
    public class CP : ParamVariable
    {
        public CP(float b, growparam.characteristic role) : base(b, growparam.role2rate["CP"][role]) { }
    }
    public class MOV : ParamVariable
    {
        public MOV(float b, growparam.characteristic role) : base(b, growparam.role2rate["MOV"][role]) { }
    }
    public class DEX : Param
    {
        public DEX(float b, growparam.characteristic role) : base(b, growparam.role2rate["DEX"][role]) { }
    }

    public class ParamMin : Param
    {
        public ParamMin(float b, float g) : base(b, g) { }
    }
    public class ParamMax : Param
    {
        public ParamMax(float b, float g) : base(b, g) { }
    }
    public abstract class ParamRanged
    {
        public ParamMin min;
        public ParamMax max;
        public ParamRanged(
            float min_base, float min_grow,
            float max_base, float max_grow)
        {
            this.min = new ParamMin(min_base, min_grow);
            this.max = new ParamMax(max_base, max_grow);
        }
    }

    public class RNG : ParamRanged
    {
        public RNG(
            float min_base, float min_grow,
            float max_base, float max_grow)
         : base(min_base, min_grow, max_base, max_grow) { }
    }
    public class SIG : ParamRanged
    {
        public SIG(
            float min_base, float min_grow,
            float max_base, float max_grow)
         : base(min_base, min_grow, max_base, max_grow) { }
    }
    public class durability : ParamVariable
    {
        public bool isUseItem;
        public durability(float b, float g, bool u = false) : base(b, g) { isUseItem = u; }
    }

    public class ParamFlat : Param
    {
        public ParamFlat(float b, float g) : base(b, g) { }
    }

    public abstract class Identifier
    {
        public string classname;
        public Identifier(string name)
        {
            this.classname = name;
        }
    }
    public class ParamSkillCondition : LV
    {
        public growparam.characteristic ch;
        public ParamSkillCondition(growparam.characteristic ch, int r = 0)
         : base(r)
        {
            this.ch = ch;
        }
    }
    public class ParamSkillLevels
    {
        public ParamSkillCondition hostile;
        public ParamSkillCondition mercy;
        public ParamSkillCondition friendship;
        public ParamSkillCondition insight;
        public ParamSkillCondition will;
        public ParamSkillLevels(
            int h,
            int m,
            int f,
            int i,
            int w = 0)
        {
            hostile = new ParamSkillCondition(growparam.characteristic.hostile, h);
            mercy = new ParamSkillCondition(growparam.characteristic.mercy, m);
            friendship = new ParamSkillCondition(growparam.characteristic.friendship, f);
            insight = new ParamSkillCondition(growparam.characteristic.insight, i);
            will = new ParamSkillCondition(growparam.characteristic.will, w);
        }
    }
    public class IdentifierPhysicalSkill : Identifier
    {
        public growparam.physicalSkill id;
        public ParamSkillLevels req;
        public IdentifierPhysicalSkill(string name, growparam.physicalSkill id, ParamSkillLevels r)
         : base(name)
        {
            this.id = id;
            req = r;
        }
    }
    public class IdentifierSocialSkill : Identifier
    {
        public growparam.socialSkill id;
        public ParamSkillLevels req;
        public IdentifierSocialSkill(string name, growparam.socialSkill id, ParamSkillLevels r)
         : base(name)
        {
            this.id = id;
            req = r;
        }
    }
    public class SkillInoperative : IdentifierSocialSkill
    {
        public SkillInoperative(string name, growparam.socialSkill id, ParamSkillLevels r)
         : base(name, id, r) { }
    }
    public class SkillOperative : IdentifierSocialSkill
    {
        public SkillOperative(string name, growparam.socialSkill id, ParamSkillLevels r)
         : base(name, id, r) { }
    }
    public class SkillSpace : IdentifierSocialSkill
    {
        public SkillSpace(string name, growparam.socialSkill id, ParamSkillLevels r)
         : base(name, id, r) { }
    }
    public class SkillSelf : IdentifierSocialSkill
    {
        public SkillSelf(string name, growparam.socialSkill id, ParamSkillLevels r)
         : base(name, id, r) { }
    }
    public class ParamEquipSkill
    {
        public SkillInoperative inoperative;
        public SkillOperative operative;
        public SkillSpace space;
        public SkillSelf self;

        public ParamEquipSkill(
            SkillInoperative h,
            SkillOperative m,
            SkillSpace f,
            SkillSelf i)
        {
            inoperative = h;
            operative = m;
            space = f;
            self = i;
        }
        public ParamSkillLevels reqSkillLevelUnrockAll()
        {
            return new ParamSkillLevels(
                (int)(inoperative.req.hostile.p + operative.req.hostile.p + space.req.hostile.p + self.req.hostile.p),
                (int)(inoperative.req.mercy.p + operative.req.mercy.p + space.req.mercy.p + self.req.mercy.p),
                (int)(inoperative.req.friendship.p + operative.req.friendship.p + space.req.friendship.p + self.req.friendship.p),
                (int)(inoperative.req.insight.p + operative.req.insight.p + space.req.insight.p + self.req.insight.p),
                (int)(inoperative.req.will.p + operative.req.will.p + space.req.will.p + self.req.will.p)
                );
        }
    }
    public class IdentifierEquip : Identifier
    {
        public LV reinforce;
        public durability d;

        public IdentifierEquip(string classname, durability d, int l = 0)
         : base(classname)
        {
            // 値の範囲内でランダムに決めたい
            this.reinforce = new LV(l);
            this.d = d;
        }
    }
    public class StatusEquip : IdentifierEquip
    {
        public growparam.characteristic role;
        public ParamEquipSkill socialSkills;
        public ParamFlat HP;
        public ParamFlat MOV;
        public ParamFlat CP;
        public ParamFlat DEX;
        public RNG RNG; 
        public SIG SIG; 
        public StatusEquip(
            string classname, 
            growparam.characteristic role,
            ParamEquipSkill skill,
            int lvr,
            durability d,
            ParamFlat HP,
            ParamFlat MOV,
            ParamFlat CP,
            ParamFlat DEX,
            RNG RNG,
            SIG SIG
            )
         : base(classname, d, lvr)
        {
            this.role = role;
            this.socialSkills = skill;
            this.HP = HP;
            this.MOV = MOV;
            this.CP = CP;
            this.DEX = DEX;
            this.RNG = RNG;
            this.SIG = SIG;
        }
    }
    public class IdentifierCharacter : Identifier
    {
        public string name;
        public growparam.characteristic role;

        public IdentifierCharacter(string classname, string name, growparam.characteristic role)
         : base(classname)
        {
            this.name = name;
            this.role = role;
        }
    }
    public class StatusCharacter : IdentifierCharacter
    {
        public StatusEquip equip;
        public ParamSkillLevels skillLVsSocial;
        public INT INT;
        public ParamVariable HP;
        public ParamVariable MOV;
        public ParamVariable CP;
        public DEX DEX;
        public StatusCharacter(
            string classname,
            string name,
            int role,
            StatusEquip e,
            ParamSkillLevels social,
            int INT,
            float HP,
            float MOV,
            float CP,
            float DEX,
            float RNGmin,
            float RNGmax,
            float SIGmin,
            float SIGmax
            )
         : base(classname, name, (growparam.characteristic)role)
        {
            this.equip = e;
            this.skillLVsSocial = social;
            this.INT = new INT(INT);
            this.HP = new HP(HP, this.role);
            this.MOV = new MOV(MOV, this.role);
            this.CP  = new CP(CP, this.role);
            this.DEX = new DEX(DEX, this.role);
        }
    }

    public class IdentifierParty : Identifier
    {
        public List<StatusCharacter> members;
        public int[] memberIds;

        public IdentifierParty(string classname, List<StatusCharacter> m, int[] ids)
         : base(classname)
        {
            members = m;
            memberIds = ids;
        }

    }
    public class StatusParty : IdentifierParty
    {
        public static int playerLvp = 1;
        public static int playerLvg = 0;
        public static int playerLvc = 0;
        public ParamSkillLevels skillLVsPhysical;
        public LVparty lv;
        public LV global;
        public LV convergence;
        public List<IdentifierEquip> items;
        public StatusParty(
            string classname,
            List<StatusCharacter> members,
            int[] ids,
            ParamSkillLevels physical,
            int lvr = 0,
            int lvg = 0,
            int lvc = 0,
            List<IdentifierEquip> items = null)
         : base(classname, members, ids)
        {
            this.skillLVsPhysical = physical;

            // 値の範囲内でランダムに決めたい
            lv = new LVparty(playerLvp + lvr);
            lv = new LVparty(playerLvg + lvg);
            lv = new LVparty(playerLvc + lvc);

            this.items = items is null ? new List<IdentifierEquip>(){ } : items;
        }
    }
}