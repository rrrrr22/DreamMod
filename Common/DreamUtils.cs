using DreamMod.Common.Graphics;
using DreamMod.Common.Systems;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace DreamMod.Common
{
    public static class DreamUtils
    {
        public static FrameCounter NewFrameCounter(this object holder, int value, bool startAutomatically = true)
        {
            return new FrameCounter(holder, value, startAutomatically);
        }
        public static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);

        }

        public static float EaseInBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return c3 * x * x * x - c1 * x * x;
        }

        public static float EaseOutExpo(float x)
        {

            return x == 1 ? 1 : 1 - MathF.Pow(2, -10 * x);
        }
        public static DrawData[] DrawData_Spliting(DrawData data, int numberOfImages, float rotation, Color color, float offset)
        {

            var datas = new DrawData[numberOfImages];
            for (int i = 0; i < numberOfImages; i++)
            {
                var newPosition = data.position + new Vector2(1, 0).RotatedBy((float)i / numberOfImages * MathHelper.TwoPi + rotation);
                datas[i] = data.CopyAndChangeDrawDataValues(positionData: newPosition + data.position.DirectionTo(newPosition) * offset, colorData: color);

            }

            return datas;
        }

        public static DrawData CopyAndChangeDrawDataValues(this DrawData data, Texture2D textureData = null, Vector2? positionData = null, Color? colorData = null, Rectangle? destinationRectData = null, Rectangle? sourceRectData = null, float? rotationData = null, Vector2? originData = null, Vector2? scaleData = null, SpriteEffects? spriteEffectsData = null)
        {
            return new(textureData ?? data.texture, positionData ?? data.position, sourceRectData ?? data.sourceRect, colorData ?? data.color, rotationData ?? data.rotation, originData ?? data.origin, scaleData ?? data.scale, spriteEffectsData ?? data.effect);
        }

        public static DrawData[] DrawData_AfterImage(DrawData data, int maxNumberOfImagesAtOnce, Color color, Vector2 directionAndLength, bool animate)
        {

            var datas = new DrawData[maxNumberOfImagesAtOnce];
            for (int i = 0; i < maxNumberOfImagesAtOnce; i++)
            {
                float progress = (float)i / maxNumberOfImagesAtOnce;
                var newPosition = Vector2.Zero;
                if (animate)
                    newPosition = Vector2.Lerp(data.position + directionAndLength, data.position, progress);
                else
                    newPosition = Vector2.Lerp(data.position + Vector2.Lerp(Vector2.Zero, directionAndLength, progress * (float)Main.timeForVisualEffects % 1), data.position, progress);
                datas[i] = data.CopyAndChangeDrawDataValues(positionData: newPosition, colorData: color * progress);

            }
            return datas;
        }
        public static void Push<T>(this T[] array, T value)
        {
            Array.Copy(array, 0, array, 1, array.Length - 1);
            array[0] = value;
        }
        // if max = 60, starting = 30,current = 45, then finalValue = (45-15) / (60 - 30) = 0.5 
        public static float PortionOfTimerAsProgress(float startingDurationValue, float maxDurationValue)
        {
            return MathHelper.Clamp(MathHelper.Lerp(0, 1, (startingDurationValue) / (maxDurationValue)), 0, 1);
        }
        public static void ApplyZDepthColor(this IZDepth zDepthHolder, ref Color drawColor)
        {
            drawColor = new Color((zDepthHolder.zDepth * 2f) * (drawColor.R / 255f), (zDepthHolder.zDepth * 2f) * (drawColor.G / 255f), (zDepthHolder.zDepth * 2f) * (drawColor.B / 255f), 1);
        }
        public static void ApplyZDepthScale(this IZDepth zDepthHolder, ref Vector2 scale) => scale *= new Vector2(MathHelper.Clamp(MathHelper.Lerp(0.0f, 1f, (zDepthHolder.zDepth)), 0, 1));
    }
}
