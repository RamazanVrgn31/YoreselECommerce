using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Iyzico;
using Entities.Dtos;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using Nest;

namespace Business.Adapters.Payment
{
    public class IyzicoPaymentAdapter : IPaymentService
    {
        private readonly IyzicoPaymentOptions _options;

        public IyzicoPaymentAdapter(IConfiguration configuration)
        {
            // appsettings.json'dan ayarları çekiyoruz
            _options = configuration.GetSection("IyzicoOptions").Get<IyzicoPaymentOptions>();
        }

        public async Task<IResult> Pay(PaymentDto paymentDto)
        {
            //1. Iyzico API entegrasyonu burada yapılacak
            
            var options= new Options
            {
                ApiKey = _options.ApiKey,
                SecretKey = _options.SecretKey,
                BaseUrl = _options.BaseUrl
            };

            //2. Ödeme isteği oluşturma (Mapping)
            var request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Price = paymentDto.Price.ToString().Replace(",", "."), // Kuruş ayracı nokta olmalı
                PaidPrice = paymentDto.Price.ToString().Replace(",", "."),
                Currency = Currency.TRY.ToString(),
                Installment = 1,// Taksit sayısı (Şimdilik 1)
                BasketId = "B67832",// Sipariş ID'si (Normalde dinamik gelir)
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),

                // KART BİLGİLERİ (Dto'dan gelenler
                PaymentCard = new PaymentCard
                {
                    CardHolderName = paymentDto.CardHolderName,
                    CardNumber = paymentDto.CardNumber,
                    ExpireMonth = paymentDto.ExpireMonth,
                    ExpireYear = paymentDto.ExpireYear,
                    Cvc = paymentDto.Cvc,
                    RegisterCard = 0 // Kartı kaydetme
                },

                // ALICI BİLGİLERİ(Şimdilik Sahte / Sabit - İleride User'dan alacağız)
                Buyer = new Buyer 
                {
                    Id = "BY789",
                    Name = "John",
                    Surname = "Doe",
                    GsmNumber = "+905350000000",
                    Email = "email@email.com",
                    IdentityNumber = "74300864791",
                    LastLoginDate = "2020-10-05 12:43:35",
                    RegistrationDate = "2013-04-21 15:12:09",
                    RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    Ip = "86.34.78.112",
                    City = "Istanbul",
                    Country = "Turkey",
                    ZipCode = "34732"
                },

                // TESLİMAT ADRESİ (Şimdilik Sahte / Sabit - İleride User'dan alacağız)

                ShippingAddress = new Address
                {
                    ContactName = "Jane Doe",
                    City = "Istanbul",
                    Country = "Turkey",
                    Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    ZipCode = "34742"
                },

                // FATURA ADRESİ (Şimdilik Sahte / Sabit - İleride User'dan alacağız)
                BillingAddress = new Address
                {
                    ContactName = "Jane Doe",
                    City = "Istanbul",
                    Country = "Turkey",
                    Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    ZipCode = "34742"
                },

                // SEPET İÇERİĞİ (Şimdilik temsili 1 ürün - İyzico bunu zorunlu tutuyor)
                BasketItems = new List<BasketItem>
                {
                    new BasketItem
                    {
                        Id = "BI101",
                        Name = "Product 1",
                        Category1 = "Category",
                        ItemType = BasketItemType.PHYSICAL.ToString(),
                        Price = paymentDto.Price.ToString().Replace(",", ".")
                    }
                }
            };

            //3. Ödemeyi gerçekleştirme
            var payment = await Task.Run(() => Iyzipay.Model.Payment.Create(request, options));

            //4. Sonucu kontrol etme ve IResult döndürme
            if (payment.Status == "success")
            {
                return new SuccessResult("Ödeme başarılı.");
            }
            
            return new ErrorResult($"Ödeme başarısız: {payment.ErrorMessage}");
            
        }
    }
}
