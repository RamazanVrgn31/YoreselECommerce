using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Core.Utilities.Helpers.FileHelper
{
    public interface IFileHelper
    {
        // Dosyayı alır, kaydeder ve kaydettiği yolun string'ini (örn: images/logo.png) döner.
        string Upload(IFormFile file, string root);

        //Eski dosyayı siler.
        void Delete(string filePath);
        //Güncelleme: Eskiyi siler,yeniyi yükler
        string Update(IFormFile file, string filePath, string root);

    }
}
