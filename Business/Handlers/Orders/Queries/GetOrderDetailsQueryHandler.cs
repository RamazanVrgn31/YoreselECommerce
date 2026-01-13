using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Business.BusinessAspects;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Extensions;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Business.Handlers.Orders.Queries
{
    public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, IDataResult<IEnumerable<OrderDetailDto>>>
    {
        private readonly IOrderDal _orderDal;
        private readonly IOrderDetailDal _orderDetailDal;
        private readonly IProductDal _productDal;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetOrderDetailsQueryHandler(IOrderDal orderDal, IOrderDetailDal orderDetailDal, IProductDal productDal, IHttpContextAccessor httpContextAccessor)
        {
            _orderDal = orderDal;
            _orderDetailDal = orderDetailDal;
            _productDal = productDal;
            _httpContextAccessor = httpContextAccessor;
        }

        [SecuredOperation]
        [LogAspect(typeof(MsSqlLogger), Priority = 0)]
        public async Task<IDataResult<IEnumerable<OrderDetailDto>>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            //Önce siparişi bul
            var order =await _orderDal.GetAsync(o => o.Id == request.OrderId);
            if (order == null)
            {
                return new ErrorDataResult<IEnumerable<OrderDetailDto>>("Sipariş bulunamadı.");
            }

            // 2. GÜVENLİK KONTROLÜ: Siparişin sahibi, şu anki kullanıcı mı?
            var currentUserId = _httpContextAccessor.HttpContext.User.GetUserId();

            // Eğer Admin değilse ve Sipariş sahibi de değilse erişimi engelle
            // (Admin kontrolü şimdilik yoksa sadece ID kontrolü yapalım)
            if (currentUserId == null || order.CustomerId != currentUserId.Value)
            {
                return new ErrorDataResult<IEnumerable<OrderDetailDto>>("Bu siparişe erişim yetkiniz yok.");
            }

            // 3. Sipariş Detaylarını Çek
            var details = await _orderDetailDal.GetListAsync(od => od.OrderId == request.OrderId);

            // A. Detaylardaki tüm ProductId'leri listeye al
            var productIds = details.Select(d => d.ProductId).ToList();

            // B. Bu ID'lere sahip tüm ürünleri TEK SEFERDE veritabanından çek (LINQ Contains)
            // Bu sayede döngü içinde veritabanına gitmiyoruz!
            var products = await _productDal.GetListAsync(p => productIds.Contains(p.Id));

            // 4. Detayları DTO'ya çevir (Ürün isimlerini de bularak)
            // Not: Bu işlem LINQ join ile daha performanslı yapılabilir ama şimdilik anlaşılaır olsun diye böyle yapıyoruz.
            var dtoList = new List<OrderDetailDto>();

            foreach (var item in details)
            {
                // Artık veritabanına gitmiyor, yukarıda çektiğimiz listeden buluyor (Çok hızlı ve güvenli)
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                dtoList.Add(new OrderDetailDto
                {
                    ProductName = product != null ? product.ProductName : "Silinmiş Ürün",
                    Price = item.UnitPrice,
                    Quantity = item.Count,
                    Total = item.UnitPrice * item.Count
                });
            }

            return new SuccessDataResult<IEnumerable<OrderDetailDto>>(dtoList);

        }
    }
}
