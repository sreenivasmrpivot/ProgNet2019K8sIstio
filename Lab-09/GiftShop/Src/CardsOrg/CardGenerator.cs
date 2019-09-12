using System;

namespace CardsOrg
{
    public class CardGenerator
    {
        /// <summary>
        /// Generates a Random Credit card number with the specified number of digits.
        /// </summary>
        /// <param name="cardNumLen"></param>  
        /// <param name="secCodeLen"></param>  
        public Card GenerateNew(int cardNumLen, int secCodeLen, string type, double amount)
        {
            var expDateTime = DateTime.Now.AddYears(1);
            var newCard = new Card() {
                Number = GetNumber(cardNumLen),
                ExpiryDate =  $"{expDateTime.ToString("MM")}/{expDateTime.ToString("yy")}" ,
                SecurityCode = GetNumber(secCodeLen),
                Type = type,
                Amount = amount
            };

            return newCard;
        }

        private string GetNumber(int noOfDigits)
        {
            string CreditCardNumber = null;
            int nums = noOfDigits;
            Random num = new Random();
            for (int i = 0; i < nums; i++)
            {
            CreditCardNumber = num.Next(9) + CreditCardNumber;
            }

            return CreditCardNumber;
        }
    }
}
