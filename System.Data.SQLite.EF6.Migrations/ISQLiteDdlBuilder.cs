using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.SQLite.EF6.Migrations
{
    public interface ISQLiteDdlBuilder
    {
        string GetCommandText();

        void AppendStringLiteral(string literalValue);

        void AppendIdentifier(string identifier);

        void AppendIdentifierList(IEnumerable<string> identifiers);

        void AppendType(EdmProperty column);

        void AppendType(TypeUsage typeUsage, bool isNullable, bool isIdentity);

        /// <summary>
        /// Appends raw SQL into the string builder.
        /// </summary>
        /// <param name="text">Raw SQL string to append into the string builder.</param>
        void AppendSql(string text);

        /// <summary>
        /// Appends raw SQL into the string builder.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="p">The p.</param>
        void AppendSql(string format, params object[] p);

        /// <summary>
        /// Appends new line for visual formatting or for ending a comment.
        /// </summary>
        void AppendNewLine();

        string CreateConstraintName(string constraint, string objectName);
    }
}
