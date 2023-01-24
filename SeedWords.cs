using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace Dictations
{
    public class SeedWords
    {
        private static List<string> actualWords = new();
        static void WordList()
        {
            var words = File.ReadAllLines(@$"{Config.FileBasePath}\seedWords.txt");
            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word))
                    continue;

                var number = word.Split("\t")[0];
                if (string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number))
                    continue;
                if (int.TryParse(number, out var x))
                {
                    actualWords.Add(word.Split("\t")[1]);
                }

            }
        }

        public static void Execute()
        {

            WordList();
            using (var db = new SqlConnection(Config.ConnectionString))
            {
                db.Open();

                foreach (var item in actualWords)
                {
                    var result = db.QuerySingle<int>($"SELECT count(*) from words where word = '{item}'");
                    if (result == 0)
                        db.Execute($"Insert into words(Word,CorrectCount) values ('{item}',0)");
                }
            }
            System.Console.WriteLine("Seed is completed...");
        }
    }
}
