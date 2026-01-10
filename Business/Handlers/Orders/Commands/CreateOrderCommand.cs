using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Business.Abstract;
using Business.BusinessAspects;
using Business.Events;
using Business.Handlers.Orders.Rules;
using Business.Handlers.Orders.Validation_Rules.Fluent_Validation;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Validation;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Extensions;
using Core.Utilities.Business;
using Core.Utilities.IoC;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Contexts;
using Entities.Concrete;
using Entities.Dtos;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Handlers.Orders.Commands
{
    
    // 1. KISIM: İSTEK (Müşteriden ne geliyor?)
    public class CreateOrderCommand : IRequest<IResult>
    {
        //public int CustomerId { get; set; }// Şimdilik elle gireceğiz (İleride Token'dan gelecek) Artık Token'dan geliyor
        public string Address { get; set; }

        // ÖDEME BİLGİLERİ 
        public PaymentDto PaymentDto { get; set; }
        //public string CardHolderName { get; set; }
        //public string CardNumber { get; set; }
        //public string ExpireMonth { get; set; }
        //public string ExpireYear { get; set; }
        //public string Cvc { get; set; }
    }

    // 2. KISIM: İŞLEYİCİ (İsteği nasıl işleyeceğiz?)
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, IResult>
    {
        // Üç tabloyla da işimiz olduğu için üçünü de çağırıyoruz
        private readonly IOrderDal _orderDal;
        private readonly IOrderDetailDal _orderDetailDal;
        private readonly IProductDal _productDal;
        private readonly IBasketService _basketService;
        private readonly IPaymentService _paymentService;
        private readonly OrderRules _orderRules;
        private readonly ProjectDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public CreateOrderCommandHandler(
            IOrderDal orderDal, 
            IOrderDetailDal orderDetailDal,
            IProductDal productDal,
            IPaymentService paymentService,
            OrderRules orderRules,
            ProjectDbContext context,
            IPublishEndpoint publishEndpoint,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
            IBasketService basketService)
        {
            _orderDal = orderDal;
            _orderDetailDal = orderDetailDal;
            _productDal = productDal;
            _paymentService = paymentService;
            _orderRules = orderRules;
            _context = context;
            _publishEndpoint = publishEndpoint;
            _httpContextAccessor = httpContextAccessor;
            _basketService = basketService;
        }

        [LogAspect(typeof(MsSqlLogger), Priority = 0)]
        [ValidationAspect(typeof(CreateOrderValidator), Priority = 2)]
        public async Task<IResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            //// --- ADIM 1: HAZIRLIK VE KONTROLLER (HENÜZ DB YAZMA YOK) ---

            //var currentUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            //if (currentUserId == null) return new ErrorResult("Kullanıcı bulunamadı.");

            // --- ADIM 0: KULLANICI VE SEPETİ BUL ---
            // Şimdilik test için sabit ID veriyoruz (Token sistemi tam oturunca alttakini açarsın)
            // var userId = _httpContextAccessor.HttpContext.User.GetUserId().ToString();
            var userId = "1"; // Test için sabit

            var basketResult = _basketService.GetBasket(userId);
            var basket = basketResult.Data;

            if (basket == null || basket.Items.Count == 0)
            {
                return new ErrorResult("Sepetiniz boş. Sipariş oluşturulamadı.");
            }
            var productUpdates = new List<Product>();

            // Döngü: Stok var mı kontrol et ve Toplam Fiyatı Hesapla
            foreach (var item in basket.Items)
            {
                var product = await _productDal.GetAsync(p => p.Id == item.ProductId);

                // İş Kuralları (Stok yetiyor mu?)
                var logicResult = BusinessRules.Run(
                    _orderRules.CheckProductExists(product, item.ProductId),
                    _orderRules.CheckProductStock(product, item.Quantity)
                );

                if (logicResult != null) return logicResult; // Kural hatası varsa dur.

                // Fiyatı ekle

                // Stoğu RAM üzerinde düş (Henüz DB'ye yansıtma)
                product.UnitsInStock -= (short)item.Quantity;
                productUpdates.Add(product);
            }

            // --- ADIM 2: ÖDEME AL (TRANSACTION ÖNCESİ) ---
            // Neden Transaction dışında? Banka servisi yavaş olabilir, DB'yi kilitlemeyelim.
            // Stoklar uygunsa parayı çekiyoruz.
            request.PaymentDto.Price = basket.TotalPrice;
            var paymentResult = await _paymentService.Pay(request.PaymentDto);

            // Ödeme Başarısızsa Direkt Dön
            if (!paymentResult.Success)
            {
                return new ErrorResult(paymentResult.Message);
            }

            // --- ADIM 3: VERİTABANI KAYIT (TRANSACTION) ---
            // Para alındı, artık veritabanına yazabiliriz.
            // EF Core'un kendi transaction mekanizmasını kullanıyoruz (TransactionScope yerine)
            // Bu, async/await ile daha uyumlu çalışır ve deadlock sorunlarını önler.

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // A. Sipariş Başlığını Oluştur
                    var order = new Order
                    {
                        CustomerId = int.Parse(userId),
                        OrderDate = DateTime.Now,
                        Address = request.Address,
                        Status = "Onaylandı", // Ödeme alındığı için Onaylandı
                        TotalAmount = basket.TotalPrice
                    };

                    _orderDal.Add(order);
                    await _orderDal.SaveChangesAsync(); // Order ID oluşsun diye save ediyoruz

                    // B. Detayları Ekle ve Stoğu Düş
                    foreach (var item in basket.Items)
                    {
                        // productUpdates listesindeki (stoğu RAM'de düşülmüş) ürünü bul
                        var product = productUpdates.First(p => p.Id == item.ProductId);

                        _orderDetailDal.Add(new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Count = item.Quantity,
                            UnitPrice = product.Price
                        });

                        // Güncellenmiş stoğu DB'ye gönder
                        _productDal.Update(product);
                    }

                    await _productDal.SaveChangesAsync();
                    await _orderDetailDal.SaveChangesAsync();

                    // C. Sepeti Temizle (Redis)
                    _basketService.DeleteBasket(userId);

                    // Her şey yolunda, işlemi onayla.
                    await transaction.CommitAsync();

                    // ⭐⭐⭐ ADIM 4: RABBITMQ'YA HABER VER ⭐⭐⭐
                    try
                    {
                        // Transaction başarılı olduysa mail kuyruğuna mesaj atıyoruz.
                        await _publishEndpoint.Publish(new OrderCreateEvent
                        {
                            OrderId = order.Id,
                            CustomerId = order.CustomerId,
                            TotalAmount = order.TotalAmount,
                        });
                    }
                    catch (Exception ex)
                    {
                        // Mail atılamadı ama sipariş başarılı. Loglayıp devam et.
                        Console.WriteLine($"UYARI: Sipariş oluştu ama mail kuyruğuna atılamadı. Hata: {ex.Message}");
                        // Logger.Error(...) kullanıyorsan buraya ekle.
                        var logger = ServiceTool.ServiceProvider.GetService<MsSqlLogger>();
                        if (logger != null)
                        {
                            // "Warn" veya "Info" seviyesinde logluyoruz, çünkü sistem çökmedi.
                            logger.Warn($"Sipariş ID: {order.Id} oluşturuldu fakat Mail Kuyruğuna atılamadı! RabbitMQ Hatası: {ex.Message}");
                        }
                    }
                    

                    return new SuccessResult("Sipariş başarıyla oluşturuldu.");
                }
                catch (Exception)
                {
                    // HATA DURUMU (Önemli Not):
                    // Kod buraya düştüyse Transaction Rollback olur (Sipariş silinir).
                    // ANCAK: Parayı yukarıda (Adım 2) çekmiştik! 
                    // Gerçek hayatta buraya "_paymentService.Refund()" (İade) kodu yazılır.
                    // Şimdilik loglayıp hatayı fırlatıyoruz.
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    
    }
}
