using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp9
{
    public class Program
    {
        public static void Main()
        {
            string provider = "Npgsql";
            DbProviderFactories.RegisterFactory(
            provider,
            NpgsqlFactory.Instance);

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            using DbConnection connection = factory.CreateConnection() ?? throw new InvalidOperationException();
            string connString = "";
            connection.ConnectionString = connString;

            using DbDataAdapter adapter =
                factory.CreateDataAdapter()
                ?? throw new InvalidOperationException();

            adapter.SelectCommand = connection.CreateCommand();
            adapter.SelectCommand.CommandText = $"""
                SELECT id, nickname, first_name, last_name, age
                FROM students_lesson14
                ORDER BY id
                """;

            DataTable table = new DataTable();
            adapter.Fill(table);

            // 1
            ShowTabel(table);

            // 2
            table.Rows[0]["age"] = (int)table.Rows[0]["age"] + 1;

            // 3
            var newRow = table.NewRow();
            newRow["nickname"] = "nick";
            newRow["first_name"] = "123";
            newRow["last_name"] = "321";
            newRow["age"] = 21;
            table.Rows.Add(newRow);

            // 4
            table.Rows[1].Delete();

            // 5
            Console.WriteLine("\nDataTable до сохранения");
            ShowTabel(table);

            // 6
            SaveTable(table, factory, connString);

            // 7
            Console.WriteLine("\nDataTable после сохранения");
            ShowTabel(table);

            // 8
            Console.WriteLine("\nпроверка БД");
            CheckDatabase(factory, connString);

        }

        static void CheckDatabase(DbProviderFactory factory, string connString)
        {
            using var connection = factory.CreateConnection()!;
            connection.ConnectionString = connString;
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id, first_name, last_name, age FROM students_lesson14 ORDER BY id";
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id"]}, " +
                    $"Имя: {reader["first_name"]}, " +
                    $"Фамилия: {reader["last_name"]}, " +
                    $"Возраст: {reader["age"]}");
            }
        }

        static void SaveTable(DataTable table, DbProviderFactory factory, string connString)
        {
            using DbConnection connection = factory.CreateConnection()!;
            connection.ConnectionString = connString;
            connection.Open();

            using DbTransaction transaction = connection.BeginTransaction();
            using DbDataAdapter adapter = factory.CreateDataAdapter()!;

            InsertCommand(connection, transaction, adapter);
            UpdateCommand(connection, transaction, adapter);
            DeleteCommand(connection, transaction, adapter);

            try
            {
                int rowsAffected = adapter.Update(table);
                transaction.Commit();
                Console.WriteLine($"сохранено строк: {rowsAffected}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"ошибка сохранения: {ex.Message}");
            }
        }

        static void InsertCommand(DbConnection connection, DbTransaction transaction, DbDataAdapter adapter)
        {
            var InsertCommand = connection.CreateCommand();
            InsertCommand.Transaction = transaction;
            InsertCommand.CommandText = """
                INSERT INTO students_lesson14 (nickname, first_name, last_name, age) 
                VALUES (@nickname, @first_name, @last_name, @age) 
                RETURNING id;
                """;
            AddParameter(InsertCommand, "@nickname", "nickname");
            AddParameter(InsertCommand, "@first_name", "first_name");
            AddParameter(InsertCommand, "@last_name", "last_name");
            AddParameter(InsertCommand, "@age", "age");

            InsertCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
            adapter.InsertCommand = InsertCommand;
        }

        static void UpdateCommand(DbConnection connection, DbTransaction transaction, DbDataAdapter adapter)
        {
            var UpdateCommand = connection.CreateCommand();
            UpdateCommand.Transaction = transaction;
            UpdateCommand.CommandText = """
                UPDATE students_lesson14 
                SET nickname = @nickname, first_name = @first_name, last_name = @last_name, age = @age 
                WHERE id = @id;
                """;
            AddParameter(UpdateCommand, "@nickname", "nickname");
            AddParameter(UpdateCommand, "@first_name", "first_name");
            AddParameter(UpdateCommand, "@last_name", "last_name");
            AddParameter(UpdateCommand, "@age", "age");
            AddParameter(UpdateCommand, "@id", "id", DataRowVersion.Original);
            adapter.UpdateCommand = UpdateCommand;
        }

        static void DeleteCommand(DbConnection connection, DbTransaction transaction, DbDataAdapter adapter)
        {
            var DeleteCommand = connection.CreateCommand();
            DeleteCommand.Transaction = transaction;
            DeleteCommand.CommandText = "DELETE FROM students_lesson14 WHERE id = @id;";
            AddParameter(DeleteCommand, "@id", "id", DataRowVersion.Original);
            adapter.DeleteCommand = DeleteCommand;
        }

        static void AddParameter(DbCommand cmd, string paramName, string sourceColumn, DataRowVersion version = DataRowVersion.Current)
        {
            DbParameter p = cmd.CreateParameter();
            p.ParameterName = paramName;
            p.SourceColumn = sourceColumn;
            p.SourceVersion = version;
            cmd.Parameters.Add(p);
        }

        static void ShowTabel(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                DataRowVersion version = DataRowVersion.Current;
                if (row.RowState == DataRowState.Deleted)
                {
                    version = DataRowVersion.Original;
                }

                Console.WriteLine(
                    $"ID: {row["id", version]}, " +
                    $"Имя: {row["first_name", version]}, " +
                    $"Фамилия: {row["last_name", version]}, " +
                    $"Возраст: {row["age", version]}, " +
                    $"Состояние: {row.RowState}");
            }
            Console.WriteLine($"Количество строк: {table.Rows.Count}");
        }
    }
}