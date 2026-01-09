using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Core.Utilities.Results;
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Orders.Queries
{
    // Parametre almıyor çünkü ID'yi Token'dan (cebinden) okuyacağız
    public class GetOrdersByUserIdQuery : IRequest<IDataResult<IEnumerable<Order>>>
    {
    }
}
