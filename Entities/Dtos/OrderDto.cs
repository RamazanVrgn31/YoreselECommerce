using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Nest;

namespace Entities.Dtos
{
    public class OrderDto :IDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }// User tablosundan gelecek
        public DateTime OrderDate{ get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
    }
}
