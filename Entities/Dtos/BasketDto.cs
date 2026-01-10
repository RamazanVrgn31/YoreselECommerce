using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Dtos
{
    public class BasketDto : IDto
    {
        public BasketDto()
        {
            Items = new List<BasketItemDto>();
        }

        public List<BasketItemDto> Items { get; set; }

        // Toplam tutarı otomatik hesaplayan property
        public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
    }
}
