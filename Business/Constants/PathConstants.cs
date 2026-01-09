using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Constants
{
    public class PathConstants
    {
        // Resimlerin yükleneceği ana klasör yolu.
        // FileHelper bu yolu kullanıp, yoksa klasörü kendi oluşturacak.

        public static string ImagesPath = "wwwroot\\Uploads\\Images\\";
        public static string DocumentsPath = "wwwroot\\Uploads\\Documents\\";
    }
}
