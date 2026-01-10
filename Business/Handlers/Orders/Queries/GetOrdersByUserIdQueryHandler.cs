using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Business.BusinessAspects;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Extensions;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceStack;

namespace Business.Handlers.Orders.Queries
{
    public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, IDataResult<IEnumerable<Order>>>
    {
        private readonly IOrderDal _orderDal;
        private readonly IHttpContextAccessor _httpContextAccessor; // Token okumak için lazım

        public GetOrdersByUserIdQueryHandler(IOrderDal orderDal, IHttpContextAccessor httpContextAccessor)
        {
            _orderDal = orderDal;
            _httpContextAccessor = httpContextAccessor;
        }

        [SecuredOperation] // Sadece login gerekli, kullanıcı kendi siparişlerini görebilir
        [LogAspect(typeof(MsSqlLogger), Priority = 0)]
        public async Task<IDataResult<IEnumerable<Order>>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Token'dan ID'yi çek (Tek satır!)
            var currendUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (currendUserId == null)
            {
                return new ErrorDataResult<IEnumerable<Order>>("Kullanıcı bulunamadı.");
            }

            // 2. Sadece bu ID'ye ait siparişleri veritabanından iste
            // (x => x.CustomerId == currentUserId) filtresi işi bitirir.
            var myOrders = await _orderDal.GetListAsync(x => x.CustomerId == currendUserId.Value);

            return new SuccessDataResult<IEnumerable<Order>>(myOrders);
        }
    }
}
