using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterSystem.StatusParameter;

namespace CharacterSystem.ResourceManager
{
    [Serializable]
    public class JsonSkillLevel
    {
        public int h;
        public int m;
        public int f;
        public int i;
        public int w = 0;

        public ParamSkillLevels make()
        {
            return new ParamSkillLevels
                (
                    this.h,
                    this.m,
                    this.f,
                    this.i
                );
        }
    }
    [Serializable]
    public class JsonSkill
    {
        public string name;
        public int key;
        public JsonSkillLevel req;
    }

    [Serializable]
    public class JsonEquipParameter
    {
        public float p;
        public int growrateKey;
    }
    [Serializable]
    public class JsonEquip
    {
        public string name;
        public int role;
        public string inoperative;
        public string operative;
        public string space;
        public string self;
        public int lvr = 0;
        public JsonEquipParameter dur;
        public JsonEquipParameter HP;
        public JsonEquipParameter MOV;
        public JsonEquipParameter CP;
        public JsonEquipParameter DEX;
        public JsonEquipParameter RNGmin;
        public JsonEquipParameter RNGmax;
        public JsonEquipParameter SIGmin;
        public JsonEquipParameter SIGmax;
        public StatusEquip make()
        {
            JsonSkill h = JsonManager.JsonRoadSkill(inoperative);
            JsonSkill m = JsonManager.JsonRoadSkill(operative);
            JsonSkill f = JsonManager.JsonRoadSkill(space);
            JsonSkill i = JsonManager.JsonRoadSkill(self);

            return new StatusEquip(
                name,
                (growparam.characteristic)role,
                new ParamEquipSkill
                (
                    new SkillInoperative(h.name, (growparam.socialSkill)h.key, h.req.make()),
                    new SkillOperative(m.name, (growparam.socialSkill)m.key, m.req.make()),
                    new SkillSpace(f.name, (growparam.socialSkill)f.key, f.req.make()),
                    new SkillSelf(i.name, (growparam.socialSkill)i.key, i.req.make())
                ),
                lvr,
                new durability(dur.p, growparam.rateDict[dur.growrateKey]),
                new ParamFlat(HP.p, growparam.rateDict[HP.growrateKey]),
                new ParamFlat(MOV.p, growparam.rateDict[MOV.growrateKey]),
                new ParamFlat(CP.p, growparam.rateDict[CP.growrateKey]),
                new ParamFlat(DEX.p, growparam.rateDict[DEX.growrateKey]),
                new RNG(RNGmin.p, RNGmin.growrateKey, RNGmax.p, RNGmax.growrateKey),
                new SIG(SIGmin.p, SIGmin.growrateKey, SIGmax.p, SIGmax.growrateKey)
            );
        }
        [Serializable]
        public class JsonCharactor
        {
            public string race;
            public int role;
            public string equip;
            public JsonSkillLevel skilltree;
            public int INT;

            // base parametor
            public float HP;
            public float MOV;
            public float CP;
            public float DEX;
            public float RNGmin;
            public float RNGmax;
            public float SIGmin;
            public float SIGmax;
            public StatusCharactor make(string name)
            {
                return new StatusCharactor(
                    race,
                    name,
                    role,
                    JsonManager.JsonRoadEquip(equip).make(),
                    skilltree.make(),
                    INT,
                    HP,
                    MOV,
                    CP,
                    DEX,
                    RNGmin,
                    RNGmax,
                    SIGmin,
                    SIGmax
                );
            }
        }
        [Serializable]
        public class JsonParty
        {
            public int partyid;
            public string[] members;
            public JsonSkillLevel skilltree;
            public int lvr = 0;
            public int lvg = 0;
            public int lvc = 0;
            public StatusParty make(int[] memberIds)
            {
                List<StatusCharactor> m = new List<StatusCharactor>();
                foreach (int id in memberIds)
                {
                    m.Add(JsonManager.JsonRoadCharactor(members[id]).make(members[id]));
                }

                return new StatusParty(
                    partyid.ToString(),
                    m,
                    memberIds,
                    skilltree.make(),
                    lvr,
                    lvg,
                    lvc
                );

            }
        }
        [Serializable]
        public class JsonPartySets
        {
            public JsonParty[] partysets;
        }
        [Serializable]
        public class JsonItem
        {
            public string name;
            public JsonEquipParameter durability;
        }
        public static class JsonManager
        {
            public static readonly string DirSkill = "Json/Skill/";
            public static readonly string DirEquip = "Json/Equip/";
            public static readonly string DirCharactor = "Json/Charactor/";
            public static readonly string DirPartySets = "Json/PartySets/";
            public static void JsonSave()
            {
            }
            public static T JsonRoad<T>(string filepath)
            {
                TextAsset json = Resources.Load<TextAsset>(filepath);
                T jsonClass = JsonUtility.FromJson<T>(json.text);
                return jsonClass;
            }
            public static JsonSkill JsonRoadSkill(string name)
            {
                return JsonRoad<JsonSkill>(DirSkill + name);
            }
            public static JsonEquip JsonRoadEquip(string name)
            {
                return JsonRoad<JsonEquip>(DirEquip + name);
            }
            public static JsonCharactor JsonRoadCharactor(string name)
            {
                return JsonRoad<JsonCharactor>(DirCharactor + name);
            }
            public static JsonPartySets JsonRoadPartySets(string name)
            {
                return JsonRoad<JsonPartySets>(DirPartySets + name);
            }

        }
    }
}
