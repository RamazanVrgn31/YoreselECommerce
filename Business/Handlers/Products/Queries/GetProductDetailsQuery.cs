    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Core.Utilities.Results;
using Entities.Dtos;
using MediatR;

namespace Business.Handlers.Products.Queries
{
    public class GetProductDetailsQuery : IRequest<IDataResult<IEnumerable<ProductDetailDto>>>
    {
    }
}
