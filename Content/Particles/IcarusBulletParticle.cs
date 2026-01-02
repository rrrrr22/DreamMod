using DreamMod.Common.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace DreamMod.Content.Particles
{
    public class IcarusBulletParticle : Particle
    {
        public override string Texture => "DreamMod/Assets/Textures/VFX/Iridescent";
        public override void PostUpdate()
        {
            base.PostUpdate();

            if (parentProjectile != null)
            {
                rotation = parentProjectile.Projectile.velocity.ToRotation() - MathHelper.Pi;
                position = parentProjectile.Projectile.Center + parentProjectile.Projectile.velocity;

            }
        }
    }
}
