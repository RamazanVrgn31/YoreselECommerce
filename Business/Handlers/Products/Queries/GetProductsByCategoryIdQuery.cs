using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Results;
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Products.Queries
{
    public class GetProductsByCategoryIdQuery : IRequest<IDataResult<IEnumerable<Product>>>
    {
        public int CategoryId { get; set; }
    }
   
}
