using Dapper;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Speech.Synthesis;

namespace Dictations
{
    public class TestWords
    {

        public enum WordCorrect
        {

            Correct = 1,
            NotCorrect = 2,
            NotDone = 3
        }

        public static void Execute(int skip, int take)
        {

            var synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            var correctCount = 0;
            var skippedCount = 0;
            using (var db = new SqlConnection(Config.ConnectionString))
            {

                var words = db.Query($@"SELECT Word,Id,CorrectCount
                            FROM words
                            WHERE CorrectCount = 0
                            ORDER BY CURRENT_TIMESTAMP
                            OFFSET {skip} ROWS
                            FETCH NEXT {take} ROWS ONLY;").ToList();

                foreach (var word in words)
                {
                    Console.WriteLine($"Word number {word.Id}");
                    synthesizer.Speak(word.Word);                    
                    var myword = Console.ReadLine().Trim() ?? "";

                    while (myword == "r")
                    {
                        synthesizer.Speak(word.Word);
                        myword = Console.ReadLine().Trim() ?? "";
                    }

                    Console.Write("Know?");
                    var know = Console.ReadLine().Trim() ?? "";
                    if (myword.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                    {
                        skippedCount++;
                        Console.WriteLine("======================================== \n");
                        continue;
                    }
                     
                    var isCorrect = word.Word.Equals(myword, StringComparison.InvariantCultureIgnoreCase);

                    if (isCorrect)
                        correctCount++;
                    else
                        Console.WriteLine($"Wrong. Correct {word.Word}");

                    var query = $@"update words set CorrectCount = '{(word.CorrectCount ?? 0) + ((isCorrect) ? 1 : -1)}',
                                    LasttestedOn = '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}', Know = '{(know == "" ? 1 : 0)}' where Id = {word.Id} ";
                    db.Execute(query);

                    Console.WriteLine("======================================== \n");
                }
                Console.WriteLine("Completed");
                Console.WriteLine($"Results is");
                Console.WriteLine($"Total questions\t{words.Count}");
                Console.WriteLine($"Correct questions\t{correctCount}");
                Console.WriteLine($"Skipped questions\t{skippedCount}");
            }
        }
    }
}
