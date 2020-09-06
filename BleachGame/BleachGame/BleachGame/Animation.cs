using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BleachGame
{
    class Animation
    {
        Texture2D texture;
        public Texture2D Texture { get { return texture; } }

        float frameTime;
        /// <summary>
        /// FrameTime is in seconds
        /// </summary>
        public float FrameTime { get { return frameTime; } }

        bool isLooping;
        public bool IsLooping { get { return isLooping; } }

        int frameCount;
        public int FrameCount { get { return frameCount; } }

        int frameWidth;
        public int FrameWidth { get { return frameWidth; } }

        int frameHeight;
        public int FrameHeight { get { return frameHeight; } }

        // Rectangle for collision handling with environment
        Rectangle boundingRectangle;
        public Rectangle BoundingRectangle { get { return boundingRectangle; } }
        
        public Animation(Texture2D texture, float frameTime, bool isLooping, int frameCount, int frameWidth, int frameHeight, int boundingWidth, int boundingHeight)
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
            this.frameCount = frameCount;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;

            int width = boundingWidth;
            int left = (FrameWidth - width) / 2;
            int height = boundingHeight;
            int top = FrameHeight - height;
            boundingRectangle = new Rectangle(left, top, width, height);
        }

    }
}
