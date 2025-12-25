using BiologyLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId}: Начало работы");

        // 1. Коллекцию из 1000 существ
        var creatures = new List<Creature>();
        var random = new Random();

        for (int i = 0; i < 1000; i++)
        {
            creatures.Add(new Creature
            {
                Id = i + 1,
                Name = $"Creature_{i + 1}",
                CanFly = random.Next(0, 2) == 1 // 50% chance to fly
            });
        }

        // 2. Сервис и MemoryStream
        var streamService = new StreamService<Creature>();
        var memoryStream = new MemoryStream();

        // 3. Progress-репортер
        var progress = new Progress<string>(msg => Console.WriteLine(msg));

        // 4. Запуск методов синхронно с задержкой
        Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId}: Запуск потоков 1 и 2");

        var writeTask = streamService.WriteToStreamAsync(memoryStream, creatures, progress);
        Thread.Sleep(200); // Задержка 200 мс
        var copyTask = streamService.CopyFromStreamAsync(memoryStream, "creatures_data.txt", progress);

        // 5. Ожидаем завершения
        await Task.WhenAll(writeTask, copyTask);

        // 6. Получаем статистику
        int flyingCount = await streamService.GetStatisticsAsync(
            "creatures_data.txt",
            c => c.CanFly);

        Console.WriteLine($"Количество существ, умеющих летать: {flyingCount}");

        // 7. Закрывает поток
        memoryStream.Close();
    }
}
