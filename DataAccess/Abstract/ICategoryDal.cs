using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess;
using Entities.Concrete;

namespace DataAccess.Abstract
{
    public interface ICategoryDal : IEntityRepository<Category>
    {
        // Burası şimdilik boş. 
        // DevArchitecture bizim yerimize Ekle/Sil/Güncelle/Listele metodlarını
        // IEntityRepository sayesinde otomatik getirecek.

    }
}
