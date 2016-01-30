using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Genesis.Core.Data
{
    public class EFCodeConfig : DbConfiguration
    {
        public EFCodeConfig()
        {
            SetProviderServices("System.Data.SqlClient", System.Data.Entity.SqlServer.SqlProviderServices.Instance);
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }
}