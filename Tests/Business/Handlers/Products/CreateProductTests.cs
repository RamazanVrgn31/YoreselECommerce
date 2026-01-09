using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Business.Constants;
using Business.Handlers.Products.Commands;
using Core.Utilities.Helpers.FileHelper;
using DataAccess.Abstract;
using Entities.Concrete;
using Moq;
using NUnit.Framework;

namespace Tests.Business.Handlers.Products
{
    [TestFixture]
    public class CreateProductTests
    {
        // 1. "Mış gibi" yapacağımız Repository (Sahte Veritabanı)
        Mock<IProductDal> _productDal;
        Mock<IFileHelper> _fileHelper;

        // 2. Test edeceğimiz gerçek sınıf
        CreateProductCommandHandler _commandHandler;
        [SetUp]
        public void Setup() 
        {
            // Her testten önce bu hazırlığı yap:
            _productDal  = new Mock<IProductDal>();
            _fileHelper = new Mock<IFileHelper>();

            // Handler'ı oluştur ve içine sahte repository'i ver
            _commandHandler = new CreateProductCommandHandler(_productDal.Object,_fileHelper.Object);
        }

        [Test]
        public async Task Handler_ValidProduct_ShouldReturnSuccess() 
        {
            // ARRANGE (Hazırlık)
            // Sanki API'den şöyle bir istek gelmiş gibi data hazırlıyoruz:
            var command = new CreateProductCommand()
            {
                ProductName = "Test Ürünü",
                Price = 1500,
                CategoryId = 2,
                UnitsInStock=10
            };
            // ACT (Eylem)
            // Handler'ın Handle metodunu çalıştırıyoruz.
            var result = await _commandHandler.Handle(command, new CancellationToken());

            // ASSERT (Kontrol)

            // 1. Sonuç başarılı (Success) dönmeli
            Assert.IsTrue(result.Success);

            // 2. Mesaj "Eklendi" olmalı
            Assert.AreEqual(Messages.Added, result.Message);

            // 3. (En Önemlisi) Repository'nin Add metodu, herhangi bir Product nesnesiyle 1 kere çağrıldı mı?
            // Bu satır, veritabanına kayıt atılmaya çalışıldığını kanıtlar.
            _productDal.Verify(x => x.Add(It.IsAny<Product>()), Times.Once);
        }
    }
}
