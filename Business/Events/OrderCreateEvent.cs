using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Events
{
    public class OrderCreateEvent
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        // Mail atmak için kullanıcının e-posta adresini de buraya ekleyebiliriz
        // Ama şimdilik User tablosundan çekeriz diye varsayalım.
    }
}
