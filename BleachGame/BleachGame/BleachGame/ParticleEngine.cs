using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BleachGame
{
    class ParticleEngine
    {
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particle> particles;
        private List<Texture2D> textures;
        
        float horizontalSpread = 0f;
        float verticalSpread = 0f;
        float angularVelocityModifier = 0f;
        float sizeModifier = 0f;
        int ttlModifier = 0;
        int numParticles = 0;

        public ParticleEngine(List<Texture2D> textures)
        {
            EmitterLocation = Vector2.Zero;
            this.textures = textures;
            this.particles = new List<Particle>();
            random = new Random();
        }

        public void SetParticleVariables(float horizontalSpread, float verticalSpread, int ttlModifier, int numParticles = 20, float angularVelocityModifier = 0.1f, float sizeModifier = 1f)
        {
            this.horizontalSpread = horizontalSpread;
            this.verticalSpread = verticalSpread;
            this.ttlModifier = ttlModifier;
            this.numParticles = numParticles;
            this.angularVelocityModifier = angularVelocityModifier;
            this.sizeModifier = sizeModifier;
        }

        private Particle GenerateNewParticle()
        {
            Texture2D texture = textures[random.Next(textures.Count)];
            Vector2 position = EmitterLocation;

            // More motion in horizontal direction than vertical
            Vector2 velocity = new Vector2(horizontalSpread * (float)(random.NextDouble() * 2 - 1), verticalSpread * (float)(random.NextDouble() * 2 - 1));
            float angle = 0;
            float angularVelocity = angularVelocityModifier * (float)(random.NextDouble() * 2 - 1);
            
            //float size = (float)random.NextDouble() + sizeModifier;
            float size = (float)random.NextDouble() * sizeModifier;
            
            
            int ttl = ttlModifier + random.Next(40);

            return new Particle(texture, position, velocity, angle, angularVelocity, size, ttl);
        }

        public void Update()
        {
            for (int i = 0; i < numParticles; i++)
            {
                particles.Add(GenerateNewParticle());
            }

            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.End();
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
            //spriteBatch.End();
            //spriteBatch.Begin();
        }
    }


    public class Particle
    {
        public Texture2D Texture { get; set; }        // The texture that will be drawn to represent the particle
        public Vector2 Position { get; set; }        // The current position of the particle        
        public Vector2 Velocity { get; set; }        // The speed of the particle at the current instance
        public float Angle { get; set; }            // The current angle of rotation of the particle
        public float AngularVelocity { get; set; }    // The speed that the angle is changing
        public float Size { get; set; }                // The size of the particle
        public int TTL { get; set; }               // The 'time to live' of the particle

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity, float angle, float angularVelocity, float size, int ttl)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Size = size;
            TTL = ttl;
        }

        public void Update()
        {
            TTL--;
            Position += Velocity;
            Angle += AngularVelocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Vector2 origin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            spriteBatch.Draw(Texture, Position, sourceRectangle, Color.White, Angle, origin, Size, SpriteEffects.None, 0f);
        }

    }
}
