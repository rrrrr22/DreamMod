using Microsoft.Xna.Framework;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace DreamMod.Common.Subworlds
{
    public class CosmicWorld : Subworld
    {
        public override int Width => 500;
        public override int Height => 500;
        public override List<GenPass> Tasks => new()
        {
            new FlatWorld_Pass("Generating The Cosmos...",0.01f)
        };
        static int beachWidth = 0;
        public override void OnEnter()
        {

        }

        public override void OnExit()
        {
        }
    }
    public class FlatWorld_Pass : GenPass
    {
        public FlatWorld_Pass(string name, double loadWeight) : base(name, loadWeight)
        {

        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            int groundLevel = 300;
            Main.spawnTileX = 250;
            Main.spawnTileY = groundLevel;
            Main.worldSurface = 500;
            Main.rockLayer = 500;
            for (int y = groundLevel; y < 500; y++)
                for (int x = 0; x < 500; x++)
                    WorldGen.PlaceWall(x, y, x % 2 == 0 ? WallID.MercuryBrickWall : WallID.StarRoyaleBrickWall, true);

            for (int x = 0; x < 500; x++)
            { 
                WorldGen.PlaceTile(x, groundLevel - 1, TileID.StarRoyaleBrick, true);
                WorldGen.PlaceTile(x, groundLevel - 2, TileID.StarRoyaleBrick, true);
                
                for(int i = 0; i < 1; i++)
                    WorldGen.PlaceTile(x, groundLevel - 23 - 25 * i, TileID.Platforms, true, style: 14);


            }

            for (int y = groundLevel - 25; y < 500; y++)
                for (int x = 0; x < 500; x++)
                    if(x % 6 == 0 && x % 4 == 0)
                        WorldGen.PlaceWall(x, (int)MathHelper.Lerp(y,y - 14,Utils.PingPongFrom01To010(x/500f)), WallID.MercuryBrickWall, true);

        }
    }
    public class CosmicWorldSystem : ModSystem
    {
        public override void PostUpdatePlayers()
        {
            

            //i hope this will not break in upcoming terraria updates...
            if (Main.myPlayer == 255 || !SubworldSystem.IsActive<CosmicWorld>())
                return;

        }
        public override void PreUpdateWorld()
        {
            if (SubworldSystem.IsActive<CosmicWorld>())
            {
                Wiring.UpdateMech();
                TileEntity.UpdateStart();
                foreach (TileEntity te in TileEntity.ByID.Values)
                {
                    te.Update();
                }
                TileEntity.UpdateEnd();
                if (++Liquid.skipCount > 1)
                {
                    Liquid.UpdateLiquid();
                    Liquid.skipCount = 0;
                }
            }
        }
    }
    public class CosmicWorldPlayer : ModPlayer
    {
        public override bool CanUseItem(Item item)
        {
            if (Main.LocalPlayer != Player || !SubworldSystem.IsActive<CosmicWorld>() || (item.createTile == -1 && item.pick == 0 && item.axe == 0))
                return true;

            return false;
        }
    }

    public class ComsicWorldGlobalNPC : GlobalNPC
    {

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (SubworldSystem.IsActive<CosmicWorld>())
                maxSpawns = 0;
        }

    }

    public class CosmicWorldModBiome : ModBiome
    {
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => new CosmicWorldModSurfaceBackgroundStyle();
         
        public override float GetWeight(Player player)
        {
            return 1f;
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override bool IsBiomeActive(Player player)
        {
            return SubworldSystem.IsActive<CosmicWorld>();
        }
    }

    public class CosmicWorldModSurfaceBackgroundStyle : ModSurfaceBackgroundStyle
    {
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
        }
        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
        {
            return ModContent.GetModBackgroundSlot("DreamMod/Common/Subworlds/CosmicBG");
        }
    }
}
