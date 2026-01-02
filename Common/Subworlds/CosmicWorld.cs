using DreamMod.Common.Graphics;
using DreamMod.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Ambience;
using Terraria.GameContent.Skies;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.Assets;
using Terraria.WorldBuilding;
using tModPorter;

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
        public override void OnEnter()
        {
            SubworldSystem.hideUnderworld = true;
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
            int groundLevel = 425;
            Main.spawnTileX = 250;
            Main.spawnTileY = groundLevel;
            Main.worldSurface = 500;
            Main.rockLayer = 500;
            for (int y = groundLevel; y < 500; y++)
                for (int x = 0; x < 500; x++)
                    WorldGen.PlaceTile(x, y, x % 2 == 0 ? TileID.MercuryBrick : TileID.StarRoyaleBrick, true);

            for (int x = 0; x < 500; x++)
            {


                for (int i = 0; i < 1; i++)
                    WorldGen.PlaceTile(x, groundLevel - 23 - 25 * i, TileID.Platforms, true, style: 14);


            }

            for (int y = groundLevel - 25; y < 500; y++)
                for (int x = 0; x < 500; x++)
                    if (x % 6 == 0 && x % 4 == 0)
                        WorldGen.PlaceWall(x, (int)MathHelper.Lerp(y, y - 14, Utils.PingPongFrom01To010(x / 500f)), WallID.MercuryBrickWall, true);

        }
    }
    public class CosmicWorldSystem : ModSystem
    {
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
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<CosmicWorldModSurfaceBackgroundStyle>();
        
        public override float GetWeight(Player player)
        {
            return 1f;
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override bool IsBiomeActive(Player player)
        {
            return true;
        }

        public override void OnInBiome(Player player)
        {
            if (SubworldSystem.IsActive<CosmicWorld>())
            {
                DisableWorldBackgroundElements();
                player.wingTime = player.wingTimeMax;
                
            }
        }

        public static void DisableWorldBackgroundElements()
        {

            for (int i = 0; i < Main.maxClouds; i++)
            {
                Main.cloud[i].active = false;
            }
            Main.cloudBGActive = 0;



        }
    }
    public class CosmicWorldModSurfaceBackgroundStyle : ModSurfaceBackgroundStyle
    {
        Asset<Texture2D> IcarusBG;
        Asset<Texture2D> IcarusBGMask;


        static ContentManager ContentManager => Main.instance.Content;
        public override void Load()
        {
            //if (Main.netMode != NetmodeID.Server)
            //{
            //    Main.RunOnMainThread(() =>
            //    {
            //        cosmicBG = TextureCube.DDSFromStreamEXT(Main.instance.GraphicsDevice, Mod.GetFileStream("Assets/Textures/Backgrounds/IcarusBG.dds"));
            //    }).Wait();
            //}
            IcarusBG = Mod.Assets.Request<Texture2D>("Assets/Textures/Backgrounds/IcarusBG");
            IcarusBGMask = Mod.Assets.Request<Texture2D>("Assets/Textures/Backgrounds/IcarusBG_Mask");
        }
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {

        }
        private static VertexRectangle rect = new();
        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
        {
            return -1;
        }
        public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
        {


            ModdedShaderHandler shader = EffectsLoader.shaderHandlers["Cosmic3DBorders"];

            shader.setProperties([Color.Goldenrod.ToVector3(), Color.PowderBlue.ToVector3(), Color.Transparent.ToVector3()],IcarusBG.Value,IcarusBGMask.Value, shaderData: new Vector4(0, 0, 0, 0));
            shader.apply();

            rect.Draw(Main.Camera.Center - Main.screenPosition, Color.White, size: Main.ScreenSize.ToVector2(), rotationCenter: Vector2.Zero);

            shader = EffectsLoader.shaderHandlers["CosmicBackground"];

            shader.setProperties([Color.Goldenrod.ToVector3(), Color.PowderBlue.ToVector3()]);
            shader.apply();

            rect.Draw(Main.Camera.Center - Main.screenPosition, Color.White, size: Main.ScreenSize.ToVector2(), rotationCenter: Main.LocalPlayer.Center);

            //shader = EffectsLoader.shaderHandlers["BackgroundBlackhole"];

            //shader.setProperties([Color.Goldenrod.ToVector3(), Color.PowderBlue.ToVector3(), Color.Transparent.ToVector3()], TextureAssets.Extra[193].Value, TextureAssets.Extra[ExtrasID.FlameLashTrailShape].Value, shaderData: new Vector4(0, 0, 0, 0));
            //shader.apply();

            //rect.Draw(Main.Camera.Center - Main.screenPosition, Color.White, size: Main.ScreenSize.ToVector2(), rotationCenter: Main.LocalPlayer.Center);

            return false;
        }
    }

}
