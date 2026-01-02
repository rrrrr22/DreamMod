using DreamMod.Common.Systems;
using DreamMod.Content.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace DreamMod.Content.Projectiles
{
    public class IcarusBullet : BetterModProjectile
    {

        public override Particle ProjectileParticle()
        {
            return Particle.NewParticle(Particle.ParticleType<IcarusBulletParticle>(), Vector2.Zero, ParticleTemplates._default with {dontDrawSelf = true, shaderID = "IcarusBullet",vertexRectSize = new Vector2(256,128), rotation = Projectile.velocity.ToRotation(), endOpacity = 1, endSize = 1f, startColor = Color.Goldenrod,endColor = Color.Red, lifetime = 1000}, this);
        }

    }
}
