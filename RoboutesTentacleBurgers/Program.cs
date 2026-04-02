

namespace RoboutesTentacleBurgers
{
    internal class Program
    {
        static void Main(string[] args)
        {


            // Environment

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddSingleton(sp => builder.HostEnvironment.Environment);

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddDebug();
                                             

            // SpectralXBXB Class Library
            builder.Services.AddScoped<SpectralXBXB.GamepadService2>();

            //change scoped to robutes
            // SpectralX GL Class Library
            builder.Services.AddScoped<GamepadService>();
            builder.Services.AddScoped<SpectralXGLX.Services.GamepadService>();


            // WarCraft Class Library

            builder.Services.AddSingleton<WarCraftLibrary.WarGameService>();

            // This is being used by SpectralXGLX and BWP
            var perfMonitor = new PerformanceMonitor();
            

            // Game Services

            var Wyrm = new BloodWyrmProtocol();
            var game = new BloodWyrmService();

            var character = new BloodWarrior();
            var mage = new BloodMage();
            var rogue = new BloodRogue();
            var monk = new BloodMonk();
            var characterHandle = new BloodCharacterHandle();

            builder.Services.AddSingleton<SplatterPuddle>();
            var levelReg = new BloodLevel();

            var enemy = new BloodEnemy();
            var skelPyscho = new BloodSkelPyscho();
            var zombie = new BloodZombiePyscho();
            var skelWar = new BloodSkelWar();
            var goatman = new BloodGoatMan();
            var boss = new BloodBoss();
            var cow = new BloodCow();
            var townSlut = new BloodTownSlut();
            var enemyHandle = new BloodDynOHandle();
            var enemyBoss = new BloodEnemyBoss();

            var staticobjectHandle = new BloodStaticHandle();
            var staticobject = new BloodStaticObject();


            var staticObjectT = new BloodStaticObjectT();
            var staticObjectC = new BloodStaticObjectC();
            var staticObjectS = new BloodStaticObjectS();
            var staticObjectGY = new BloodStaticObjectGY();
            var staticO1010 = new BloodStaticO1010();
            var staticO1020 = new BloodStaticO1020();
            var staticO1030 = new BloodStaticO1030();
            var staticO1040 = new BloodStaticO1040();
            var staticO1050 = new BloodStaticO1050();
            var staticO1060 = new BloodStaticO1060();


            var waveReg = new BloodWaveReg();
            var waveReg1010 = new BloodWaveReg1010();
            var waveReg1020 = new BloodWaveReg1020();
            var waveReg1030 = new BloodWaveReg1030();
            var waveReg1040 = new BloodWaveReg1040();
            var waveReg1050 = new BloodWaveReg1050();
            var waveReg1060 = new BloodWaveReg1060();
            var waveRegGY = new BloodWaveRegGY();
            var waveRegC = new BloodWaveRegC();



            var dynamicobj = new BloodDynamicObj();
            var dynamicobjHandle = new BloodDynOHandle();
            var breakable = new BloodBreakables();
            var breakableHandle = new BloodBreakHandle();
            var physics = new BloodPhysics();
            var physicsHandle = new BloodPhysicsHandle();

            var lifeCycles = new BloodLifeCycles();
            var lifeCyclesS = new BloodLifeCyclesS();


            var lightC = new DirectionalLightCycle();
            var lightCave = new DLightCycleC();
            var lightHandle = new BloodLightHandle();





            levelReg.Game = game;

            builder.Services.AddSingleton(Wyrm);
            builder.Services.AddSingleton(game);
            builder.Services.AddSingleton(perfMonitor);

            builder.Services.AddSingleton(character);
            builder.Services.AddSingleton(mage);
            builder.Services.AddSingleton(rogue);
            builder.Services.AddSingleton(monk);
            builder.Services.AddSingleton(levelReg);
            builder.Services.AddSingleton(characterHandle);

            builder.Services.AddSingleton(enemy);
            builder.Services.AddSingleton(zombie);
            builder.Services.AddSingleton(skelPyscho);
            builder.Services.AddSingleton(skelWar);
            builder.Services.AddSingleton(goatman);
            builder.Services.AddSingleton(boss);
            builder.Services.AddSingleton(cow);
            builder.Services.AddSingleton(townSlut);
            builder.Services.AddSingleton(enemyBoss);
            builder.Services.AddSingleton(enemyHandle);

            builder.Services.AddSingleton(staticobjectHandle);
            builder.Services.AddSingleton(staticobject);
            builder.Services.AddSingleton(staticObjectT);
            builder.Services.AddSingleton(staticObjectC);
            builder.Services.AddSingleton(staticObjectS);
            builder.Services.AddSingleton(staticObjectGY);
            builder.Services.AddSingleton(staticO1010);
            builder.Services.AddSingleton(staticO1020);
            builder.Services.AddSingleton(staticO1030);
            builder.Services.AddSingleton(staticO1040);
            builder.Services.AddSingleton(staticO1050);
            builder.Services.AddSingleton(staticO1060);

            builder.Services.AddSingleton(waveReg);
            builder.Services.AddSingleton(waveReg1010);
            builder.Services.AddSingleton(waveReg1020);
            builder.Services.AddSingleton(waveReg1030);
            builder.Services.AddSingleton(waveReg1040);
            builder.Services.AddSingleton(waveReg1050);
            builder.Services.AddSingleton(waveReg1060);
            builder.Services.AddSingleton(waveRegGY);
            builder.Services.AddSingleton(waveRegC);

            builder.Services.AddSingleton(dynamicobj);
            builder.Services.AddSingleton(dynamicobjHandle);
            builder.Services.AddSingleton(breakable);
            builder.Services.AddSingleton(breakableHandle);
            builder.Services.AddSingleton(physics);
            builder.Services.AddSingleton(physicsHandle);

            builder.Services.AddSingleton(lifeCycles);
            builder.Services.AddSingleton(lifeCyclesS);





            builder.Services.AddSingleton(lightC);
            builder.Services.AddSingleton(lightCave);
            builder.Services.AddSingleton(lightHandle);


            // Root Components
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // HTTP Client
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Auth (Optional)
            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Local", options.ProviderOptions);
            });

            builder.Build().RunAsync();


        }
    }
}
