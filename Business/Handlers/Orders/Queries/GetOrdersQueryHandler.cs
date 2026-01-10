using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Business.BusinessAspects;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Contexts;
using Entities.Concrete;
using Entities.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Business.Handlers.Orders.Queries
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IDataResult<IEnumerable<OrderDto>>>
    {
        private readonly ProjectDbContext _context;

        public GetOrdersQueryHandler(ProjectDbContext context)
        {
            
            _context = context;
        }
        [SecuredOperation("Admin")]
        [CacheAspect] //Performans için 60 dakika cache'le
        [LogAspect(typeof(FileLogger))]
        public async Task<IDataResult<IEnumerable<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var result = from order in _context.Orders
                         join user in _context.Users on order.CustomerId equals user.UserId
                         orderby order.OrderDate descending
                         select new OrderDto
                         {
                             OrderId = order.Id,
                             CustomerId = order.CustomerId,
                             CustomerName = user.FullName,
                             OrderDate = order.OrderDate,
                             TotalAmount = order.TotalAmount,
                             Status = order.Status,
                             Address = order.Address
                         };

            var list = await result.ToListAsync(cancellationToken);

            // Tüm siparişleri getir
            return new SuccessDataResult<IEnumerable<OrderDto>>(list);
        }
    }
}
