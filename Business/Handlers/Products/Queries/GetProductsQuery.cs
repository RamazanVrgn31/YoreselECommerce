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
    // Cevap olarak Ürün Listesi döneceğini söylüyoruz
    public class GetProductsQuery : IRequest<IDataResult<IEnumerable<Product>>>
    {
        
        

    }
}
