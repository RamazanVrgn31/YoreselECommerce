using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Concrete
{
    public class Order : IEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; } // Siparişi veren müşteri (User ID)
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } // Örn: "Onaylandı", "Hazırlanıyor"
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }//Toplam sipariş tutarı
    }
}
