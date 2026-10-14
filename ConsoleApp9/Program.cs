using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
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
                SELECT id, nickname, first_name, last_name
                FROM students_lesson13
                ORDER BY id
                LIMIT 10
                """;

            DataTable table = new DataTable();
            adapter.Fill(table);

            ShowTabel(table);
            Console.WriteLine();

            // 1
            table.Rows[0]["first_name"] = "Тимофей";

            // 2
            DataRow newRow = table.NewRow();
            newRow["first_name"] = "Мария";
            newRow["last_name"] = "Иванова";
            table.Rows.Add(newRow);

            // 3
            table.Rows[1].Delete();

            // 4
            ShowChangelog(table);

            // 5
            Console.WriteLine("\nвызов RejectChanges()\n");
            table.RejectChanges();
            ShowTabel(table);
        }
        static void ShowChangelog(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                switch (row.RowState)
                {
                    case DataRowState.Modified:
                        {
                            string oldName = row["first_name", DataRowVersion.Original].ToString()!;
                            string newName = row["first_name", DataRowVersion.Current].ToString()!;
                            Console.WriteLine($"[MODIFIED] {oldName} → {newName}");
                            break;
                        }
                    case DataRowState.Added:
                        {
                            string firstName = row["first_name", DataRowVersion.Current].ToString()!;
                            string lastName = row["last_name", DataRowVersion.Current].ToString()!;
                            Console.WriteLine($"[ADDED] {firstName} {lastName}");
                            break;
                        }
                    case DataRowState.Deleted:
                        {
                            string firstName = row["first_name", DataRowVersion.Original].ToString()!;
                            string lastName = row["last_name", DataRowVersion.Original].ToString()!;
                            Console.WriteLine($"[DELETED] {firstName} {lastName}");
                            break;
                        }
                }
            }
        }

        static void ShowTabel(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                Console.WriteLine(
                    $"ID: {row["id"]}, " +
                    $"Имя: {row["first_name"]}, " +
                    $"Фамилия: {row["last_name"]}, " +
                    $"Состояние: {row.RowState} ");
            }
            Console.WriteLine($"Количество строк: {table.Rows.Count}");
        }
    }
}