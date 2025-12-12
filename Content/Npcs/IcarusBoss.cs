using DreamMod.Common.Systems;
using Origins;
using Origins.Items.Weapons.Demolitionist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            AIState.StateType<IcarusDashState>()        
            ];
        }

    }

    public class IcarusIdleState : AIState
    {

        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);

            NPC.velocity *= 0.95f;          


        }
    
    }

    public class IcarusDashState : AIState
    {
        public override void OnEntered(int oldState)
        {
            if(oldState == StateType<IcarusIdleState>())
            {
                
            }
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            
        }
        
    }
}
