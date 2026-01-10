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
    //1. Kısım: KOMUT
    public class DeleteProductCommand : IRequest<IResult>
    {
        public int Id { get; set; }
    }
    //2. Kısım: İŞLEYİCİ

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, IResult>
    {
        private readonly IProductDal _productDal;
        public DeleteProductCommandHandler(IProductDal productDal)
        {
            _productDal = productDal;
        }

        [SecuredOperation("Admin")]
        [CacheRemoveAspect("GetProduct")]
        public async Task<IResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            // Silinecek kaydı veritabanından bul
            var productToDelete = await _productDal.GetAsync(p => p.Id == request.Id);
            if (productToDelete == null)
            {
                return new ErrorResult("Ürün bulunamadı.");
            }

            // Ürünü sil
            _productDal.Delete(productToDelete);
            await _productDal.SaveChangesAsync();


            return new SuccessResult("Ürün başarıyla silindi.");
        }
    }
}
