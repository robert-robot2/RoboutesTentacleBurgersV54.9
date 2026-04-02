namespace RoboutesTentacleBurgers.Physics
{
    public class BloodPhysics
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public class UndeadGF : IPhysics
        {
            // Core identity
            public int PhysX { get => UndeadGFX; set => UndeadGFX = value; }
            public int PhysY { get => UndeadGFY; set => UndeadGFY = value; }
            public int PhysWidth { get => UndeadGFWidth; set => UndeadGFWidth = value; }
            public int PhysHeight { get => UndeadGFHeight; set => UndeadGFHeight = value; }

            // State
            public bool PhysIsActive => UndeadGFIsActive;
            public Rectangle PhysCollisionBox => UndeadGFCollisionBox;

            // Physics values
            public float VelocityX { get; set; }
            public float VelocityY { get; set; }
            public float Mass { get; set; } = 1.0f;

            // Sovereign tick law
            public void PhysTickUpdate(IBloodiCharacter character) => TickKickGF(character);

            // Styles
            public string PhysSpriteStyle => UndeadGFSpriteStyle;
            public string PhysDebugStyle => UndeadGFSpriteDebugStyle;
            public string PhysCollisionStyle => UndeadGFCollisionBoxStyle;

            // Core UndeadGF fields
            public int UndeadGFX { get; set; }
            public int UndeadGFY { get; set; }
            public int UndeadGFWidth { get; set; } = 16;
            public int UndeadGFHeight { get; set; } = 16;
            public bool UndeadGFIsActive { get; set; } = true;

            public Rectangle UndeadGFCollisionBox =>
                new Rectangle(
                    UndeadGFX + UndeadGFCollisionShiftX,
                    UndeadGFY + UndeadGFCollisionShiftY,
                    UndeadGFCollisionWidth,
                    UndeadGFCollisionHeight
                );

            private const int UndeadGFCollisionWidth = 24;
            private const int UndeadGFCollisionHeight = 24;
            private const int UndeadGFCollisionShiftX = 0;
            private const int UndeadGFCollisionShiftY = 0;

            public void TickKickGF(IBloodiCharacter character)
            {
                foreach (var gf in BloodUndeadGFRegistry.All)
                {
                    if (!UndeadGFIsActive) continue;

                    // Apply velocity to position
                    UndeadGFX += (int)VelocityX;

                    // Apply friction
                    VelocityX *= 0.88f;

                    // Kick logic
                    if (character != null && character.CharIsAlive &&
                        UndeadGFCollisionBox.IntersectsWith(character.CharCollisionBox))
                    {
                        VelocityX += 1.2f; // Kick right
                    }
                }
            }

            /*
                public void TryKickGF(IBloodiCharacter character)
            {
                foreach (var gf in BloodUndeadGFRegistry.All)
                {
                    if (!UndeadGFIsActive || character == null || !character.CharIsAlive) continue;

                    if (UndeadGFCollisionBox.IntersectsWith(character.CharCollisionBox))
                    {
                        VelocityX += 1.2f; // Kick right
                    }
                }
            }

            public void TickKickGF(IBloodiCharacter character)
            {
                foreach (var gf in BloodUndeadGFRegistry.All)
                {
                    if (!UndeadGFIsActive) continue;

                    // Apply velocity to position
                    UndeadGFX += (int)VelocityX;

                    // Apply friction
                    VelocityX *= 0.88f;

                    TryKickGF(character);
                }
            }

             
             
             
             
             
             
             
             
             
             
             
             
             */








            public string UndeadGFSpriteStyle =>
                $"position:absolute; left:{UndeadGFX}px; top:{UndeadGFY}px; " +
                $"width:{UndeadGFWidth}px; height:{UndeadGFHeight}px; " +
                $"background-image:url('/iAssets/UndeadGFhead.png'); " +
                $"background-repeat:no-repeat; background-size:cover; z-index:39;";

            public string UndeadGFSpriteDebugStyle =>
                $"position:absolute; left:{UndeadGFX}px; top:{UndeadGFY}px; " +
                $"width:{UndeadGFWidth}px; height:{UndeadGFHeight}px; " +
                $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

            public string UndeadGFCollisionBoxStyle =>
                $"position:absolute; left:{UndeadGFX + UndeadGFCollisionShiftX}px; top:{UndeadGFY + UndeadGFCollisionShiftY}px; " +
                $"width:{UndeadGFCollisionWidth}px; height:{UndeadGFCollisionHeight}px; " +
                $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:2500;";
        }

        public static class BloodUndeadGFRegistry
        {
            public static List<UndeadGF> All = new();

            public static void SpawnUndeadGF(int count)
            {
                var rand = new Random();

                for (int i = 0; i < count; i++)
                {
                    All.Add(new UndeadGF
                    {
                        UndeadGFX = rand.Next(200, 1994),
                        UndeadGFY = rand.Next(200, 1994)
                    });
                }
            }
        }
    }
}
