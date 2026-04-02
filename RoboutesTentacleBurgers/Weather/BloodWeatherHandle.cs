using RoboutesTentacleBurgers.Lighting;

namespace RoboutesTentacleBurgers.Weather
{


    public enum WeatherType
    {
        Forest,
        Snow,
        // Add more as needed
    }

    public class BloodWeatherHandle
    {
        public IWeather? Weather { get; set; }



            public BloodWeatherHandle(WeatherType type = WeatherType.Forest)
            {
                Weather = type switch
                {
                    WeatherType.Forest => new BloodLifeCycles
                    {
                        IsFogEnabled = true,
                        FogSpeedX = 0.4f,
                        FogSpeedY = 0.4f
                    },


                    
                    WeatherType.Snow => new BloodLifeCyclesS
                  {
                       IsFogEnabled = true,
                      FogSpeedX = 0.4f,
                        FogSpeedY = 0.4f
                   },
                    

                    _ => new BloodLifeCycles
                    {
                        IsFogEnabled = true,
                        FogSpeedX = 0.4f,
                        FogSpeedY = 0.4f
                    }
                };
            }

        

    }
}