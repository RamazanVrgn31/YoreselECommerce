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

namespace Business.Handlers.Categories.Commands
{
    // 1. Kısım: KOMUT
    public class UpdateCategoryCommand : IRequest<IResult>
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
    // 2. Kısım: İŞLEYİCİ
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, IResult>
    {
        private readonly ICategoryDal _categoryDal;

        public UpdateCategoryCommandHandler(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }

        [SecuredOperation("Admin")]
        [CacheRemoveAspect("GetCategoriesQuery")]
        public async Task<IResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            // 1. Güncellenecek kaydı veritabanından bul
            var categoryToUpdate = await _categoryDal.GetAsync(c => c.CategoryId == request.CategoryId);

            if (categoryToUpdate == null)
            {
                return new ErrorResult("Kategori bulunamadı.");
            }
            //Yeni değerleri ata

            categoryToUpdate.CategoryName = request.CategoryName;
            categoryToUpdate.DisplayOrder = request.DisplayOrder;
            categoryToUpdate.IsActive = request.IsActive;

            // 3. Değişiklikleri kaydet
            _categoryDal.Update(categoryToUpdate);
            await _categoryDal.SaveChangesAsync();

            return new SuccessResult("Kategori başarıyla güncellendi.");

        }
    }
}
