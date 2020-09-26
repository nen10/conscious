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
            
        }

        public enum characteristic 
        {
            will,
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
        public readonly float b; // base value
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
        public ParamVariable(float v, float b, float g) : base(v, b, g) { m = v; c = m; }
        public ParamVariable(float v, float b, float g, float c) : base(v, b, g) { this.c = c; }

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
        // experience = comprehend : 理解
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
        public INT(float v) : base(v) { }
    }


    public class HP : ParamVariable
    {
        
        public HP(float b, float g) : base(b, g) { }
        public HP(float v, float b, float g) : base(v, b, g) { }
        public HP(float v, float b, float g, float c) : base(v, b, g) { }
    }
    public class CP : ParamVariable
    {
        
        public CP(float b, float g) : base(b, g) { }
        public CP(float v, float b, float g) : base(v, b, g) { }
        public CP(float v, float b, float g, float c) : base(v, b, g) { }
    }
    public class MOV : ParamVariable
    {
        
        public MOV(float b, float g) : base(b, g) { }
        public MOV(float v, float b, float g) : base(v, b, g) { }
        public MOV(float v, float b, float g, float c) : base(v, b, g) { }
    }
    public class DEX : Param
    {
        public DEX(float b, float g) : base(b, g) { }
        public DEX(float v, float b, float g) : base(v, b, g) { }
    }

    public class ParamMin : Param
    {
        public ParamMin(float b, float g) : base(b, g) { }
        public ParamMin(float v, float b, float g) : base(v, b, g) { }
    }
    public class ParamMax : Param
    {
        public ParamMax(float b, float g) : base(b, g) { }
        public ParamMax(float v, float b, float g) : base(v, b, g) { }
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
        public ParamRanged(
            float min, float min_base, float min_grow,
            float max, float max_base, float max_grow)
        {
            this.min = new ParamMin(min, min_base, min_grow);
            this.max = new ParamMax(max, max_base, max_grow);
        }
    }

    public class RNG : ParamRanged
    {
        public RNG(
            float min_base, float min_grow,
            float max_base, float max_grow)
         : base(min_base, min_grow, max_base, max_grow) { }
        public RNG(
            float min, float min_base, float min_grow,
            float max, float max_base, float max_grow)
         : base(min, min_base, min_grow, max, max_base, max_grow) { }
    }
    public class SIG : ParamRanged
    {
        public SIG(
            float min_base, float min_grow,
            float max_base, float max_grow)
         : base(min_base, min_grow, max_base, max_grow) { }
        public SIG(
            float min, float min_base, float min_grow,
            float max, float max_base, float max_grow)
         : base(min, min_base, min_grow, max, max_base, max_grow) { }
    }
    public class durability : ParamVariable
    {
        public durability(float v, float b, float g) : base(v, b, g) { }
        public durability(float v, float b, float g, float c) : base(v, b, g) { }
    }

    public class ParamFlat : Param
    {
        public ParamFlat(float v, float b, float g) : base(v, b, g) { }
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
    public class ParamEquepSkill
    {
        public SkillInoperative inoperative;
        public SkillOperative operative;
        public SkillSpace space;
        public SkillSelf self;
        public ParamEquepSkill(
            string name_h, growparam.socialSkill id_h, ParamSkillLevels req_h,
            string name_m, growparam.socialSkill id_m, ParamSkillLevels req_m,
            string name_f, growparam.socialSkill id_f, ParamSkillLevels req_f,
            string name_i, growparam.socialSkill id_i, ParamSkillLevels req_i)
        {
            inoperative = new SkillInoperative(name_h, id_h, req_h);
            operative = new SkillOperative(name_m, id_m, req_m);
            space = new SkillSpace(name_f, id_f, req_f);
            self = new SkillSelf(name_i, id_i, req_i);
        }
        public ParamSkillLevels skillUnrockAll()
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
            this.reinforce = new LV(l);
            this.d = d;
        }

    }
    public class IdentifierCharactor : Identifier
    {
        public string name;
        public growparam.characteristic role;

        public IdentifierCharactor(string classname, string name, growparam.characteristic role)
         : base(classname)
        {
            this.name = name;
            this.role = role;
        }

    }

    public class IdentifierParty : Identifier
    {
        public List<StatusCharactor> members;

        public IdentifierParty(string classname)
         : base(classname)
        {
            // jsonとか読み込んでmemberをつくろう
            members = null;
        }

    }
    public abstract class Status
    {
    }
    public class StatusEquip : IdentifierEquip
    {
        public growparam.characteristic role;
        public ParamEquepSkill socialSkills;
        public ParamFlat HP;
        public ParamFlat MOV;
        public ParamFlat CP;
        public ParamFlat DEX;
        public RNG RNG; 
        public SIG SIG; 



        public StatusEquip(string classname, durability d, int l = 0)
         : base(classname, d, l)
        {
            // jsonとか読み込んで作ろう
            role = growparam.characteristic.free;
            socialSkills = null;
            HP = null;
            MOV = null;
            CP = null;
            DEX = null;
            RNG = null;
            SIG = null;
        }
    }
    public class StatusCharactor : IdentifierCharactor
    {
        public StatusEquip equip;
        public ParamSkillLevels skillLVsSocial;
        public HP HP;
        public MOV MOV;
        public CP CP;
        public DEX DEX;
        public RNG RNG; 
        public SIG SIG; 


        public StatusCharactor(
            string classname, string name, growparam.characteristic role,
            StatusEquip e, ParamSkillLevels social)
         : base(classname, name ,role)
        {
            this.equip = e;
            this.skillLVsSocial = social;
            // jsonとか読み込んで作ろう
            HP = null;
            MOV = null;
            CP = null;
            DEX = null;
            RNG = null;
            SIG = null;
        }
    }
    public class StatusParty : IdentifierParty
    {
        public ParamSkillLevels skillLVsPhysical;
        public LVparty lv;
        public LV global;
        public LV convergence;
        public List<IdentifierEquip> items;
        public StatusParty(
            string classname, string partyname,
            ParamSkillLevels physical,
            int pLv = 1, int gLv = 1, int cLv = 1,
            List<IdentifierEquip> items = null)
         : base(classname)
        {
            this.skillLVsPhysical = physical;
            this.lv = new LVparty(pLv);
            this.global = new LV(gLv);
            this.convergence = new LV(cLv);
            this.items = items;
        }
    }
}