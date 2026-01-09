using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Contexts;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfProductDal : EfEntityRepositoryBase<Product, ProjectDbContext>, IProductDal
    {
        public EfProductDal(ProjectDbContext context) : base(context)
        {
        }

        public async Task<List<ProductDetailDto>> GetProductDetailsAsync()
        {
            // DOĞRUSU: Base sınıftan gelen hazır "Context" nesnesini kullanıyoruz.
            var result = from p in Context.Products
                             join c in Context.Categories
                             on p.CategoryId equals c.CategoryId
                             select new ProductDetailDto
                             {
                                 ProductId = p.Id,
                                 ProductName = p.ProductName,
                                 CategoryName = c.CategoryName, //Kategori ismi eklendi
                                 Price = p.Price,
                                 UnitsInStock = p.UnitsInStock,
                                 ImageUrl = p.ImageUrl,
                             };

                return await result.ToListAsync();
            
        }

    }
}
