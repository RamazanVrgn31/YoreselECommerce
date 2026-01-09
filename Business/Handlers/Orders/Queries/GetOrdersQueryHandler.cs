using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Orders.Queries
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IResult>
    {
       private readonly IOrderDal _orderDal;
        public GetOrdersQueryHandler(IOrderDal orderDal)
        {
            _orderDal = orderDal;
        }
        public async Task<IResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            // Tüm siparişleri getir
            var orders = await _orderDal.GetListAsync();
            return new SuccessDataResult<IEnumerable<Order>>(orders);
        }
    }
}
