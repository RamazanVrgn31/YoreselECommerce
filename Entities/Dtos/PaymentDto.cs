using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Dtos
{
    public class PaymentDto :IDto
    {
        public string CardHolderName { get; set; } // Kartın üzerindeki isim
        public string CardNumber { get; set; }     // 16 haneli numara
        public string ExpireMonth { get; set; }    // Son kullanma Ayı
        public string ExpireYear { get; set; }     // Son kullanma Yılı
        public string Cvv { get; set; }            // Arkadaki 3 haneli kod
        public decimal Price { get; set; }         // Çekilecek tutar
    }
}
