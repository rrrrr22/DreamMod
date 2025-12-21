using DreamMod.Common.Graphics;
using DreamMod.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace DreamMod.Content.Particles
{
    public class IcarusOrbParticleChild : Particle
    {
        public override string Texture => "Terraria/Images/Extra_98";
        public override void PostUpdate()
        {
            if(parent != null)
            {
                position = parent.position + (rotation.ToRotationVector2() * 64);
                rotation += .125f;
            }
        }

    }
}
