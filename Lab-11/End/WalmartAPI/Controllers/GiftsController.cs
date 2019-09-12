using Microsoft.AspNetCore.Mvc;

using CardsOrg;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace WalmartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftsController : ControllerBase
    {
        /// <summary>
        /// Generates a new Walmart Card.
        /// </summary>
        [HttpGet("card")]
        public ActionResult<Card> GetCard(double amount)
        {
            if (!Request.Headers.Contains(new KeyValuePair<string, StringValues>("X-API-KEY", "GiftShopClient")))
            {
                return BadRequest("You must supply an API key header called X-API-KEY");
            }
            return new CardGenerator().GenerateNew(16, 3, "Walmart", amount);
        }
    }
}
