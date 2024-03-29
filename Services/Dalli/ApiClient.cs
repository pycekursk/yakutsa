﻿namespace yakutsa.Services.Dalli
{
    using RetailCRMCore.Models;
    using RetailCRMCore.Models.Base;

    using System.Net.Http.Headers;
    using System.Text;
    using System.Xml.Serialization;

    using yakutsa.Models;
    using yakutsa.Services.Dalli.Models;

    public class ApiClient : IDeliveryModule
    {
        async Task<string> GetDeliveryTypes()
        {
            string result = String.Empty;
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            var someXmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><services><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth></services>";
            var stringContent = new StringContent(someXmlString, Encoding.UTF8, "application/xml");
            var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
            result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public List<DeliveryCost> Calculate(Address address, Cart cart, bool cashservices = false, List<string> partners = null)
        {
            partners ??= new List<string> { "BOXBERRY", "SDEK", "DS", "5POST", "PickPoint" };

            //partners ??= new List<string> { "BOXBERRY", "SDEK", "RUPOST" };

            List<Task> requestTasks = new List<Task>();
            XmlSerializer serializer = new XmlSerializer(typeof(DeliveryCost));
            List<DeliveryCost> pVZPoints = new List<DeliveryCost>();

            int width = 0;
            int height = 0;
            int length = 0;

            double weight = 0;
            decimal price = 0;

            string cashservicesString = cashservices ? $"<price>{cart.Price}</price><inshprice>{cart.Price}</inshprice><cashservices>YES</cashservices>" : "";

            cart.CartProducts.ForEach(cp =>
            {
                weight += cp.Offer.weight / (float)1000;
                width += width == 0 ? cp.Offer.width : width;
                length += length == 0 ? cp.Offer.length : length;
                height += cp.Offer.height;
            });

            partners.ForEach((p) =>
            {
                requestTasks.Add(Task.Run(async () =>
                      {
                          HttpClient httpClient = new HttpClient();
                          httpClient.DefaultRequestHeaders
                            .Accept
                            .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                          try
                          {
                              string priceString = weight.ToString().Replace(',', '.');

                              string requestString = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><deliverycost><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth><partner>{p}</partner><townto>{address.city}</townto><oblname>{address.region}</oblname>{cashservicesString}<weight>{priceString}</weight><length>{length}</length><width>{width}</width><height>{height}</height><output>x2</output></deliverycost>";
                              var stringContent = new StringContent(requestString, Encoding.UTF8, "application/xml");
                              var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
                              var xmlString = await response.Content.ReadAsStringAsync();

                              Console.WriteLine(requestString);
                              Console.WriteLine(xmlString);

                              using (StringReader reader = new StringReader(xmlString))
                              {
                                  var cost = (DeliveryCost)serializer.Deserialize(reader);
                                  var pvzType = cost?.Price?.FirstOrDefault(prc => prc.ServiceType.ToString().Split("_")[0] == "PVZ");
                                  if (pvzType != null)
                                  {
                                      var points = GetPVZList(address, cost?.Partner);
                                      pvzType.PVZList = points;
                                      pvzType.IsPVZType = true;
                                  }

                                  cost.Code = "dalli-servicedalli-v2";
                                  if (cost.Partner != null)
                                      pVZPoints.Add(cost);
                              }
                          }
                          catch (Exception exc)
                          {
                              Console.WriteLine(exc.Message);
                          }
                      }));
            });

            Task.WaitAll(requestTasks.ToArray());

            return pVZPoints;
        }

        public Pvzlist GetPVZList(Address address, string? partner)
        {
            List<Task> requestTasks = new List<Task>();
            XmlSerializer serializer = new XmlSerializer(typeof(Pvzlist));
            Pvzlist? pVZPoints = new Pvzlist();

            requestTasks.Add(Task.Run(async () =>
              {
                  HttpClient httpClient = new HttpClient();
                  httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                  try
                  {
                      string requestString = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><pvzlist><auth token=\"cd94b1b2049e7e35db64ec756bc49073\"></auth><town>{address.city}</town><partner>{partner}</partner></pvzlist>";
                      var stringContent = new StringContent(requestString, Encoding.UTF8, "application/xml");
                      var response = await httpClient.PostAsync("https://api.dalli-service.com/v1/", stringContent);
                      var xmlString = await response.Content.ReadAsStringAsync();
                      using (StringReader reader = new StringReader(xmlString))
                      {
                          pVZPoints = (Pvzlist)serializer.Deserialize(reader);
                      }
                  }
                  catch (Exception exc)
                  {
                      Console.WriteLine(exc.Message);
                      //throw exc;
                  }
              }));
            Task.WaitAll(requestTasks.ToArray());
            return pVZPoints;
        }
        public string Code { get; set; }
    }
}
