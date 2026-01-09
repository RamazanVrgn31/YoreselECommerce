using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Handlers.Products.Commands;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Business.Handlers.Products.Validation_Rules
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator() 
        {
            RuleFor(x => x.ProductName).NotEmpty().WithMessage("Ürün adı boş olamaz.");
            RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Lütfen bir kategori seçiniz.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Ürün fiyatı 0'dan büyük olmalıdır.");
            RuleFor(x => x.UnitsInStock).GreaterThanOrEqualTo(0).WithMessage("Stok adedi negatif olamaz.");
            RuleFor(x => x.Image)
                .NotNull().WithMessage("Lütfen bir ürün resmi seçiniz.") // Resim zorunlu olsun
                .Must(IsImage).WithMessage("Sadece .jpg, .jpeg, .png veya .webp formatında resim yükleyebilirsiniz.")
                .Must(IsSizeValid).WithMessage("Dosya boyutu 5MB'dan büyük olamaz.");
        }

        // --- YARDIMCI METOTLAR (Helper Methods) ---

        // 1. Uzantı Kontrolü Yapan Fonksiyon
        private bool IsImage(IFormFile file)
        {
            // Eğer dosya null ise yukarıdaki .NotNull() kuralı hatayı verir, burası true dönebilir.
            if (file == null) return true;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            // Dosya adının sonundaki uzantıyı alıp küçültüyoruz (.JPG -> .jpg)
            var extension = Path.GetExtension(file.FileName).ToLower();

            return allowedExtensions.Contains(extension);
        }

        // 2. Boyut Kontrolü Yapan Fonksiyon (İsteğe bağlı)
        private bool IsSizeValid(IFormFile file)
        {
            if (file == null) return true;

            // 5 MB Limit (Byte cinsinden hesaplanır: 1024 * 1024 = 1 MB)
            return file.Length <= 5 * 1024 * 1024;
        }
    }
}
