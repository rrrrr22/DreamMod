using DreamMod.Common;
using DreamMod.Common.Graphics;
using DreamMod.Common.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
        private static Asset<Texture2D> texture;
        public override void Load()
        {
            texture = Mod.Assets.Request<Texture2D>("Assets/Textures/VFX/Iridescent");
        }
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = Projectile.height = 256;
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
            float explosionAlpha = MathHelper.Lerp(1, 0, MathHelper.Clamp((float)(Projectile.timeLeft - 150) / 30, 0, 1));

            ModdedShaderHandler shader = EffectsLoader.shaderHandlers["IcarusFlames"];
            shader.setProperties([Color.Orange.ToVector3(),Color.Cyan.ToVector3(),Color.Cyan.ToVector3()], TextureAssets.Extra[193].Value,texture.Value, shaderData: new Vector4(Projectile.rotation, Projectile.timeLeft, Projectile.Size.X * 5, Projectile.Size.Y));
            shader.apply();
            rect.Draw(Projectile.Center - Main.screenPosition,Color.White,Projectile.Size * new Vector2(2,2) * explosionAlpha,0,Projectile.Center);
            shader = EffectsLoader.shaderHandlers["GenericExplosion"];
            shader.setProperties([Color.Cyan.ToVector3(),Color.Red.ToVector3(),Color.Wheat.ToVector3()],TextureAssets.Extra[193].Value ,texture.Value, shaderData: new Vector4(Projectile.rotation, Projectile.timeLeft, Projectile.Size.Length(), explosionAlpha));
            shader.apply();
            rect.Draw(Projectile.Center - Main.screenPosition,Color.White,Projectile.Size * new Vector2(1,1)* ((explosionAlpha)),0,Projectile.Center);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            
        }
    }
}
