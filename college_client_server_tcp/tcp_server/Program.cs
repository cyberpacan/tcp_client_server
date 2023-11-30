using System.Net.Sockets;
using System.Net;
using System.Text;

namespace tcp_server
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Loopback, 5000);

            try
            {
                server.Start();
                Console.WriteLine($"Сервер запущен по адресу {server.LocalEndpoint}. Ожидание подключений...");

                while(true)
                {
                    //прием входящих подключений
                    var client = await server.AcceptTcpClientAsync();
                    Console.WriteLine($"Входящее подключение: {client.Client.RemoteEndPoint}");
                    //создаем новую задачу в отдельном потоке для обслуживания клиента
                    Task.Run(async () => await WorkWithClientAsync(client));
                    //или
                    /*new Thread(async () => await WorkWithClientAsync(client)).Start();*/
                }
            }
            finally
            {
                Console.WriteLine("Сервер завершил работу");
                server.Stop();
            }
        }

        static async Task WorkWithClientAsync(TcpClient client)
        {
            var words = getDictionary();//получаем словарь

            //получаем поток взаимодействия между клиентом и сервером
            var stream = client.GetStream();
            //текстовый поток для чтения данных
            using var streamReader = new StreamReader(stream);
            //текстовый поток для отправки данных
            using var streamWriter = new StreamWriter(stream);

            while (true)
            {
                //считываем запрошенное слово до переноса строки
                var word = await streamReader.ReadLineAsync();

                if (word == "END") break;//завершаем работу сервера, если слово END

                Console.WriteLine($"Запрошен перевод слова {word}");
                //если слово не найдено
                if (word is null || !words.TryGetValue(word, out var translate)) translate = "не найдено в словаре";
                //отправляем перевод слова клиенту
                await streamWriter.WriteLineAsync(translate);
                await streamWriter.FlushAsync();//очищаем буффер текстового потока
            }
            client.Close();//завершаем работу с клиентом
        }

        static Dictionary<string, string> getDictionary()
        {
            Dictionary<string, string> words = new Dictionary<string, string>();
            string filePath = "words.txt";

            try
            {
                //считываем все данные из файла
                string[] lines = File.ReadAllLines(filePath);

                foreach(string line in lines )
                {
                    string[] parts = line.Split(';');//делим строку на ключ-значение по ";"
                    words.Add(parts[0], parts[1]);//записыаем ключ и значение в словарь
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            }
            return words;
        }
    }
}