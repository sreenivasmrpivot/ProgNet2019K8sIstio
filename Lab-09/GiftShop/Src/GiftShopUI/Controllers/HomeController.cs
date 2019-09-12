using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GiftShopUI.Models;

using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace GiftShopUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private IConfiguration _config;

        public HomeController(IHttpClientFactory clientFactory,
            IConfiguration config)
        {
            _config = config;
            _clientFactory = clientFactory;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Gifts(Order model)
        {
            if(!(model.GiftOptions == 0) && model.Amount > 0)
            {
                return RedirectToAction("Order", model);
            }
            else 
            {
                ViewBag.Result = "Select Gift Card Type and Amount.";
                return View(model);
            }
        }

        public async Task<IActionResult> Order(Order model)
        {
            Card card = null;

            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_config.GetValue<string>("ExternalDependencies_GiftShopAPI_BaseUrl")}api/Gifts/{model.GiftOptions.ToString().ToLower()}/card?amount={model.Amount}");
            request.Headers.Add("Accept", "application/json");

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                card = await response.Content
                    .ReadAsAsync<Card>();

                model.OrderResponse = card;
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
