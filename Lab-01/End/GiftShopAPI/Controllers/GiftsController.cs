using Microsoft.AspNetCore.Mvc;
using CardsOrg;
using Microsoft.Extensions.Configuration;

namespace GiftShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftsController : ControllerBase
    {
        private IConfiguration _config;

        public GiftsController(
            IConfiguration config)
        {
            _config = config;
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
    }
}
