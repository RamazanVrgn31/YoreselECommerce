using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Business.Concrete
{
    public class BasketManager : IBasketService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IProductDal _productDal;

        public BasketManager(IDistributedCache distributedCache, IProductDal productDal)
        {
            _distributedCache = distributedCache;
            _productDal = productDal;
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
                // C. YOKSA: Veritabanından Ürün Bilgilerini Çek
                var product = _productDal.Get(p => p.Id == basketItemDto.ProductId);
                if (product == null)
                {
                    return new ErrorResult("Ürün bulunamadı.");
                }

                // Ürün bilgilerini BasketItemDto'ya ekle
                basketItemDto.ProductName = product.ProductName;
                basketItemDto.Price = product.Price;

                //  yeni ürünü sepete ekle
                basket.Items.Add(basketItemDto);
            }

            // 3. Güncellenmiş sepeti Redis'e kaydet (Mevcut Update metodunu kullanıyoruz)
            return UpdateBasket(userId, basket);
        }
    }
}
