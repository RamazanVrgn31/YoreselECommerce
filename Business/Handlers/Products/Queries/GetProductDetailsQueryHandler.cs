using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Aspects.Autofac.Caching;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Dtos;
using MediatR;

namespace Business.Handlers.Products.Queries
{
    public class GetProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, IDataResult<IEnumerable<ProductDetailDto>>>
    {
        private readonly IProductDal _productDal;
        public GetProductDetailsQueryHandler(IProductDal productDal)
        {
            _productDal = productDal;
        }

        [CacheAspect(10)]
        public async Task<IDataResult<IEnumerable<ProductDetailDto>>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _productDal.GetProductDetailsAsync();
            return new SuccessDataResult<IEnumerable<ProductDetailDto>>(result);
        }
    }
}
