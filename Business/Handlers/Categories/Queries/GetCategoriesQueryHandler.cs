using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Categories.Queries
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IDataResult<IEnumerable<Category>>>
    {
        private readonly ICategoryDal _categoryDal;

        // Constructor Injection: Veritabanı erişim sınıfını (DAL) içeri alıyoruz.
        public GetCategoriesQueryHandler(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }


        // Asıl işin yapıldığı yer
        [LogAspect(typeof(MsSqlLogger))] // Yapılan işlemi logla
        [CacheAspect(10)]
        public async Task<IDataResult<IEnumerable<Category>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            // Veritabanından listeyi çek
            var list = await _categoryDal.GetListAsync();

            // Sonucu "Başarılı" etiketiyle paketleyip döndür
            return new SuccessDataResult<IEnumerable<Category>>(list);
        }
    }
}
