using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Handlers.Orders.Commands;
using FluentValidation;

namespace Business.Handlers.Orders.Validation_Rules.Fluent_Validation
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderValidator()
        {
            //// 1. Sepet listesi boş olamaz
            //RuleFor(x => x.Items)
            //    .NotEmpty().WithMessage("Sipariş vermek için sepette en az bir ürün olmalıdır.")
            //    .NotNull().WithMessage("Sepet bilgisi gönderilmedi.");
            //// 2. Listenin içindeki her bir elemanı kontrol et (Advanced!)
            //// Sepetteki her bir "item" için şu kuralları işlet:
            //RuleForEach(x => x.Items).ChildRules(items  =>
            //{
            //    items.RuleFor(x => x.ProductId)
            //        .GreaterThan(0).WithMessage("Geçersiz ürün ID'si.");
            //    items.RuleFor(x => x.Count)
            //        .GreaterThan(0).WithMessage("Ürün adedi 0'dan büyük olmalıdır.")
            //        .LessThan(100).WithMessage("Bir üründen en fazla 99 adet sipariş verebilirsiniz.");
            //});
            // 1. Adres Kontrolü
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Teslimat adresi boş olamaz.")
                .MinimumLength(5).WithMessage("Adres en az 10 karakter olmalıdır.");

            // 2. Ödeme Bilgileri Kontrolü
            RuleFor(x => x.PaymentDto.CardHolderName)
                .NotEmpty().WithMessage("Kart üzerindeki isim gereklidir.");

            //RuleFor(x => x.PaymentDto.CardNumber)
            //    .NotEmpty().WithMessage("Kart numarası gereklidir.")
            //    .CreditCard().WithMessage("Geçerli bir kredi kartı numarası giriniz.");
            // Not: .CreditCard() FluentValidation'ın hazır özelliğidir, Luhn algoritmasıyla kontrol eder.

            RuleFor(x => x.PaymentDto.ExpireMonth)
                .NotEmpty().WithMessage("Son kullanma ayı gereklidir.")
                .Matches(@"^(0[1-9]|1[0-2])$").WithMessage("Ay bilgisi 01-12 arasında olmalıdır.");

            RuleFor(x => x.PaymentDto.ExpireYear)
                .NotEmpty().WithMessage("Son kullanma yılı gereklidir.")
                .MinimumLength(2).WithMessage("Yıl bilgisi geçersiz.");

            RuleFor(x => x.PaymentDto.Cvv)
                .NotEmpty().WithMessage("CVV kodu gereklidir.")
                .Length(3, 4).WithMessage("CVV kodu 3 veya 4 haneli olmalıdır.");
        }
    }
}
