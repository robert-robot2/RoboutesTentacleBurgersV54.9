

using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using static RoboutesTentacleBurgers.Services.ZIndexCache;
using static System.Reflection.Metadata.BlobBuilder;



public class BloodWyrmService
{


    public BloodLevel LevelReg { get; set; } = default!;
    public NavigationManager NavManager { get; set; } = default!;



    public BloodCharacterHandle CharacterHandle { get; set; } = new();
    // Safe proxy, allows null checks
    public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;
    //public IBloodiCharacter ActiveCharacter { get; set; }= default!;     
    public BloodEnemyHandle EnemyHandle { get; set; } = new();
    public IiEnemy? ActiveEnemy => EnemyHandle.ActiveEnemy;
    //  public IiEnemy? ActiveEnemy { get; set; } = default!;
    public BloodDynOHandle DynOHandle { get; set; } = new();
    public IDynamicO? ActiveDynO => DynOHandle.ActiveDynO;
    public BloodBreakHandle BreakHandle { get; set; } = new();
    public IBreakables? Breakables => BreakHandle.Breakables;
    public BloodPhysicsHandle PhysicsHandle { get; set; } = new();
    public IPhysics? Physics => PhysicsHandle.Physics;


    public BloodLightHandle LightHandle { get; set; } = new();
    public ILighting? Lighting => LightHandle.Lighting;
    public BloodWeatherHandle WeatherHandle { get; set; } = new();
    public IWeather? Weather => WeatherHandle.Weather;

    public BloodStaticHandle StaticHandle { get; set; } = new();
    public IStatic? ActiveStatic => StaticHandle.ActiveStatic;

    // Input system
    public BloodInput Input { get; set; } = default!;
    // Metrics
    public Func<Task>? RequestUIRefresh { get; set; }
    // Remove 'static' keyword
    public PerformanceMonitor PerfMonitor { get; set; } = new PerformanceMonitor();

    // Viewport

    public int ViewportWidth { get; set; } = 1024;
    public int ViewportHeight { get; set; } = 768;
    public int ScrollEdgeBufferX => 0; // ( X up, X Down)
    public int ScrollEdgeBufferY => 0; // (Y up, Y Down)
    public string ViewportXpx => $"-{ViewportX}px";
    public string ViewportYpx => $"-{ViewportY}px";
    public int ViewportX
    {
        get
        {
            if (ActiveCharacter is null) return 0;
            int centerX = ActiveCharacter.CharX;
            return Math.Clamp(centerX - ViewportWidth / 2, 0, 2048 - ViewportWidth - ScrollEdgeBufferX);
        }
    }
    public int ViewportY
    {
        get
        {
            if (ActiveCharacter is null) return 0;
            int centerY = ActiveCharacter.CharY;
            return Math.Clamp(centerY - ViewportHeight / 2, 0, 2048 - ViewportHeight - ScrollEdgeBufferY);
        }
    }

    // In Game service

    public bool ShowDebugOverlay { get; set; } = false;
    public bool ShowDebugOverlay2 { get; set; } = false;
    public bool ShowPerformanceOverlay { get; set; } = false;

    public bool showGameMenu = true;
    public bool showOptionsMenu = false;
    public bool showControlsMenu = false;
    public bool showCharacterMenu = false;
    public bool showInventoryMenu = false;
    private bool isInventoryVisible = false;
    public bool isMenuVisible = false;

    public void HandleStart()
    {
        showCharacterMenu = false;
        showGameMenu = false;
        showControlsMenu = false;
        showOptionsMenu = false;
    }
    public void HandleCharSelect()
    {
        showCharacterMenu = true;
        showGameMenu = false;
        showControlsMenu = false;
        showOptionsMenu = false;
    }
    public void HandleOptions()
    {
        showOptionsMenu = true;
        showGameMenu = false;
    }
    public void HandleControls()
    {
        showControlsMenu = true;
        showGameMenu = false;
    }
    public void HandleMainMenu()
    {
        isMenuVisible = !isMenuVisible;

        if (isMenuVisible)
        {
            showGameMenu = true;
            showControlsMenu = false;
            showOptionsMenu = false;
            showCharacterMenu = false;
            showInventoryMenu = false;
        }
        else
        {
            showGameMenu = false;
            showControlsMenu = false;
            showOptionsMenu = false;
            showCharacterMenu = false;
            showInventoryMenu = false;
        }
    }
    public void HandleRMainMenu()
    {
        showGameMenu = true;
        showControlsMenu = false;
        showOptionsMenu = false;
        showCharacterMenu = false;
        showInventoryMenu = false;
    }
    public void HandleInv()
    {
        isInventoryVisible = !isInventoryVisible;

        if (isInventoryVisible)
        {
            showGameMenu = false;
            showControlsMenu = false;
            showOptionsMenu = false;
            showCharacterMenu = false;
            showInventoryMenu = true;
        }
        else
        {
            showGameMenu = false;
            showControlsMenu = false;
            showOptionsMenu = false;
            showCharacterMenu = false;
            showInventoryMenu = false;
        }
    }
    public void HandleQuit()
    {
        NavManager.NavigateTo("/");
    }





    public void ExecutePunch()
    {
        try
        {
            foreach (var registry in BloodEnemyHandle.AllRegistries)
            {
                foreach (var enemy in registry)
                {
                    if (enemy.EnemyIsAlive && ActiveCharacter.CharPunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                    {
                        try
                        {
                            enemy.AddXp(LevelReg, 1.0);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"AddXp failed for {enemy.GetType().Name}: {ex.Message}");
                            // Continue anyway
                        }
                    }
                }
            }

            EnemyHandle.UpdateActiveEnemy();
            ActiveCharacter?.CharAttack(ActiveEnemy!, Breakables!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! EXCEPTION IN ExecutePunch: {ex.Message}");
        }
    }

    public void ExecuteSpecial()
    {
        try
        {
            foreach (var registry in BloodEnemyHandle.AllRegistries)
            {
                foreach (var enemy in registry)
                {
                    if (enemy.EnemyIsAlive &&
                        ActiveCharacter.CharPunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                    {
                        try
                        {
                            // SHould be make this the multipleir for Double XP? with int multiplier or a better way this seems easy?

                            enemy.AddXp(LevelReg, 2.0);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"AddXp failed for {enemy.GetType().Name}: {ex.Message}");
                            // Continue anyway
                        }
                    }
                }
            }

            EnemyHandle.UpdateActiveEnemy();
            ActiveCharacter?.CharSpecialAttack(ActiveEnemy!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! EXCEPTION IN ExecuteSpecial: {ex.Message}");
        }
    }




    // Timer setup
    private System.Timers.Timer tickTimer;

    public void InitTickTimer()
    {
        if (tickTimer == null)
        {
            tickTimer = new System.Timers.Timer(16.666);
            tickTimer.Elapsed += TickTimerElapsed;
            tickTimer.AutoReset = true;
            tickTimer.Start();
        }
    }

    private void TickTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Fire and forget, but capture exceptions
        _ = TickLoopAsync();
    }
    private bool _isTicking = false;


    public async Task TickLoopAsync()
    {
        if (_isTicking) return;   // guard: skip if already running
        _isTicking = true;

        try
        {
            if (ActiveCharacter == null) return;

            PerfMonitor.StartFrame();

            StartTickLoop();   // synchronous game logic

            PerfMonitor.EndFrame();

            if (RequestUIRefresh != null)
            {
                //  Console.WriteLine("RequestUIRefresh IS SET"); // ← Add this
                await RequestUIRefresh();
            }
            else
            {
                //  Console.WriteLine("RequestUIRefresh IS NULL"); // ← Add this
            }
        }
        finally
        {
            _isTicking = false;  // release guard
        }
    }


    // The core tick method that runs continuously (kept as void)
    public void StartTickLoop()
    {
        if (ActiveCharacter == null) return;

        using (PerfMonitor.MeasureSection("Character Animation"))
        {
            ActiveCharacter.CharTickAllAnimation();
        }

        using (PerfMonitor.MeasureSection("Enemy Updates"))
        {
            if (BloodEnemyHandle.AllRegistries != null && BloodEnemyHandle.AllRegistries.Any())
            {
                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive)
                        {
                            enemy.EnemyMove(ActiveCharacter);
                            enemy.EnemyTickAllAnimation();
                        }
                    }
                }
            }
        }
        using (PerfMonitor.MeasureSection("Static"))
        {
            foreach (var obj in BloodStaticObject.BloodStaticRegistry.All)
            {


            }

        }

        using (PerfMonitor.MeasureSection("Lighting"))
        {
            if (Lighting != null) { Lighting.LightingUpdate(); }

        }


        using (PerfMonitor.MeasureSection("Weather"))
        {

            if (Weather != null)
            {

                Weather.AdvanceFog();
                Weather.TickWeather(0.5);
                Weather.TickParticles(1.0 / 60.0);
            }


        }



        using (PerfMonitor.MeasureSection("DynamicO"))
        {
            foreach (var registry in BloodDynOHandle.AllRegistries)
            {
                foreach (var dynObj in registry)
                {
                    dynObj.DynTickUpdate(ActiveCharacter);
                }
            }
        }
        using (PerfMonitor.MeasureSection("Breakables"))
        {
            foreach (var registry in BloodBreakHandle.AllRegistries)
            {
                foreach (var breakables in registry)
                {
                    if (breakables.BreakIsAlive)
                    {

                    }
                }
            }

        }
        using (PerfMonitor.MeasureSection("Physics"))
        {
            foreach (var registry in BloodPhysicsHandle.AllRegistries)
            {
                foreach (var physics in registry)
                {
                    if (physics.PhysIsActive)
                    {
                        physics.PhysTickUpdate(ActiveCharacter);
                    }
                }
            }



        }
    }
    // ADDED: Stop timer method
    public void StopTickTimer()
    {
        tickTimer?.Stop();
        tickTimer?.Dispose();
    }

    public void ClearAll()
    {
        ZIndexCache.InvalidateCache();
        BloodEnemy.BloodSkeletonRegistry.All.Clear();
        BloodZombiePyscho.BloodZombiePyschoRegistry.All.Clear();
        BloodSkelPyscho.BloodSkelPyschoRegistry.All.Clear();
        BloodSkelWar.BloodSkeletonWarRegistry.All.Clear();
        BloodGoatMan.BloodGoatmanRegistry.All.Clear();
        BloodBoss.BloodScavBossRegistry.All.Clear();
        BloodEnemyBoss.BloodSkeletonRegistry.All.Clear();
        BloodCat.BloodSkeletonRegistry.All.Clear();

        BloodSplatterRegistry.All.Clear();




        BloodStaticObject.CaveRegistry.All.Clear();

        BloodStaticObjectT.HouseRegistry.All.Clear();
        BloodStaticObjectT.Fountain001Registry.All.Clear();
        BloodStaticObjectT.ShrineRegistry.All.Clear();
        BloodStaticObjectT.Tavern001Registry.All.Clear();


        BloodStaticObjectS.HouseRegistry.All.Clear();

        BloodStaticObject.BloodStaticRegistry.All.Clear();
        BloodStaticObjectC.BloodStaticRegistryC.All.Clear();
        BloodStaticO1010.BloodStaticRegistry1010.All.Clear();
        BloodStaticO1020.BloodStaticRegistry1020.All.Clear();
        BloodStaticO1030.BloodStaticRegistry1030.All.Clear();
        BloodStaticObjectGY.BloodStaticRegistryGY.All.Clear();
        BloodStaticObjectS.BloodStaticRegistryS.All.Clear();
        BloodStaticObjectT.BloodStaticRegistryT.All.Clear();
        BloodStaticO1040.BloodStaticRegistry1040.All.Clear();
        BloodStaticO1050.BloodStaticRegistry1050.All.Clear();
        BloodStaticO1060.BloodStaticRegistry1060.All.Clear();


        BloodBreakables.DummyRegistry.All.Clear();
        BloodDynamicObj.BloodCampFireRegistry.All.Clear();

        BloodPhysics.BloodUndeadGFRegistry.All.Clear();
        BloodCow.BloodCowRegistry.All.Clear();
        BloodTownSlut.BloodTownSlutRegistry.All.Clear();

        BloodDynamicObj.BloodCheeseRegistry.All.Clear();
        BloodDynamicObj.BloodHealPotRegistry.All.Clear();
        BloodDynamicObj.BloodMedHealPotRegistry.All.Clear();
        BloodDynamicObj.BloodManaPotRegistry.All.Clear();
        BloodDynamicObj.BloodStrPotRegistry.All.Clear();
        BloodDynamicObj.BloodCelPotRegistry.All.Clear();
        BloodDynamicObj.BloodAlcPotRegistry.All.Clear();
        BloodDynamicObj.BloodIntPotRegistry.All.Clear();
    }


}

