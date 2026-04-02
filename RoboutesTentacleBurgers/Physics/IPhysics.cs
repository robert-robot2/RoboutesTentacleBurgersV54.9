namespace RoboutesTentacleBurgers.Physics
{
    public interface IPhysics
    {
        // Core identity
        int PhysX { get; set; }
        int PhysY { get; set; }
        int PhysWidth { get; set; }
        int PhysHeight { get; set; }

        // State
        bool PhysIsActive { get; }
        Rectangle PhysCollisionBox { get; }

        // Physics values
        float VelocityX { get; set; }
        float VelocityY { get; set; }
        float Mass { get; set; }

        // Sovereign tick law
        void PhysTickUpdate(IBloodiCharacter character);

        // Styles
        string PhysSpriteStyle { get; }
        string PhysDebugStyle { get; }
        string PhysCollisionStyle { get; }
    }
}
