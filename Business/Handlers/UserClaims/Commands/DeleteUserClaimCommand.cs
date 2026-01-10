using System.Threading;
using System.Threading.Tasks;
using Business.BusinessAspects;
using Business.Constants;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Caching;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using Core.Utilities.Results;
using DataAccess.Abstract;
using MediatR;

namespace Business.Handlers.UserClaims.Commands
{
    public class DeleteUserClaimCommand : IRequest<IResult>
    {
        public int Id { get; set; }


        public class DeleteUserClaimCommandHandler : IRequestHandler<DeleteUserClaimCommand, IResult>
        {
            private readonly IUserClaimRepository _userClaimRepository;
            private readonly ICacheManager _cacheManager;

            public DeleteUserClaimCommandHandler(IUserClaimRepository userClaimRepository, ICacheManager cacheManager)
            {
                _userClaimRepository = userClaimRepository;
                _cacheManager = cacheManager;
            }

            [SecuredOperation("Admin")]
            [CacheRemoveAspect("GetOrdersQuery")]
            [LogAspect(typeof(MsSqlLogger))]
            public async Task<IResult> Handle(DeleteUserClaimCommand request, CancellationToken cancellationToken)
            {
                var entityToDelete = await _userClaimRepository.GetAsync(x => x.UserId == request.Id);
                
                if (entityToDelete == null)
                {
                    return new ErrorResult("Kullanıcı claim'i bulunamadı.");
                }

                var userId = entityToDelete.UserId; // UserId'yi kaydet

                _userClaimRepository.Delete(entityToDelete);
                await _userClaimRepository.SaveChangesAsync();

                // Cache'i temizle - Yetki değişikliği yapıldığı için cache'i güncellemek gerekiyor
                _cacheManager.Remove($"{CacheKeys.UserIdForClaim}={userId}");

                return new SuccessResult(Messages.Deleted);
            }
        }
    }
}