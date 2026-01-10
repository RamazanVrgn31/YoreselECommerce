using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Business.BusinessAspects;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Validation;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Utilities.Helpers.FileHelper;
using IResult = Core.Utilities.Results.IResult;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Business.Constants;
using Core.Utilities.Results;
using System.IO;
using Business.Handlers.Products.Validation_Rules;

namespace Business.Handlers.Products.Commands
{
    // 1. KISIM: İSTEK (Frontend'den ne gelecek?)
    public class CreateProductCommand :IRequest<IResult>
    {
        public int CategoryId { get; set; }// Hangi kategoriye ait?
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int UnitsInStock { get; set; }
        public IFormFile Image { get; set; }

    }

    // 2. KISIM: İŞLEYİCİ (Handler)

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, IResult>
    {
        private readonly IProductDal _productDal;
        private readonly IFileHelper _fileHelper;

        // Startup.cs'e eklediğimiz IProductDal -> EfProductDal eşleşmesi sayesinde burası dolacak.

        public CreateProductCommandHandler(IProductDal productDal, IFileHelper fileHelper)
        {
            _productDal = productDal;
            _fileHelper = fileHelper;
        }



        [SecuredOperation("Admin")]
        [ValidationAspect(typeof(CreateProductValidator), Priority = 1)]
        [LogAspect(typeof(MsSqlLogger), Priority =0)]
        [CacheRemoveAspect("GetProduct")]
        public async Task<IResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // --- 3. RESMİ KAYDET ---
            // Eğer resim gönderildiyse Upload et, gönderilmediyse null veya varsayılan bir resim ata.

            string imagePath = "default.png";

            if (request.Image != null)
            {
                
                // "wwwroot/Images/" klasörüne kaydeder ve yeni ismini döner.
                // Not: Klasör yoksa FileHelper senin için oluşturacak.

                imagePath = _fileHelper.Upload(request.Image, PathConstants.ImagesPath);
            }

            // Veritabanı nesnesini oluşturuyoruz

            var product = new Product
            {
                CategoryId = request.CategoryId,
                ProductName = request.ProductName,
                Description = request.Description ?? "",// Boş gelirse patlamasın
                Price = request.Price,
                UnitsInStock = request.UnitsInStock,
                ImageUrl = imagePath ,
                IsActive = true

            };

            // Veritabanına ekle

            _productDal.Add(product);
            await _productDal.SaveChangesAsync();
            return new SuccessResult("Ürün başarıyla eklendi.");
        }
    }
}
