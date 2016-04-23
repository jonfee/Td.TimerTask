using Microsoft.Data.Entity;

namespace Td.Kylin.DataCache.Context
{
    internal sealed class PostgreSqlDataContext : DataContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseNpgsql(CacheStartup.SqlConnctionString);
        }
    }
}
