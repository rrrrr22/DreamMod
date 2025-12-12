using DreamMod.Common.Subworlds;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DreamMod.Content.Items
{

    public class DebugItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_1";
        public override void SetDefaults()
        {
            Item.consumable = false;
            Item.width = Item.height = 32;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool? UseItem(Player player)
        {
            if (SubworldSystem.Current == null && player.ItemAnimationJustStarted)
                SubworldSystem.Enter<CosmicWorld>();
            return true;
        }
    }
}
