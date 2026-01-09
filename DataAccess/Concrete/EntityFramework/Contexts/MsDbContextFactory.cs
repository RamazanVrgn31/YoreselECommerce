using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DataAccess.Concrete.EntityFramework.Contexts
{
    class MsDbContextFactory : IDesignTimeDbContextFactory<MsDbContext>
    {
        public MsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MsDbContext>();

            // Replace with your actual connection string
            var connectionString = "Server=DESKTOP-HVFH6CK\\SQLEXPRESS;Database=YoreselEcommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString);

            // Constructor IConfiguration istediği için boş bir tane oluşturup veriyoruz.
            // Zaten yukarıda UseSqlServer dediğimiz için içeride tekrar config okumaya çalışmayacak.
            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder.Build();

            return new MsDbContext(optionsBuilder.Options, configuration);
        }
    }
}
