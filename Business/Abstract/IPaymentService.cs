using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Results;
using Entities.Dtos;

namespace Business.Abstract
{
    public interface IPaymentService
    {
        Task<IResult> Pay(PaymentDto paymentDto);
    }
}
