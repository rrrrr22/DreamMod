using DreamMod.Common;
using DreamMod.Common.Graphics;
using DreamMod.Common.Graphics.Primitives;
using DreamMod.Common.Systems;
using DreamMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Buffs;
using Origins.Items.Weapons.Demolitionist;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static DreamMod.Content.Npcs.IcarusBoss;

namespace DreamMod.Content.Npcs
{
    [AutoloadBossHead]
    public class IcarusBoss : FiniteStateMachineNPC
    {
        public const int CONTACTDAMAGE = 50;
        public enum Perspectives
        {
            Front = 0,
            Side = 1,
        }
        public Perspectives CurrentPerspective
        {
            get => (Perspectives)(int)(NPC.localAI[1]);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.width = 32;
            NPC.height = 128;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.damage = CONTACTDAMAGE;
            NPC.defense = 125;
            NPC.noTileCollide = true;
            NPC.lifeMax = 125000;
            NPC.scale = 1f;
            AnimationType = NPCID.DukeFishron;
            dashFlashAlphaTweens.Tweens =
            [
            new Tween<float>(MathHelper.Lerp,false).SetProperties(0,1,TweenEaseType.OutBack,20),
            new Tween<float>(MathHelper.Lerp,false).SetProperties(1,0.8f,TweenEaseType.OutBack,25)
            ];

            dashFlashLengthTweens.Tweens =
            [
            new Tween<float>(MathHelper.Lerp,false).SetProperties(0,128,TweenEaseType.OutBack,30),
            new Tween<float>(MathHelper.Lerp,false).SetProperties(128,0,TweenEaseType.None,15)
            ];

            dashFlashRotationTweens.Tweens =
            [
            new Tween<float>(MathHelper.Lerp,false).SetProperties(0,0,TweenEaseType.OutBack,15),
            new Tween<float>(MathHelper.Lerp,false).SetProperties(MathHelper.TwoPi / 3f,0,TweenEaseType.OutBack,30),
            ];
        }
        public override int[] RegisterStates()
        {
            return [
            AIState.StateType<IcarusIdleState>(),
            AIState.StateType<IcarusDashState>(),
            AIState.StateType<IcarusFlamethrowerState>(),
            AIState.StateType<IcarusRepositionToPlayerY>(),
            AIState.StateType<IcarusSlamState>(),
            ];
        }
        public TweenHandler<float> dashFlashLengthTweens = new();
        public TweenHandler<float> dashFlashRotationTweens = new();
        public TweenHandler<float> dashFlashAlphaTweens = new();
        private static VertexStrip strip = new();
        private static VertexRectangle rect = new();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D bodyTexture = TextureAssets.Npc[Type].Value;
            //drawColor = (CurrentPerspective == Perspectives.Side ? Color.Silver : Color.Firebrick);
            this.ApplyZDepthColor(ref drawColor);
            Vector2 scale = new Vector2(NPC.scale *5);
            this.ApplyZDepthScale(ref scale);
            Rectangle rectangle = bodyTexture.Frame(1,8,0,1);
            var drawData = new DrawData(bodyTexture, NPC.Center - (screenPos) - Vector2.UnitY * MathHelper.Lerp(1, 0, zDepth) * 64f, null, drawColor, NPC.rotation, bodyTexture.Size() / 2f, scale, NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

            DrawVFX(drawData, spriteBatch);
            DrawThrustersAndCore(drawData.position);

            drawData.Draw(spriteBatch);

            return false;
        }
        public void DrawThrustersAndCore(Vector2 screenPos)
        {


            switch (CurrentPerspective)
            {

                case Perspectives.Side:
                    {
                        bool isVelocityBased = NPC.velocity.Length() < 30;

                        Vector2 dirAndLength = new Vector2(75 * -NPC.direction, 75);
                        dirAndLength = isVelocityBased ? dirAndLength : -NPC.velocity * 7;

                        DrawOneThruster(screenPos + new Vector2(32, 32) * zDepth + dirAndLength, dirAndLength * zDepth, (!isVelocityBased ? dirAndLength.Length() / 2 : 0) + 128 * zDepth, Color.Turquoise, screenPos, false, Vector2.UnitY * MathHelper.Lerp(0,1,zDepth) * 120f);
                        DrawOneThruster(screenPos - new Vector2(32, -32) * zDepth  + dirAndLength, dirAndLength * zDepth, (!isVelocityBased ? dirAndLength.Length() / 2 : 0) + 128 * zDepth, Color.Turquoise, screenPos, true, Vector2.UnitY * MathHelper.Lerp(0,1,zDepth) * 120f);
                        break;
                    }

                case Perspectives.Front:
                    {
                        bool isVelocityBased = NPC.velocity.Length() < 30;
                        Vector2 dirAndLength = new Vector2(75, 75);
                        dirAndLength = isVelocityBased ? dirAndLength : -NPC.velocity * 7;

                        DrawOneThruster(screenPos + new Vector2(32, 32) + dirAndLength* zDepth, dirAndLength, (!isVelocityBased ? dirAndLength.Length() / 2 : 0)  + 128 * MathHelper.Clamp(MathHelper.Lerp(0.0f,1f,zDepth),0,1f), Color.Turquoise, screenPos, false, -Vector2.UnitY * MathHelper.Lerp(1,0,zDepth) * 120f);

                        dirAndLength = new Vector2(-75, 75);
                        dirAndLength = isVelocityBased ? dirAndLength : -NPC.velocity * 7;

                        DrawOneThruster(screenPos - new Vector2(32, -32) + dirAndLength * zDepth, dirAndLength, (!isVelocityBased ? dirAndLength.Length() / 2 : 0) + 128 * MathHelper.Clamp(MathHelper.Lerp(0.0f,1f,zDepth),0,1f), Color.Turquoise, screenPos, true, -Vector2.UnitY * MathHelper.Lerp(1,0,zDepth) * 120f);

                        break;
                    }

            }

        }

        public void DrawOneThruster(Vector2 position, Vector2 lengthAndDirection, float width, Color color, Vector2 screenPos, bool flip, Vector2 originOffset = default)
        {
            ModdedShaderHandler shader = EffectsLoader.shaderHandlers["IcarusThrusters"];
            shader.setProperties([Color.Turquoise.ToVector3(), Color.Turquoise.ToVector3(), Color.Turquoise.ToVector3()], TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion].Value, shaderData: new Vector4(flip ? 1 : 1));
            shader.apply();

            Vector2[] positions = [position, position + lengthAndDirection];
            float[] rotations = [position.DirectionTo(position + lengthAndDirection).ToRotation(), position.DirectionTo(position - lengthAndDirection).ToRotation()];
            if (flip)
            {
                positions = [position, position + lengthAndDirection];
                rotations = [position.DirectionTo(position + lengthAndDirection).ToRotation(), position.DirectionTo(position - lengthAndDirection).ToRotation() + MathHelper.TwoPi];

            }
            //strip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => MathHelper.Lerp(width, 0, p), -Main.screenPosition, true);
            //strip.DrawTrail();
            //width *= 1.5f;
            //strip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => MathHelper.Lerp(width, 0, p), -Main.screenPosition, true);
            //strip.DrawTrail();
            //width *= 2f;
            //strip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => MathHelper.Lerp(width, 0, p), -Main.screenPosition, true);
            //strip.DrawTrail();
            //strip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => MathHelper.Lerp(width, MathHelper.Lerp(width,0,p), p), -Main.screenPosition, true);
            //strip.DrawTrail();
            rect.Draw(position, Color.White, new Vector2(lengthAndDirection.Length() * 2, width), lengthAndDirection.ToRotation(), (position));

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
        public override void PostOnSpawn(IEntitySource source)
        {
            spawnPosition = NPC.Center;
            spawnRotation = NPC.rotation;
        }
        private static VertexRectangle muzzleFlash = new();
        public Vector2 spawnPosition = Vector2.Zero;
        public float spawnRotation = 0;
        public void PostNPCDrawVFX()
        {
        
            ModdedShaderHandler shader = EffectsLoader.shaderHandlers["IcarusMuzzleFlash"];
            shader.setProperties([Color.White.ToVector3(),Color.White.ToVector3(),Color.White.ToVector3()],TextureAssets.Extra[193].Value);
            shader.apply();
            muzzleFlash.Draw(spawnPosition - Main.screenPosition,Color.White,new Vector2(128,32),spawnRotation,spawnPosition - Main.screenPosition,64);
        
        }
        public void DrawVFX(DrawData mainSprite, SpriteBatch spriteBatch)
        {
            if (NPC.velocity.Length() > 0)
            {
                var datas = DreamUtils.DrawData_AfterImage(mainSprite, 15, Color.AliceBlue, -NPC.velocity.SafeNormalize(Vector2.UnitY) * 128, true);
                foreach (DrawData data in datas)
                    data.Draw(spriteBatch);
            }
            if (dashFlashLengthTweens.currentTween.state == TweenState.Running && dashFlashRotationTweens.currentTween.state == TweenState.Running && dashFlashAlphaTweens.currentTween.state == TweenState.Running)
            {
                Color color = Color.White;

                if (currentState == AIState.StateType<IcarusFlamethrowerState>())
                    color = Color.Orange;
                if (currentState == AIState.StateType<IcarusSlamState>())
                    color = Color.Blue;


                var datas = DreamUtils.DrawData_Spliting(mainSprite, 3, dashFlashRotationTweens.currentTween.currentProgress, color * dashFlashAlphaTweens.currentTween.currentProgress, dashFlashLengthTweens.currentTween.currentProgress);
                foreach (DrawData data in datas)
                    data.Draw(spriteBatch);
            }

        }

        public override void PreStateUpdate()
        {
            dashFlashAlphaTweens.Update();
            dashFlashLengthTweens.Update();
            dashFlashRotationTweens.Update();

            if (NPC.localAI[0] == 1)
            {
                dashFlashAlphaTweens.PlayTweens();
                dashFlashLengthTweens.PlayTweens();
                dashFlashRotationTweens.PlayTweens();
                NPC.localAI[0] = 0;
            }
        }
    }
    // ai2 = state to run
    // ai0 = dash counter
    public class IcarusIdleState : AIState
    {
        Vector2 anchorPos = Vector2.Zero;
        public override void OnEntered(int oldState)
        {
            anchorPos = NPC.Center;
            NPC.localAI[1] = (float)Perspectives.Side;
            NPC.rotation = 0;
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);

            NPC.TargetClosest();
                        NPC.localAI[1] = (float)Perspectives.Side;

            //if (counter % 60 > 10)
            //    NPC.velocity *= 0.7f;

            //if (counter % 180 == 0)
            //{
            //    NPC.velocity = NPC.DirectionTo(NPC.targetRect.Center()) * 60;

            //} else 
            //{
            //     if (counter % 60 == 0)
            //        NPC.velocity = NPC.DirectionTo(NPC.targetRect.Center()).RotateRandom(Main.rand.NextBool() == true ? MathHelper.ToRadians(135) : -MathHelper.ToRadians(135)) * 20;
            //}



            NPC.Center = Vector2.Lerp(NPC.Center, NPC.targetRect.Center() + new Vector2(550 * NPC.direction * -1, -300), 0.1f);

            if ((Main.rand.Next(30) == 0 && counter > 120))
            {
                switch (Main.rand.Next(2))
                {
                    case 0:
                        {
                            ChangeState(StateType<IcarusFlamethrowerState>());

                            break;
                        }
                    case 1:
                        {
                            ChangeState(StateType<IcarusSlamState>());
                            break;
                        }
                }

                NPC.netUpdate = true;
            }

        }

    }

    public class IcarusDashState : AIState
    {
        public override void OnEntered(int oldState)
        {
            NPC.ai[0]--;
            NPC.velocity = (Target.Center + Target.velocity.SafeNormalize(Vector2.UnitY) * 40).DirectionFrom(NPC.Center) * 40;
            NPC.localAI[1] = (float)Perspectives.Side;
            SoundEngine.PlaySound(SoundID.Item131 with { Pitch = 0},NPC.Center);
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {

            if (counter % 2 == 0 && counter < 45)
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(3 * NPC.direction, -15), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 3, -1).rotation = NPC.direction * MathHelper.PiOver2 + MathHelper.Pi;

            if (counter >= 45)
            {
                NPC.velocity *= 0.8f;
            }

            if (counter == 60)
            {
                NPC.velocity *= 0f;
                if (NPC.ai[0] > 0)
                {
                    ChangeState(StateType<IcarusFlamethrowerState>());
                }
                else
                    ChangeState(StateType<IcarusIdleState>());

            }

        }

    }

    public class IcarusFlamethrowerState : AIState
    {
        public override void OnEntered(int oldState)
        {
            base.OnEntered(oldState);
            if (NPC.ai[0] <= 0)
                NPC.ai[0] = 3;
            NPC.localAI[0] = 1;
            NPC.localAI[1] = (float)Perspectives.Side;

        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);
            NPC.Center = Vector2.Lerp(NPC.Center, Target.Center + new Vector2(500 * NPC.direction * -1, MathF.Sin(counter * .1f) * 50), 0.2f) + Target.velocity;

            if (counter < 30)
                return;

            for (int i = 0; i < 1; i++)
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 17).rotation = Main.rand.NextVector2Circular(0, 15).ToRotation();


            if (counter >= 100)
                ChangeState(StateType<IcarusRepositionToPlayerY>());

        }
    }
    public class IcarusSlamState : AIState
    {
        public bool hasReachedPlayer = false;
        public bool isMovingBackToNormalZ = false;
        public bool isFinishedSlamming = false;
        public override void OnEntered(int oldState)
        {
            base.OnEntered(oldState);
            NPC.damage = 0;
            hasReachedPlayer = false;
            isMovingBackToNormalZ = false;
            isFinishedSlamming = false;
            NPC.localAI[1] = (float)Perspectives.Front;

        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            if (counter == 70 || counter == 260)
                NPC.localAI[0] = 1;

            if (isFinishedSlamming)
            {
                NPC.velocity *= 0.9f;
                if (counter == 60)
                {
                    NPC.velocity = Vector2.Zero;
                    ChangeState(StateType<IcarusIdleState>());

                }
                NPC.localAI[1] = (float)Perspectives.Front;

                return;
            }

            if (counter <= 60)
            {
                zDepth = MathHelper.Lerp(1, 0, DreamUtils.EaseOutBack(((counter) / 30f)));

                return;
            }

            if (counter <= 120f)
            {
                NPC.Center = Target.Center + new Vector2(-NPC.direction * 150, -NPC.height - 175);
                zDepth = MathHelper.Lerp(0, 1, DreamUtils.EaseOutBack(((counter - 60f) / 60f)));

                return;
            }

            base.OnStateUpdate(info);
            if (counter % 15 > 5 && counter < 310) NPC.velocity *= 0.75f;
            if (counter % 15 == 14 && counter < 250)
            {
                if (counter != 14)
                    for (int i = 0; i < 3; i++)
                        Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 15, -1).rotation = new Vector2(-NPC.direction, 0).ToRotation();
                NPC.localAI[1] = (float)Perspectives.Side;
                SoundEngine.PlaySound(SoundID.Item131 with { Pitch = 1},NPC.Center);

                NPC.velocity = new Vector2(NPC.direction * (Target.velocity.Length() + 65), -5);
            }

            if (counter == 265) NPC.velocity *= 0;
            if (counter == 310) NPC.velocity = new Vector2(0, 60);
            if (counter >= 310 && counter % 2 == 0)
            {
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 12, -1).rotation = 0;
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 12, -1).rotation = MathHelper.Pi;
                NPC.localAI[1] = (float)Perspectives.Front;
                NPC.rotation = Vector2.UnitY.ToRotation();
                SoundEngine.PlaySound(SoundID.Item131 with { Pitch = 1},NPC.Center);

            }
            if (NPC.Center.Y > Target.Center.Y + 500)
            {

                isFinishedSlamming = true;
                counter = 0;
                NPC.rotation = 0;
            }
        }
    }
    public class IcarusRepositionToPlayerY : AIState
    {
        float repositionToY = 0;
        public override void OnEntered(int oldState)
        {
            base.OnEntered(oldState);
            repositionToY = Target.Center.Y;
            NPC.velocity = Vector2.Zero;
            NPC.localAI[1] = (float)Perspectives.Side;

        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            base.OnStateUpdate(info);

            if (counter < 30)
                NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(NPC.Center.X, repositionToY), 0.1f);

            if (counter == 60)
            {
                if (NPC.ai[0] > 0)
                {
                    ChangeState(StateType<IcarusDashState>());
                }
                else
                    ChangeState(StateType<IcarusIdleState>());

            }
        }
    }
}
