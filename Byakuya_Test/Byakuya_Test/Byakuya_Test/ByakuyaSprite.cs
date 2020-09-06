using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Byakuya_Test
{
    public enum ByakuyaState {standing, running, jumping, ducking, dashUp, falling, dash};


    class ByakuyaSprite
    {
        // ===== Sprites ====
        public Texture2D TexStanding { get; set; }
        public Texture2D TexRunning { get; set; }
        public Texture2D TexJumping { get; set; }
        public Texture2D TexDashUp { get; set; }
        public Texture2D TexDashRight { get; set; }

        // ==== State variables ====
        ByakuyaState state = ByakuyaState.standing;
        ByakuyaState oldState = ByakuyaState.standing;
        int currentFrame = 0;
        float FrameCounter = 0.0f;
        bool flip = false;
        
        

        // ==== Movement ====
        Vector2 location = new Vector2(0, 0);

        // Running
        float speed = 0.3f;
        
        // Jumping
        bool airborne = false;
        float jumpSpeed = 0.0f;
        float initialGroundLevel = 0;
        
        // Falling
        float dropSpeed = 0.0f;
        float gravity = 0.005f;

        // Dashing
        float dashSpeed = 0; 

        // ==== Functions ====

        public ByakuyaSprite(int X, int Y)
        {
            location.X = X;
            location.Y = Y;
        }

        public void LoadSprites(Texture2D standing, Texture2D running, Texture2D jumping, Texture2D dashUp, Texture2D dashRight)
        {
            TexStanding = standing;
            TexRunning = running;
            TexJumping = jumping;
            TexDashUp = dashUp;
            TexDashRight = dashRight;
        }

        public void Update(GameTime gameTime)
        {
            if (state != oldState)
            {
                // New state activated
                oldState = state;
                currentFrame = 0;
                FrameCounter = 0;

                // Only do this for start of jumping
                if (state == ByakuyaState.jumping)
                {
                    jumpSpeed = 1.0f;
                    initialGroundLevel = location.Y;
                }
                // only do this for start of dashUp
                else if (state == ByakuyaState.dashUp)
                {
                    initialGroundLevel = location.Y;
                    dashSpeed = 60.0f;
                }
                else if (state == ByakuyaState.falling)
                {
                    dropSpeed = 0.0f;
                }
            }
            else
            {
                // Determine the frame rate of the animation for each state by adjusting the float value 

                if (state == ByakuyaState.standing)
                {
                    FrameCounter += 0.004f * gameTime.ElapsedGameTime.Milliseconds; // Finer calculation
                    currentFrame = (int)FrameCounter % 7;                           // Determine sprite image to use
                    
                    // Reset counters to loop sprite animation
                    if(FrameCounter >= 7)                   
                    {
                        currentFrame = 0;
                        FrameCounter = 0;
                    }
                }
                else if (state == ByakuyaState.running)
                {
                    FrameCounter += 0.01f * gameTime.ElapsedGameTime.Milliseconds;  // Finer calculation
                    currentFrame = (int)FrameCounter % 6;                           // Determine sprite image to use
                    
                    // Reset counters to loop sprite animation
                    if (FrameCounter >= 6)
                    {
                        currentFrame = 0;
                        FrameCounter = 0;
                    }
                }
                else if (state == ByakuyaState.jumping)
                {
                    FrameCounter += 0.015f * gameTime.ElapsedGameTime.Milliseconds; // Finer calculation
                    
                    // Use the last frame of jumping while still airborne and jumping time has run out
                    if (FrameCounter >= 5)
                        currentFrame = 4;
                    else
                        currentFrame = (int)FrameCounter % 5;                       // Determine sprite image to use

                    // Determine if original pre-jump height has been reached
                    if (location.Y - jumpSpeed * gameTime.ElapsedGameTime.Milliseconds >= initialGroundLevel)
                    {
                        // Landed
                        FrameCounter = 5;
                        currentFrame = 5;
                        location.Y = initialGroundLevel;
                        airborne = false;
                        oldState = ByakuyaState.ducking;
                    }
                    else
                    {
                        // still airborne
                        location.Y -= jumpSpeed * gameTime.ElapsedGameTime.Milliseconds;    // Change in position
                        jumpSpeed -= gravity * gameTime.ElapsedGameTime.Milliseconds;        // decceleration due to gravity
                    }
                    
                }
                else if (state == ByakuyaState.ducking)
                {
                    currentFrame = 5;
                }
                else if (state == ByakuyaState.dashUp)
                {
                    FrameCounter += gameTime.ElapsedGameTime.Milliseconds; // FrameCounter is x. Equation is e^(-(x^2)). Consider 8 frames (x values) of sprites first. 
                    int squeeze_1 = 3; // squeeze graph to only move quickly in centre 2 frames.
                    int offset_2 = 4; // bring peak to 4th frame.
                    float stretch_3 = 62.5f; // stretch graph to fit 500ms.
                    // Edit dashSpeed to increase the distance moved during the peak.

                    location.Y -= (float)(dashSpeed * Math.Exp(-(Math.Pow(squeeze_1 * (FrameCounter / stretch_3 - offset_2), 2))));  

                    // Use FrameCounter as a timer: stretch_3 * 8 = total time in miliseconds that animation will last. 

                    // Use the last frame of dashUp while still dashing and time has run out
                    if (FrameCounter / stretch_3 >= 8)
                    {
                        // Finished dashUp. Proceed to drop to ground. 
                        state = ByakuyaState.falling;
                        currentFrame = 7;
                    }
                    else
                    {
                        // Still dashing
                        currentFrame = (int)(FrameCounter / stretch_3);                       // Determine sprite image to use
                    }
                }
                else if (state == ByakuyaState.falling)
                {
                    FrameCounter += 0.015f * gameTime.ElapsedGameTime.Milliseconds; // Finer calculation

                    currentFrame = (int)FrameCounter % 2 + 3;                       // Determine sprite image to use


                    // Determine if original pre-jump height has been reached
                    if (location.Y + dropSpeed * gameTime.ElapsedGameTime.Milliseconds >= initialGroundLevel)
                    {
                        // Landed
                        FrameCounter = 5;
                        currentFrame = 5;
                        location.Y = initialGroundLevel;
                        airborne = false;
                        oldState = ByakuyaState.ducking;
                    }
                    else
                    {
                        // still airborne
                        location.Y += dropSpeed * gameTime.ElapsedGameTime.Milliseconds;    // Change in position
                        dropSpeed += gravity * gameTime.ElapsedGameTime.Milliseconds;        // decceleration due to gravity
                    }
                }
                else if (state == ByakuyaState.dash)
                {
                    FrameCounter += gameTime.ElapsedGameTime.Milliseconds; // FrameCounter is x. Equation is e^(-(x^2)). Consider 8 frames (x values) of sprites first. 
                    int squeeze_1 = 3; // squeeze graph to only move quickly in centre 2 frames.
                    int offset_2 = 4; // bring peak to 4th frame.
                    float stretch_3 = 62.5f; // stretch graph to fit 500ms.
                    // Edit dashSpeed to increase the distance moved during the peak.

                    location.X += (float)(dashSpeed * Math.Exp(-(Math.Pow(squeeze_1 * (FrameCounter / stretch_3 - offset_2), 2))));

                    // Use FrameCounter as a timer: stretch_3 * 8 = total time in miliseconds that animation will last. 

                    // Use the last frame of dash while still dashing and time has run out
                    if (FrameCounter / stretch_3 >= 8)
                    {
                        // Finished dash. Break out of dash. 
                        airborne = false;
                        oldState = ByakuyaState.standing;
                        currentFrame = 7;
                    }
                    else
                    {
                        // Still dashing
                        currentFrame = (int)(FrameCounter / stretch_3);                       // Determine sprite image to use
                    }
                }

            }
        }

        
        public void Draw(SpriteBatch spriteBatch)
        {
            int width = 0;
            int height = 0;
            Texture2D texture = TexStanding;

            // Determine the number of frames per state and other drawing parameters
            if (state == ByakuyaState.standing)
            {
                width = 76;
                height = 105;
                texture = TexStanding;
            }
            else if (state == ByakuyaState.running)
            {
                width = 76;
                height = 105;
                texture = TexRunning;                
            }
            else if (state == ByakuyaState.jumping || state == ByakuyaState.ducking || state == ByakuyaState.falling)
            {
                width = 88;
                height = 105;
                texture = TexJumping;
            }
            else if (state == ByakuyaState.dashUp)
            {
                width = 76;
                height = 128;
                texture = TexDashUp;                
            }
            else if (state == ByakuyaState.dash)
            {
                width = 124;
                height = 105;
                texture = TexDashRight;
            }

            Rectangle sourceRect = new Rectangle(width * currentFrame, 0, width, height);
            Rectangle destRect = new Rectangle((int)location.X, (int)location.Y, width, height);
            if(!flip)
                spriteBatch.Draw(texture, destRect, sourceRect, Color.White, 0, new Vector2(width/2, height - 2), SpriteEffects.None, 0);
            else
                spriteBatch.Draw(texture, destRect, sourceRect, Color.White, 0, new Vector2(width/2, height - 2), SpriteEffects.FlipHorizontally, 0);
        }

        public void RunUp(GameTime gameTime)
        {
            if (!airborne)
            {
                state = ByakuyaState.running;
                location.Y -= speed * (float)gameTime.ElapsedGameTime.Milliseconds;
            }
        }

        public void RunDown(GameTime gameTime)
        {
            if (!airborne)
            {
                state = ByakuyaState.running;
                location.Y += speed * (float)gameTime.ElapsedGameTime.Milliseconds;
            }
        }

        public void RunLeft(GameTime gameTime)
        {
            // Do not change state when jumping
            if (!airborne)
                state = ByakuyaState.running;
            
            location.X -= speed * (float)gameTime.ElapsedGameTime.Milliseconds;
            flip = true;
        }

        public void RunRight(GameTime gameTime)
        {
            // Do not change state when jumping
            if (!airborne)
                state = ByakuyaState.running;
            
            location.X += speed * (float)gameTime.ElapsedGameTime.Milliseconds;
            flip = false;
        }

        public void Jump()
        {
            if (!airborne)
            {
                state = ByakuyaState.jumping;
                airborne = true;
            }
        }

        public void Duck()
        {
            if (!airborne)
            {
                state = ByakuyaState.ducking;
            }
        }

        public void DashUp()
        {
            if (!airborne)
            {
                state = ByakuyaState.dashUp;
                airborne = true;
            }
        }
        public void DashRight()
        {
            if (!airborne)
            {
                state = ByakuyaState.dash;
                dashSpeed = 80.0f;
                airborne = true;
            }
        }
        public void DashLeft()
        {
            if (!airborne)
            {
                state = ByakuyaState.dash;
                dashSpeed = -80.0f;
                flip = true;
                airborne = true;
            }
        }

        public void Stand()
        {
            if (!airborne)
            {
                state = ByakuyaState.standing;
            }
        }
        

    }
}
