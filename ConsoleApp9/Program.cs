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
                SELECT id, nickname, first_name, last_name
                FROM students_lesson13
                ORDER BY id
                LIMIT 10
                """;

            DataTable table = new DataTable();
            adapter.Fill(table);

            // 1
            ShowTabel(table);
            Console.WriteLine();

            // 2
            Console.WriteLine("2: новая строка");
            
            DataRow newRow = table.NewRow();
            newRow["id"] = 999;
            newRow["nickname"] = "321";
            newRow["first_name"] = "123";
            newRow["last_name"] = "321";

            Console.WriteLine($"состояние: {newRow.RowState}");
            Console.WriteLine();

            // 3
            Console.WriteLine("3: добавление строки в таблицу");
            
            table.Rows.Add(newRow);
            
            Console.WriteLine($"состояние: {newRow.RowState}");
            Console.WriteLine();
            //ShowTabel(table);

            // 4
            Console.WriteLine("4: изменение имени у первого студента");

            table.Rows[0]["first_name"] = "newName";

            Console.WriteLine($"новое имя: {table.Rows[0]["first_name"]}, состояние: {table.Rows[0].RowState}");
            //ShowTabel(table);
            Console.WriteLine();

            // 5
            Console.WriteLine("5: Удаление второго студента");

            table.Rows[1].Delete();
            Console.WriteLine($"состояние: {table.Rows[1].RowState}");
            //ShowTabel(table);
            Console.WriteLine();

            // 7
            Console.WriteLine("7: вызов RejectChanges()");
            
            table.RejectChanges();
            
            ShowTabel(table);
            Console.WriteLine();

            // 8
            Console.WriteLine("8: вызов AcceptChanges()");

            DataRow newRow2 = table.NewRow();
            newRow2["id"] = 111;
            newRow2["nickname"] = "OOOOO";
            newRow2["first_name"] = "777";
            newRow2["last_name"] = "888";
            table.Rows.Add(newRow2);

            table.Rows[0]["first_name"] = "888";
            table.Rows[1].Delete();

            table.AcceptChanges();
            ShowTabel(table);
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