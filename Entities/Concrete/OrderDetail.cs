using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Concrete
{
    public class OrderDetail : IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }// Hangi siparişe ait?
        public int ProductId { get; set; }// Hangi ürün?
        public int Count { get; set; } // Kaç adet?
        public decimal UnitPrice { get; set; }// Ürünün O ANKİ satış fiyatı (Fiyat değişirse etkilenmemesi için)
    }
}
