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
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework.Contexts;
using Entities.Concrete;
using Entities.Dtos;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Business.Handlers.Orders.Commands
{
    // YARDIMCI SINIF: Sepetteki her bir kalemi temsil eder
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
    // 1. KISIM: İSTEK (Müşteriden ne geliyor?)
    public class CreateOrderCommand : IRequest<IResult>
    {
        //public int CustomerId { get; set; }// Şimdilik elle gireceğiz (İleride Token'dan gelecek) Artık Token'dan geliyor
        public string Address { get; set; }
        public List<OrderItemDto> Items { get; set; }// Sepetteki ürünler listesi

        // ÖDEME BİLGİLERİ 
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpireMonth { get; set; }
        public string ExpireYear { get; set; }
        public string Cvc { get; set; }
    }

    // 2. KISIM: İŞLEYİCİ (İsteği nasıl işleyeceğiz?)
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, IResult>
    {
        // Üç tabloyla da işimiz olduğu için üçünü de çağırıyoruz
        private readonly IOrderDal _orderDal;
        private readonly IOrderDetailDal _orderDetailDal;
        private readonly IProductDal _productDal;
        private readonly IPaymentService _paymentService;
        private readonly OrderRules _orderRules;
        private readonly ProjectDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public CreateOrderCommandHandler(IOrderDal orderDal, IOrderDetailDal orderDetailDal, IProductDal productDal, IPaymentService paymentService, OrderRules orderRules, ProjectDbContext context, IPublishEndpoint publishEndpoint, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _orderDal = orderDal;
            _orderDetailDal = orderDetailDal;
            _productDal = productDal;
            _paymentService = paymentService;
            _orderRules = orderRules;
            _context = context;
            _publishEndpoint = publishEndpoint;
            _httpContextAccessor = httpContextAccessor;
        }

        [LogAspect(typeof(MsSqlLogger), Priority = 0)]
        [ValidationAspect(typeof(CreateOrderValidator), Priority = 2)]
        public async Task<IResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // --- ADIM 1: HAZIRLIK VE KONTROLLER (HENÜZ DB YAZMA YOK) ---

            var currentUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (currentUserId == null) return new ErrorResult("Kullanıcı bulunamadı.");

            decimal totalAmount = 0;
            var productUpdates = new List<Product>();

            // Döngü: Stok var mı kontrol et ve Toplam Fiyatı Hesapla
            foreach (var item in request.Items)
            {
                var product = await _productDal.GetAsync(p => p.Id == item.ProductId);

                // İş Kuralları (Stok yetiyor mu?)
                var logicResult = BusinessRules.Run(
                    _orderRules.CheckProductExists(product, item.ProductId),
                    _orderRules.CheckProductStock(product, item.Count)
                );

                if (logicResult != null) return logicResult; // Kural hatası varsa dur.

                // Fiyatı ekle
                totalAmount += product.Price * item.Count;

                // Stoğu RAM üzerinde düş (Henüz DB'ye yansıtma)
                product.UnitsInStock -= (short)item.Count;
                productUpdates.Add(product);
            }

            // --- ADIM 2: ÖDEME AL (TRANSACTION ÖNCESİ) ---
            // Neden Transaction dışında? Banka servisi yavaş olabilir, DB'yi kilitlemeyelim.
            // Stoklar uygunsa parayı çekiyoruz.

            var paymentResult = await _paymentService.Pay(new PaymentDto
            {
                CardHolderName = request.CardHolderName,
                CardNumber = request.CardNumber,
                ExpireMonth = request.ExpireMonth,
                ExpireYear = request.ExpireYear,
                Cvc = request.Cvc,
                Price = totalAmount
            });

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
                        CustomerId = currentUserId.Value,
                        OrderDate = DateTime.Now,
                        Address = request.Address,
                        Status = "Onaylandı", // Ödeme alındığı için Onaylandı
                        TotalAmount = totalAmount
                    };

                    _orderDal.Add(order);
                    await _orderDal.SaveChangesAsync(); // Order ID oluşsun diye save ediyoruz

                    // B. Detayları Ekle ve Stoğu Düş
                    foreach (var item in request.Items)
                    {
                        // productUpdates listesindeki (stoğu RAM'de düşülmüş) ürünü bul
                        var product = productUpdates.First(p => p.Id == item.ProductId);

                        _orderDetailDal.Add(new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Count = item.Count,
                            UnitPrice = product.Price
                        });

                        // Güncellenmiş stoğu DB'ye gönder
                        _productDal.Update(product);
                    }

                    await _productDal.SaveChangesAsync();
                    await _orderDetailDal.SaveChangesAsync();

                    // Her şey yolunda, işlemi onayla.
                    await transaction.CommitAsync();

                    // ⭐⭐⭐ ADIM 4: RABBITMQ'YA HABER VER ⭐⭐⭐
                    // Transaction başarılı olduysa mail kuyruğuna mesaj atıyoruz.
                    await _publishEndpoint.Publish<OrderCreateEvent>(new OrderCreateEvent
                    {
                        OrderId = order.Id,
                        CustomerId = order.CustomerId,
                        TotalAmount = order.TotalAmount,
                    });

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
