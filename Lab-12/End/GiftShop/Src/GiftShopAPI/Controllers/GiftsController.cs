using Microsoft.AspNetCore.Mvc;

using CardsOrg;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace GiftShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftsController : ControllerBase
    {
        private IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public GiftsController(
            IConfiguration config,
            IHttpClientFactory clientFactory)
        {
            _config = config;
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Returns Gift Options.
        /// </summary>
        [HttpGet("options")]
        public ActionResult<IEnumerable<string>> GetGiftOptions()
        {
            return new List<string>() { "GiftShop", "Tesco", "Walmart"};
        }

        /// <summary>
        /// Generates a new GiftShop Card.
        /// </summary>
        [HttpGet("giftshop/card")]
        public ActionResult<Card> GetGiftShopCard(double amount)
        {
            if(amount > _config.GetValue<double>("App:BusinessSettings:MaxCardAmount"))
                return UnprocessableEntity($"Gift card amount cannot exceed {_config.GetValue<double>("App:BusinessSettings:MaxCardAmount")}");

            return new CardGenerator().GenerateNew(16, 3, "GiftShop", amount);
        }

        /// <summary>
        /// Generates a new Tesco Card.
        /// </summary>
        [HttpGet("tesco/card")]
        public async Task<ActionResult<Card>> GetTescoCardAsync(double amount)
        {
            if(amount > _config.GetValue<double>("App:BusinessSettings:MaxCardAmount"))
                return UnprocessableEntity($"Gift card amount cannot exceed {_config.GetValue<double>("App:BusinessSettings:MaxCardAmount")}");

            Card card = null;

            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_config.GetValue<string>("TescoAPIBaseUrl")}/api/Gifts/card?amount={amount}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-API-KEY", _config.GetValue<string>("TescoAPIKey")); // Candidate for Secrets

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                card = await response.Content
                    .ReadAsAsync<Card>();
            }

            return card;
        }

        /// <summary>
        /// Generates a new Walmart Card.
        /// </summary>
        [HttpGet("walmart/card")]
        public async Task<ActionResult<Card>> GetWalmartCardAsync(double amount)
        {
            if(amount > _config.GetValue<double>("App:BusinessSettings:MaxCardAmount"))
                return UnprocessableEntity($"Gift card amount cannot exceed {_config.GetValue<double>("App:BusinessSettings:MaxCardAmount")}");

            Card card = null;

            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_config.GetValue<string>("WalmartAPIBaseUrl")}/api/Gifts/card?amount={amount}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-API-KEY", _config.GetValue<string>("WalmartAPIKey"));

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                card = await response.Content
                    .ReadAsAsync<Card>();
            }

            return card;
        }
    }
}
