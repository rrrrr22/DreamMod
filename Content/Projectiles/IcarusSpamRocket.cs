using DreamMod.Common;
using DreamMod.Common.Graphics;
using DreamMod.Common.Graphics.Primitives;
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
        public int MaxTimeLeft = 150;

        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.RocketI;
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 30 + Main.rand.Next(0,60);

        }
        public ref float target => ref Projectile.ai[0];
        public override void AI()
        {
            base.AI();
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * Projectile.ai[1];

            if(target != -1)
            {
                Player player = Main.player[(int)target];
               // float distCatchingUp = MathHelper.Clamp(Utils.GetLerpValue(1,4,player.Distance(Projectile.Center) / 250),1,4);
                Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.Center.DirectionTo(player.Center).ToRotation(),0.025f * MathHelper.Lerp(.25f,3,(float)Projectile.timeLeft / MaxTimeLeft));
                Projectile.velocity += Projectile.ai[2] == -1 ? Vector2.Zero : player.velocity;
                Projectile.tileCollide = false;
            }

            Dust.NewDustPerfect(Projectile.Center,DustID.Smoke,Vector2.Zero,newColor: Color.Orange);
            


        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D dingStar = Mod.Assets.Request<Texture2D>("Assets/Textures/VFX/DingStar").Value;
            

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            Main.EntitySpriteDraw(texture,Projectile.Center - Main.screenPosition,null,Color.White,Projectile.velocity.ToRotation() + MathHelper.PiOver2,texture.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(dingStar,Projectile.Center - Main.screenPosition,null,Color.White,Projectile.velocity.ToRotation() + MathHelper.PiOver2,dingStar.Size() / 2f,MathF.Sin(Projectile.timeLeft * 0.2f) * 0.1f + .1f, SpriteEffects.None);



            return false;
        }

        public override void OnKill(int timeLeft)
        {
            PunchCameraModifier p = new(Projectile.Center,Main.rand.NextVector2Circular(1,1),5,3,5,100000);
            Main.instance.CameraModifiers.Add(p);
            Projectile.NewProjectile(Projectile.GetSource_Death(),Projectile.Center,Vector2.Zero,ModContent.ProjectileType<IcarusFlame>(),50,0);
            if(Main.rand.NextBool(5))
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 1},Projectile.Center);

        }

    }
}
