using Castle.DynamicProxy;
using Core.Aspects.Autofac.Exception;
using Core.Aspects.Autofac.Logging;
using Core.CrossCuttingConcerns.Logging.Serilog.Loggers;
using System;
using System.Linq;
using System.Reflection;

namespace Core.Utilities.Interceptors
{
    public class AspectInterceptorSelector : IInterceptorSelector
    {
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            var classAttributes = type.GetCustomAttributes<MethodInterceptionBaseAttribute>(true).ToList();
            var methodAttributes =
                type.GetMethods()?.Where(p => p.Name == method.Name).FirstOrDefault().GetCustomAttributes<MethodInterceptionBaseAttribute>(true);
            if (methodAttributes != null)
            {
                classAttributes.AddRange(methodAttributes);
            }
            // LogAspect'te kullanılan logger tipini bul
            Type loggerType = typeof(FileLogger); // Varsayılan logger

            // Önce method attribute'larından LogAspect'in constructor parametresini oku
            var methodInfo = type.GetMethods()?.FirstOrDefault(p => p.Name == method.Name);
            var attributeData = methodInfo?.GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType == typeof(LogAspect))
                ?? type.GetCustomAttributesData()
                    .FirstOrDefault(a => a.AttributeType == typeof(LogAspect));

            // LogAspect bulunduysa, logger tipini constructor parametresinden al
            if (attributeData != null && attributeData.ConstructorArguments.Count > 0)
            {
                var loggerTypeArg = attributeData.ConstructorArguments[0];
                if (loggerTypeArg.Value is Type)
                {
                    loggerType = (Type)loggerTypeArg.Value;
                }
            }

            // ExceptionLogAspect'i bulunan logger tipi ile ekle
            classAttributes.Add(new ExceptionLogAspect(loggerType));
            //classAttributes.Add(new ExceptionLogAspect(typeof(FileLogger)));
            return classAttributes.OrderBy(x => x.Priority).ToArray();
        }
    }
}