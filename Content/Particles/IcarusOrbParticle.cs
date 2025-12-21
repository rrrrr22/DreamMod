using DreamMod.Common.Systems;
using Microsoft.Build.Construction;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace DreamMod.Content.Particles
{
    internal class IcarusOrbParticle : Particle
    {
        public override string Texture => "Terraria/Images/Extra_49";
        public override void PostUpdate()
        {   
            if(parentProjectile != null)
                position = parentProjectile.Projectile.Center;
        }

        public override void OnSpawn()
        {
            for(float i = 0; i < MathHelper.TwoPi; i += MathHelper.TwoPi / 4)
            {
                NewParticle(ParticleType<IcarusOrbParticleChild>(), Vector2.Zero, ParticleTemplates._default with {dontDrawSelf = true, stripShaderID = "IcarusOrbStar",parent = this, rotation = i, endOpacity = 1, endSize = 1f, startColor = Color.Lerp(Color.Red,Color.Lerp(Color.Green,Color.Blue,(i/2f) / (MathHelper.TwoPi / 2)),1+(i/2f) / (MathHelper.TwoPi / 2)), lifetime = 1000}, parentProjectile);
            }
        }
    }
}
