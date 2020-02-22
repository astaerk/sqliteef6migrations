using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Diagnostics;

namespace System.Data.SQLite.EF6.Migrations
{
    /// <summary>
    /// Migration Ddl generator for SQLite
    /// </summary>
    public class SQLiteMigrationSqlGenerator : MigrationSqlGenerator
    {

        protected const string BATCHTERMINATOR = ";\r\n";

        protected ISQLiteDdlBuilderFactory _sqliteDdlBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteMigrationSqlGenerator"/> class with the default <see cref="SQLiteDdlBuilderFactory"/>.
        /// </summary>
        public SQLiteMigrationSqlGenerator()
            : this(new SQLiteDdlBuilderFactory())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteMigrationSqlGenerator"/> class.
        /// </summary>
        /// <param name="sqliteDdlBuilderFactory">a factory that is able to create instances of <see cref="ISQLiteDdlBuilder"/></param>
        public SQLiteMigrationSqlGenerator(ISQLiteDdlBuilderFactory sqliteDdlBuilderFactory)
        {
            _sqliteDdlBuilderFactory = sqliteDdlBuilderFactory;

            base.ProviderManifest = ((DbProviderServices)(new SQLiteProviderFactory()).GetService(typeof(DbProviderServices))).GetProviderManifest("");
        }

        /// <summary>
        /// Converts a set of migration operations into database provider specific SQL.
        /// </summary>
        /// <param name="migrationOperations">The operations to be converted.</param>
        /// <param name="providerManifestToken">Token representing the version of the database being targeted.</param>
        /// <returns>
        /// A list of SQL statements to be executed to perform the migration operations.
        /// </returns>
        public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
        {
            List<MigrationStatement> migrationStatements = new List<MigrationStatement>();

            foreach (MigrationOperation migrationOperation in migrationOperations)
                migrationStatements.Add(GenerateStatement(migrationOperation));
            return migrationStatements;
        }

        protected virtual MigrationStatement GenerateStatement(MigrationOperation migrationOperation)
        {
            MigrationStatement migrationStatement = new MigrationStatement();
            migrationStatement.BatchTerminator = BATCHTERMINATOR;
            migrationStatement.Sql = GenerateSqlStatement(migrationOperation);
            return migrationStatement;
        }

        private string GenerateSqlStatement(MigrationOperation migrationOperation)
        {
            dynamic concreteMigrationOperation = migrationOperation;
            return GenerateSqlStatementConcrete(concreteMigrationOperation);
        }

        private string GenerateSqlStatementConcrete(MigrationOperation migrationOperation)
        {
            Debug.Assert(false);
            return string.Empty;
        }


        #region History operations

        protected virtual string GenerateSqlStatementConcrete(HistoryOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();

            foreach (DbModificationCommandTree commandTree in migrationOperation.CommandTrees)
            {
                List<DbParameter> parameters;
                // Take care because here we have several queries so we can't use parameters...
                switch (commandTree.CommandTreeKind)
                {
                    case DbCommandTreeKind.Insert:
                        ddlBuilder.AppendSql(SQLiteDmlBuilder.GenerateInsertSql((DbInsertCommandTree)commandTree, out parameters, true));
                        break;
                    case DbCommandTreeKind.Delete:
                        ddlBuilder.AppendSql(SQLiteDmlBuilder.GenerateDeleteSql((DbDeleteCommandTree)commandTree, out parameters, true));
                        break;
                    case DbCommandTreeKind.Update:
                        ddlBuilder.AppendSql(SQLiteDmlBuilder.GenerateUpdateSql((DbUpdateCommandTree)commandTree, out parameters, true));
                        break;
                    case DbCommandTreeKind.Function:
                    case DbCommandTreeKind.Query:
                    default:
                        throw new InvalidOperationException(string.Format("Command tree of type {0} not supported in migration of history operations", commandTree.CommandTreeKind));
                }
                ddlBuilder.AppendSql(BATCHTERMINATOR);
            }

            return ddlBuilder.GetCommandText();

        }

        #endregion

        #region Move operations (not supported by Jet)

        protected virtual string GenerateSqlStatementConcrete(MoveProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Move operations not supported by SQLite");
        }

        protected virtual string GenerateSqlStatementConcrete(MoveTableOperation migrationOperation)
        {
            throw new NotSupportedException("Move operations not supported by SQLite");
        }

        #endregion


        #region Procedure related operations (not supported by Jet)
        protected virtual string GenerateSqlStatementConcrete(AlterProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }

        protected virtual string GenerateSqlStatementConcrete(CreateProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }


        protected virtual string GenerateSqlStatementConcrete(DropProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }


        protected virtual string GenerateSqlStatementConcrete(RenameProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }

        #endregion


        #region Rename operations


        protected virtual string GenerateSqlStatementConcrete(RenameColumnOperation migrationOperation)
        {
            throw new NotSupportedException("Cannot rename objects with Jet");
        }

        protected virtual string GenerateSqlStatementConcrete(RenameIndexOperation migrationOperation)
        {
            throw new NotSupportedException("Cannot rename objects with Jet");
        }

        protected virtual string GenerateSqlStatementConcrete(RenameTableOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();

            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            ddlBuilder.AppendSql(" RENAME TO ");
            ddlBuilder.AppendIdentifier(migrationOperation.NewName);

            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Columns
        protected virtual string GenerateSqlStatementConcrete(AddColumnOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();

            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" ADD COLUMN ");

            ColumnModel column = migrationOperation.Column;

            ddlBuilder.AppendIdentifier(column.Name);
            ddlBuilder.AppendSql(" ");
            TypeUsage storeType = ProviderManifest.GetStoreType(column.TypeUsage);
            ddlBuilder.AppendType(storeType, column.IsNullable ?? true, column.IsIdentity);
            ddlBuilder.AppendNewLine();


            return ddlBuilder.GetCommandText();
        }

        protected virtual string GenerateSqlStatementConcrete(DropColumnOperation migrationOperation)
        {
            throw new NotSupportedException("Drop column not supported by SQLite");
        }

        protected virtual string GenerateSqlStatementConcrete(AlterColumnOperation migrationOperation)
        {
            throw new NotSupportedException("Alter column not supported by SQLite");
        }

        #endregion


        #region Foreign keys creation

        protected virtual string GenerateSqlStatementConcrete(AddForeignKeyOperation migrationOperation)
        {

            /* 
             * SQLite supports foreign key creation only during table creation
             * At least, during table creation we could try to create relationships but it
             * Requires that we sort tables in dependency order (and that there is not a circular reference
             *
             * Actually we do not create relationship at all
            */

            return "";
        }

        #endregion

        #region Primary keys creation

        protected virtual string GenerateSqlStatementConcrete(AddPrimaryKeyOperation migrationOperation)
        {
            // Actually primary key creation is supported only during table creation

            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();
            ddlBuilder.AppendSql(" PRIMARY KEY (");
            ddlBuilder.AppendIdentifierList(migrationOperation.Columns);
            ddlBuilder.AppendSql(")");
            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Table operations

        protected virtual string GenerateSqlStatementConcrete(AlterTableOperation migrationOperation)
        {
            /* 
             * SQLite does not support alter table
             * We should rename old table, create the new table, copy old data to new table and drop old table
            */

            throw new NotSupportedException("Alter column not supported by SQLite");

        }

        protected virtual string GenerateSqlStatementConcrete(CreateTableOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();

            ddlBuilder.AppendSql("CREATE TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            ddlBuilder.AppendSql(" (");
            ddlBuilder.AppendNewLine();

            bool first = true;
            string autoincrementColumnName = null;
            foreach (ColumnModel column in migrationOperation.Columns)
            {
                if (first)
                    first = false;
                else
                    ddlBuilder.AppendSql(",");

                ddlBuilder.AppendSql(" ");
                ddlBuilder.AppendIdentifier(column.Name);
                ddlBuilder.AppendSql(" ");
                if (column.IsIdentity)
                {
                    autoincrementColumnName = column.Name;
                    ddlBuilder.AppendSql(" integer constraint ");
                    ddlBuilder.AppendIdentifier(ddlBuilder.CreateConstraintName("PK", migrationOperation.Name));
                    ddlBuilder.AppendSql(" primary key autoincrement");
                    ddlBuilder.AppendNewLine();
                }
                else
                {
                    TypeUsage storeTypeUsage = ProviderManifest.GetStoreType(column.TypeUsage);
                    ddlBuilder.AppendType(storeTypeUsage, column.IsNullable ?? true, column.IsIdentity);
                    ddlBuilder.AppendNewLine();
                }

            }

            if (migrationOperation.PrimaryKey != null && autoincrementColumnName == null)
            {
                ddlBuilder.AppendSql(",");
                ddlBuilder.AppendSql(GenerateSqlStatementConcrete(migrationOperation.PrimaryKey));
            }

            ddlBuilder.AppendSql(")");

            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Index

        protected virtual string GenerateSqlStatementConcrete(CreateIndexOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();
            ddlBuilder.AppendSql("CREATE ");
            if (migrationOperation.IsUnique)
                ddlBuilder.AppendSql("UNIQUE ");
            ddlBuilder.AppendSql("INDEX ");
            ddlBuilder.AppendIdentifier(SQLiteProviderManifestHelper.GetFullIdentifierName(migrationOperation.Table, migrationOperation.Name));
            ddlBuilder.AppendSql(" ON ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" (");
            ddlBuilder.AppendIdentifierList(migrationOperation.Columns);
            ddlBuilder.AppendSql(")");

            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Drop

        protected virtual string GenerateSqlStatementConcrete(DropForeignKeyOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();
            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.PrincipalTable);
            ddlBuilder.AppendSql(" DROP CONSTRAINT ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            return ddlBuilder.GetCommandText();

        }

        protected virtual string GenerateSqlStatementConcrete(DropPrimaryKeyOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();
            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" DROP CONSTRAINT ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            return ddlBuilder.GetCommandText();
        }

        protected virtual string GenerateSqlStatementConcrete(DropIndexOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();
            ddlBuilder.AppendSql("DROP INDEX ");
            ddlBuilder.AppendIdentifier(SQLiteProviderManifestHelper.GetFullIdentifierName(migrationOperation.Table, migrationOperation.Name));
            ddlBuilder.AppendSql(" ON ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            return ddlBuilder.GetCommandText();
        }

        protected virtual string GenerateSqlStatementConcrete(DropTableOperation migrationOperation)
        {
            ISQLiteDdlBuilder ddlBuilder = _sqliteDdlBuilderFactory.GetSQLiteDdlBuilder();
            ddlBuilder.AppendSql("DROP TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            return ddlBuilder.GetCommandText();
        }

        #endregion

    }
}
