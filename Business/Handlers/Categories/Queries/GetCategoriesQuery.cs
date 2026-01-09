using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Core.Utilities.Results;
using Entities.Concrete;
using MediatR;

namespace Business.Handlers.Categories.Queries
{
    // Bu sınıf bir "İstek"tir (Request). 
    // Cevap olarak "IDataResult<IEnumerable<Category>>" döneceğini taahhüt eder.
    public class GetCategoriesQuery : IRequest<IDataResult<IEnumerable<Category>>>
    {
        // Listeleme yaparken parametre (ID vs.) gerekmediği için içi boş.
    }
}
