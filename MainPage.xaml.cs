using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using uwpWeather.Classes;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media.Imaging;

namespace uwpWeather {

    public sealed partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();

            RunFullScreen();
        }

        private void RunFullScreen() {
           ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }

        private async void getDataAsync(object sender, RoutedEventArgs e) {

            dataWait.Visibility = Visibility.Visible;
            weatherRing.IsActive = true;

            var position = await LocationManager.GetPosition();

            var lat = position.Coordinate.Point.Position.Latitude;
            var lon = position.Coordinate.Point.Position.Longitude;

            CallDataAsync(lat, lon);
        }

        private async void CallDataAsync(double lat, double lon) {

            const string BASEURL = "http://api.openweathermap.org/data/2.5/weather?";
            const string APIKEY = "d3b450256f7cf6125f8f26eef24a5966";

            // api construction. I am using string interpolation
            var url = $"{BASEURL}lat={lat}&lon={lon}&appid={APIKEY}&units=metric";

            /* 1) Grab data from the URL and deserialize the object.
             * 2) We provide custom icons
             * 3) Becouse we are not sure if we are going to get an expection, we wrap everything in a try-catch block
             * 4) Is a good practice, to let the user know that something is hapenning on the background, So we have a progress ring, 
             * that it will disapear when we returned wih data, and draw the UI
             * 5) if we get an exception, we throw the exception 
             */

            try {
                var client = new HttpClient();
                var response = await client.GetStringAsync(new Uri(url));
                var parseObj = JsonConvert.DeserializeObject<dynamic>(response);

                // I do not like these icons, so I am going to use my own 
                var imgObj = string.Format("ms-appx:///Assets/Weather/{0}.png", parseObj.weather[0].icon);

                DrawComponents(parseObj, imgObj, lat, lon);

            } catch (Exception ex) {

                throw ex;
            }
        }

        
        //Custom method to draw the UI

        private void DrawComponents(dynamic parseObj, string imgObj, Double lat, Double lon) {

            imgIcon.Source = new BitmapImage(new Uri(imgObj, UriKind.Absolute));
            userLocale.Text = $"{parseObj.name}, {parseObj.sys.country}";
            userLocaleTemp.Text = $"{(int)parseObj.main.temp}°C";
            userMinTemp.Text = $"Low: {(int)parseObj.main.temp_min}°C";
            userMaxTemp.Text = $"High:  {(int)parseObj.main.temp_max}°C";
            userLocatioHumidity.Text = $"humidity: {(int)parseObj.main.humidity}%";
            weatherIconDesc.Text = $"{parseObj.weather[0].description}";

             var MyLandmarks = new List<MapElement>();

            BasicGeoposition geoposition = new BasicGeoposition {
                Latitude = lat,
                Longitude = lon
            };


            Geopoint GeoPoint = new Geopoint(geoposition);

            var spaceNeedleIcon = new MapIcon {
                Location = GeoPoint,
                ZIndex = 0,
                Title = "You are here"
            };

            MyLandmarks.Add(spaceNeedleIcon);

            var LandmarksLayer = new MapElementsLayer {
                ZIndex = 1,
                MapElements = MyLandmarks
            };

            userMapLocale.Layers.Add(LandmarksLayer);

            userMapLocale.Center = GeoPoint;
            userMapLocale.ZoomLevel = 20;

            userMapLocale.Visibility = Visibility.Visible;
            weatherRing.IsActive = false;
            dataWait.Visibility = Visibility.Collapsed;
        }
    }
}
