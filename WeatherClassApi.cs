using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace WeatherApiClass
{
    public class WeatherApi
    {
        public static string api_token = "api_token";
        public string inputCityName;
        public static string nameCity;
        internal static float temperatureCity;
        internal static string weatherCity;
        internal static double tempFeelsLikeCity;
        internal static string WindAnswer;
        internal static string answer;
        internal static float visibilityCity;
        internal static int pressureCity;
        internal static string countryCity;
        internal static int humidityCity;
        internal static double windSpeedCity;
        internal static int windDegCity;
        internal static double lonCity;
        internal static double latCity;

        internal static bool ResponseIsNormal;

        internal static int aqi;
        internal static double co;
        internal static double no;
        internal static double no2;
        internal static double o3;
        internal static double so2;
        internal static double pm2_5;
        internal static double pm10;
        internal static double nh3;

        public static void Weather(string city_name)
        {
            try
            {
                string url = "https://api.openweathermap.org/data/2.5/weather?q=" + city_name + "&appid=" + api_token;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest?.GetResponse();
                string WeathResponse;

                using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
                {
                    WeathResponse = streamReader.ReadToEnd();
                }
                
                WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(WeathResponse);

                nameCity = weatherResponse.Name;
                temperatureCity = weatherResponse.Main.Temp - 273;
                tempFeelsLikeCity = weatherResponse.Main.feels_like - 273;
                weatherCity = weatherResponse.weather[0].main;
                visibilityCity = weatherResponse.Visibility / 1000;
                pressureCity = weatherResponse.Main.Pressure;
                countryCity = weatherResponse.sys.Country;
                humidityCity = weatherResponse.Main.Humidity;
                windSpeedCity = weatherResponse.wind.speed;
                windDegCity = weatherResponse.wind.deg;
                lonCity = weatherResponse.coord.lon;
                latCity = weatherResponse.coord.lat;
                GetAirPollution(lonCity, latCity);
                ResponseIsNormal = true;
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Error to connect with home.openweathermap.org or request if failed!");
                ResponseIsNormal = false;
                return;
            }
        }

        private static void GetAirPollution(double lon, double lat)
        {
            try
            {
                var Lon = Convert.ToInt32(Math.Round(lon));
                var Lat = Convert.ToInt32(Math.Round(lat));

                string url = $"http://api.openweathermap.org/data/2.5/air_pollution?lat={Lat}&lon={Lon}&appid={api_token}";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest?.GetResponse();

                string AirPollutionResult;

                using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
                {
                    AirPollutionResult = streamReader.ReadToEnd();
                }

                AirPollution airPollution = JsonConvert.DeserializeObject<AirPollution>(AirPollutionResult);
                aqi = airPollution.List[0].Main.aqi;
                co = airPollution.List[0].Components.co;
                no = airPollution.List[0].Components.no;
                no2 = airPollution.List[0].Components.no2;
                o3 = airPollution.List[0].Components.o3;
                so2 = airPollution.List[0].Components.so2;
                pm2_5 = airPollution.List[0].Components.pm2_5;
                pm10 = airPollution.List[0].Components.pm10;
                nh3 = airPollution.List[0].Components.nh3;
                ResponseIsNormal = true;
            }
            catch (WebException)
            {
                Console.WriteLine("Error to connect with home.openweathermap.org or request is failed!");
                ResponseIsNormal = false;
                return;
            }
        }

        public static void Celsius(float celsius)
        {
            if (celsius == 0)
            {
                answer = "Уххх, какой холод у вас тут, придётся одеться потеплее...";
            }
            else if (celsius >= -14 & celsius < -4)
            {
                answer = "Ох ты ж блин! Сегодня бы не стать сосулкой, лучше одеться очень тепло...";
            }
            else if (celsius <= 10 & celsius < 18)
            {
                answer = "Сегодня прохладненько, лучше надеть куртку и толстовку, чтобы не замёрзнуть!";
            }
            else if (celsius <= 18 & celsius < 20)
            {
                answer = "Сегодня температура вполне приемлемая! Ветровки хватит на сегодня";
            }
            else if (celsius >= 20 & celsius < 25)
            {
                answer = "Отличная погодка! Не так ли?!";
            }
            else if (celsius > 25)
            {
                answer = "Сегодня жарко, придётся легко одеться, чтобы не зажариться!";
            }


        }

        public static void WindCourse(int WindDeg)
        {
            if (WindDeg == 0 | WindDeg < 45)
            {
                WindAnswer = "С";
            }
            else if (WindDeg >= 45 & WindDeg < 90)
            {
                WindAnswer = "С-В";
            }
            else if (WindDeg >= 90 & WindDeg < 135)
            {
                WindAnswer = "В";
            }
            else if (WindDeg >= 135 & WindDeg < 180)
            {
                WindAnswer = "Ю-В";
            }
            else if (WindDeg >= 180 & WindDeg < 225)
            {
                WindAnswer = "Ю";
            }
            else if (WindDeg >= 225 & WindDeg < 270)
            {
                WindAnswer = "Ю-З";
            }
            else if (WindDeg >= 270 & WindDeg < 315)
            {
                WindAnswer = "З";
            }
            else if (WindDeg >= 315 & WindDeg < 360)
            {
                WindAnswer = "С-З";
            }
            else if (WindDeg == 360)
            {
                WindAnswer = "С";
            }
        }
    }

    public class AirPollution
    {
        public Coord Coord;

        public List[] List;
    }

    public class List
    {
        public long dt;

        public Main Main;

        public Components Components;
    }

    public class Main
    {
        public int aqi;
    }

    public class Components
    {
        public double co;

        public double no;

        public double no2;

        public double o3;

        public double so2;

        public double pm2_5;

        public double pm10;

        public double nh3;
    }

    public class WeatherResponse
    {

        public Coord coord;

        public WeatherInfo Main { get; set; }

        public string Name { get; set; }

        public Weather[] weather { get; set; }

        public float Visibility { get; set; }
        
        public Sys sys { get; set; }

        public Wind wind { get; set; }
        
    }

    public class Weather
    {
        public string main { get; set; }

        public string Description { get; set; }
    }

    public class WeatherInfo
    {
        public float Temp { get; set; }

        public double feels_like { get; set; }

        public int Humidity { get; set; }

        public int Pressure { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public string Country { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }

        public int deg { get; set; }
    }

    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }
}

