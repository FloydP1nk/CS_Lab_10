using MySql.Data.MySqlClient;

class Program
{
    static public string Parse(string inputString)
    {
        int lastCommaIndex = inputString.LastIndexOf(',');

        // Проверяем, что найден хотя бы один индекс запятой
        if (lastCommaIndex != -1)
        {
            int secondLastCommaIndex = inputString.LastIndexOf(',', lastCommaIndex - 1);

            // Проверяем, что найдены оба индекса
            if (secondLastCommaIndex != -1)
            {
                // Получаем подстроку между предпоследней и последней запятой
                string result =
                    inputString.Substring(secondLastCommaIndex + 1, lastCommaIndex - secondLastCommaIndex - 1);

                return result;
            }
            return "";
        }
        return "";
    }

    async static Task Main()
    {
        string connectionString = "server=localhost;user=root;password=dindon07;database=Lab_10;";

        MySqlConnection connection = new MySqlConnection(connectionString);

        try
        {
            connection.Open();
            Console.WriteLine("Соединение установлено!");
            List<String> Data = new List<string>(); // лист тикетов
            string filePath = "/Users/pavelerokhin/CS_Labs/Lab_09/ticker.txt";
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    Data.Add(reader.ReadLine());
                    Console.WriteLine(line);
                }
            }

            long id = 0;
            List<string> stonks = new List<string> { "up", "down" };
            Random random = new Random();


            foreach (string ticket in Data) // Проход по Data
            {
                int stonk = random.Next(2);
                HttpClient httpClient = new HttpClient();
                // string csvData;
                string url =
                    $"https://query1.finance.yahoo.com/v7/finance/download/{ticket}?period1={DateTimeOffset.Now.ToUnixTimeSeconds() - 86400}&period2={DateTimeOffset.Now.ToUnixTimeSeconds()}&interval=1d&events=history&includeAdjustedClose=true";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (StreamReader sr1 = new StreamReader(stream))
                {
                    string csvData = sr1.ReadToEnd();
                    string updateQuery =
                        "INSERT INTO Prices (id, tickerid, prices, todaysCondition) VALUES (@id, @TickerId, @Prices, @TodaysCondition)";
                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        // Установка значений параметров

                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@TickerId", ticket);
                        command.Parameters.AddWithValue("@Prices", Parse(csvData));
                        command.Parameters.AddWithValue("@TodaysCondition", stonks[stonk]);
                        id++;

                        // Выполнение команды
                        command.ExecuteNonQuery();
                    }
                }
            }

            connection.Close();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Ошибка подключения: " + ex.Message);
        }
    }
}