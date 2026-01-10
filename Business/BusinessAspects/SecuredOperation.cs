using System;
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
using MassTransit.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Business.BusinessAspects
{
    /// <summary>
    /// This Aspect control the user's roles in HttpContext by inject the IHttpContextAccessor.
    /// It is checked by writing as [SecuredOperation] on the handler.
    /// If a valid authorization cannot be found in aspect, it throws an exception.
    /// </summary>
    public class SecuredOperation : MethodInterception
    {

        private string[] _roles;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheManager _cacheManager;

        public SecuredOperation(string roles)
        {
            _roles = roles.Split(',');
            _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>();
            _cacheManager = ServiceTool.ServiceProvider.GetService<ICacheManager>();
        }
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

            // 2. Eğer hiç rol belirtilmediyse ([SecuredOperation]), sadece login yeterli.
            if (_roles == null || _roles.Length == 0)
            {
                return;
            }

            // 2. Cache Key oluştur
            var cacheKey = $"{CacheKeys.UserIdForClaim}={userId}";



            // 3. Cache'den yetkileri çek
            var oprClaims = _cacheManager.Get<IEnumerable<string>>(cacheKey);

            if (oprClaims == null || !oprClaims.Any())
            {
                // ServiceTool ile UserRepository'i çağır (Dependency Injection)
                var userRepository = ServiceTool.ServiceProvider.GetService<IUserRepository>();

                if (userRepository == null)
                {
                    throw new SecurityException("Yetki kontrolü yapılamadı. Sistem hatası.");
                }

                // Veritabanından yetkileri taze taze çek
                // Normal kullanıcıların hiç claim'i olmayabilir (boş liste döner)
                var claims = userRepository.GetClaims(userId.Value);

                // Sadece isimlerini al (string listesi olarak)
                // Eğer kullanıcının hiç yetkisi yoksa boş liste döner
                oprClaims = claims?.Select(x => x.Name).ToList() ?? new List<string>();

                // Bulduklarımızı Cache'e yaz ki bir dahakine veritabanını yormayalım (60 dakika sakla)
                // Boş liste bile olsa cache'e yaz (bir dahakine DB'ye gitmesin)
                _cacheManager.Add(cacheKey, oprClaims, 60);
            }


            //var operationName = invocation.TargetType.ReflectedType.Name;
            // DeclaringType (Interface) değil, TargetType (Gerçek Sınıf) ismini alıyoruz
            //var operationName = invocation.TargetType.Name;

            // 4. Kullanıcının belirtilen rollerden en az birine sahip olup olmadığını kontrol et
            // Normal kullanıcıların hiç claim'i yoksa (boş liste), buraya düşmez çünkü
            // zaten parametresiz [SecuredOperation] kullanıldığında yukarıda return ediliyor.
            // Buraya geldiyse demek ki bir rol belirtilmiş ve yetki kontrolü yapılacak.
            
            if (oprClaims == null || !oprClaims.Any())
            {
                // Kullanıcının hiç yetkisi yok ve bir rol bekleniyor, erişim reddedilir
                throw new SecurityException(Messages.AuthorizationsDenied);
            }

            // İstenen rollerden herhangi birini kullanıcıda bulursak erişim izni ver
            foreach (var role in _roles)
            {
                var trimmedRole = role?.Trim();
                
                if (string.IsNullOrEmpty(trimmedRole))
                    continue;

                // Kullanıcının yetkilerinde bu rol var mı kontrol et
                if (oprClaims.Any(claim => 
                    string.Equals(claim?.Trim(), trimmedRole, StringComparison.OrdinalIgnoreCase)))
                {
                    return; // Yetki var, erişim izni ver
                }
            }

            // Hiçbir rol bulunamadıysa erişim reddedilir
            throw new SecurityException(Messages.AuthorizationsDenied);
        }
    }
}