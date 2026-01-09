//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection.Metadata;
//using System.Security.Claims;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Business.Abstract;
//using Business.Handlers.Orders.Commands;
//using Business.Handlers.Orders.Rules;
//using DataAccess.Abstract;
//using DataAccess.Concrete.EntityFramework;
//using DataAccess.Concrete.EntityFramework.Contexts;
//using Entities.Concrete;
//using MassTransit;
//using Microsoft.AspNetCore.Http;
//using Moq;
//using NUnit.Framework;

//namespace Tests.Business.Handlers.Orders
//{
//    [TestFixture]
//    public class CreateOrderTests
//    {
//        // Tüm bağımlılıkların sahtelerini (Mock) tanımlıyoruz
//        Mock<IOrderDal> _orderDal;
//        Mock<IProductDal> _productDal;
//        Mock<IOrderDetailDal> _orderDetaiDal;
//        Mock<IHttpContextAccessor> _httpContextAccessor;
//        Mock<IPaymentService> _paymentService;
//        Mock<ProjectDbContext> _context;
//        Mock<IPublishEndpoint> _publishEndpoint;
//        CreateOrderCommandHandler _handler;

//        [SetUp]
//        public void Setup() 
//        {
//            // tüm bağımlılıkların sahtesini oluşturuyoruz
//            _orderDal = new Mock<IOrderDal>();
//            _productDal = new Mock<IProductDal>();
//            _orderDetaiDal = new Mock<IOrderDetailDal>();
//            _httpContextAccessor = new Mock<IHttpContextAccessor>();
//            _paymentService = new Mock<IPaymentService>();
//            _context = new Mock<ProjectDbContext>();
//            _publishEndpoint = new Mock<IPublishEndpoint>();

//            // --- 1. MOCK USER (Sanki Token ile giriş yapılmış gibi) ---
//            var userClaims = new List<Claim>
//            {
//                new Claim(ClaimTypes.NameIdentifier, "1") // User ID = 1
//            };

//            var identity = new ClaimsIdentity(userClaims, "TestAuthType");
//            var claimsPrincipal = new ClaimsPrincipal(identity);

//            var httpContext = new DefaultHttpContext();
//            httpContext.User =claimsPrincipal;

//            // Accessor'a "Biri sana Context sorarsa bu sahte context'i ver" diyoruz.
//            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

//            // --- 2. ORDER RULES ---
//            // OrderRules genellikle logic içerdiği için direkt new'leyebiliriz.
//            // Eğer OrderRules içinde de DAL kullanıyorsan onları da mocklamamız gerekirdi ama
//            // şu anki yapıda parametre olarak entity aldığı için sorun yok.
//            var orderRules = new OrderRules(_productDal.Object);

//            // --- 3. HANDLER'I AYAĞA KALDIR ---
//            _handler = new CreateOrderCommandHandler(
//                _orderDal.Object,
//                _orderDetaiDal.Object,
//                _productDal.Object,
//                _paymentService.Object,
//                orderRules,
//                _publishEndpoint.Object,
//                _httpContextAccessor.Object,
//                _context.Object


//            );

//        }

//        [Test]
//        public async Task Handle_ValidRequest_ShouldCreateOrder_AddDetails_AndReduceStock() 
//        {
//            // --- ARRANGE (Hazırlık) ---
//            var command = new CreateOrderCommand
//            {
//                Address = "Test Adresi",
//                Items = new List<OrderItemDto>
//                {
//                    // 100 TL'lik üründen 2 tane alıyoruz.
//                    new OrderItemDto { ProductId = 1, Count = 2 }
//                }

//            };

//            // Mock Product: Veritabanından ID=1 istenirse, stoğu 50 olan bu ürünü dön.
//            // Bu sayede "Stok Yetersiz" hatasına takılmayız.
//            var mockProduct = new Product
//            {
//                Id = 1, 
//                ProductName = "Test",
//                UnitsInStock = 58,
//                Price=1500
//            };
//            _productDal.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Product, bool>>>()))
//                       .ReturnsAsync(mockProduct);

//            //----ACT(Eylem)----
//            var result = await _handler.Handle(command, new CancellationToken());

//            // --- ASSERT (Doğrulama) ---

//            // 1. İşlem Başarılı mı?
//            Assert.IsTrue(result.Success);
//            Assert.AreEqual("Sipariş başarıyla oluşturuldu.", result.Message);

//            // 2. Order tablosuna kayıt atıldı mı?
//            _orderDal.Verify(x => x.Add(It.IsAny<Order>()), Times.Once);

//            // 3. OrderDetail tablosuna kayıt atıldı mı?
//            _orderDetaiDal.Verify(x => x.Add(It.IsAny<OrderDetail>()), Times.Once);

//            // 4. Product tablosunda güncelleme (stok düşümü) yapıldı mı?
//            _productDal.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);
//        }
//    }
//}
