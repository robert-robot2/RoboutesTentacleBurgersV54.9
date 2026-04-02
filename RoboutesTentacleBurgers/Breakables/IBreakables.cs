namespace RoboutesTentacleBurgers.Breakables
{
    public interface IBreakables
    {

        int BreakX { get; set; }
        int BreakY { get; set; }
        int BreakWidth { get; set; }
        int BreakHeight { get; set; }
        int BreakHitPoints { get; set; }
        bool BreakIsAlive { get; }
        Rectangle BreakCollisionBox { get; }

        void BreakTakeDamage(int amount);
        void BreakClearHitEffects();
        string? BreakHitEffectPath { get; }
        string BreakSpriteStyle { get; }
        string BreakSpriteDebugStyle { get; }
        string BreakCollisionDebugStyle { get; }




    }
}
