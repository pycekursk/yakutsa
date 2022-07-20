using RetailCRMCore.Models;

using yakutsa.Services.GeoHelper.Models;

namespace yakutsa.Services.GeoHelper
{
    public class ApiClient
    {
        HttpClient _httpClient;
        string _apiKey = string.Empty;

        public ApiClient(string? apiKey = null)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://geohelper.info/api/v1/") };
            _apiKey = apiKey ??= "kVlzyaLyTz6PwyUwNpBjZue210rN1Fv7";
        }

        public async Task<Address> GetVerifiedAddress(SuggestionAddress? suggestionAddress)
        {
            Address result = new();
            result.region = $"{suggestionAddress.Region} {suggestionAddress.RegionTypeFull}";
            result.city = $"{suggestionAddress.City}";

            var regions = await GetRegions("name", $"{result.region}");
            result.regionId = regions?.result?.FirstOrDefault()?.id;
            var cities = await GetCities("name", result.city, result.regionId);
            result.cityId = cities?.result?.FirstOrDefault()?.id;
            result.cityType = suggestionAddress.CityType;
            result.streetType = suggestionAddress.StreetType;
            result.street = suggestionAddress.Street;

            var streets = await GetStreets("name", $"{suggestionAddress.Street}", result.cityId);

            result.streetId = streets?.result?.FirstOrDefault()?.id;
            result.index = streets?.result?.FirstOrDefault()?.postCode;
            result.building = suggestionAddress?.House;
            result.flat = suggestionAddress?.Flat;
            result.countryIso = suggestionAddress?.CountryIsoCode;

            return result;
        }

        async Task<Response<Region>?> GetRegions(string filterName, string filterValue)
        {
            Response<Region>? result = null;
            var response = await _httpClient.GetAsync($"http://geohelper.info/api/v1/regions?apiKey={_apiKey}&locale[lang]=ru&filter[{filterName}]={filterValue}");
            var responseJson = await response.Content.ReadAsStringAsync();
            result = Newtonsoft.Json.JsonConvert.DeserializeObject<Response<Region>?>(responseJson);
            return result;
        }

        async Task<Response<City>?> GetCities(string filterName, string filterValue, int? regionId)
        {
            Response<City>? result = null;
            var response = await _httpClient.GetAsync($"http://geohelper.info/api/v1/cities?apiKey={_apiKey}&locale[lang]=ru&filter[{filterName}]={filterValue}&filter[regionId]={regionId}");
            var responseJson = await response.Content.ReadAsStringAsync();
            result = Newtonsoft.Json.JsonConvert.DeserializeObject<Response<City>?>(responseJson);
            return result;
        }

        async Task<Response<Street>?> GetStreets(string filterName, string filterValue, int? cityId)
        {
            Response<Street>? result = null;
            string url = $"http://geohelper.info/api/v1/streets?apiKey={_apiKey}&locale[lang]=ru&filter[{filterName}]={filterValue}&filter[cityId]={cityId}";
            Console.WriteLine($"\n{url}\n");
            var response = await _httpClient.GetAsync(url);
            var responseJson = await response.Content.ReadAsStringAsync();
            result = Newtonsoft.Json.JsonConvert.DeserializeObject<Response<Street>?>(responseJson);
            return result;
        }

        //http://geohelper.info/api/v1/cities?apiKey=kVlzyaLyTz6PwyUwNpBjZue210rN1Fv7&locale%5Blang%5D=ru&filter[name]=%D0%B3%D0%BE%D1%80%D0%BE%D0%B4%20%D0%BA%D1%83%D1%80%D1%81%D0%BA&filter[regionId]=27
    }

}
