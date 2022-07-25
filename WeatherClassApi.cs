using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace WeatherApiClass
{
    public class WeatherApi
    {
        public static string api_token = "token_from_openweather";
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
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Error to connect with home.openweathermap.org or request if failed!");
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
