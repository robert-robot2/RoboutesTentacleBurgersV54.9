



using RoboutesTentacleBurgers.Breakables;
using RoboutesTentacleBurgers.DynamicObjects;
using RoboutesTentacleBurgers.Lighting;
using RoboutesTentacleBurgers.Physics;
using RoboutesTentacleBurgers.Weather;
using static BloodStaticO1040;
using static BloodStaticObject;
using static BloodZombiePyscho;
namespace RoboutesTentacleBurgers.Maps
{
    public partial class DarkForest1040 : ComponentBase, IDisposable
    {
        public BloodLightHandle LightHandle { get; set; } = new();
        public ILighting? Lighting => LightHandle.Lighting;
        public BloodWeatherHandle WeatherHandle { get; set; } = new();
        public IWeather? Weather => WeatherHandle.Weather;
        public BloodStaticHandle StaticHandle { get; set; } = new();
        public IStatic? ActiveStatic => StaticHandle.ActiveStatic;

        public BloodCharacterHandle CharacterHandle => Game.CharacterHandle;  // ← Proxy!
        public BloodInput Input { get; set; } = default!;  // ← Add this

        [Inject] public IJSRuntime JS { get; set; } = default!;
        public BloodEnemyHandle EnemyHandle { get; set; } = new();
        public IiEnemy? ActiveEnemy => EnemyHandle.ActiveEnemy;
        //  public IiEnemy? ActiveEnemy { get; set; } = default!;
        public BloodDynOHandle DynOHandle { get; set; } = new();
        public IDynamicO? ActiveDynO => DynOHandle.ActiveDynO;
        public BloodBreakHandle BreakHandle { get; set; } = new();
        public IBreakables? Breakables => BreakHandle.Breakables;
        public BloodPhysicsHandle PhysicsHandle { get; set; } = new();
        public IPhysics? Physics => PhysicsHandle.Physics;
        // Composite instance created on-demand from the page's concrete handles in the partial class
        // (this lives in the same partial class as BloodWyrmProtocol.razor.cs so it can access those fields)
        public RoboutesTentacleBurgers.Services.BloodAllHandle allhandles
            => new()
            {
                LightHandle = LightHandle,
                WeatherHandle = WeatherHandle,
                StaticHandle = StaticHandle,
                CharacterHandle = CharacterHandle,
                EnemyHandle = EnemyHandle,
                DynOHandle = DynOHandle,
                BreakHandle = BreakHandle,
                PhysicsHandle = PhysicsHandle
            };




        private System.Timers.Timer waveCheckTimer;


        protected override void OnInitialized()
        {
            // Set up UI refresh callback FIRST
            Game.RequestUIRefresh = () => InvokeAsync(StateHasChanged);

            // Clear world state
            Game.ClearAll();
            Spawn();
            // Load first wave
            WaveReg1040.LoadWave(1);


            // Wave timer
            waveCheckTimer = new System.Timers.Timer(16);
            waveCheckTimer.Elapsed += (_, __) => InvokeAsync(() => WaveReg1040.TryAdvanceWave());
            waveCheckTimer.Start();

       

            EnemyHandle = new BloodEnemyHandle();
            EnemyHandle.UpdateActiveEnemy();
            Game.EnemyHandle = EnemyHandle;
            LightHandle = new BloodLightHandle(LightingType.Directional);
            Game.LightHandle = LightHandle;
            WeatherHandle = new BloodWeatherHandle(WeatherType.Forest);
            Game.WeatherHandle = WeatherHandle;
            StaticHandle = new BloodStaticHandle();
            Game.StaticHandle = StaticHandle;

            // Add input if not already set
            if (Game.Input == null)
            {
                Input = new BloodInput(Game, JS);
                // No need to register again - already registered on main page
                Game.Input = Input;
            }
            else
            {
                Input = Game.Input;  // Reuse existing
            }
            // Initialize and START the tick timer LAST
            Game.InitTickTimer();
        }


        public void Spawn()
        {
            // Spawn Static Set
            HandleObjSpawn2();
            // Spawn NPC entitites
            BloodPhysics.BloodUndeadGFRegistry.SpawnUndeadGF(15);
            BloodCow.BloodCowRegistry.SpawnCows(10);
            BloodTownSlut.BloodTownSlutRegistry.SpawnTownSluts(1);
            BloodCat.BloodSkeletonRegistry.SpawnSkeletons(10);
            // Spawn consumables 

            BloodDynamicObj.BloodHealPotRegistry.SpawnHealPots(5);
            BloodDynamicObj.BloodMedHealPotRegistry.SpawnMedHealPots(5);
            BloodDynamicObj.BloodManaPotRegistry.SpawnManaPots(5);
            BloodDynamicObj.BloodStrPotRegistry.SpawnStrPots(5);
            BloodDynamicObj.BloodCelPotRegistry.SpawnCelPots(5);
            BloodDynamicObj.BloodAlcPotRegistry.SpawnAlcPots(5);
            BloodDynamicObj.BloodIntPotRegistry.SpawnIntPots(15);





        }


        // Plasma Engine Static Objects

        public void HandleObjSpawn2()
        {

            var allEnemies = new List<IiEnemy>();
            var alldynO = new List<IDynamicO>();
            var allBreaks = new List<IBreakables>();
            var allPhysics = new List<IPhysics>();
            var allStatics = new List<IStatic>();

            allEnemies.AddRange(BloodEnemy.BloodSkeletonRegistry.All);
            allEnemies.AddRange(BloodZombiePyscho.BloodZombiePyschoRegistry.All);
            allEnemies.AddRange(BloodSkelPyscho.BloodSkelPyschoRegistry.All);
            allEnemies.AddRange(BloodSkelWar.BloodSkeletonWarRegistry.All);
            allEnemies.AddRange(BloodGoatMan.BloodGoatmanRegistry.All);
            allEnemies.AddRange(BloodBoss.BloodScavBossRegistry.All);
            allEnemies.AddRange(BloodCow.BloodCowRegistry.All);
            allEnemies.AddRange(BloodTownSlut.BloodTownSlutRegistry.All);

            var dict = BloodStaticO1040.BloodStaticList1040.ForestDictionary();
            BloodStaticO1040.BloodStaticRegistry1040.SpawnByType(
                dict,
                2048, 2048,
                25, 150,
                25, 200,
                allEnemies,
                alldynO,
                allBreaks,
                allPhysics,
                allStatics


            );






        }



        public void Dispose()
        {
            waveCheckTimer?.Stop();
            waveCheckTimer?.Dispose();

        }


    }



}
