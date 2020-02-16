using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.SQLite.EF6.Migrations
{
    public interface ISQLiteDdlBuilderFactory
    {
        ISQLiteDdlBuilder GetSQLiteDdlBuilder();
    }
}
