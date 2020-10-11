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
        public enum ControlMode
        {
            nonPlayer = -1,
            integrate,
            body,
            mind,
        }
        public static Party playable;
        public static List<Party> nonPlayer;
        public static Dictionary<HexSprite, Character> Hex2Char;

        public static void Init(HexSprite p)
        {
            playable = new Party(CharacterManager.FACTION.player, p);
            nonPlayer = new List<Party>(){ };
            Hex2Char = new Dictionary<HexSprite, Character>() { };
        }
        public static void SetHex2Char()
        {
            playable.SetHex2Char();
            foreach(Party p in nonPlayer)
            {
                p.SetHex2Char();
            }
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
            SetMode(MODE.social);
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
            SetMode(MODE.physical);
            // BGM変更
        }


        public void SetMode(MODE m)
        {
            RemoveHex2Char();
            this.mode = m;
            SetHex2Char();
        }
        public void SetHex2Char()
        {
            if(this.mode == MODE.physical)
            {
                this.members[this.pReaderId].SetHex2Char();
            }
            else
            {
                foreach(Character ch in this.members)
                {
                    ch.SetHex2Char();
                }

            }
        }
        public void RemoveHex2Char()
        {
            if(this.mode == MODE.physical)
            {
                CharacterManager.Hex2Char.Remove(this.pos);
            }
            else
            {
                foreach(Character ch in this.members)
                {
                    ch.RemoveHex2Char();
                }

            }
        }
    }

    public class Character
    {
        public Hex posRel;
        public Party community;
        public HexSprite pos {get { return community.pos + posRel; } }
        public int posParty;
        public bool isPReader;
        public CharacterManager.FACTION faction;
        public GameObject prehubVisual;
        public GameObject prehubPointer;
        public AnimationController ani;
        public StatusCharacter status;
        public Dictionary<HexSprite, (int, List<HexUnit>)> accessibleArea;
        public Vector3 GetWorldPosition()
        {
            return (this.pos).positonWorldPixelMobs();
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
            this.community = p;
            this.JoinParty();
            this.InitShape(raceName, skin);

        }
        public void JoinParty()
        {
            this.community.members.Add(this);
            this.posParty = this.community.members.Count - 1;
            this.isPReader = (this.posParty == 0);
            this.faction = this.community.faction;
        }
        public void InitShape(string raceName, string skin = "")
        {
            GameObject origin = (GameObject)Resources.Load("Character/Prehub/" + raceName + skin);
            this.prehubVisual = Instantiate(origin, this.GetWorldPosition(), Quaternion.identity);
            this.community.pos.SetGridAsParent(prehubVisual);
            this.ani = prehubVisual.GetComponent<AnimationController>();
            this.prehubVisual.SetActive(this.isPReader || this.community.mode != Party.MODE.physical);
        }
        public void InitByTurn()
        {
            this.status.MOV.c = this.status.MOV.m;
            this.accessibleArea = this.pos.GetMinPaths((int)this.status.MOV.c);
        }

        public void HideFromMap(float timeFade = 0f)
        {
            // posRel = HexGenerator.O;
            // fade out
            this.prehubVisual.SetActive(false);
        }
        public void AppearToMap(float timeFade = 0f)
        {
            this.prehubVisual.SetActive(true);
            this.prehubVisual.transform.SetPositionAndRotation(this.GetWorldPosition(), Quaternion.identity);
            // fade in
        }
        public void Move(Hex h)
        {
            this.posRel += h;
            // animation
        }
        public void LeaveParty(Party nextP)
        {
            this.community.members.RemoveAt(posParty);
            if(this.community.members.Count == 0)
            {
                CharacterManager.RemoveParty(this.community);
            }
            else 
            {
                this.community.members[0].isPReader |= this.isPReader;
                this.community.ChangeFormationByLeave();
            }
            this.posRel = this.pos - nextP.pos;
            this.community = nextP;
            this.JoinParty();
        }

        public void SetHex2Char()
        {
            CharacterManager.Hex2Char.Add(this.pos, this);
        }
        public void RemoveHex2Char()
        {
            CharacterManager.Hex2Char.Remove(this.pos);
        }

        public void SetPointer(string pointer)
        {
            GameObject origin = (GameObject)Resources.Load("Pointer/" + pointer);
            this.prehubPointer = Instantiate(origin, this.pos.positonWorldPointer(), Quaternion.identity);
            this.community.pos.SetGridAsParent(prehubPointer);
        }
        public void RemovePointer(float time = 0f)
        {
            if (!(this.prehubPointer is null))
            {
                Destroy(this.prehubPointer, time);
            }
            this.prehubPointer = null;
        }
        public void SwitchPointer(string pointer)
        {
            RemovePointer();
            SetPointer(pointer);
        }
        public bool AttentionBody(HexSprite h)
        {
            switch(this.community.mode)
            {
                case Party.MODE.physical: return PlayPhysicalBody(h);
                case Party.MODE.social: return PlaySocialBody(h);
                case Party.MODE.existential: return PlaySocialBody(h);
            }
            return false;
        }
        public bool AttentionMind(HexSprite h)
        {
            switch(this.community.mode)
            {
                case Party.MODE.physical: return PlayPhysicalMind(h);
                case Party.MODE.social: return PlaySocialMind(h);
                case Party.MODE.existential: return PlaySocialMind(h);
            }
            return false;
        }
        public bool PlayPhysicalBody(HexSprite h)
        {
            int d = Hex.L1Distance(h, this.pos);
            if(d == 0)
                return ConsiousDive(h);
            if(CharacterManager.Hex2Char.ContainsKey(h))
            {
                Character target = CharacterManager.Hex2Char[h];
                if(d == 1)
                    return ConsiousTalking(target, h);
                return ConsiousApport(target, h);
            }
            if (h.hasWall())
                return ConsiousBrakeWall(h);
            if(accessibleArea.ContainsKey(h))
                return ConsiousStepping(h);
            return ConsiousTeleport(h);
        }
        public bool PlayPhysicalMind(HexSprite h)
        {
            int d = Hex.L1Distance(h, this.pos);
            if(d == 0)
                return ConsiousMeditation(h);
            return ConsiousWatching(h);
        }
        public bool PlaySocialBody(HexSprite h)
        {
            int d = Hex.L1Distance(h, this.pos);
            if(d == 0)
                return ConsiousDeparture(h);
            if(CharacterManager.Hex2Char.ContainsKey(h))
            {
                Character target = CharacterManager.Hex2Char[h];
                if(d == 1)
                    return ConsiousTalking(target, h);
                return ConsiousApport(target, h);
            }
            if (h.hasWall())
                return ConsiousBrakeWall(h);
            if(accessibleArea.ContainsKey(h))
                return ConsiousWalking(h);
            return ConsiousTeleport(h);
        }
        public bool PlaySocialMind(HexSprite h)
        {
            if(!CharacterManager.Hex2Char.ContainsKey(h))
                return Consious2space(h);
            Character target = CharacterManager.Hex2Char[h];
            if(this.community != target.community)
                return Consious2inoperate(target, h);
            if(this != target)
                return Consious2operate(target, h);
            return Consious2self(target, h);
        }
        public bool Consious2space(HexSprite h)
        {
            return false;
            // TODO: write friendship skill
        }
        public bool Consious2inoperate(Character ch, HexSprite h)
        {
            return false;
            // TODO: write hostile skill
        }
        public bool Consious2operate(Character ch, HexSprite h)
        {
            return false;
            // TODO: write mercy skill
        }
        public bool Consious2self(Character ch, HexSprite h)
        {
            return false;
            // TODO: write insight skill
        }

        /*
        public bool ConsiousFriendship(HexSprite h)
        {
        }
        public bool ConsiousHostile(Character ch, HexSprite h)
        {
        }
        public bool ConsiousMercy(Character ch, HexSprite h)
        {
        }
        public bool ConsiousInsight(Character ch, HexSprite h)
        {
        }
        */

        public bool ConsiousDeparture(HexSprite h)
        {
            return false;
            // TODO: write Departure process
        }
        public bool ConsiousDive(HexSprite h)
        {
            return false;
            // TODO: write Dive process
        }

        public bool ConsiousTalking(Character ch, HexSprite h)
        {
            return false;
            // TODO: write Talking process
        }
        public bool ConsiousBrakeWall(HexSprite h)
        {
            return false;
            // TODO: write BrakeWall process
        }
        public bool ConsiousStepping(HexSprite h)
        {
            return false;
            // TODO: write Stepping process
        }
        public bool ConsiousWalking(HexSprite h)
        {
            return false;
            // TODO: write walking process
        }
        public bool ConsiousTeleport(HexSprite h)
        {
            return false;
            // TODO: write teleport process
        }
        public bool ConsiousApport(Character ch, HexSprite h)
        {
            return false;
            // TODO: write Apport process
        }
        public bool ConsiousMeditation(HexSprite h)
        {
            return false;
            // TODO: write Meditation process
        }
        public bool ConsiousWatching(HexSprite h)
        {
            return false;
            // TODO: write Watching process
        }

        public HexSprite ConsiousPhilosophicalZombie()
        {
            // TODO: write non-player AI 
            return this.pos;
        }
    }    
}
