using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Concrete
{
    public class Category : IEntity
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; } //Sıralama numarası  
        public bool IsActive { get; set; } = true;// Varsayılan olarak aktif olsun
    }
}
