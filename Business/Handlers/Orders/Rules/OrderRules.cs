using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;

namespace Business.Handlers.Orders.Rules
{
    public class OrderRules
    {
        // Kuralları kontrol etmek için verilere ihtiyacımız var
        private readonly IProductDal _productDal;

        public OrderRules(IProductDal productDal)
        {
            _productDal = productDal;
        }

        // KURAL 1: Ürün gerçekten var mı?
        // Bu metot geriye IResult döner: Başarılıysa (null veya Success), Hatalıysa (ErrorResult)

        public IResult CheckProductExists(Product product, int productId) 
        {
            if (product == null)
            {
                return new ErrorResult($"Ürün bulunamadı. Ürün Id: {productId}");
            }
            return new SuccessResult();
        }

        // KURAL 2: Stok yeterli mi?
        public IResult CheckProductStock (Product product, int requestedQuantity)
        {
            // Ürünün aktif olup olmadığını da burada kontrol edebiliriz
            if (!product.IsActive)
            {
                return new ErrorResult($"{product.ProductName} ürünü şu an satışa kapalı.");
            }
            if (product.UnitsInStock < requestedQuantity)
            {
                return new ErrorResult($"{product.ProductName} ürününden stokta sadece {product.UnitsInStock} adet bulunmaktadır.");
            }
            return new SuccessResult();
        }
    }
}
