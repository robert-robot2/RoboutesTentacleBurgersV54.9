

namespace RoboutesTentacleBurgers.Pages
{
    public partial class BloodWyrmProtocol : ComponentBase, IDisposable
    {

        public BloodLightHandle LightHandle { get; set; } = new();
        public ILighting? Lighting => LightHandle.Lighting;

        public BloodWeatherHandle WeatherHandle { get; set; } = new();
        public IWeather? Weather => WeatherHandle.Weather;

        public BloodStaticHandle StaticHandle { get; set; } = new();
        public IStatic? ActiveStatic => StaticHandle.ActiveStatic;


        // Just proxy to Game's handle
        public BloodCharacterHandle CharacterHandle => Game.CharacterHandle;
        public IBloodiCharacter? ActiveCharacter => Game.CharacterHandle.ActiveCharacter;

        public BloodEnemyHandle EnemyHandle { get; set; } = new();
        public IiEnemy? ActiveEnemy => EnemyHandle.ActiveEnemy;
        //  public IiEnemy? ActiveEnemy { get; set; } = default!;
        public BloodDynOHandle DynOHandle { get; set; } = new();
        public IDynamicO? ActiveDynO => DynOHandle.ActiveDynO;
        public BloodBreakHandle BreakHandle { get; set; } = new();
        public IBreakables? Breakables => BreakHandle.Breakables;
        public BloodPhysicsHandle PhysicsHandle { get; set; } = new();
        public IPhysics? Physics => PhysicsHandle.Physics;

        private System.Timers.Timer? waveCheckTimer;

        [Inject] public BloodLevel LevelReg { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        public BloodInput Input { get; set; } = default!;

        public void HandleCharacterSelected(string sectChar)
        {
            switch (sectChar)
            {
                case "Warrior": CharacterHandle.SpawnWarrior(); break;
                case "Mage": CharacterHandle.SpawnMage(); break;
                case "Rogue": CharacterHandle.SpawnRogue(); break;
                case "Monk": CharacterHandle.SpawnMonk(); break;
                default: CharacterHandle.SpawnWarrior(); break;
            }
            //  CharacterHandle.HasSpawned = true;
            //  Game.CharacterHandle = CharacterHandle;

            // NOW start the tick since character exists
            Game.InitTickTimer();
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            // Set up UI refresh callback FIRST
            Game.RequestUIRefresh = () => InvokeAsync(StateHasChanged);
            Game.LevelReg = LevelReg;
            // Clear world state
            Game.ClearAll();
            Spawn();
            // Load first wave
            WaveReg.LoadWave(1);
            // Wave timer
            waveCheckTimer = new System.Timers.Timer(16);
            waveCheckTimer.Elapsed += (_, __) => InvokeAsync(() => WaveReg.TryAdvanceWave());
            waveCheckTimer.AutoReset = true;
            waveCheckTimer.Start();
            // Reuse existing character handle
            //    CharacterHandle = Game.CharacterHandle ?? new BloodCharacterHandle();
            //   CharacterHandle.HasSpawned = true;

            EnemyHandle = new BloodEnemyHandle();
            EnemyHandle.UpdateActiveEnemy();
            Game.EnemyHandle = EnemyHandle;

            LightHandle = new BloodLightHandle(LightingType.Directional);
            Game.LightHandle = LightHandle;

            WeatherHandle = new BloodWeatherHandle(WeatherType.Forest);
            Game.WeatherHandle = WeatherHandle;
            StaticHandle = new BloodStaticHandle();
            Game.StaticHandle = StaticHandle;


            // Create and register input system
            // Create Input here so it's not null during render
            // but do NOT call Register() yet — JS isn't ready
            Input = new BloodInput(Game, JS);
            Game.Input = Input;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // NOW the DOM exists, JS interop is safe
                await Input.Register();
            }
        }


        public void Spawn()
        {

            BloodStaticObject.CaveRegistry.SpawnCaves(1);


            HandleObjSpawn();


            // Spawn NPC entities
            BloodPhysics.BloodUndeadGFRegistry.SpawnUndeadGF(15);
            BloodCow.BloodCowRegistry.SpawnCows(10);
            BloodTownSlut.BloodTownSlutRegistry.SpawnTownSluts(3);
            BloodCat.BloodSkeletonRegistry.SpawnSkeletons(10);

            BloodDynamicObj.BloodCampFireRegistry.SpawnCampFire(2);
            BloodBreakables.DummyRegistry.SpawnDummys(7);


            BloodDynamicObj.BloodHealPotRegistry.SpawnHealPots(10);
            BloodDynamicObj.BloodMedHealPotRegistry.SpawnMedHealPots(15);
            BloodDynamicObj.BloodManaPotRegistry.SpawnManaPots(10);
            BloodDynamicObj.BloodStrPotRegistry.SpawnStrPots(10);
            BloodDynamicObj.BloodCelPotRegistry.SpawnCelPots(10);
            BloodDynamicObj.BloodAlcPotRegistry.SpawnAlcPots(10);
            BloodDynamicObj.BloodIntPotRegistry.SpawnIntPots(10);

        }

        public void HandleObjSpawn()
        {
            var allEnemies = new List<IiEnemy>();
            var alldynO = new List<IDynamicO>();
            var allBreaks = new List<IBreakables>();
            var allPhysics = new List<IPhysics>();
            var allStatics = new List<IStatic>();

            allStatics.AddRange(BloodStaticObject.CaveRegistry.All);

            allEnemies.AddRange(BloodEnemy.BloodSkeletonRegistry.All);
            allEnemies.AddRange(BloodZombiePyscho.BloodZombiePyschoRegistry.All);
            allEnemies.AddRange(BloodSkelPyscho.BloodSkelPyschoRegistry.All);
            allEnemies.AddRange(BloodSkelWar.BloodSkeletonWarRegistry.All);
            allEnemies.AddRange(BloodGoatMan.BloodGoatmanRegistry.All);
            allEnemies.AddRange(BloodBoss.BloodScavBossRegistry.All);
            allEnemies.AddRange(BloodCow.BloodCowRegistry.All);
            allEnemies.AddRange(BloodTownSlut.BloodTownSlutRegistry.All);

            var dict = BloodStaticObject.BloodStaticList.ForestDictionary();
            BloodStaticObject.BloodStaticRegistry.SpawnByType(
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






    }
}