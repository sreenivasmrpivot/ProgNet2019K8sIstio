using System.ComponentModel.DataAnnotations;

namespace GiftShopUI.Models
{
    public class Order  
    {  
        [Display( Name = "CardType" )]  
        public GiftOptions GiftOptions { get; set; }  

        [Display( Name = "Amount" )]  
        public double Amount { get; set; } 
    
        [Display( Name = "OrderResponse" )]  
        public Card OrderResponse { get; set; }  
    }      
}