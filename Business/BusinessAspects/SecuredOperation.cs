using System.Collections.Generic;
using System.Linq;
using System.Security;
using Business.Constants;
using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Caching;
using Core.Extensions;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using DataAccess.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Business.BusinessAspects
{
    /// <summary>
    /// This Aspect control the user's roles in HttpContext by inject the IHttpContextAccessor.
    /// It is checked by writing as [SecuredOperation] on the handler.
    /// If a valid authorization cannot be found in aspect, it throws an exception.
    /// </summary>
    public class SecuredOperation : MethodInterception
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheManager _cacheManager;


        public SecuredOperation()
        {
            _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>();
            _cacheManager = ServiceTool.ServiceProvider.GetService<ICacheManager>();
        }

        protected override void OnBefore(IInvocation invocation)
        {
            //var userId = _httpContextAccessor.HttpContext?.User.Claims
            //    .FirstOrDefault(x => x.Type.EndsWith("nameidentifier"))?.Value;

            //if (userId == null)
            //{
            //    throw new SecurityException(Messages.AuthorizationsDenied);
            //}

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();

            if (userId == null)
            {
                throw new SecurityException(Messages.AuthorizationsDenied);
            }

            // 2. Cache Key oluştur
            var cacheKey = $"{CacheKeys.UserIdForClaim}={userId}";



            // 3. Cache'den yetkileri çek
            var oprClaims = _cacheManager.Get<IEnumerable<string>>(cacheKey);

            if (oprClaims == null || !oprClaims.Any())
            {
                // ServiceTool ile UserRepository'i çağır (Dependency Injection)
                var userRepository = ServiceTool.ServiceProvider.GetService<IUserRepository>();

                // Veritabanından yetkileri taze taze çek
                var claims = userRepository.GetClaims(userId.Value);

                // Sadece isimlerini al (string listesi olarak)
                oprClaims = claims.Select(x => x.Name).ToList();

                // Bulduklarımızı Cache'e yaz ki bir dahakine veritabanını yormayalım (60 dakika sakla)
                _cacheManager.Add(cacheKey, oprClaims, 60);
            }


            //var operationName = invocation.TargetType.ReflectedType.Name;
            // DeclaringType (Interface) değil, TargetType (Gerçek Sınıf) ismini alıyoruz
            var operationName = invocation.TargetType.Name;

            // --- DÜZELTME: NULL KONTROLÜ ---
            // Eğer oprClaims NULL ise (yani veri yoksa), Contains çağırma!
            if (oprClaims != null && oprClaims.Contains(operationName))
            {
                return;
            }

            // Null ise veya yetki yoksa hata fırlat
            throw new SecurityException(Messages.AuthorizationsDenied);
        }
    }
}