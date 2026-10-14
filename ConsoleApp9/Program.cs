using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ConsoleApp11
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

            DataView view = table.DefaultView;
            while (true)
            {
                Console.WriteLine("1. показать всех");
                Console.WriteLine("2. сортировать по имени");
                Console.WriteLine("3. студенты старше 38 лет");
                Console.Write("Выберите действие: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        view.RowFilter = "";
                        view.Sort = "id ASC";
                        ShowView(view);
                        break;

                    case "2":
                        view.Sort = "first_name ASC";
                        ShowView(view);
                        break;

                    case "3":
                        view.RowFilter = "age > 38"; // в моей бд нету студентов возрастом меньше 18 лет
                        view.Sort = "id ASC";
                        ShowView(view);
                        break;

                    default:
                        Console.WriteLine("неверный ввод");
                        break;
                }
            }
        }

        static void ShowView(DataView view)
        {
            Console.WriteLine($"\nтекущий фильтр: {view.RowFilter}, сортировка: {view.Sort}):");

            foreach (DataRowView rowView in view)
            {
                Console.WriteLine(
                    $"ID: {rowView["id"]}, " +
                    $"Имя: {rowView["first_name"]}, " +
                    $"Фамилия: {rowView["last_name"]}, " +
                    $"Возраст: {rowView["age"]}, " +
                    $"Ник: {rowView["nickname"]}");
            }
            Console.WriteLine($"\nКоличество строк: {view.Count}");
        }
    }
}