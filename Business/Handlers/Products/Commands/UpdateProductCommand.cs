using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Business.BusinessAspects;
using Core.Aspects.Autofac.Caching;
using Core.Utilities.Results;
using DataAccess.Abstract;
using MediatR;

namespace Business.Handlers.Products.Commands
{
    // 1. Kısım: KOMUT
    public class UpdateProductCommand : IRequest<IResult>
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int UnitsInStock { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } 
        public bool IsActive { get; set; }
    }
    // 2. Kısım: İŞLEYİCİ
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, IResult>
    {
        private readonly IProductDal _productDal;
        public UpdateProductCommandHandler(IProductDal productDal)
        {
            _productDal = productDal;
        }

        [SecuredOperation("Admin")]
        [CacheRemoveAspect("GetProduct")]
        public async Task<IResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Güncellenecek kaydı veritabanından bul
            var productToUpdate = await _productDal.GetAsync(p => p.Id == request.Id);

            if (productToUpdate == null)
            {
                return new ErrorResult("Ürün bulunamadı.");
            }

            //Yeni değerleri ata
            productToUpdate.ProductName = request.ProductName;
            productToUpdate.Price = request.Price;
            productToUpdate.UnitsInStock = request.UnitsInStock;
            productToUpdate.Description = request.Description;
            productToUpdate.ImageUrl = request.ImageUrl;
            productToUpdate.IsActive = request.IsActive;
            // 3. Değişiklikleri kaydet
            _productDal.Update(productToUpdate);
            await _productDal.SaveChangesAsync();


            return new SuccessResult("Ürün başarıyla güncellendi.");
        }
    }
}
