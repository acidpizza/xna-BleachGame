using System;
using System.Collections.Generic;
using System.Timers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace BleachGame
{
    class Byakuya : Character
    {
        #region Byakuya Variables
        
        // ==== Animations ====
        protected Animation slashAnimation1;
        protected Animation slashAnimation2;
        protected Animation airSlashAnimation;
        protected Animation shikaiSetupAnimation;
        protected Animation shikaiReleaseAnimation;
        protected Animation shikaiEndAnimation;

        protected Animation shikaiIdleAnimation;
        protected Animation shikaiRunAnimation;
        protected Animation shikaiJumpAnimation;
        protected Animation shikaiDashVerticalAnimation;
        protected Animation shikaiDashHorizontalAnimation;
        protected Animation shikaiFallingAnimation;
        protected Animation shikaiDuckAnimation;
        protected Animation shikaiGroundAttackAnimation;
        protected Animation shikaiAirAttackAnimation;

        // ==== Reiatsu ====
        Texture2D shikaiSetupReiatsu;

        // ==== Attempted Actions ====
        bool attemptSlash;
        bool attemptShikai;
        bool attemptEndShikai;

        // ==== Slash States ====
        enum GroundSlashState { slashState1, slashState2 };
        GroundSlashState groundSlashState = GroundSlashState.slashState1;
        float maxGroundSlashTime1 = 0.3f;
        float maxGroundSlashTime2 = 0.4f;
        float groundSlashTime = 0f;
        float airSlashTime = 0f;
        float maxAirSlashTime = 0.3f;

        // ==== Shikai Ground Attack ====
        float maxShikaiGroundAttackTime = 0.7f;
        // ==== Shikai Air Attack ====
        float maxShikaiAirAttackTime = 0.6f;

        // ==== Senbonzakura ====
        GraphicsDevice graphicsDevice;
        ParticleEngine senbonzakuraParticleEngine;
        ParticleEngine senbonzakuraTextureParticleEngine;

        enum ShikaiState { shikaiSetup, shikaiCharge, shikaiRelease, shikaiDelay, endShikai, endShikaiDelay} // only for shikai transitions
        ShikaiState shikaiState = ShikaiState.shikaiSetup;
        Texture2D shikaiBlade;
        float shikaiBladeAlpha = 0f;
        Vector2 shikaiBladeScale = new Vector2(1.0f, 1.0f);
        bool shikaiMode = false; 

        float maxShikaiSetupTime = 0.3f;
        float maxShikaiChargeTime = 0.5f;
        float maxShikaiReleaseTime = 0.8f;
        float maxShikaiDelayTime = 0.8f;
        float shikaiTime = 0f;

        Timer shikaiTimer = new Timer(15000);
         
        #endregion

        public Byakuya(Level level, Vector2 position, GraphicsDevice graphicsDevice) 
            : base(level,position)
        {
            this.graphicsDevice = graphicsDevice;
            LoadContent();
            InitialiseVariables();
            Reset(position);
        }

        #region Initialisation

        protected override void InitialiseVariables()
        {
            // ==== Running variables ====
            HorizontalSpeed = 400f;

            // ==== Jumping variables ====
            MaxJumpTime = 300; // in milliseconds because jumpTime is in milliseconds
            JumpLaunchVelocity = 500f;
            jumpPower = 4f;
            Gravity = 3000f;
            MaxFallSpeed = 700f;

            // ==== Dashing variables ====
            maxDashTime = 0.5f;
            dashSpeed = 80f;
            dashUpSpeed = 80f;

            // ==== Shikai Timer ====
            shikaiTimer.Elapsed += new ElapsedEventHandler(OnEndShikai);
            shikaiTimer.Enabled = false;
        }

        protected override void LoadContent()
        {
            // ==== Byakuya ====
            idleAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_standing"), 0.2f, true, 7, 76, 105, 40, 105);
            runAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_running"), 0.1f, true, 6, 76, 105, 40, 105);
            jumpAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_jumping"), 0.1f, false, 5, 88, 105, 40, 105);
            dashVerticalAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_dash_up"), maxDashTime / 8.0f, false, 8, 76, 128, 40, 105);
            dashHorizontalAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_dash_right"), maxDashTime / 8.0f, false, 8, 124, 105, 80, 105);
            fallingAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_falling"), 0.1f, true, 2, 88, 105, 40, 105);
            duckAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_duck"), 0.1f, true, 1, 88, 105, 40, 105);
            slashAnimation1 = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_groundslash_1"), 0.05f, false, 6, 168, 105, 40, 105);
            slashAnimation2 = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_groundslash_2"), 0.05f, false, 8, 176, 120, 40, 105);
            airSlashAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_airSlash"), 0.05f, false, 6, 118, 118, 40, 105);
            shikaiSetupAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_shikaiSetup"), maxShikaiSetupTime / 4f, false, 4, 108, 140, 40, 105);
            shikaiReleaseAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_shikaiRelease"), 0.3f, true, 3, 108, 140, 40, 105);
            shikaiEndAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_shikaiEnd"), 0.3f, false, 1, 108, 140, 40, 105);
            

            // ==== Shikai Byakuya ====
            shikaiIdleAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/shikai_byakuya_standing"), 0.2f, true, 7, 76, 105, 40, 105);
            shikaiRunAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/shikai_byakuya_running"), 0.1f, true, 6, 76, 105, 40, 105);
            shikaiJumpAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/shikai_byakuya_jumping"), 0.1f, false, 5, 88, 105, 40, 105);
            shikaiDashVerticalAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/shikai_byakuya_dash_up"), maxDashTime / 8.0f, false, 8, 76, 128, 40, 105);
            shikaiDashHorizontalAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/shikai_byakuya_dash_right"), maxDashTime / 8.0f, false, 8, 124, 105, 80, 105);
            shikaiFallingAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/shikai_byakuya_falling"), 0.1f, true, 2, 88, 105, 40, 105);
            shikaiDuckAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/shikai_byakuya_duck"), 0.1f, true, 1, 88, 105, 40, 105);
            shikaiGroundAttackAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/byakuya_shikai_GroundAttack"), 0.15f, false, 4, 100, 105, 40, 105);
            shikaiAirAttackAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Byakuya/shikai/byakuya_shikai_AirAttack"), 0.1f, false, 3, 116, 115, 40, 105);
            

            // ==== Blade ====
            shikaiBlade = level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_shikaiBlade");

            // === Reiatsu (Fade in) ====
            shikaiSetupReiatsu = level.Content.Load<Texture2D>("Sprites/Byakuya/byakuya_shikaiSetup_reiatsu");
            
            // === Senbonzakura Particles ====
            InitialiseSenbonzakuraParticleEngine();
        }

        protected void InitialiseSenbonzakuraParticleEngine()
        {
            List<Texture2D> textures = new List<Texture2D>();

            
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { new Color(255, 189, 239) });
            textures.Add(texture);

            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { new Color(255, 156, 214) });
            textures.Add(texture);

            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { new Color(214, 107, 173) });
            textures.Add(texture);

            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { new Color(181, 66, 140) });
            textures.Add(texture);

            senbonzakuraParticleEngine = new ParticleEngine(textures);

            textures = new List<Texture2D>();
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon1"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon2"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon3"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon4"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon5"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon6"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon7"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon8"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon9"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon10"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon11"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon12"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon13"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon14"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon15"));
            textures.Add(level.Content.Load<Texture2D>("Sprites/Byakuya/senbonzakura/senbon16"));
            senbonzakuraTextureParticleEngine = new ParticleEngine(textures);
        }

        #endregion
        
        public override void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            GetInput(keyboardState);
            
            UpdateCharacterState();

            ApplyCharacterActions(gameTime);

            senbonzakuraParticleEngine.Update();
            senbonzakuraTextureParticleEngine.Update();

        }
        


        /// <summary>
        /// Byakuya Get Input - Keyboard to Action mappings. Inputs are independent of priority of actions.
        /// </summary>
        protected void GetInput(KeyboardState keyboardState)
        {
            ResetStates();

            // ==== Dashing in 4 directions ====

            if (keyboardState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.Up))
            {
                DashVertical(true);
            }
            else if (keyboardState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.Down))
            {
                DashVertical(false);
            }
            else if (keyboardState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.Right))
            {
                DashHorizontal(false);
            }
            else if (keyboardState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.Left))
            {
                DashHorizontal(true);
            }
            
            // ==== Ground Slash 1 ====

            if (keyboardState.IsKeyDown(Keys.D))
            {
                Slash();
            }
               
            // ==== Left Right Down movement - normal movement ====
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                Duck();
            }
            else if (keyboardState.IsKeyDown(Keys.Right))
            {
                Run(false);
            }
            else if (keyboardState.IsKeyDown(Keys.Left))
            {
                Run(true);
            }
                     
            // ==== Jumping ====

            if (keyboardState.IsKeyDown(Keys.A))
            {
                Jump();
            }

            // ==== Shikai - Senbonzakura ====

            if (keyboardState.IsKeyDown(Keys.X))
            {
                Shikai();
            }

        }

        protected void ResetStates()
        {
            attemptDash = false;
            attemptDashLeft = false;
            attemptDashRight = false;
            attemptDashUp = false;
            attemptDashDown = false;
            attemptJump = false;
            attemptRun = false;
            attemptRunLeft = false;
            attemptRunRight = false;
            attemptDuck = false;
            
            attemptSlash = false;
            attemptShikai = false;
        }
        
        /// <summary>
        /// Determine the character state in this current update cycle and play appropriate animations. Flipping of sprite is done here.
        /// Handles action priorities. 
        /// </summary>
        protected void UpdateCharacterState()
        {
            prevCharacterState = characterState;
            // Only take valid states
            if (characterState != CharacterState.indeterminate)
                prevValidCharacterState = characterState;

            if (attemptEndShikai && isOnGround)
            {
                startEndShikaiState();
            }
            else
            {
                if (characterState == CharacterState.dashing || characterState == CharacterState.groundAttack || characterState == CharacterState.shikai)
                {
                    // Cannot be interrupted
                }
                else if (characterState == CharacterState.airAttackRising)
                {
                    #region Air Attack Rising

                    if (isOnGround)
                    {
                        // Reached the ground. See which attempted action to decide what state to turn to.
                        CancelSlashing();
                        if (shikaiMode)
                            CancelSenbonzakuraParticles();

                    }
                    else
                    {
                        if (!attemptJump)
                        {
                            CancelJumping();
                            // Don't restart the animation for attackSlash
                            characterState = CharacterState.airAttackFalling;
                        }
                    }

                    #endregion
                }
                else if (characterState == CharacterState.airAttackFalling)
                {
                    #region Air Attack Falling

                    if (isOnGround)
                    {
                        // Reached the ground. See which attempted action to decide what state to turn to.
                        CancelSlashing();
                        if (shikaiMode)
                            CancelSenbonzakuraParticles();
                    }

                    #endregion
                }
                else if (characterState == CharacterState.jumping)
                {
                    #region jumping

                    if (attemptDash)
                    {
                        CancelJumping();
                        startDashState();
                    }
                    else if (attemptSlash)
                    {
                        if (attemptJump)
                        {
                            startAirSlashingRisingState();
                        }
                        else
                        {
                            CancelJumping();
                            startAirSlashingFallingState();
                        }
                    }
                    else if (!attemptJump)
                    {
                        // Whenever jump button is released, stop jump
                        CancelJumping();
                        startFallingState();
                    }
                    else if (attemptJump)
                    {
                        if (attemptRunLeft)
                            flip = SpriteEffects.FlipHorizontally;
                        else if (attemptRunRight)
                            flip = SpriteEffects.None;
                    }

                    #endregion
                }
                else if (characterState == CharacterState.falling)
                {
                    #region falling

                    if (attemptDash)
                    {
                        startDashState();
                    }
                    else if (attemptSlash)
                    {
                        startAirSlashingFallingState();
                    }
                    else if (isOnGround)
                    {
                        // Reached the ground. See which attempted action to decide what state to turn to.
                        indeterminateState();
                    }
                    else
                    {
                        if (attemptRunLeft)
                            flip = SpriteEffects.FlipHorizontally;
                        else if (attemptRunRight)
                            flip = SpriteEffects.None;
                    }

                    #endregion
                }
                else if (characterState == CharacterState.running)
                {
                    #region running

                    if (!isOnGround)
                    {
                        // Ran off a platform - start falling
                        startFallingState();

                        if (attemptDash)
                        {
                            startDashState();
                        }
                    }
                    else
                    {
                        // Actions that can be performed when running on ground according to priority:
                        if (attemptDash)
                        {
                            startDashState();
                        }
                        else if (attemptJump)
                        {
                            startJumpingState();
                        }
                        else if (attemptSlash)
                        {
                            startGroundSlashingState();
                        }
                        else if (attemptDuck)
                        {
                            startDuckingState();
                        }
                        else if (attemptShikai && !shikaiMode)
                        {
                            startShikaiSetupState();
                        }
                        else if (!attemptRun)
                        {
                            indeterminateState();
                        }
                        else if (attemptRun)
                        {
                            if (attemptRunLeft)
                                flip = SpriteEffects.FlipHorizontally;
                            else if (attemptRunRight)
                                flip = SpriteEffects.None;
                        }
                    }

                    #endregion
                }
                else if (characterState == CharacterState.ducking)
                {
                    #region ducking

                    if (!attemptDuck)
                        indeterminateState();
                    else if (attemptDash)
                    {
                        startDashState();
                    }
                    else if (attemptJump)
                    {
                        startJumpingState();
                    }

                    #endregion
                }
                // Assumption that idle is always on Ground
                else if (characterState == CharacterState.idle)
                {
                    #region idle

                    // This is the priority of actions
                    if (attemptDash)
                        startDashState();
                    else if (attemptJump)
                        startJumpingState();
                    else if (attemptSlash)
                        startGroundSlashingState();
                    else if (attemptDuck)
                        startDuckingState();
                    else if (attemptShikai && !shikaiMode)
                        startShikaiSetupState();
                    else if (attemptRun)
                        startRunningState();

                    // Do not start idle again if already idle 

                    #endregion
                }


                // New if statement because conditions above may lead to indeterminate state.
                // Resolve indetermine state. Assumption that indetermine states are always on Ground
                if (characterState == CharacterState.indeterminate)
                {
                    #region indeterminate

                    // This is the priority of actions
                    if (attemptDash)
                        startDashState();
                    else if (attemptJump)
                        startJumpingState();
                    else if (attemptSlash)
                        startGroundSlashingState();
                    else if (attemptDuck)
                        startDuckingState();
                    else if (attemptShikai && !shikaiMode)
                        startShikaiSetupState();
                    else if (attemptRun)
                        startRunningState();
                    else
                        startIdleState();

                    #endregion
                }
            }
        }
        
        /// <summary>
        /// Handles movement and changes states (time based animations) if necessary
        /// </summary>
        protected void ApplyCharacterActions(GameTime gameTime)
        {
            float timeElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = position;

            if (characterState == CharacterState.dashing)
            {
                DoDash(timeElapsed);
            }
            else
            {
                if (characterState == CharacterState.jumping)
                {
                    DoBasicMovement(timeElapsed);
                    DoJump(gameTime);
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }
                else if (characterState == CharacterState.falling)
                {
                    DoBasicMovement(timeElapsed);
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }
                else if (characterState == CharacterState.running)
                {
                    DoBasicMovement(timeElapsed);
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }
                else if (characterState == CharacterState.ducking)
                {
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }
                else if (characterState == CharacterState.groundAttack)
                {
                    DoGroundSlash(timeElapsed);
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }
                else if (characterState == CharacterState.airAttackRising)
                {
                    DoAirSlashRising(timeElapsed, gameTime);
                    DoBasicMovement(timeElapsed);
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }
                else if (characterState == CharacterState.airAttackFalling)
                {
                    DoAirSlashFalling(timeElapsed);
                    DoBasicMovement(timeElapsed);
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);

                }
                else if (characterState == CharacterState.shikai)
                {
                    DoShikai(timeElapsed);
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }
                else if (characterState == CharacterState.idle)
                {
                    DoGravity(timeElapsed);
                    ApplyVelocity(timeElapsed);
                }

                HandleCollisions();
            }

            // If the collision stopped us from moving, reset the velocity to zero.
            if (position.X == previousPosition.X)
                velocity.X = 0;

            if (position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        #region Byakuya Start State Functions - Play animation, set internal variables, reset timers
        
        protected override void startIdleState()
        {
            characterState = CharacterState.idle;
            if (shikaiMode)
            {
                sprite.PlayAnimation(shikaiIdleAnimation);
            }
            else
            {
                sprite.PlayAnimation(idleAnimation);
            }
        }

        protected override void startRunningState()
        {
            characterState = CharacterState.running;
            if (shikaiMode)
            {
                sprite.PlayAnimation(shikaiRunAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }
        }

        protected override void startDuckingState()
        {
            characterState = CharacterState.ducking;
            if (shikaiMode)
            {
                sprite.PlayAnimation(shikaiDuckAnimation);
            }
            else
            {
                sprite.PlayAnimation(duckAnimation);
            }
        }

        protected override void startDashState()
        {
            characterState = CharacterState.dashing;
            dashTime = 0;

            if (attemptDashLeft)
            {
                dashState = DashState.dashLeft;
                flip = SpriteEffects.FlipHorizontally;
                if (shikaiMode)
                {
                    sprite.PlayAnimation(shikaiDashHorizontalAnimation);
                }
                else
                {
                    sprite.PlayAnimation(dashHorizontalAnimation);
                }
            }
            else if (attemptDashRight)
            {
                dashState = DashState.dashRight;
                flip = SpriteEffects.None;
                if (shikaiMode)
                {
                    sprite.PlayAnimation(shikaiDashHorizontalAnimation);
                }
                else
                {
                    sprite.PlayAnimation(dashHorizontalAnimation);
                }
            }
            else if (attemptDashUp)
            {
                dashState = DashState.dashUp;
                if (shikaiMode)
                {
                    sprite.PlayAnimation(shikaiDashVerticalAnimation);
                }
                else
                {
                    sprite.PlayAnimation(dashVerticalAnimation);
                }
            }
            else if (attemptDashDown)
            {
                dashState = DashState.dashDown;
                if (shikaiMode)
                {
                    sprite.PlayAnimation(shikaiDashVerticalAnimation);
                }
                else
                {
                    sprite.PlayAnimation(dashVerticalAnimation);
                }
            }

        }

        protected override void startJumpingState()
        {
            characterState = CharacterState.jumping;
            if (shikaiMode)
            {
                sprite.PlayAnimation(shikaiJumpAnimation);
            }
            else
            {
                sprite.PlayAnimation(jumpAnimation);
            }
            jumpTime = 0;
        }

        protected override void startFallingState()
        {
            characterState = CharacterState.falling;
            if (shikaiMode)
            {
                sprite.PlayAnimation(shikaiFallingAnimation);
            }
            else
            {
                sprite.PlayAnimation(fallingAnimation);
            }
        }

        /// <summary>
        /// Handles both standard and shikai ground attacks. Initiates senbonzakura starting positions.
        /// </summary>
        protected void startGroundSlashingState()
        {
            // Only can swap directions at start of attack
            if (attemptRunLeft)
                flip = SpriteEffects.FlipHorizontally;
            else if (attemptRunRight)
                flip = SpriteEffects.None;
            
            characterState = CharacterState.groundAttack;
            groundSlashTime = 0f;

            // Continue previous slash with correct slash state
            if (prevValidCharacterState == CharacterState.groundAttack)
            {
                // Only do level 2 slashing if previous move was slash

                if (groundSlashState == GroundSlashState.slashState1)
                {
                    groundSlashState = GroundSlashState.slashState2;

                    if (shikaiMode)
                    {
                        sprite.PlayAnimation(shikaiGroundAttackAnimation);

                        // Create senbonzakura attack
                        senbonzakuraParticleEngine.EmitterLocation = position + new Vector2(0, -110);
                        senbonzakuraTextureParticleEngine.EmitterLocation = position + new Vector2(0, -110);
                    }
                    else
                    {
                        sprite.PlayAnimation(slashAnimation2);
                    }
                }
                else if (groundSlashState == GroundSlashState.slashState2)
                {
                    groundSlashState = GroundSlashState.slashState1;

                    if (shikaiMode)
                    {
                        sprite.PlayAnimation(shikaiGroundAttackAnimation);

                        // Create senbonzakura attack
                        senbonzakuraParticleEngine.EmitterLocation = position + new Vector2(0, -30);
                        senbonzakuraTextureParticleEngine.EmitterLocation = position + new Vector2(0, -30);
                    }
                    else
                    {
                        sprite.PlayAnimation(slashAnimation1);
                    }
                }
            }
            else
            {
                // start new slash with level 1 slashing
                groundSlashState = GroundSlashState.slashState1;

                if (shikaiMode)
                {
                    sprite.PlayAnimation(shikaiGroundAttackAnimation);

                    // Create senbonzakura attack
                    senbonzakuraParticleEngine.EmitterLocation = position + new Vector2(0, -30);
                    senbonzakuraTextureParticleEngine.EmitterLocation = position + new Vector2(0, -30);
                }
                else
                {
                    sprite.PlayAnimation(slashAnimation1);
                }
            }
            
        }

        protected void startAirSlashingRisingState()
        {
            // Only can swap directions at start of attack
            if (attemptRunLeft)
                flip = SpriteEffects.FlipHorizontally;
            else if (attemptRunRight)
                flip = SpriteEffects.None;

            characterState = CharacterState.airAttackRising;
            airSlashTime = 0f;
            
            if (shikaiMode)
            {
                sprite.PlayAnimation(shikaiAirAttackAnimation);
            }
            else
            {
                sprite.PlayAnimation(airSlashAnimation);
            }
        }

        protected void startAirSlashingFallingState()
        {
            // Only can swap directions at start of attack
            if (attemptRunLeft)
                flip = SpriteEffects.FlipHorizontally;
            else if (attemptRunRight)
                flip = SpriteEffects.None;

            characterState = CharacterState.airAttackFalling;
            airSlashTime = 0f;

            if (shikaiMode)
            {
                sprite.PlayAnimation(shikaiAirAttackAnimation);
            }
            else
            {
                sprite.PlayAnimation(airSlashAnimation);
            }
        }

        protected void startShikaiSetupState()
        {
            characterState = CharacterState.shikai;
            shikaiTime = 0f;
            sprite.PlayAnimation(shikaiSetupAnimation);
            shikaiState = ShikaiState.shikaiSetup;
        }

        protected void startShikaiChargeState()
        {
            characterState = CharacterState.shikai;
            shikaiTime = 0f;
            shikaiState = ShikaiState.shikaiCharge;
            shikaiBladeAlpha = 0f; // start shikai blade as transparent
        }

        protected void startShikaiReleaseState()
        {
            characterState = CharacterState.shikai;
            shikaiTime = 0f;
            sprite.PlayAnimation(shikaiReleaseAnimation);
            shikaiState = ShikaiState.shikaiRelease;

            // Create senbonzakura particles, initial position, and particle direction
            senbonzakuraParticleEngine.EmitterLocation = position - new Vector2(0, 120);
            senbonzakuraParticleEngine.SetParticleVariables(2f, 0.3f, 30, 5, 0.1f, 2);

            senbonzakuraTextureParticleEngine.EmitterLocation = position - new Vector2(0, 120);
            senbonzakuraTextureParticleEngine.SetParticleVariables(2f, 0.3f, 30, 3);
        }

        protected void startEndShikaiState()
        {
            attemptEndShikai = false;
            shikaiMode = false;
            characterState = CharacterState.shikai;
            shikaiState = ShikaiState.endShikai;
            shikaiTime = 0f;
            sprite.PlayAnimation(shikaiReleaseAnimation);

            // Create senbonzakura particles, initial position, and particle direction
            senbonzakuraParticleEngine.EmitterLocation = position - new Vector2(0, 78);
            senbonzakuraParticleEngine.SetParticleVariables(1f, 1f, 0, 5, 0.1f, 2f);

            // End senbonzakura texture particles
            senbonzakuraTextureParticleEngine.SetParticleVariables(0, 0, 0, 0);

        }


        #endregion

        #region Byakuya Movelist - Attempt Actions

        protected void Slash()
        {
            attemptSlash = true;
        }

        protected void Shikai()
        {
            attemptShikai = true;
        }

        #endregion

        #region Byakuya Actions - Implement Actions

        /// <summary>
        /// Time Slashes and change slash level automatically when time is up.
        /// </summary>
        protected void DoGroundSlash(float timeElapsed)
        {
            groundSlashTime += timeElapsed;

            float maxGroundSlashTime = 0f;

            if (!shikaiMode)
            {
                if (groundSlashState == GroundSlashState.slashState1)
                    maxGroundSlashTime = maxGroundSlashTime1;
                else if (groundSlashState == GroundSlashState.slashState2)
                    maxGroundSlashTime = maxGroundSlashTime2;
            }
            else
            {
                // Shikai ground attack
                maxGroundSlashTime = maxShikaiGroundAttackTime;

                DoSenbonzakuraGroundAttackPath(timeElapsed);
            }

            // End the slash if time is up
            if (groundSlashTime > maxGroundSlashTime)
            {
                CancelSlashing();
                CancelSenbonzakuraParticles();
            }
        }

        void DoSenbonzakuraGroundAttackPath(float timeElapsed)
        {
            // Calculate senbonzakura paths. x will go from 0 - 1 for the entire duration.
            float x = groundSlashTime / maxShikaiGroundAttackTime;
            float SenbonTrailX = 0;
            float SenbonTrailY = 0;
            float SenbonSize = 0;

            if (groundSlashState == GroundSlashState.slashState1)
            {
                SenbonTrailX = 40f / (float)Math.PI * (float)Math.Cos(20.0f / Math.PI * x);
                SenbonTrailY = -80f * timeElapsed / maxShikaiGroundAttackTime;
                SenbonSize = 4 * x - 4 * x * x;
            }
            else if (groundSlashState == GroundSlashState.slashState2)
            {
                //SenbonTrailX = 40f / (float)Math.PI * (float)Math.Cos(20.0f / Math.PI * x);
                //SenbonTrailY = 80f * timeElapsed / maxShikaiGroundAttackTime;
                SenbonTrailX = 24 * (0.7f - 2 * x);
                SenbonTrailY = 120f * timeElapsed / maxShikaiGroundAttackTime;
                SenbonSize = 4 * x * (0.85f - x) / 0.7225f;
                if (SenbonSize < 0)
                    SenbonSize = 0;
            }

            if (flip == SpriteEffects.None)
            {
                // Adjust position
                senbonzakuraParticleEngine.EmitterLocation += new Vector2(SenbonTrailX, SenbonTrailY);
                // Adjust size
                senbonzakuraParticleEngine.SetParticleVariables(2.5f, 0.6f, 10, 20, 0.1f, SenbonSize * 2);

                senbonzakuraTextureParticleEngine.EmitterLocation += new Vector2(SenbonTrailX, SenbonTrailY);
                senbonzakuraTextureParticleEngine.SetParticleVariables(2.5f, 0.6f, 10, 20, 0.1f, SenbonSize);
            }
            else
            {
                senbonzakuraParticleEngine.EmitterLocation += new Vector2(-SenbonTrailX, SenbonTrailY);
                senbonzakuraParticleEngine.SetParticleVariables(2.5f, 0.6f, 10, 20, 0.1f, SenbonSize * 2);

                senbonzakuraTextureParticleEngine.EmitterLocation += new Vector2(-SenbonTrailX, SenbonTrailY);
                senbonzakuraTextureParticleEngine.SetParticleVariables(2.5f, 0.6f, 10, 20, 0.1f, SenbonSize);
            }
        }

        /// <summary>
        /// Time slashes and jumping. Apply velocity for jumping if still jumping while slashing
        /// </summary>
        protected void DoAirSlashRising(float timeElapsed, GameTime gameTime)
        {
            airSlashTime += timeElapsed;
            jumpTime += gameTime.ElapsedGameTime.Milliseconds;
            float maxAirAttackTime = 0;
            if (shikaiMode)
                maxAirAttackTime = maxShikaiAirAttackTime;
            else
                maxAirAttackTime = maxAirSlashTime;


            if (airSlashTime > maxAirAttackTime)
            {
                // End the slash if time is up
                CancelSlashing();
                if (shikaiMode)
                    CancelSenbonzakuraParticles();

                if (jumpTime > MaxJumpTime)
                {
                    // Not jumping.
                    startFallingState();
                }
                else
                {
                    // Still jumping so dont alter jumpTime. 
                    characterState = CharacterState.jumping;
                }
            }
            else
            {
                DoSenbonzakuraAirAttackPath(timeElapsed);
            }

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

                if (airSlashTime > maxAirAttackTime)
                {
                    // Finished slashing

                    startFallingState();
                }
                else
                {
                    // Still slashing so don't alter airSlashTime
                    characterState = CharacterState.airAttackFalling;
                }
            }
            else
            {
                // Other conditions - jumptime <= 0
                CancelJumping();
                startFallingState();
                if (shikaiMode)
                    CancelSenbonzakuraParticles();
            }
        }

        /// <summary>
        /// Time slashes while falling.
        /// </summary>
        protected void DoAirSlashFalling(float timeElapsed)
        {
            airSlashTime += timeElapsed;
            float maxAirAttackTime = 0;
            if (shikaiMode)
                maxAirAttackTime = maxShikaiAirAttackTime;
            else
                maxAirAttackTime = maxAirSlashTime;

            // End the slash if time is up
            if (airSlashTime > maxAirAttackTime)
            {
                CancelSlashing();
                if (shikaiMode)
                    CancelSenbonzakuraParticles();

                if (!isOnGround)
                {
                    startFallingState();
                }
            }
            else
            {
                DoSenbonzakuraAirAttackPath(timeElapsed);
            }
        }

        void DoSenbonzakuraAirAttackPath(float timeElapsed)
        {
            // Move senbonzakura in shikai mode
            if (shikaiMode)
            {
                // Calculate senbonzakura paths. x will go from 0 - 1 for the entire duration.
                float x = airSlashTime / maxShikaiAirAttackTime;
                float SenbonTrailX = 0;
                float SenbonTrailY = 0;
                float SenbonSize = 0;

                /*if(flip == SpriteEffects.None)
                    SenbonTrailX = position.X + 20 + 50*4*x*(0.83f-x)/0.83f/0.83f;
                else
                    SenbonTrailX = position.X - 20 - 50 * 4 * x * (0.83f - x) / 0.83f / 0.83f;

                SenbonTrailY = position.Y - 120 + 120 * (float)Math.Pow(x, 0.125f);
                //SenbonSize = 4 * x * (0.85f - x) / 0.7225f;
                //if (SenbonSize < 0)
                 //   SenbonSize = 0;
                SenbonSize = 1f;

                // Adjust position
                senbonzakuraParticleEngine.EmitterLocation = new Vector2(SenbonTrailX, SenbonTrailY);
                // Adjust size
                senbonzakuraParticleEngine.SetParticleVariables(1f, 0.6f, 20, 15, 0.1f, SenbonSize * 2);
                */
                if (flip == SpriteEffects.None)
                    SenbonTrailX = position.X + 80 * -(float)Math.Cos(22.0f / Math.PI * x);
                else
                    SenbonTrailX = position.X - 80 * -(float)Math.Cos(22.0f / Math.PI * x);
                
                SenbonTrailY = position.Y - 160 + 300f * x;
                
                SenbonSize = 0.5f*((float)-Math.Cos(20f/Math.PI * x) + 1);
                if (SenbonSize < 0)
                   SenbonSize = 0;
                
                // Adjust position
                senbonzakuraParticleEngine.EmitterLocation = new Vector2(SenbonTrailX, SenbonTrailY);
                // Adjust size
                senbonzakuraParticleEngine.SetParticleVariables(1f, 0.5f, 5, 35, 0.1f, SenbonSize * 2);


                senbonzakuraTextureParticleEngine.EmitterLocation = new Vector2(SenbonTrailX, SenbonTrailY);
                senbonzakuraTextureParticleEngine.SetParticleVariables(1f, 0.5f, 5, 35, 0.1f, SenbonSize);
                
            }
        }
                
        protected void CancelSlashing()
        {
            groundSlashTime = 0f;
            airSlashTime = 0f;
            indeterminateState();
        }

        protected void CancelSenbonzakuraParticles()
        {
            senbonzakuraParticleEngine.SetParticleVariables(0, 0, 0, 0);
            senbonzakuraTextureParticleEngine.SetParticleVariables(0, 0, 0, 0);
        }

        protected void DoShikai(float timeElapsed)
        {
            shikaiTime += timeElapsed;

            // ==== Starting Shikai ====

            if (shikaiState == ShikaiState.shikaiSetup)
            {
                // Give time to bring sword to vertical position.
                if (shikaiTime > maxShikaiSetupTime)
                {
                    startShikaiChargeState();
                }
            }
            else if (shikaiState == ShikaiState.shikaiCharge)
            {
                // Sword glows
                if (shikaiTime > maxShikaiChargeTime)
                {
                    startShikaiReleaseState();
                }
                else
                {
                    // Reduce transparency of shikai blade
                    shikaiBladeAlpha = shikaiTime / maxShikaiChargeTime * 1.0f;
                }
            }
            else if (shikaiState == ShikaiState.shikaiRelease)
            {
                if (shikaiTime > maxShikaiReleaseTime)
                {
                    shikaiState = ShikaiState.shikaiDelay;
                    shikaiTime = 0f;
                    CancelSenbonzakuraParticles();
                }
                else
                {
                    // Reduce size of shikai blade as blade scatters. 
                    shikaiBladeScale.Y = MathHelper.Clamp((maxShikaiReleaseTime - shikaiTime) / maxShikaiReleaseTime * 1.0f, 0f, 1.0f);
                    // Move senbonzakura particle emitter downwards with blade. 
                    senbonzakuraParticleEngine.EmitterLocation = senbonzakuraParticleEngine.EmitterLocation + new Vector2(0, 42f / maxShikaiReleaseTime * timeElapsed);
                    senbonzakuraTextureParticleEngine.EmitterLocation = senbonzakuraTextureParticleEngine.EmitterLocation + new Vector2(0, 42f / maxShikaiReleaseTime * timeElapsed);
                }
            }
            else if (shikaiState == ShikaiState.shikaiDelay)
            {
                // Give time for blade to finish scattering
                if (shikaiTime > maxShikaiDelayTime)
                {
                    CancelShikai();
                    StartShikaiMode();
                }
            }

            // ===== Ending Shikai ======

            else if (shikaiState == ShikaiState.endShikai)
            {
                if (shikaiTime > maxShikaiReleaseTime)
                {
                    shikaiState = ShikaiState.endShikaiDelay;
                    shikaiTime = 0f;
                    CancelSenbonzakuraParticles();
                    sprite.PlayAnimation(shikaiEndAnimation);
                }
                else
                {
                    // Increase size of shikai blade as blade restores. Move senbonzakura particle emitter upwards with blade. 
                    shikaiBladeScale.Y = MathHelper.Clamp(shikaiTime / maxShikaiReleaseTime * 1.0f, 0f, 1.0f);
                    senbonzakuraParticleEngine.EmitterLocation = senbonzakuraParticleEngine.EmitterLocation - new Vector2(0, 42f / maxShikaiReleaseTime * timeElapsed);
                }
            }
            else if (shikaiState == ShikaiState.endShikaiDelay)
            {
                // Give time for blade to finish restoring
                if (shikaiTime > maxShikaiDelayTime)
                {
                    CancelShikai();
                }
                else
                {
                    // Increase transparency of shikai blade
                    shikaiBladeAlpha = (maxShikaiChargeTime - shikaiTime) / maxShikaiChargeTime * 1.0f;
                }
            }
        }

        protected void CancelShikai()
        {
            CancelSenbonzakuraParticles();
            shikaiState = ShikaiState.shikaiSetup;
            shikaiTime = 0f;
            indeterminateState();

        }

        /// <summary>
        /// Start the timer for shikai mode and enable shikai mode to play shikai sprites. 
        /// </summary>
        protected void StartShikaiMode()
        {
            shikaiMode = true;
            shikaiTimer.Enabled = true;
        }

        private void OnEndShikai(object source, ElapsedEventArgs e)
        {
            attemptEndShikai = true;
            shikaiTimer.Enabled = false;
        }

        #endregion


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            
            // ==== Reiatsu Images ====
            if (characterState == CharacterState.shikai)
            {
                if (shikaiState == ShikaiState.shikaiCharge || shikaiState == ShikaiState.endShikaiDelay)
                {
                    spriteBatch.Draw(shikaiSetupReiatsu, position, new Rectangle(0, 0, 108, 140), new Color(255, 255, 255, shikaiBladeAlpha), 0f, new Vector2(54, 140), 1f, flip, 0f);
                }
            }

            // ==== Character Images ====
            base.Draw(gameTime, spriteBatch);

            // ==== Shikai Blade Images ====
            if (shikaiState == ShikaiState.shikaiCharge || shikaiState == ShikaiState.endShikaiDelay)
            {
                spriteBatch.Draw(shikaiBlade, position - new Vector2(0, 78), shikaiBlade.Bounds, new Color(255,255,255, shikaiBladeAlpha), 0f, new Vector2((float)shikaiBlade.Width / 2f, shikaiBlade.Height), 1f, flip, 0f);
            }
            else if (shikaiState == ShikaiState.shikaiRelease || shikaiState == ShikaiState.endShikai)
            {
                spriteBatch.Draw(shikaiBlade, position - new Vector2(0, 78), shikaiBlade.Bounds, Color.White, 0f, new Vector2((float)shikaiBlade.Width / 2f, shikaiBlade.Height), shikaiBladeScale, flip, 0f);
            }
            senbonzakuraParticleEngine.Draw(spriteBatch);
            senbonzakuraTextureParticleEngine.Draw(spriteBatch);


        }

    }

}
