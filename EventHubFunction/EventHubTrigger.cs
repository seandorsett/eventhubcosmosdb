using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventHubFunction;

public class EventHubTrigger
{
    private readonly ILogger<EventHubTrigger> _logger;

    public EventHubTrigger(ILogger<EventHubTrigger> logger)
    {
        _logger = logger;
    }

    [Function("EventHubTrigger")]
    [CosmosDBOutput("%CosmosDB:DatabaseName%", 
                    "%CosmosDB:ContainerName%",
                    Connection = "CosmosDB:ConnectionString",
                    CreateIfNotExists = true,
                    PartitionKey = "/id")]
    public SensorData Run(
        [EventHubTrigger("%EventHub:EventHubName%", Connection = "EventHub:ConnectionString")] string[] messages)
    {
        _logger.LogInformation($"Processing batch of {messages.Length} messages from Event Hub");

        // Process the first message in the batch (for simplicity)
        // In production, you'd want to process all messages
        var message = messages[0];
        
        _logger.LogInformation($"Message received: {message}");

        try
        {
            var sensorData = JsonSerializer.Deserialize<SensorData>(message);
            
            if (sensorData == null)
            {
                _logger.LogWarning("Failed to deserialize message");
                return new SensorData();
            }

            _logger.LogInformation($"Saving sensor data to Cosmos DB - ID: {sensorData.Id}, Sensor: {sensorData.SensorId}");
            
            // Return the object to be saved to Cosmos DB via output binding
            return sensorData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing message: {ex.Message}");
            throw;
        }
    }
}

public class SensorData
{
    public string? id { get; set; }
    public string? Id { get; set; }
    public int MessageNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string? SensorId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}
