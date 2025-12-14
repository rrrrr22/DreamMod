using DreamMod.Common.Systems;
using DreamMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Buffs;
using Origins.Items.Weapons.Demolitionist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace DreamMod.Content.Npcs
{
    [AutoloadBossHead]
    public class IcarusBoss : FiniteStateMachineNPC
    {

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.width = 32;
            NPC.height = 128;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.damage = 120;
            NPC.defense = 125;
            NPC.noTileCollide = true;
            NPC.lifeMax = 125000;
        }
        public override int[] RegisterStates()
        {
            return [
            AIState.StateType<IcarusIdleState>(),
            AIState.StateType<IcarusDashState>(),
            AIState.StateType<IcarusFlamethrowerState>(),
            AIState.StateType<IcarusRepositionToPlayerY>(),
            AIState.StateType<IcarusSlamState>(),
            ];
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D bodyTexture = TextureAssets.Npc[Type].Value;
            spriteBatch.Draw(bodyTexture, NPC.Center - screenPos, drawColor);

            return false;
        }
    }
    // ai2 = state to run after repostionY state
    // ai0 = dash counter
    public class IcarusIdleState : AIState
    {
        Vector2 anchorPos = Vector2.Zero;
        public override void OnEntered(int oldState)
        {
            anchorPos = NPC.Center;
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);

            NPC.TargetClosest();                

            //if (counter % 60 > 10)
            //    NPC.velocity *= 0.7f;

            //if (counter % 180 == 0)
            //{
            //    NPC.velocity = NPC.DirectionTo(NPC.targetRect.Center()) * 60;

            //} else 
            //{
            //     if (counter % 60 == 0)
            //        NPC.velocity = NPC.DirectionTo(NPC.targetRect.Center()).RotateRandom(Main.rand.NextBool() == true ? MathHelper.ToRadians(135) : -MathHelper.ToRadians(135)) * 20;
            //}
    

            anchorPos = Vector2.Lerp(NPC.Center, NPC.targetRect.Center() + new Vector2(550 * NPC.direction * -1,-300), 0.1f);

            NPC.Center = anchorPos;

            if(NPC.ai[0] > 0 || (Main.rand.Next(300) == 0 && counter > 30))
            {
                 ChangeState(StateType<IcarusRepositionToPlayerY>());
                 NPC.ai[2] = StateType<IcarusDashState>();
                 NPC.netUpdate = true;
            }

        }

    }

    public class IcarusDashState : AIState
    {
        public override void OnEntered(int oldState)
        {
            NPC.ai[0]--;
            if (oldState == StateType<IcarusRepositionToPlayerY>())
            {
                NPC.velocity = new Vector2(40 * NPC.direction, 0);
                NPC.ai[0] = 2;
            }
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {

            if(counter % 2 == 0)
                Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center,new Vector2(3 * NPC.direction,-15),ModContent.ProjectileType<IcarusSpamRocket>(),50,0);
            if(counter == 60)
                ChangeState(StateType<IcarusIdleState>());

        }

    }

    public class IcarusFlamethrowerState : AIState
    {
        public override void OnEntered(int oldState)
        {
            base.OnEntered(oldState);
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);
        }
    }
    public class IcarusSlamState : AIState
    {
        public override void OnEntered(int oldState)
        {
            base.OnEntered(oldState);
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);
        }
    }
    public class IcarusRepositionToPlayerY : AIState
    {
        float repositionToY = 0;
        public override void OnEntered(int oldState)
        {
            base.OnEntered(oldState);
            repositionToY = Target.Center.Y;
            NPC.velocity = Vector2.Zero;
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);

            if(counter < 30)
            NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(NPC.Center.X,repositionToY), 0.1f);

            if(counter == 60)
                ChangeState((int)NPC.ai[2]);
        }
    }
}
