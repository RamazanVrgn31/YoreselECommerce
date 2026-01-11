using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Results;
using Entities.Dtos;

namespace Business.Abstract
{
    public interface IBasketService
    {
        // Kullanıcının sepetini getirir
        IDataResult<BasketDto> GetBasket(string userId);

        // Sepeti günceller (Ekleme ve Çıkarma işlemleri burada yapılır)
        IResult UpdateBasket(string userId, BasketDto basketDto);

        // Sipariş tamamlanınca sepeti siler
        IResult DeleteBasket(string userId);

        IResult AddItemToBasket(string userId, BasketItemDto basketItemDto);
    }
}
