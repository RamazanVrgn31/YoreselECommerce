using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Products.Queries
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IDataResult<IEnumerable<Product>>>
    {
        private readonly IProductDal _productDal;

        public GetProductsQueryHandler(IProductDal productDal)
        {
            _productDal = productDal;
        }


        [LogAspect(typeof(MsSqlLogger))]
        [CacheAspect(10)]
        public async Task<IDataResult<IEnumerable<Product>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            // Tüm ürünleri çek
            var productList = await _productDal.GetListAsync();

            return new SuccessDataResult<IEnumerable<Product>>(productList);

        }
    }
}
