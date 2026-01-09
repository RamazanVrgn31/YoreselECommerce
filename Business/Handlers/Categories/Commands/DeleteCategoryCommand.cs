using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Aspects.Autofac.Caching;
using Core.Utilities.Results;
using DataAccess.Abstract;
using MediatR;

namespace Business.Handlers.Categories.Commands
{
    // 1. Kısım: İSTEK (Sadece ID yeterli)
    public class DeleteCategoryCommand : IRequest<IResult>
    {
        public int CategoryId { get; set; }
    }
    // 2. Kısım: İŞLEYİCİ

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, IResult>
    {
        private readonly ICategoryDal _categoryDal;

        public DeleteCategoryCommandHandler(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }

        [CacheRemoveAspect("GetCategoriesQuery")]
        public Task<IResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            // Silinecek kategoriyi bul
            var categoryToDelete = _categoryDal.GetAsync(c => c.CategoryId == request.CategoryId);

            if (categoryToDelete == null)
            {
                return Task.FromResult<IResult>(new ErrorResult("Kategori bulunamadı."));
            }
            // Kategoriyi sil
            _categoryDal.Delete(categoryToDelete.Result);
            _categoryDal.SaveChangesAsync().Wait();

            return Task.FromResult<IResult>(new SuccessResult("Kategori başarıyla silindi."));
        }
    }
}
