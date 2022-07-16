using yakutsa.Services.RussianPost.Models;

namespace yakutsa.Services.RussianPost
{
    public class ApiClient
    {
        const string authorizationAccessToken = "AccessToken hQGgYArpd2akH1Y2XdaGG9IaZBlotbIG";
        const string xUserAuthorizationToken = "Basic bWF4a3V0c0B5YW5kZXgucnU6UXdlcnR5MTIz";
        const string baseUrl = "https://otpravka-api.pochta.ru";
        HttpClient _client;

        public ApiClient()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(baseUrl);
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationAccessToken);
            _client.DefaultRequestHeaders.Add("X-User-Authorization", xUserAuthorizationToken);
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Services.RussianPost.Models.AddressNormalizationResponse?> NormalizeAdress(string[] addresses)
        {

            var data = new List<Services.RussianPost.Models.AddressNormalizationRequest>();
            foreach (string address in addresses)
            {
                var addressRequest = new Services.RussianPost.Models.AddressNormalizationRequest { originaladdress = address, id = Guid.NewGuid().ToString() };
                data.Add(addressRequest);
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var jsonData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(baseUrl + "/1.0/clean/address", jsonData);
            var responseContent = await response.Content.ReadAsStringAsync();
            AddressNormalizationResponse[]? addressNormalizationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AddressNormalizationResponse[]>(responseContent);



            return addressNormalizationResponse?.FirstOrDefault();
        }

        public void Calculate(AddressNormalizationResponse normalizedAddress, yakutsa.Models.Cart cart)
        {
            Calculate(normalizedAddress.index, cart);
        }

        public async Task<DeliveryCostResponse?> Calculate(string index, yakutsa.Models.Cart cart)
        {
            var requestData = new DeliveryCostRequest();
            requestData.indexfrom = "119334";
            requestData.indexto = index;
            requestData.maildirect = 643;
            requestData.mailtype = "ONLINE_PARCEL";
            requestData.mailcategory = "ORDINARY";
            requestData.entriestype = "SALE_OF_GOODS";
            requestData.mass = (int)cart.Weight;
            requestData.withsimplenotice = true;


            int width = 0;
            int height = 0;
            int length = 0;
            int weight = 0;

            cart.CartProducts.ForEach(cp =>
            {
                weight += cp.Offer.weight;
                width += width == 0 ? cp.Offer.width : width;
                length += length == 0 ? cp.Offer.length : length;
                height += cp.Offer.height;
            });

            requestData.dimension = new Dimension { height = height, width = width, length = length };


            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var jsonData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(baseUrl + "/1.0/tariff", jsonData);
            var responseContent = await response.Content.ReadAsStringAsync();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<DeliveryCostResponse>(responseContent);

            return data;
        }
    }
}