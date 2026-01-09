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
            // 1. Sepet listesi boş olamaz
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Sipariş vermek için sepette en az bir ürün olmalıdır.")
                .NotNull().WithMessage("Sepet bilgisi gönderilmedi.");
            // 2. Listenin içindeki her bir elemanı kontrol et (Advanced!)
            // Sepetteki her bir "item" için şu kuralları işlet:
            RuleForEach(x => x.Items).ChildRules(items  =>
            {
                items.RuleFor(x => x.ProductId)
                    .GreaterThan(0).WithMessage("Geçersiz ürün ID'si.");
                items.RuleFor(x => x.Count)
                    .GreaterThan(0).WithMessage("Ürün adedi 0'dan büyük olmalıdır.")
                    .LessThan(100).WithMessage("Bir üründen en fazla 99 adet sipariş verebilirsiniz.");
            });
        }
    }
}
