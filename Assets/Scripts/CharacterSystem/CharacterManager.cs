using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Object;
using MapSystem.SpriteManager;
using MapSystem.HexCoordinateSystem;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using CharacterSystem.StatusParameter;

namespace CharacterSystem
{
    public static class CharacterManager
    {
        public enum FACTION 
        {
            player,
            enemy,
            civil,
        }
        public static Party playable;
        public static List<Party> nonPlayer;
        public static Dictionary<HexSprite, Character> Hex2Char;

        public static void Init(HexSprite p)
        {
            playable = new Party(CharacterManager.FACTION.player, p);
            nonPlayer = new List<Party>(){ };
        }
        public static void GenParty(Party p)
        {
            switch (p.faction)
            {
                case FACTION.enemy: nonPlayer.Add(p); break;
                case FACTION.civil: nonPlayer.Add(p); break;
                case FACTION.player: playable = p; break;
            }
        }
        public static void RemoveParty(Party p)
        {
            switch (p.faction)
            {
                case FACTION.enemy: nonPlayer.Remove(p); break;
                case FACTION.civil: nonPlayer.Remove(p); break;
                case FACTION.player: break;
            }
        }

    }
    public class Party
    {
        public enum MODE 
        {
            existential,
            physical,
            social,
        }
        public CharacterManager.FACTION faction;
        public HexSprite pos;
        public MODE mode;
        public List<Character> members;
        public List<Hex> rut;
        public int pReaderId{ get { return this.members.Find((x) => x.isPReader).posParty; } }
        public int formUpTurn{ get { return members.Count - rut.Count; } }
        public Party(CharacterManager.FACTION f, HexSprite p, MODE m = MODE.physical)
        {
            this.faction = f;
            CharacterManager.GenParty(this);
            this.pos = p;
            this.mode = m;
            this.members = new List<Character>() { };
            this.rut = new List<Hex>() { };
        }
        public void ChengePhysicalReader(bool isInverse = false)
        {
            int i = isInverse ? -1 : 1;
            int id = this.pReaderId;
            this.members[id].HideFromMap();
            this.members[id].isPReader = false;

            int nextId = MathHex.SafeMod(id + i, this.members.Count);
            this.members[nextId].AppearToMap();
            this.members[nextId].isPReader = true;
        }
        public void ChangeFormation(List<int> formation)
        {
            List<Character> newFormation = new List<Character>() { };
            foreach(int i in formation)
            {
                newFormation.Add(this.members[i]);
                this.members[i].posParty = i;
            }
            this.members = newFormation;
        }
        public void ChangeFormation()
        {
            List<Character> newFormation = new List<Character>() { };
            for (int i = 0; i < this.members.Count; i++)
            {
                newFormation.Add(this.members.Find((x) => x.posParty == i));
            }
            this.members = newFormation;
        }

        public void ChangeFormationByLeave()
        {
            for (int i = 0; i < this.members.Count; i++)
            {
                this.members[i].posParty = i;
            }
        }

        public void Move(Hex m)
        {
            // animation
            this.pos += m;

            if(this.formUpTurn == 0)
            {
                this.rut.RemoveAt(this.rut.Count - 1);
            }
            for (int i = 0; i < this.rut.Count; i++)
            {
                this.rut[i] -= m;
            }
            this.rut.Insert(0,HexGenerator.O);
        }

        public void ModeP2S()
        {
            if(this.formUpTurn > 0)
            {
                return; //
            }
            foreach(Character m in this.members)
            {
                m.posRel = this.rut[m.posParty];
                m.AppearToMap();
            }
            this.mode = MODE.social;
            // BGM変更
        }
        public void ModeS2P(int controllMemderId)
        {
            this.rut.RemoveRange(0, this.rut.Count);
            this.rut.Add(HexGenerator.O);
            foreach(Character m in this.members)
            {
                if(m.posParty == controllMemderId)
                {
                    m.isPReader = true;
                    this.pos += m.posRel;
                    m.posRel = HexGenerator.O;
                }
                else
                {
                    m.HideFromMap();
                    m.isPReader = false;
                }
            }
            this.mode = MODE.physical;
            // BGM変更
        }
    }

    public class Character
    {
        public Hex posRel;
        public Party home;
        public int posParty;
        public bool isPReader;
        public CharacterManager.FACTION faction;
        public GameObject instancePrehub;
        public AnimationController ani;
        public Vector3 GetWorldPosition()
        {
            return (this.home.pos + posRel).positonWorldPixelMobs();
        }
        public Character(
            Party party,
            string raceName,
            string skin = "")
        {
            this.posRel = HexGenerator.O;
            this.Init(party, raceName, skin);
        }
        public Character(
            Party party,
            Hex h,
            string raceName,
            string skin = "")
        {
            this.posRel = h;
            this.Init(party, raceName, skin);
        }
        public Character(
            HexSprite p,
            string raceName,
            string skin = "",
            CharacterManager.FACTION f = CharacterManager.FACTION.enemy,
            Party.MODE m = Party.MODE.physical)
        {
            this.posRel = HexGenerator.O;
            this.Init(new Party(f, p, m), raceName, skin);
        }
        public Character(
            Hex h,
            string raceName,
            string skin = "",
            CharacterManager.FACTION f = CharacterManager.FACTION.enemy,
            Party.MODE m = Party.MODE.physical)
        {
            this.posRel = HexGenerator.O;
            this.Init(
                new Party(f, CharacterManager.playable.pos + h, m),
                raceName,
                skin);
        }
        public void Init(Party p, string raceName, string skin)
        {
            this.home = p;
            this.JoinParty();
            this.shapeInit(raceName, skin);

        }
        public void JoinParty(Party nextP)
        {
            this.LeaveParty();
            this.posRel = this.home.pos + this.posRel - nextP.pos;
            this.home = nextP;
            this.JoinParty();
        }
        public void JoinParty()
        {
            this.home.members.Add(this);
            this.posParty = this.home.members.Count - 1;
            this.isPReader = (this.posParty == 0);
            this.faction = this.home.faction;
        }
        public void shapeInit(string raceName, string skin = "")
        {
            GameObject origin = (GameObject)Resources.Load("Character/Prehub/" + raceName + skin);
            this.instancePrehub = Instantiate(origin, this.GetWorldPosition(), Quaternion.identity);
            this.home.pos.SetGridAsParent(instancePrehub);
            this.ani = instancePrehub.GetComponent<AnimationController>();
            this.instancePrehub.SetActive(this.isPReader || this.home.mode != Party.MODE.physical);
        }
        public void HideFromMap(float timeFade = 0f)
        {
            // posRel = HexGenerator.O;
            // fade out
            this.instancePrehub.SetActive(false);
        }
        public void AppearToMap(float timeFade = 0f)
        {
            this.instancePrehub.SetActive(true);
            this.instancePrehub.transform.SetPositionAndRotation(this.GetWorldPosition(), Quaternion.identity);
            // fade in
        }
        public void Move(Hex h)
        {
            this.posRel += h;
            // animation
        }
        public void LeaveParty()
        {
            this.home.members.RemoveAt(posParty);
            if(this.home.members.Count == 0)
            {
                CharacterManager.RemoveParty(this.home);
            }
            else 
            {
                this.home.members[0].isPReader |= this.isPReader;
                this.home.ChangeFormationByLeave();
            }
        }

    }
    
}
