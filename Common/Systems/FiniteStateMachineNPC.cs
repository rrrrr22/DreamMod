using DreamMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Tiles.Riven.Shelf_Coral_TE;

namespace DreamMod.Common.Systems
{
    public abstract class FiniteStateMachineNPC : ModNPC, IZDepth
    {
        public NPCAIStates states = null;
        public int currentState
        {
            get
            {
                if (states != null)
                    return states.currentState.type;
                return -1;

            }

        }
        public int currentStateCounter => states.currentState.counter;
        public static int StateType<T>() where T : AIState => ModContent.GetInstance<T>().type;

        public override sealed void OnSpawn(IEntitySource source)
        {
            zDepth = 1;
            states = new(NPC, RegisterStates());
            PostOnSpawn(source);
        }
        public virtual void PostOnSpawn(IEntitySource source)
        {

        }
        public virtual int[] RegisterStates()
        {
            return [];
        }
        public override sealed bool PreAI()
        {
            PreStateUpdate();
            states.Update();
            //zDepth = MathF.Sin((float)Main.timeForVisualEffects * 0.01f) * 2 + 2;

            PostStateUpdate();
            return false;
        }
        public virtual void PreStateUpdate() { }
        public virtual void PostStateUpdate()
        {
        }
        public static int NewProjectileWithMPCheck(IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return Projectile.NewProjectile(spawnSource, position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
            else
                return -1;
        }
        public static int NewNPCWithMPCheck(IEntitySource source, Vector2 Position, int Type, int Start = 0, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f, int Target = 255)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return NPC.NewNPC(source, (int)Position.X, (int)Position.Y, Type, Start, ai0, ai1, ai2, ai3, Target);
            else
                return -1;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.aiAction);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.aiAction = reader.ReadInt32();
        }

        public float zDepth { get; set; }
    }
    public class AIState : ILoadable
    {
        public static AIState NewAIState(NPC npc, int type)
        {
            AIState state = (AIState)AIStates[type].MemberwiseClone();
            state.NPC = npc;
            return state;
        }
        public virtual void StateDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
        
        }
        public float zDepth
        {
            get => ((FiniteStateMachineNPC)NPC.ModNPC).zDepth; 
            
            set
            {
                ((FiniteStateMachineNPC)NPC.ModNPC).zDepth = value;
            }
        }
        public static int StateType<T>() where T : AIState => ModContent.GetInstance<T>().type;
        public static readonly List<AIState> AIStates = new List<AIState>();
        public int counter = 0;
        public Action<AIState> changeState;
        public Player Target
        {
            get
            {
                NPC.TargetClosest();
                if (NPC.HasValidTarget)
                {
                    Player player = Main.player[NPC.target];
                    if (player.Center.X == NPC.Center.X)
                        NPC.direction = player.direction;
                    return player;

                }
                return null;
            }

        }
        public void ChangeState(int state)
        {
            NPC.aiAction = state;
            changeState.Invoke(this);
        }
        public NPC NPC { get; set; } = null;
        public virtual void OnEntered(int oldState) { }
        public virtual void OnStateUpdate(CommonNPCInfo info) { }
        public virtual void OnExited(int newState) { }
        public int type { get; private set; }
        public void Load(Mod mod)
        {
            type = AIStates.Count;
            AIStates.Add(this);
        }
        public void Unload()
        {
        }

        public bool ChangeStateIfTargetNull(int state)
        {

            if (Target == null)
            {
                ChangeState(state);
                return true;
            }

            return false;
        }
    }

    public class NPCAIStates
    {
        public NPCAIStates(NPC npc, params int[] states)
        {
            this.npc = npc;
            foreach (int type in states)
            {
                this.states[type] = AIState.NewAIState(npc, type);
                this.states[type].changeState += OnStateChange;
            }
            currentState = this.states[states[0]];
        }
        private Dictionary<int, AIState> states = [];
        public bool justHitTheGround = false;
        public bool inGround = false;
        public bool justMovedAwayFromTheGround = false;
        public bool afterimages = false;
        public void Update()
        {

            if (justHitTheGround)
            {
                justHitTheGround = false;

            }

            if (npc.collideY && !inGround)
            {

                justHitTheGround = true;
                inGround = true;
            }

            if (justMovedAwayFromTheGround)
            {
                justMovedAwayFromTheGround = false;
            }

            if (!npc.collideY && inGround)
            {
                inGround = false;
                justMovedAwayFromTheGround = true;
            }

            CommonNPCInfo info = new CommonNPCInfo();
            info.justMovedAwayFromTheGround = justMovedAwayFromTheGround;
            info.inGround = inGround;
            info.justHitTheGround = justHitTheGround;
            currentState.OnStateUpdate(info);
            currentState.counter++;
        }
        public void OnStateChange(AIState oldState)
        {
            oldState.counter = 0;
            oldState.OnExited(npc.aiAction);
            AIState newState = states[npc.aiAction];
            currentState = newState;
            currentState.OnEntered(oldState.type);
        }

        public AIState currentState = null;
        public NPC npc = null;

    }

    public struct CommonNPCInfo
    {
        public bool justHitTheGround;
        public bool inGround;
        public bool justMovedAwayFromTheGround;
    }

    public class Boss_Despawn : AIState
    {

        public override void OnStateUpdate(CommonNPCInfo info)
        {
            NPC.position.Y += 30;
            NPC.velocity = Vector2.Zero;
            NPC.EncourageDespawn(30);
        }

    }

}
