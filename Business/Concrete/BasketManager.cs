using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Abstract;
using Core.Utilities.Results;
using Entities.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Business.Concrete
{
    public class BasketManager : IBasketService
    {
        private readonly IDistributedCache _distributedCache;

        public BasketManager(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public IDataResult<BasketDto> GetBasket(string userId)
        {
            var key = $"basket_{userId}";//Redis Anahtarı: "basket:123"
            var basketString = _distributedCache.GetString(key);

            if (string.IsNullOrEmpty(basketString))
            {
                // Sepet boşsa null dönmek yerine boş bir sepet nesnesi dönelim

                return new SuccessDataResult<BasketDto>(new BasketDto());
            }
            var basket = JsonConvert.DeserializeObject<BasketDto>(basketString);
            return new SuccessDataResult<BasketDto>(basket);
        }

        public IResult UpdateBasket(string userId, BasketDto basketDto)
        {
            var key = $"basket_{userId}";
            var jsonBasket = JsonConvert.SerializeObject(basketDto);

            //Redis'e yazıyoruz
            _distributedCache.SetString(key, jsonBasket);
            return new SuccessResult("Sepet Güncellendi");
        }

        public IResult DeleteBasket(string userId)
        {
            var key = $"basket_{userId}";
            _distributedCache.Remove(key);
            return new SuccessResult("Sepet Temizlendi");
        }

        public IResult AddItemToBasket(string userId, BasketItemDto basketItemDto)
        {
            var basketResult = GetBasket(userId);
            var basket = basketResult.Data;

            if (basket == null)
            {
                basket = new BasketDto();
            }

            // 2. Ürün sepette zaten var mı kontrol et?
            var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == basketItemDto.ProductId);

            if (existingItem != null)
            {
                // Ürün zaten varsa, miktarını artır
                existingItem.Quantity += basketItemDto.Quantity;
            }
            else
            {
                // Ürün yoksa, yeni ürünü sepete ekle
                basket.Items.Add(basketItemDto);
            }

            // 3. Güncellenmiş sepeti Redis'e kaydet (Mevcut Update metodunu kullanıyoruz)
            return UpdateBasket(userId, basket);
        }
    }
}
