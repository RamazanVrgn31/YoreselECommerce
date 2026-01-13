using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Dtos
{
    public class OrderDetailDto : IDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }// O anki satış fiyatı
        public decimal Total { get; set; }// Price * Quantity
        // İstersen resim de ekleyebilirsin:
        // public string ImageUrl { get; set; }
    }
}
