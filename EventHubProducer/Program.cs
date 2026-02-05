using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EventHubProducer;

class Program
{
    static async Task Main(string[] args)
    {
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration["EventHub:ConnectionString"];
        var eventHubName = configuration["EventHub:EventHubName"];

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName))
        {
            Console.WriteLine("Error: Event Hub connection string or name not configured in appsettings.json");
            return;
        }

        Console.WriteLine("Event Hub Message Producer");
        Console.WriteLine("==========================\n");

        // Create a producer client that you can use to send events to an event hub
        await using var producerClient = new EventHubProducerClient(connectionString, eventHubName);

        Console.WriteLine($"Connected to Event Hub: {eventHubName}");
        Console.WriteLine("Type 'quit' to exit\n");

        while (true)
        {
            Console.Write("Enter message count to send (or 'quit' to exit): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (!int.TryParse(input, out int messageCount) || messageCount <= 0)
            {
                Console.WriteLine("Please enter a valid positive number.\n");
                continue;
            }

            await SendMessagesAsync(producerClient, messageCount);
        }

        Console.WriteLine("\nShutting down...");
    }

    static async Task SendMessagesAsync(EventHubProducerClient producerClient, int messageCount)
    {
        try
        {
            var successCount = 0;

            for (int i = 1; i <= messageCount; i++)
            {
                var message = new
                {
                    Id = Guid.NewGuid().ToString(),
                    MessageNumber = i,
                    Timestamp = DateTime.UtcNow,
                    SensorId = $"Sensor-{Random.Shared.Next(1, 11)}",
                    Temperature = Math.Round(Random.Shared.NextDouble() * 40 + 10, 2),
                    Humidity = Math.Round(Random.Shared.NextDouble() * 100, 2)
                };

                var jsonMessage = JsonSerializer.Serialize(message);
                var eventData = new EventData(jsonMessage);

                // Create a batch for each message (simpler approach for this POC)
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
                
                if (!eventBatch.TryAdd(eventData))
                {
                    throw new Exception($"Message {i} is too large to fit in a batch");
                }

                await producerClient.SendAsync(eventBatch);
                successCount++;
            }

            Console.WriteLine($"✓ Successfully sent {successCount} messages to Event Hub\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error sending messages: {ex.Message}\n");
        }
    }
}

