using System.Net.Sockets;
using System.Text;

namespace tcp_client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            using TcpClient client = new TcpClient();
            Console.WriteLine("Клиент запущен");
            await client.ConnectAsync("127.0.0.1", 5000);//подключение к серверу
            var words = new string[] { "red", "green", "yellow", "blue" };//массив со словами(запросом) пользователя для перевода

            if (client.Connected)
            {
                Console.WriteLine($"Подключение к {client.Client.RemoteEndPoint} установлено");

                var stream = client.GetStream();
                //текстовый поток для чтения данных
                using var streamReader = new StreamReader(stream);
                //текстовый поток для отправки данных
                using var streamWriter = new StreamWriter(stream);

                foreach (var word in words)
                {
                    await streamWriter.WriteLineAsync(word);//отправляем слово на сервер для перевода
                    await streamWriter.FlushAsync();//очищаем буффер
                    //получаем перевод слова
                    var translate = await streamReader.ReadLineAsync();
                    
                    Console.WriteLine($"Запрошенное слово: {word}\r\nПеревод: {translate}\n");
                }

                await streamWriter.WriteLineAsync("END");//отправляем маркер окончания работы с клиентом
                await streamWriter.FlushAsync();
                Console.WriteLine("Все сообщения отправлены");
            }
            else
            {
                Console.WriteLine("Не удалось подключиться");
            }
        }
    }
}