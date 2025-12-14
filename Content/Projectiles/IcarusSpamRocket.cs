using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Items.Weapons.Ammo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace DreamMod.Content.Projectiles
{
    public class IcarusSpamRocket : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.RocketI;
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            
        }

        public override void AI()
        {
            base.AI();
            Dust.NewDustPerfect(Projectile.Center,DustID.Smoke,Vector2.Zero,newColor: Color.Orange);
            Projectile.velocity.Y += .25f;
            Projectile.velocity.WithMaxLength(15);

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D dingStar = Mod.Assets.Request<Texture2D>("Assets/Textures/VFX/DingStar").Value;
            Main.EntitySpriteDraw(texture,Projectile.Center - Main.screenPosition,null,Color.White,Projectile.velocity.ToRotation() + MathHelper.PiOver2,texture.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(dingStar,Projectile.Center - Main.screenPosition,null,Color.White,Projectile.velocity.ToRotation() + MathHelper.PiOver2,dingStar.Size() / 2f,MathF.Sin(Projectile.timeLeft * 0.2f) * 0.1f + .1f, SpriteEffects.None);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            PunchCameraModifier p = new(Projectile.Center,Main.rand.NextVector2Circular(1,1),5,3,5,100000);
            Main.instance.CameraModifiers.Add(p);
            Projectile.NewProjectile(Projectile.GetSource_Death(),Projectile.Center,Vector2.Zero,ModContent.ProjectileType<IcarusFlame>(),50,0);

            return true;
        }
    }
}
