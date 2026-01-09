using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nest;

namespace Core.Utilities.Helpers.FileHelper
{
    public class FileHelperManeger : IFileHelper
    {
        public void Delete(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string Update(IFormFile file, string filePath, string root)
        {
            //Eski dosyayı sil
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            //Yeni dosyayı yükle
            return Upload(file,root);
        }

        public string Upload(IFormFile file, string root)
        {
            if (file.Length>0)
            {
                //Klasör yoksa oluştur
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }
                //Dosyanın uzantısını al(örn: .png,.jpg)
                string extension = Path.GetExtension(file.FileName);

                //Benzersiz bir isim oluştur(Guid)
                string guid = Guid.NewGuid().ToString();

                //Yeni dosya adı: guid+uzantı
                string newFileName = guid + extension;

                //Tam dosya yolu
                string filePath = Path.Combine(root, newFileName);

                //Dosyayı Kopyala
                using (FileStream fileStream = File.Create(filePath))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }

                //Kaydedilen dosya yolunu döner
                return newFileName;
            }

            return null;
        }
    }
}
