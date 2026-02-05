using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                    CreateIfNotExists = true)]
    public SensorData[] Run(
        [EventHubTrigger("%EventHub:EventHubName%", Connection = "EventHub:ConnectionString")] string[] messages)
    {
        _logger.LogInformation($"Processing batch of {messages.Length} messages from Event Hub");

        var sensorDataList = new List<SensorData>();

        foreach (var message in messages)
        {
            try
            {
                _logger.LogInformation($"Message received: {message}");

                var sensorData = JsonSerializer.Deserialize<SensorData>(message);
                
                if (sensorData == null)
                {
                    _logger.LogWarning("Failed to deserialize message");
                    continue;
                }

                _logger.LogInformation($"Saving sensor data to Cosmos DB - ID: {sensorData.id}, Sensor: {sensorData.SensorId}");
                
                sensorDataList.Add(sensorData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
                // Continue processing other messages even if one fails
            }
        }

        _logger.LogInformation($"Successfully processed {sensorDataList.Count} messages");
        
        // Return array of documents to be saved to Cosmos DB
        return sensorDataList.ToArray();
    }
}

public class SensorData
{
    [JsonPropertyName("Id")]
    public string? id { get; set; }  // Maps to "Id" from JSON, stores as "id" for Cosmos DB
    public int MessageNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string? SensorId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}



