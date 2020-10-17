using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Object;
using MapSystem.SpriteManager;
using MapSystem.HexCoordinateSystem;
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
        public void Teleport(HexSprite m)
        {
            this.pos = m;
            this.rut = new List<Hex>() { };
            this.rut.Insert(0,HexGenerator.O);
        }

        public bool ModeP2S()
        {
            if(this.formUpTurn > 0)
            {
                // TODO: ターン経過でメンバーが増えるようにする？
                return false; 
            }
            foreach(Character m in this.members)
            {
                if(m.isPReader) continue;
                m.posRel = this.rut[m.posParty];
                m.AppearToMap();
            }
            SetMode(MODE.social);
            // BGM変更
            return true;
        }
        public bool ModeS2P(int controllMemderId)
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
            return true;
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
        public Vector3 posWorld {get { return (this.pos).positonWorldPixelMobs(); } }
        public int posParty;
        public bool isPReader;
        public CharacterManager.FACTION faction;
        public GameObject prehubVisual;
        public GameObject prehubPointer;
        public AnimationController ani;
        public StatusCharacter status;
        public Dictionary<HexSprite, (int, List<HexUnit>, HexSprite)> accessibleArea;
        public Character(
            Party party,
            string characterName,
            string raceName,
            string skin = "")
        {
            this.posRel = HexGenerator.O;
            this.Init(party, characterName, raceName, skin);
        }
        public Character(
            Party party,
            Hex h,
            string characterName,
            string raceName,
            string skin = "")
        {
            this.posRel = h;
            this.Init(party, characterName, raceName, skin);
        }
        public Character(
            HexSprite p,
            string characterName,
            string raceName,
            string skin = "",
            CharacterManager.FACTION f = CharacterManager.FACTION.enemy,
            Party.MODE m = Party.MODE.physical)
        {
            this.posRel = HexGenerator.O;
            this.Init(new Party(f, p, m), characterName, raceName, skin);
        }
        public Character(
            Hex h,
            string characterName,
            string raceName,
            string skin = "",
            CharacterManager.FACTION f = CharacterManager.FACTION.enemy,
            Party.MODE m = Party.MODE.physical)
        {
            this.posRel = HexGenerator.O;
            this.Init(
                new Party(f, CharacterManager.playable.pos + h, m),
                characterName,
                raceName,
                skin);
        }
        public void Init(Party p, string characterName, string raceName, string skin)
        {
            this.community = p;
            this.JoinParty();
            this.LoadStatus(characterName);
            this.InitShape(raceName, skin);
        }
        public void JoinParty()
        {
            this.community.members.Add(this);
            this.posParty = this.community.members.Count - 1;
            this.isPReader = (this.posParty == 0);
            this.faction = this.community.faction;
        }
        public void LoadStatus(string characterName)
        {
            // TODO: jsonからステータスを作成する
            this.status = null;
        }
        public void InitShape(string raceName, string skin = "")
        {
            GameObject origin = (GameObject)Resources.Load("Character/Prehub/" + raceName + skin);
            this.prehubVisual = Instantiate(origin, this.posWorld, Quaternion.identity);
            this.community.pos.SetGridAsParent(prehubVisual);
            this.ani = prehubVisual.GetComponent<AnimationController>();
            this.ani.map = this.community.pos.map;
            this.ani.status = this.status;
            //this.ani.target = pos.positonWorldPixelMobs();
            this.prehubVisual.SetActive(this.isPReader || this.community.mode != Party.MODE.physical);

        }
        public void InitBySocialTurn()
        {
            this.status.MOV.c = this.status.MOV.m;
            this.accessibleArea = this.pos.GetMinPaths((int)this.status.MOV.c);
        }
        public void InitByPhysicalTurn()
        {
            this.accessibleArea = this.pos.GetMinPaths(1);
        }
        public void HideFromMap(float timeFade = 0f)
        {
            // posRel = HexGenerator.O;
            // TODO: fade out
            this.prehubVisual.SetActive(false);
        }
        public void AppearToMap(float timeFade = 0f)
        {
            this.prehubVisual.SetActive(true);
            this.prehubVisual.transform.SetPositionAndRotation(this.posWorld, Quaternion.identity);
            // TODO: fade in
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
        public bool AttentionBody(HexSprite h, Character ch = null)
        {
            switch(this.community.mode)
            {
                case Party.MODE.physical: return PlayPhysicalBody(h, ch);
                case Party.MODE.social: return PlaySocialBody(h, ch);
                case Party.MODE.existential: return PlaySocialBody(h, ch);
            }
            return false;
        }
        public bool AttentionMind(HexSprite h, Character ch = null)
        {
            switch(this.community.mode)
            {
                case Party.MODE.physical: return PlayPhysicalMind(h, ch);
                case Party.MODE.social: return PlaySocialMind(h, ch);
                case Party.MODE.existential: return PlaySocialMind(h, ch);
            }
            return false;
        }
        public bool PlayPhysicalBody(HexSprite h, Character ch)
        {
            /*/ スキル効果はパーティに依存して決定される /*/
            bool isPhysical = true;
            int d = Hex.L1Distance(h, this.pos);
            if(d == 0)
                return ConsiousDive(h);
            if(ch is null)
            {
                if (accessibleArea.ContainsKey(h))
                    return ConciousMove(h, isPhysical);
                if (h.hasWall())
                    return ConsiousBrakeWall(h, isPhysical);
                return ConsiousTeleport(this, h, isPhysical);
            }
            d = Hex.L1Distance(ch.pos, this.pos);
            if (d == 1)
            {
                if (status.role == growparam.characteristic.hostile)
                    return ConsiousTeleport(ch, h, isPhysical);
                return ConsiousTalking(ch, h);
            }
            return ConsiousApport(ch, h, d, isPhysical);
        }
        public bool PlayPhysicalMind(HexSprite h, Character ch)
        {
            /*/ スキル効果は依存関係なしに自由意志によって決定される /*/
            int d = Hex.L1Distance(h, this.pos);
            if(d == 0)
                return ConsiousMeditation(h);
            return ConsiousWatching(h);
        }
        public bool PlaySocialBody(HexSprite h, Character ch)
        {
            /*/ スキル効果はキャラクターに依存して決定される /*/
            bool isPhysical = false;
            if(ch is null)
            {
                if (accessibleArea.ContainsKey(h))
                    return ConciousMove(h, isPhysical);
                if (h.hasWall())
                    return ConsiousBrakeWall(h, isPhysical);
                return ConsiousTeleport(this, h, isPhysical);
            }
            int d = Hex.L1Distance(ch.pos, this.pos);
            if(d == 0)
                return ConsiousDeparture(h);
            if (d == 1)
            {
                if (status.role == growparam.characteristic.hostile)
                    return ConsiousTeleport(ch, h, isPhysical);
                return ConsiousTalking(ch, h);
            }
            return ConsiousApport(ch, h, d, isPhysical);
        }
        public bool PlaySocialMind(HexSprite h, Character ch)
        {
            /*/ スキル効果は役割 = 装備に依存して決定される /*/
            if(ch is null)
                return Consious2space(h);
            if(this.community != ch.community)
                return Consious2inoperate(ch, h);
            if(this != ch)
                return Consious2operate(ch, h);
            return Consious2self(ch, h);
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

        public bool ConsiousDive(HexSprite h)
        {
            this.community.ModeP2S();
            return true;
        }
        public bool ConsiousDeparture(HexSprite h)
        {
            this.community.ModeS2P(this.posParty);
            return true;
        }
        public bool ConciousMove(HexSprite h, bool isPhysical)
        {
            (int, List<HexUnit>, HexSprite) p = accessibleArea[h];
            HexSprite t = h;
            if(!(p.Item3 is null))
            {
                t = p.Item3;

                // TODO: trap process

                // GameObject origin = (GameObject)Resources.Load("Trap/" + this.ani.map.Hex2Trap[t]);
                // GameObject prehubTrap = Instantiate(origin, t.positonWorldTrap(), Quaternion.identity);
                // // デフォルトでfalseにしておく ( プレハブ編集画面の目玉マーク )
                // this.prehubTrap.SetActive(false);
                // this.community.pos.SetGridAsParent(prehubTrap);
                // this.ani.trapPrepared = prehubTrap;
            }
            this.ani.pos = this.pos;
            this.ani.ParamaterControllInit(status.MOV, isPhysical);
            this.ani.StopAllCoroutines();
            this.ani.BeginWalk();
            this.ani.targetList = accessibleArea[t].Item2;

            if(isPhysical)
            {
                foreach(HexUnit u in accessibleArea[t].Item2)
                {
                    this.community.Move(u);
                }
                return true;
            }
            this.posRel += t - this.posRel;
            return true;
        }

        public bool ConsiousTeleport(Character ch, HexSprite h, bool isPhysical)
        {
            // TODO: write teleport process
            // (isPhysical ? yhis.community : this).(teleport skill).levelに依存して決まる値
            // 
            int range = 0;
            if(range < h.norm) return false;
            
            // trap の確認
            // teleport移動のアニメーション

            if(ch.community.mode == Party.MODE.physical)
            {
                ch.community.Teleport(h);
            }
            this.posRel += h - this.posRel;
            return true;
        }
        public bool ConsiousApport(Character ch, HexSprite h, int d, bool isPhysical)
        {
            // TODO: write Apport process
            // (isPhysical ? yhis.community : this).(apport skill).levelに依存して決まる値
            int range = 0;
            if(range < d) return false;

            // trap の確認
            // apport移動のアニメーション

            if(ch.community.mode == Party.MODE.physical)
            {
                ch.community.Teleport(h);
            }
            this.posRel += h - this.posRel;
            return true;
        }
        public bool ConsiousBrakeWall(HexSprite h, bool isPhysical)
        {
            // TODO: write BrakeWall process
            // (isPhysical ? this.community : this).(brake skill).levelに依存して決まる値
            // (装備スキルの場合はどうするの？)
            // スキルレベルそのものを受け取った方が楽では？
            int range = 0;
            if(range < h.norm) return false;
            // layerGround の h に普通の地面を上書き
            // h の周囲1マス以内で layerOnGround の wall を再設定
            // brake のアニメーション
            return false;
        }
        public bool ConsiousTalking(Character ch, HexSprite h)
        {
            // TODO: write Talking process
            // めっちゃ後回しでいい
            return false;
        }
        public bool ConsiousMeditation(HexSprite h)
        {
            // TODO: write Meditation process
            // 仕様未定
            return false;
        }
        public bool ConsiousWatching(HexSprite h)
        {
            // TODO: write Watching process
            // 仕様未定
            return false;
        }
        public bool ConsiousStay(HexSprite h)
        {
            // TODO: write Stay process
            // ターン経過処理
            return false;
        }

        public (HexSprite, Character, bool) ConsiousPhilosophicalZombie()
        {
            // TODO: write non-player AI 
            bool isBody = true;
            /*

            switch(this.community.mode)
            {
                case Party.MODE.physical:
                    // 近づく、離れる、ランダムウォーク、動かない
                    // load something_1
                    isBody = true; // 動かない場合はfalse (meditation, stayする扱い)
                    brake;
                case Party.MODE.social:
                    // 役割に応じた選択、できなければ移動
                    //load something_2
                    brake;
                case Party.MODE.existential:
                    // イベント戦闘用の特殊なAI
                    // load something_3
                    isBody = false;
                    brake;
            }

            // 想定する呼ばれ方
            (HexSprite, Character) p = ch.ConsiousPhilosophicalZombie();
            if(p.Item3) AttentionBody(p.Item1, p.Item2);
            else AttentionMind(p.Item1, p.Item2);

            */

            return (this.pos, this, isBody);
        }
    }    
}
