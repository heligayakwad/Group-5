using Microsoft.Maui.Controls.Maps;
using WeatherApp.Models;
using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class HomeView : ContentPage
{
    HomeViewModel vm;
    RestService _restService;
    public HomeView(HomeViewModel homeViewModel)
    {
        InitializeComponent();
        _restService = new RestService();
        vm = homeViewModel;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        await vm.AddUser();

        // Initial loading based on location
        WeatherData weatherData = await
                _restService.
                GetWeatherData(GenerateRequestURL(Constants.OpenWeatherMapEndpoint));

        BindingContext = weatherData;

    }

    protected override bool OnBackButtonPressed()
    {
        Task<bool> answer = DisplayAlert("Exit", "Do you want to quit?", "Yes", "No");
        answer.ContinueWith(task =>
        {
            if (task.Result)
            {
                Application.Current.Quit();
            }
        });
        return true;
    }

    async void OnGetWeatherButtonClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_cityEntry.Text))
        {
            WeatherData weatherData = await
                _restService.
                GetWeatherData(GenerateRequestURL(Constants.OpenWeatherMapEndpoint));

            BindingContext = weatherData;
        }
    }

    string GenerateRequestURL(string endPoint)
    {
        string requestUri = endPoint;
        requestUri += $"?q={_cityEntry.Text}";
        requestUri += "&units=imperial";
        requestUri += $"&APPID={Constants.OpenWeatherMapAPIKey}";
        return requestUri;
    }
}