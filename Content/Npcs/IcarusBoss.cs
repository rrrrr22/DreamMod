using DreamMod.Common;
using DreamMod.Common.Graphics;
using DreamMod.Common.Graphics.Primitives;
using DreamMod.Common.Systems;
using DreamMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
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
    public class IcarusBossCore : FiniteStateMachineNPC
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.None;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 15;
            NPCID.Sets.TrailingMode[Type] = 3;

        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.width = 32;
            NPC.height = 32;
            NPC.knockBackResist = 0f;
            NPC.boss = false;
            NPC.aiStyle = -1;
            NPC.damage = CONTACTDAMAGE;
            NPC.defense = 100;
            NPC.noTileCollide = true;
            NPC.lifeMax = 250000;
            NPC.scale = 1f;
        }
        public override void PostOnSpawn(IEntitySource source)
        {
            NPC.realLife = NPC.NewNPCDirect(null, NPC.Center, ModContent.NPCType<IcarusBoss>(), 0).whoAmI;
        }
        public override int[] RegisterStates()
        {
            return [AIState.StateType<CoreStateIdle>()];
        }
        private static Asset<Texture2D> texture;
        public override void Load()
        {
            texture = ModContent.Request<Texture2D>("DreamMod/Assets/Textures/VFX/Iridescent");

        }
        private static VertexRectangle rect = new VertexRectangle();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            for (int i = NPC.oldPos.Length - 1; i >= 0; i--)
            {

                ModdedShaderHandler shader = EffectsLoader.shaderHandlers["IcarusCore"];
                shader.setProperties([Color.Lerp(Color.Cyan, Color.GreenYellow, Utils.PingPongFrom01To010((float)i / NPC.oldPos.Length + (float)Main.timeForVisualEffects / 30f)).ToVector3()], texture.Value, shaderData: new Vector4(i, NPC.oldPos.Length, 1 - (float)(i) / NPC.oldPos.Length, 0));
                shader.apply();
                rect.Draw(NPC.oldPos[i] + NPC.Hitbox.Size() / 2f - Main.screenPosition, Color.White, new Vector2(256, 256) * NPC.ai[0], 0, NPC.oldPos[i] + NPC.Hitbox.Size() / 2f - Main.screenPosition);
            }
            ModdedShaderHandler shader2 = EffectsLoader.shaderHandlers["IcarusOrbStar"];
            shader2.setProperties([Color.GreenYellow.ToVector3()], texture.Value);
            shader2.apply();
            rect.Draw(NPC.Center - Main.screenPosition, Color.White, new Vector2(256, 256) * NPC.ai[0], MathHelper.PiOver2,  NPC.Center - Main.screenPosition);
            return false;
        }

    }

    public class CoreStateIdle : AIState
    {

        public override void OnStateUpdate(CommonNPCInfo info)
        {
            IcarusBoss icarus = Main.npc[NPC.realLife].ModNPC as IcarusBoss;
            if (icarus != null && icarus.NPC.active)
            {
                if (icarus.NPC.velocity == Vector2.Zero)
                    NPC.Center = Vector2.Lerp(NPC.Center, icarus.NPC.Center - new Vector2(0, 155), 0.5f);
                else
                    NPC.Center = Vector2.Lerp(NPC.Center, icarus.NPC.Center + icarus.NPC.velocity.SafeNormalize(Vector2.UnitY) * 350, 0.25f);

                Vector2 scale = new Vector2(1);
                icarus.ApplyZDepthScale(ref scale);
                NPC.ai[0] = scale.X;

                if(icarus.zDepth < 1)
                {
                    NPC.oldPos = new Vector2[NPC.oldPos.Length];
                    NPC.Center = icarus.NPC.Center - new Vector2(0, 155);
                }
            }
        }

    }

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
            NPC.defense = 200;
            NPC.noTileCollide = true;
            NPC.lifeMax = 250000;
            NPC.scale = 1f;
            AnimationType = NPCID.DukeFishron;

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
        private static VertexStrip strip = new();
        private static VertexRectangle rect = new();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D bodyTexture = TextureAssets.Npc[Type].Value;
            //drawColor = (CurrentPerspective == Perspectives.Side ? Color.Silver : Color.Firebrick);
            this.ApplyZDepthColor(ref drawColor);
            Vector2 scale = new Vector2(NPC.scale * 5);
            this.ApplyZDepthScale(ref scale);
            Rectangle rectangle = bodyTexture.Frame(1, 8, 0, 1);
            var drawData = new DrawData(bodyTexture, NPC.Center - (screenPos) - Vector2.UnitY * MathHelper.Lerp(1, 0, zDepth), null, drawColor, NPC.rotation, bodyTexture.Size() / 2f, scale, NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            states?.currentState.StatePreDraw(drawData, spriteBatch, screenPos, drawColor);
            DrawVFX(drawData, spriteBatch);
            DrawThrustersAndCore(drawData.position);
            drawData.Draw(spriteBatch);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            states?.currentState.StatePostDraw(spriteBatch, screenPos, drawColor);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

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

                        DrawOneThruster(screenPos + new Vector2(32, 32) * zDepth + dirAndLength, dirAndLength * zDepth, (!isVelocityBased ? dirAndLength.Length() / 2 : 0) + 128 * zDepth, Color.Turquoise, screenPos, false, Vector2.UnitY * MathHelper.Lerp(0, 1, zDepth) * 120f);
                        DrawOneThruster(screenPos - new Vector2(32, -32) * zDepth + dirAndLength, dirAndLength * zDepth, (!isVelocityBased ? dirAndLength.Length() / 2 : 0) + 128 * zDepth, Color.Turquoise, screenPos, true, Vector2.UnitY * MathHelper.Lerp(0, 1, zDepth) * 120f);
                        break;
                    }

                case Perspectives.Front:
                    {
                        bool isVelocityBased = NPC.velocity.Length() < 30;
                        Vector2 dirAndLength = new Vector2(75, 75);
                        dirAndLength = isVelocityBased ? dirAndLength : -NPC.velocity * 7;

                        DrawOneThruster(screenPos + new Vector2(32, 32) * zDepth + dirAndLength * zDepth, dirAndLength * zDepth, (!isVelocityBased ? dirAndLength.Length() / 2 : 0) + 128 * MathHelper.Clamp(MathHelper.Lerp(0.0f, 1f, zDepth), 0, 1f), Color.Turquoise, screenPos, false, -Vector2.UnitY * MathHelper.Lerp(1, 0, zDepth) * 120f);

                        dirAndLength = new Vector2(-75, 75);
                        dirAndLength = isVelocityBased ? dirAndLength : -NPC.velocity * 7;

                        DrawOneThruster(screenPos - new Vector2(32, -32) * zDepth + dirAndLength * zDepth, dirAndLength * zDepth, (!isVelocityBased ? dirAndLength.Length() / 2 : 0) + 128 * MathHelper.Clamp(MathHelper.Lerp(0.0f, 1f, zDepth), 0, 1f), Color.Turquoise, screenPos, true, -Vector2.UnitY * MathHelper.Lerp(1, 0, zDepth) * 120f);

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
            shader.setProperties([Color.White.ToVector3(), Color.White.ToVector3(), Color.White.ToVector3()], TextureAssets.Extra[193].Value);
            shader.apply();
            muzzleFlash.Draw(spawnPosition - Main.screenPosition, Color.White, new Vector2(128, 32), spawnRotation, spawnPosition - Main.screenPosition, 64);

        }
        public void DrawVFX(DrawData mainSprite, SpriteBatch spriteBatch)
        {
            if (NPC.velocity.Length() > 0)
            {
                var datas = DreamUtils.DrawData_AfterImage(mainSprite, 15, Color.AliceBlue, -NPC.velocity.SafeNormalize(Vector2.UnitY) * 128, true);
                foreach (DrawData data in datas)
                    data.Draw(spriteBatch);
            }
        }
    }

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

            //if (Counter % 60 > 10)
            //    NPC.velocity *= 0.7f;

            //if (Counter % 180 == 0)
            //{
            //    NPC.velocity = NPC.DirectionTo(NPC.targetRect.Center()) * 60;

            //} else 
            //{
            //     if (Counter % 60 == 0)
            //        NPC.velocity = NPC.DirectionTo(NPC.targetRect.Center()).RotateRandom(Main.rand.NextBool() == true ? MathHelper.ToRadians(135) : -MathHelper.ToRadians(135)) * 20;
            //}



            NPC.Center = Vector2.Lerp(NPC.Center, NPC.targetRect.Center() + new Vector2(550 * NPC.direction * -1, -300), 0.1f);

            if ((Main.rand.Next(30) == 0 && Counter > 120))
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
                    case 2:
                        {
                            ChangeState(StateType<IcarusSuper>());
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
            SoundEngine.PlaySound(SoundID.Item131 with { Pitch = 0 }, NPC.Center);
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {

            if (Counter % 2 == 0 && Counter < 45)
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(3 * NPC.direction, -15), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 3, -1).rotation = NPC.direction * MathHelper.PiOver2 + MathHelper.Pi;

            if (Counter >= 45)
            {
                NPC.velocity *= 0.8f;
            }

            if (Counter == 60)
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
            NPC.Center = Vector2.Lerp(NPC.Center, Target.Center + new Vector2(850 * NPC.direction * -1, MathF.Sin(Counter * .1f) * 50), 0.2f) + Target.velocity;

            if (Counter < 30)
                return;

            if (Counter % 15 == 0)
            {
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Target.DirectionFrom(NPC.Center) * 25, ModContent.ProjectileType<IcarusBullet>(), 50, 0, -1, Target.whoAmI, 17);
                SoundEngine.PlaySound(SoundID.Item12 with { Pitch = -1f }, NPC.Center);

            }


            for (int i = 0; i < 1; i++)
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 17).rotation = Main.rand.NextVector2Circular(0, 15).ToRotation();


            if (Counter >= 100)
                ChangeState(StateType<IcarusRepositionToPlayerY>());

        }
    }
    public class IcarusSlamState : IcarusState
    {
        public bool hasReachedPlayer = false;
        public bool isMovingBackToNormalZ = false;
        public bool isFinishedSlamming = false;
        public TweenHandler<float> dashFlashLengthTweens = new();
        public TweenHandler<float> dashFlashRotationTweens = new();
        public TweenHandler<float> dashFlashAlphaTweens = new();
        int flashDuration = 20;
        public override void SetDefaults()
        {
            base.SetDefaults();
            dashFlashAlphaTweens.Tweens =
            [
                new Tween<float>(MathHelper.Lerp).SetProperties(1,0,TweenEaseType.None,7),
                new Tween<float>(MathHelper.Lerp).SetProperties(0,1,TweenEaseType.None,7)
            ];

            dashFlashLengthTweens.Tweens =
            [
                new Tween<float>(MathHelper.Lerp).SetProperties(1,128,TweenEaseType.None,7),
                new Tween<float>(MathHelper.Lerp).SetProperties(128,0,TweenEaseType.None,7),
            ];
        }
        public override void OnEntered(int oldState)
        {
            base.OnEntered(oldState);
            NPC.damage = 0;
            hasReachedPlayer = false;
            isMovingBackToNormalZ = false;
            isFinishedSlamming = false;
            NPC.localAI[1] = (float)Perspectives.Front;


        }
        public override void StatePreDraw(DrawData mainSprite, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Counter >= 110)
                return;

            var datas = DreamUtils.DrawData_Spliting(mainSprite, 3, MathHelper.PiOver2 / 3, Color.Blue * dashFlashAlphaTweens.currentTween.currentProgress, dashFlashLengthTweens.currentTween.currentProgress);
            foreach (DrawData data in datas)
                data.Draw(spriteBatch);
        }
        public override void OnStateUpdate(CommonNPCInfo info)
        {
            DrawDingAtValue(60);
            if (isFinishedSlamming)
            {
                NPC.velocity *= 0.9f;
                if (Counter == 60)
                {
                    NPC.velocity = Vector2.Zero;
                    ChangeState(StateType<IcarusIdleState>());

                }
                NPC.localAI[1] = (float)Perspectives.Front;

                return;
            }

            if (Counter <= 60)
            {
                zDepth = MathHelper.Lerp(1, 0, DreamUtils.EaseOutBack(((Counter) / 30f)));

                return;
            }

            if (Counter <= 120f)
            {
                NPC.Center = Target.Center + new Vector2(-NPC.direction * 150, -NPC.height - 175);
                zDepth = MathHelper.Lerp(0, 1, DreamUtils.EaseOutBack(((Counter - 60f) / 60f)));



                return;
            }

            if (Counter < 110)
            {
                dashFlashAlphaTweens.PlayTweens();
                dashFlashLengthTweens.PlayTweens();
                //dashFlashRotationTweens.PlayTweens();
            }

            base.OnStateUpdate(info);
            if (Counter % 15 > 5 && Counter < 310) NPC.velocity *= 0.75f;
            if (Counter % 15 == 14 && Counter < 250)
            {
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Target.DirectionFrom(NPC.Center) * 25, ModContent.ProjectileType<IcarusBullet>(), 50, 0, -1, Target.whoAmI, 17);
                SoundEngine.PlaySound(SoundID.Item12 with { Pitch = -1f }, NPC.Center);

                if (Counter != 14)
                    for (int i = 0; i < 2; i++)
                        Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 15, -1).rotation = new Vector2(-NPC.direction, 0).ToRotation();
                NPC.localAI[1] = (float)Perspectives.Side;
                SoundEngine.PlaySound(SoundID.Item131 with { Pitch = 1 }, NPC.Center);

                NPC.velocity = new Vector2(NPC.direction * (Target.velocity.Length() + 65), -5);
            }

            if (Counter == 265) NPC.velocity *= 0;
            if (Counter == 310) NPC.velocity = new Vector2(0, 60);
            if (Counter >= 310 && Counter % 2 == 0)
            {
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 12, -1).rotation = 0;
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0), ModContent.ProjectileType<IcarusSpamRocket>(), 50, 0, -1, Target.whoAmI, 12, -1).rotation = MathHelper.Pi;
                NPC.localAI[1] = (float)Perspectives.Front;
                NPC.rotation = Vector2.UnitY.ToRotation();
                SoundEngine.PlaySound(SoundID.Item131 with { Pitch = 1 }, NPC.Center);

            }
            if (NPC.Center.Y > Target.Center.Y + 500)
            {

                isFinishedSlamming = true;
                stateCounter.currentFramesPassedOrRemained = 0;
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

            if (Counter < 30)
                NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(NPC.Center.X, repositionToY), 0.1f);

            if (Counter == 60)
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

    public class IcarusSuper : AIState
    {

    }
    public abstract class IcarusState : AIState
    {
        private static Asset<Texture2D> texture;
        public TweenHandler<float> dingTweenRotation = new();
        public TweenHandler<Vector2> dingTweenScale = new();
        public override void SetDefaults()
        {
            dingTweenRotation.Tweens =
            [
                new Tween<float>(MathHelper.Lerp).SetProperties(0,1,TweenEaseType.None,55),
                new Tween<float>(MathHelper.Lerp).SetProperties(3.34f,0,TweenEaseType.None,10)
            ];

            dingTweenScale.Tweens =
            [
                new Tween<Vector2>(Vector2.Lerp).SetProperties(Vector2.Zero,new Vector2(256,512),TweenEaseType.None,55),
                new Tween<Vector2>(Vector2.Lerp).SetProperties(new Vector2(128 * 15f,512 * 3f),Vector2.Zero,TweenEaseType.None,10),
            ];

        }
        public override void Load()
        {
            texture = ModContent.Request<Texture2D>("DreamMod/Assets/Textures/VFX/Iridescent");

        }
        bool isDashStarted = false;

        private static VertexRectangle rect = new();
        public override void StatePostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (dingTweenScale.currentTween?.currentDuration.state == FrameCounterState.Running)
            {
                ModdedShaderHandler shader = EffectsLoader.shaderHandlers["IcarusDing"];
                shader.setProperties([Color.White.ToVector3()], texture.Value);
                shader.apply();

                rect.Draw(NPC.Center - Main.screenPosition, Color.White, dingTweenScale.currentTween.currentProgress, MathHelper.PiOver4 * DreamUtils.EaseOutBack(dingTweenRotation.currentTween.currentProgress), NPC.Center - Main.screenPosition);
                rect.Draw(NPC.Center - Main.screenPosition, Color.White, dingTweenScale.currentTween.currentProgress * .25f, MathHelper.PiOver4 + MathHelper.PiOver2, NPC.Center - Main.screenPosition);

            }
        }
        public void DrawDingAtValue(int startingDuration)
        {
            if (Counter == startingDuration)
            {
                dingTweenRotation.PlayTweens();
                dingTweenScale.PlayTweens();
            }

        }
    }
}
