using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Realms.Sync;
using Realms;
using WeatherApp.Models;

namespace WeatherApp.ViewModels
{
    [QueryProperty(nameof(User), "Data")]
    public partial class HomeViewModel : BaseViewModel
    {
        private Realm realm;
        private FlexibleSyncConfiguration config;
        private readonly IGeolocation geolocation;
        private readonly IConnectivity connectivity;

        public HomeViewModel(IGeolocation geolocation, IConnectivity connectivity)
        {
            this.geolocation = geolocation;
            this.connectivity = connectivity;
            WeatherData = "NA";
        }

        [ObservableProperty]
        UserModel user;

        [ObservableProperty]
        string currentlocation;

        [ObservableProperty]
        string weatherData;

        [ObservableProperty]
        Location startlocation;

        [ObservableProperty]
        string locationInput;
        public async Task InitialiseRealm()
        {
            var cUser = App.RealmApp.CurrentUser;
            if (cUser == null)
            {
                await Application.Current.MainPage.DisplayAlert("Logged out", "User not logged in", "Close");
                return;
            }
            config = new FlexibleSyncConfiguration(cUser);
            realm = await Realm.GetInstanceAsync(config);

            realm.Subscriptions.Update(() =>
            {
                var currentUser = realm.All<UserModel>().Where(t => t.Email == User.Email);
                realm.Subscriptions.Add(currentUser);
            });

            await realm.Subscriptions.WaitForSynchronizationAsync();

        }

        public async Task AddUser()
        {
            await InitialiseRealm();

            try
            {
                var oldUser = realm.All<UserModel>().Where(t => t.Email == User.Email);
                if (!oldUser.Any())
                {
                    var newUser = new UserModel
                    {
                        Name = User.Name,
                        Email = User.Email,
                        Password = User.Password,
                    };
                    realm.Write(() =>
                    {
                        realm.Add(newUser);
                    });

                }

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Close");

            }

        }

        public async Task CheckLocation(Microsoft.Maui.Controls.Maps.Map map)
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Application.Current.MainPage.DisplayAlert("No Internet", "Oops! You seem to be offline", "Close");
                return;
            }

            var location = await geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(10),
                    RequestFullAccuracy = true,
                });
            }

            if (location == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to fetch location", "Close");
                return;
            }
            
            Startlocation = location;
            Currentlocation = await GetGeocodeReverseData(location.Latitude, location.Longitude);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Microsoft.Maui.Maps.Distance.FromMeters(100)));

            // Add pin for current location
            Pin currentPin = new()
            {
                Label = Currentlocation,
                Address = "Your location",
                Type = PinType.Generic,
                Location = location
            };
            map.Pins.Add(currentPin);
        }

        private async Task<string> GetGeocodeReverseData(double latitude, double longitude)
        {
            IEnumerable<Placemark> placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);

            Placemark place = placemarks?.FirstOrDefault();

            if (place != null)
            {
                return $"{place.FeatureName}, {place.SubLocality}, {place.Locality}, {place.AdminArea}, {place.CountryName} - {place.PostalCode}";

            }

            return "No data";
        }

        public async Task HandleMapClick(Microsoft.Maui.Controls.Maps.Map map, double latitude, double longitude)
        {
            Currentlocation = await GetGeocodeReverseData(latitude, longitude);
            Startlocation = new Location(latitude, longitude);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(map.VisibleRegion.Center,
                Microsoft.Maui.Maps.Distance.FromMeters(100)));

            map.Pins.Clear();
            map.MapElements.Clear();

            // Add pin for current location
            Pin currentPin = new()
            {
                Label = Currentlocation,
                Address = "Your result",
                Type = PinType.SearchResult,
                Location = Startlocation
            };
            map.Pins.Add(currentPin);
        }

        public async Task ShowWeatherInfo(Microsoft.Maui.Controls.Maps.Map map)
        {
            if (map == null || Currentlocation == "")
            {
                await Application.Current.MainPage.DisplayAlert("Location service failed!", "Oops! Failed to get location", "Close");
                return;
            }

        }
    }
}
