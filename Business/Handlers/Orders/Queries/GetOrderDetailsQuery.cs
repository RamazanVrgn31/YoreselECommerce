using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Results;
using Entities.Dtos;
using MediatR;

namespace Business.Handlers.Orders.Queries
{
    // Bize OrderId ver, sana o siparişin satırlarını (ürünlerini) verelim.
    public class GetOrderDetailsQuery : IRequest<IDataResult<IEnumerable<OrderDetailDto>>>
    {
        public int OrderId { get; set; }
    }
}
