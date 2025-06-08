using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BiologyLibrary
{
    public class StreamService<T>
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task WriteToStreamAsync(Stream stream, IEnumerable<T> data, IProgress<string> progress)
        {
            await _semaphore.WaitAsync();
            try
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                progress?.Report($"Поток {threadId}: Начало записи в поток");

                var writer = new StreamWriter(stream);
                await writer.WriteLineAsync("Id,Name,CanFly"); // заголовок

                int count = 0;
                int total = 0;
                foreach (var item in data)
                {
                    total++;
                }

                foreach (var item in data)
                {
                    await writer.WriteLineAsync(item.ToString());
                    count++;
                    await Task.Delay(30); // Задержка для имитации долгой записи (~3 сек на 100 элементов)

                    int percent = (int)((double)count / total * 100);
                    progress?.Report($"Поток {threadId}: Прогресс записи: {percent}%");
                }

                await writer.FlushAsync();
                progress?.Report($"Поток {threadId}: Запись завершена");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CopyFromStreamAsync(Stream stream, string fileName, IProgress<string> progress)
        {
            await _semaphore.WaitAsync();
            try
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                progress?.Report($"Поток {threadId}: Начало копирования из потока в файл");

                stream.Position = 0; // Возврат в начало потока

                using (var fileStream = File.Create(fileName))
                {
                    int bufferSize = 1024;
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    long totalBytes = stream.Length;
                    long totalBytesRead = 0;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        int percent = (int)((double)totalBytesRead / totalBytes * 100);
                        progress?.Report($"Поток {threadId}: Прогресс копирования: {percent}%");
                    }
                }

                progress?.Report($"Поток {threadId}: Копирование завершено");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<int> GetStatisticsAsync(string fileName, Func<T, bool> filter)
        {
            int count = 0;

            using (var reader = new StreamReader(fileName))
            {
                await reader.ReadLineAsync(); // Пропуск заголовка

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // парсинг
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        var creature = new Creature
                        {
                            Id = int.Parse(parts[0].Split(':')[1].Trim()),
                            Name = parts[1].Split(':')[1].Trim(),
                            CanFly = bool.Parse(parts[2].Split(':')[1].Trim())
                        };

                        if (filter((T)(object)creature))
                            count++;
                    }
                }
            }

            return count;
        }
    }
}