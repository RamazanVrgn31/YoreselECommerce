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
    public class EfCategoryDal : EfEntityRepositoryBase<Category, ProjectDbContext>, ICategoryDal
    {
        // "Bana bir ProjectDbContext ver, ben de onu babama (base) vereyim" diyoruz
        public EfCategoryDal(ProjectDbContext context) : base(context)
        {
        }


        // Süslü parantezlerin içi BOŞ kalacak.
        // Çünkü tüm işi "EfEntityRepositoryBase" (Baba sınıf) yapıyor.
    }
}
