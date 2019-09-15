
namespace System.Data.SQLite.EF6.Migrations
{
    public class SQLiteDdlBuilderFactory : ISQLiteDdlBuilderFactory
    {
        public ISQLiteDdlBuilder GetSQLiteDdlBuilder()
        {
            return new SQLiteDdlBuilder();
        }
    }
}
