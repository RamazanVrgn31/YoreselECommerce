using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Business.Events;
using Core.Utilities.Mail;
using MassTransit;

namespace Business.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreateEvent>
    {
        private readonly IMailService _mailService;

        public OrderCreatedEventConsumer(IMailService mailService)
        {
            _mailService = mailService;
        }

        public async Task Consume(ConsumeContext<OrderCreateEvent> context)
        {
            // RabbitMQ'dan gelen mesajın içeriği:
            var orderId = context.Message.OrderId;
            var amount = context.Message.TotalAmount;

            //------------------------------------------------
            var siparis = context.Message;


            // 2. Dosyayı oku
            string body = "";
            try
            {
                // 1. Bu kodun çalıştığı Assembly'i (Business.dll) al
                var assembly = Assembly.GetExecutingAssembly();

                // 2. Gömülü dosyanın tam "Namespace" adını bulmamız lazım.
                // Genelde şöyledir: "ProjeAdi.KlasörYolu.DosyaAdi.Uzantisi"
                // Klasörler nokta (.) ile ayrılır.
                string resourceName = "Business.Consumers.Email_Templates.SiparisOnay.html";

                // 3. Dosyayı akış (Stream) olarak oku
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        // Eğer null geliyorsa resourceName yanlıştır.
                        // Loglara mevcut isimleri yazdırıp kontrol edebilirsin:
                        // var names = string.Join(", ", assembly.GetManifestResourceNames());
                        // Console.WriteLine("Mevcut Kaynaklar: " + names);
                        throw new Exception("HTML Şablonu Assembly içinde bulunamadı!");
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        body = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                // Dosya bulunamazsa logla veya basit bir text gönder (Fail-safe)
                Console.WriteLine("Mail şablonu bulunamadı: " + ex.Message);
                body = $"Sipariş alındı: {siparis.OrderId}. Tutar: {siparis.TotalAmount}";
            }

            // 3. Placeholders (Yer tutucuları) gerçek verilerle değiştir
            body = body.Replace("{{MusteriAdi}}", "Değerli Müşterimiz") // İsim verisi event'te yoksa sabit yazabilirsin
                       .Replace("{{SiparisNo}}", siparis.OrderId.ToString())
                       .Replace("{{Tutar}}", $"{siparis.TotalAmount:C2}"); // Para birimi formatı
            //----------------------------------------------------
            // Mail içeriği oluşturma
            var email = new EmailMessage
            {
                Subject = "Siparişiniz Alındı",
                // Gerçekte User tablosundan gelir
                ToAddresses = {new EmailAddress { Name="Müşteri", Address ="musteri@ornek.com"} },
                Content=body
                //Content = $@"
                //    <h3>Merhaba, Siparişiniz Onaylandı!</h3>
                //    <p>Sipariş Numaranız: <strong>{orderId}</strong></p>
                //    <p>Toplam Tutar: <strong>{amount:C2}</strong></p>
                //    <p>Bizi tercih ettiğiniz için teşekkürler.</p>"

            };

            // Maili Gönder (Core katmanındaki MailManager çalışacak)
             _mailService.Send(email);

            // Konsola da yazalım ki çalıştığını görelim
            System.Console.WriteLine($"[RabbitMQ] Sipariş {orderId} için mail gönderildi!");

            await Task.CompletedTask;
        }
    }
}
