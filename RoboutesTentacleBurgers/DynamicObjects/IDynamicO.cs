

namespace RoboutesTentacleBurgers.DynamicObjects
{
    public interface IDynamicO
    {
        // Position and size
        int DynX { get; set; }
        int DynY { get; set; }
        int DynWidth { get; set; }
        int DynHeight { get; set; }

        // Collision box
        Rectangle DynCollisionBox { get; }

        // Update hook (e.g. flicker, animation, AI)
        void DynTickUpdate(IBloodiCharacter character);

        bool DynIsActive { get; } 

        // Style string for rendering
        string DynSpriteStyle { get; }

        // Debug style (optional)
        string DynDebugStyle { get; }

        string DynDebugCollisionStyle { get; }

    }
}
