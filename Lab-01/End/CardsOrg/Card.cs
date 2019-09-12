using System;

namespace CardsOrg
{
    public class Card
    {
        public string Number { get; set; }
        public string ExpiryDate { get; set; }
        public string SecurityCode { get; set; }
        public string Type { get; set; }
        public double Amount { get; set; }
    }
}
