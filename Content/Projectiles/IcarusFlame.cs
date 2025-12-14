using DreamMod.Common.Graphics;
using DreamMod.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DreamMod.Content.Projectiles
{
    public class IcarusFlame : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = Projectile.height = 128;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.rotation = Main.rand.NextFloatDirection();
            Projectile.timeLeft = 180;
        }
        private static VertexRectangle rect = new();
        public override bool PreDraw(ref Color lightColor)
        {

            Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            ModdedShaderHandler shader = EffectsLoader.shaderHandlers["IcarusFlames"];
            shader.setProperties([Color.Orange.ToVector3(),Color.Red.ToVector3(),Color.Cyan.ToVector3()], TextureAssets.Extra[193].Value,TextureAssets.Projectile[Type].Value, shaderData: new Vector4(Projectile.rotation, Projectile.timeLeft, Projectile.Size.X * 5, Projectile.Size.Y));
            shader.apply();
            rect.Draw(Projectile.Center - Main.screenPosition,Color.White,Projectile.Size * new Vector2(5,2)* Utils.GetLerpValue(0,1,(float)(Projectile.timeLeft) / 30,true),0,Projectile.Center);


            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            
            Main.spriteBatch.End();
    		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

        }

    }
}
