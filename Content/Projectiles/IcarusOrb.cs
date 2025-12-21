using DreamMod.Common.Systems;
using DreamMod.Content.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DreamMod.Content.Projectiles
{
    public class IcarusOrb : BetterModProjectile
    {

       public override void SetStaticDefaults()
        {
        }
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.hostile = true;
            Projectile.width = Projectile.height = 64;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 1000;
            maxTimeLeft = 1000;

        }

        public override Particle ProjectileParticle()
        {
            return Particle.NewParticle(Particle.ParticleType<IcarusOrbParticle>(),Projectile.Center,ParticleTemplates._default with { shaderID = "IcarusOrbStar", vertexRectSize = new Vector2(256f), dontDrawSelf = true, startColor = Color.Red, endSize = 1f }, this);
        }

    }
}
