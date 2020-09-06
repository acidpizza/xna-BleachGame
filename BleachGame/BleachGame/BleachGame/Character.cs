using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BleachGame
{
    abstract class Character
    {
        #region Fixed Variables / Properties

        // Parent Level
        public Level level { get; private set; }

        // ==== Animations ====
        protected Animation idleAnimation;
        protected Animation runAnimation;
        protected Animation jumpAnimation;
        protected Animation dashVerticalAnimation;
        protected Animation dashHorizontalAnimation;
        protected Animation fallingAnimation;
        protected Animation duckAnimation;
        protected AnimationPlayer sprite;

        // ==== Character States ====
        protected enum CharacterState { indeterminate, idle, running, dashing, jumping, falling, groundAttack, ducking, airAttackRising, airAttackFalling, shikai };
        protected CharacterState characterState = CharacterState.idle;
        protected CharacterState prevValidCharacterState = CharacterState.idle;
        protected CharacterState prevCharacterState = CharacterState.idle;

        protected SpriteEffects flip = SpriteEffects.None;
        public bool isAlive { get; private set; }
        protected bool idle = true;

        public Vector2 position;
        public Vector2 velocity;

        // ==== Attempted Actions ====
        protected bool attemptDash, attemptDashLeft, attemptDashRight, attemptDashUp, attemptDashDown;
        protected bool attemptJump;
        protected bool attemptRun, attemptRunLeft, attemptRunRight;
        protected bool attemptDuck;

        // ==== Collision states ====
        protected float previousBottom;

        // ==== Jump states ====
        protected bool isOnGround { get; set; }
        protected int jumpTime = 0; // in milliseconds because float cannot be accurately 0.
        
        // ==== Dash state ====
        protected float dashTime = 0;
        protected enum DashState { notDashing, dashLeft, dashRight, dashUp, dashDown };
        protected DashState dashState = DashState.notDashing;

        /// <summary>
        /// Gets Bounding Rectangle in world coords.
        /// </summary>
        public Rectangle BoundingRectangle
        {            
            get
            {
                int left = (int)Math.Round(position.X - sprite.Origin.X) + sprite.Animation.BoundingRectangle.X;
                int top = (int)Math.Round(position.Y - sprite.Origin.Y) + sprite.Animation.BoundingRectangle.Y;

                return new Rectangle(left, top, sprite.Animation.BoundingRectangle.Width, sprite.Animation.BoundingRectangle.Height);
            }
        }

        #endregion

        #region Configurable Variables

        // ==== Running variables ====
        protected float HorizontalSpeed = 400;

        // ==== Jumping variables ====
        protected int MaxJumpTime = 300; // in milliseconds because jumpTime is in milliseconds
        protected float JumpLaunchVelocity = 500f;
        protected float jumpPower = 4;
        protected float Gravity = 3000;
        protected float MaxFallSpeed = 700;

        // ==== Dashing variables ====
        protected float maxDashTime = 0.5f;
        protected float dashSpeed = 80;
        protected float dashUpSpeed = 80;

        #endregion

        #region Public Functions

        public Character(Level level, Vector2 position)
        {
            this.level = level;
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        public void Reset(Vector2 position)
        {
            this.position = position;
            this.velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        public abstract void Update(GameTime gameTime, KeyboardState keyboardState);

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, position, flip);
        }

        #endregion

        #region Initialisation
        
        /// <summary>
        /// Initialise all the configurable variables here
        /// </summary>
        protected abstract void InitialiseVariables();

        /// <summary>
        /// Load all the textures for animation
        /// </summary>
        protected abstract void LoadContent();
        
        #endregion

        #region Character Start State Functions

        protected virtual void startIdleState()
        {
            characterState = CharacterState.idle;
            sprite.PlayAnimation(idleAnimation);
        }

        protected virtual void startRunningState()
        {
            characterState = CharacterState.running;
            sprite.PlayAnimation(runAnimation);
        }

        protected virtual void startDuckingState()
        {
            characterState = CharacterState.ducking;
            sprite.PlayAnimation(duckAnimation);
        }

        protected virtual void startDashState()
        {
            characterState = CharacterState.dashing;
            dashTime = 0;

            if (attemptDashLeft)
            {
                dashState = DashState.dashLeft;
                flip = SpriteEffects.FlipHorizontally;
                sprite.PlayAnimation(dashHorizontalAnimation);
            }
            else if (attemptDashRight)
            {
                dashState = DashState.dashRight;
                flip = SpriteEffects.None;
                sprite.PlayAnimation(dashHorizontalAnimation);
            }
            else if (attemptDashUp)
            {
                dashState = DashState.dashUp;
                sprite.PlayAnimation(dashVerticalAnimation);
            }
            else if (attemptDashDown)
            {
                dashState = DashState.dashDown;
                sprite.PlayAnimation(dashVerticalAnimation);
            }

        }

        protected virtual void startJumpingState()
        {
            characterState = CharacterState.jumping;
            sprite.PlayAnimation(jumpAnimation);
            jumpTime = 0;
        }

        protected virtual void startFallingState()
        {
            characterState = CharacterState.falling;
            sprite.PlayAnimation(fallingAnimation);
        }

        protected void indeterminateState()
        {
            characterState = CharacterState.indeterminate;
        }

        #endregion
        
        #region Character Move List - Attempt Actions

        protected void DashHorizontal(bool flip)
        {
            attemptDash = true;
            if (flip)
            {
                attemptDashLeft = true;
                attemptDashRight = false;
            }
            else
            {
                attemptDashRight = true;
                attemptDashLeft = false;
            }
        }

        protected void DashVertical(bool up)
        {
            attemptDash = true;
            if (up)
            {
                attemptDashUp = true;
                attemptDashDown = false;
            }
            else
            {
                attemptDashDown = true;
                attemptDashUp = false;
            }
        }

        protected void Run(bool flip)
        {
            attemptRun = true;
            if (flip)
            {
                attemptRunLeft = true;
                attemptRunRight = false;
            }
            else
            {
                attemptRunRight = true;
                attemptRunLeft = false;
            }
        }

        protected void Duck()
        {
            attemptDuck = true;
        }

        protected void Jump()
        {
            attemptJump = true;
        }

        #endregion      
        
        #region Character Actions - Implement Actions

        /// <summary>
        /// Adjust Position based on velocity
        /// </summary>
        protected void ApplyVelocity(float timeElapsed)
        {
            // Apply velocity.
            position += velocity * timeElapsed;

            // This is to stabilise the sprite on collision
            position = new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y));
        }

        /// <summary>
        /// Adjust velocity for right and left movement
        /// </summary>
        protected void DoBasicMovement(float timeElapsed)
        {
            /*
            if (attemptRunRight)
                velocity.X = HorizontalSpeed;
            else if (attemptRunLeft)
                velocity.X = -HorizontalSpeed;
            */

            if (attemptRunRight)
                position.X += HorizontalSpeed * timeElapsed;
            else if (attemptRunLeft)
                position.X -= HorizontalSpeed * timeElapsed;            

        }

        /// <summary>
        /// Adjust Y-velocity for gravity.
        /// </summary>
        protected void DoGravity(float timeElapsed)
        {
            velocity.Y = MathHelper.Clamp(velocity.Y + Gravity * timeElapsed, -MaxFallSpeed, MaxFallSpeed);
        }

        /// <summary>
        /// Positions are automatically updated. Handles collision also.
        /// </summary>
        protected void DoDash(float timeElapsed)
        {
            dashTime += timeElapsed; // dashTime is x. Equation is (-2x)(e^(-(x^2))). Consider 8 frames (x values) of sprites first. 

            // End the dash if time is up. 
            if (dashTime > maxDashTime)
            {
                CancelDashing();

                // Air dash should end with falling.
                if (!isOnGround)
                    startFallingState();

                return;
            }

            int squeeze_1 = 3; // squeeze graph to only move quickly in centre 2 frames.
            int offset_2 = 4; // bring peak to 4th frame.
            float squeeze_3 = 16; // squeeze 8 frames to fit 0.5s.
            // Edit dashSpeed to increase the distance moved during the peak.

            if (dashState == DashState.dashUp)
                position.Y -= (float)(dashUpSpeed * Math.Exp(-(Math.Pow(squeeze_1 * (squeeze_3 * dashTime - offset_2), 2))));
            else if (dashState == DashState.dashDown)
                position.Y += (float)(dashUpSpeed * Math.Exp(-(Math.Pow(squeeze_1 * (squeeze_3 * dashTime - offset_2), 2))));
            else if (dashState == DashState.dashLeft)
                position.X -= (float)(dashSpeed * Math.Exp(-(Math.Pow(squeeze_1 * (squeeze_3 * dashTime - offset_2), 2))));
            else if (dashState == DashState.dashRight)
                position.X += (float)(dashSpeed * Math.Exp(-(Math.Pow(squeeze_1 * (squeeze_3 * dashTime - offset_2), 2))));


            if (dashTime / maxDashTime > 0.3 && dashTime / maxDashTime < 0.7) // Do special dash collision during the period with large movement
                HandleDashCollisions();
            else
                HandleCollisions();

        }

        protected void CancelDashing()
        {
            indeterminateState();
            dashTime = 0; // Reset the dash time.
            dashState = DashState.notDashing;
        }

        /// <summary>
        /// Adjust velocity for jumping
        /// </summary>
        protected void DoJump(GameTime gameTime)
        {
            jumpTime += gameTime.ElapsedGameTime.Milliseconds;

            // If we are in the ascent of the jump
            if (0 < jumpTime && jumpTime <= MaxJumpTime)
            {
                // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                velocity.Y = -JumpLaunchVelocity * (1.0f - (float)Math.Pow((float)jumpTime / (float)MaxJumpTime, jumpPower));
            }
            else if (jumpTime > MaxJumpTime)
            {
                // Reached the apex of the jump
                CancelJumping();
                startFallingState();
            }
            else
            {
                // Other conditions
                CancelJumping();
                startFallingState();
            }
        }

        protected void CancelJumping()
        {
            jumpTime = 0;
            indeterminateState();
        }

        #endregion

        #region Collision

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        protected void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = level.GetTileBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.platform)
                            {

                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || isOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    position = new Vector2(position.X, position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                position = new Vector2(position.X + depth.X, position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }
            
            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// This is to handle tunneling from dashes. Considers the dash direction.
        /// </summary>
        protected void HandleDashCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int x = leftTile; x <= rightTile; ++x)
            {
                for (int y = topTile; y <= bottomTile; ++y)
                {
                    // If this tile is collidable,
                    TileCollision collision = level.GetCollision(x, y);
                    if (collision == TileCollision.Impassable) // only cannot dash through impassable blocks
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = level.GetTileBounds(x, y);

                        int dashDirection = 0;
                        if (dashState == DashState.dashLeft)
                            dashDirection = 1;
                        else if (dashState == DashState.dashRight)
                            dashDirection = 2;
                        else if (dashState == DashState.dashUp)
                            dashDirection = 3;
                        else if (dashState == DashState.dashDown)
                            dashDirection = 4;

                        Vector2 depth = RectangleExtensions.GetDashIntersectionDepth(bounds, tileBounds, dashDirection);
                        if (depth != Vector2.Zero)
                        {

                            float absDepthY = depth.Y; // GetDashIntersectionDepth should always return positive values
                            float absDepthX = depth.X; // GetDashIntersectionDepth should always return positive values

                            // Dash downwards
                            if (dashState == DashState.dashDown)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Offset upwards 
                                position = new Vector2(position.X, position.Y - absDepthY);
                            }
                            // Dash upwards
                            else if (dashState == DashState.dashUp)
                            {
                                // Offset downwards
                                position = new Vector2(position.X, position.Y + absDepthY);
                            }
                            // Dash right
                            else if (dashState == DashState.dashRight)
                            {
                                // Offset left
                                position = new Vector2(position.X - absDepthX, position.Y);

                            }
                            // Dash left
                            else if (dashState == DashState.dashLeft)
                            {
                                // Offset right.
                                position = new Vector2(position.X + absDepthX, position.Y);
                            }

                            // Perform further collisions with the new bounds.
                            bounds = BoundingRectangle;
                        }
                    }
                }
            }


            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }
                
        #endregion
    }
}
