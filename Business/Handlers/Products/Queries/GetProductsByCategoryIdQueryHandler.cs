using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Products.Queries
{
    public class GetProductsByCategoryIdQueryHandler : IRequestHandler<GetProductsByCategoryIdQuery, IDataResult<IEnumerable<Product>>>
    {
        private readonly IProductDal _productDal;

        public GetProductsByCategoryIdQueryHandler(IProductDal productDal)
        {
            _productDal = productDal;
        }

        //[LogAspect(typeof(MsSqlLogger))]
        public async Task<IDataResult<IEnumerable<Product>>> Handle(GetProductsByCategoryIdQuery request, CancellationToken cancellationToken)
        {
            var products = await _productDal.GetListAsync(p => p.CategoryId == request.CategoryId);
            return new SuccessDataResult<IEnumerable<Product>>(products);
        }
    }
}
