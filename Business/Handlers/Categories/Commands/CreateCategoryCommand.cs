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
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Categories.Commands
{
    // 1. KISIM: İSTEK (Request) - Dışarıdan ne gelecek?
    public class CreateCategoryCommand : IRequest<IResult>
    {
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; }
    }
    // 2. KISIM: İŞLEYİCİ (Handler) - Ne yapılacak?
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, IResult>
    {
        private readonly ICategoryDal _categoryDal;

        // Senin Startup.cs'de tanımladığın DI yapısı burayı besleyecek
        public CreateCategoryCommandHandler(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }

        [SecuredOperation("Admin")]
        [CacheRemoveAspect("GetCategoriesQuery")]
        public async Task<IResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Yeni kategori nesnesi oluşturuluyor
            var category = new Category
            {
                CategoryName = request.CategoryName,
                DisplayOrder = request.DisplayOrder,
                IsActive = true //varsayılan olarak aktif
            };

            // Veritabanına ekleme işlemi
            _categoryDal.Add(category);

            //Değişiklikleri kaydet
            await _categoryDal.SaveChangesAsync();

            return new SuccessResult("Kategori başarıyla oluşturuldu.");
        }
    }

}
