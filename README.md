# Azure Event Hub + Azure Functions + Cosmos DB POC

This repository contains a proof-of-concept (POC) that demonstrates how to:
1. Send messages from a console application to Azure Event Hub
2. Process messages with an Azure Function triggered by Event Hub
3. Persist messages to Azure Cosmos DB

## Architecture

```
Console App (EventHubProducer)
    ↓ (sends messages)
Azure Event Hub
    ↓ (triggers)
Azure Function (EventHubFunction)
    ↓ (persists)
Azure Cosmos DB
```

## Prerequisites

Before running this POC, you need:

1. **.NET 8.0 SDK** or later installed
2. **Azure Subscription** with the following resources:
   - Azure Event Hub Namespace and Event Hub
   - Azure Cosmos DB Account with SQL API
   - Azure Storage Account (for Azure Functions)
3. **Azure Functions Core Tools** (optional, for local debugging)

## Azure Resources Setup

### 1. Create an Event Hub

1. Create an Event Hub Namespace in Azure Portal
2. Create an Event Hub within the namespace (e.g., "sensor-events")
3. Get the connection string from "Shared access policies"

### 2. Create a Cosmos DB Account

1. Create a Cosmos DB account with SQL API
2. Create a database (e.g., "SensorDatabase")
3. Create a container (e.g., "SensorData") with partition key `/id`
4. Get the connection string from "Keys" section

### 3. Create a Storage Account

1. Create a Storage Account (required for Azure Functions)
2. Get the connection string

## Configuration

### Console App (EventHubProducer)

Edit `EventHubProducer/appsettings.json`:

```json
{
  "EventHub": {
    "ConnectionString": "Endpoint=sb://YOUR_NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=...",
    "EventHubName": "YOUR_EVENT_HUB_NAME"
  }
}
```

### Azure Function (EventHubFunction)

1. Copy `EventHubFunction/local.settings.json.template` to `EventHubFunction/local.settings.json`
2. Edit `EventHubFunction/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "YOUR_STORAGE_ACCOUNT_CONNECTION_STRING",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "EventHub:ConnectionString": "YOUR_EVENT_HUB_CONNECTION_STRING",
    "EventHub:EventHubName": "YOUR_EVENT_HUB_NAME",
    "CosmosDB:ConnectionString": "YOUR_COSMOS_DB_CONNECTION_STRING",
    "CosmosDB:DatabaseName": "SensorDatabase",
    "CosmosDB:ContainerName": "SensorData"
  }
}
```

## Running the POC

### Step 1: Restore Dependencies

```bash
# Restore EventHubProducer
cd EventHubProducer
dotnet restore

# Restore EventHubFunction
cd ../EventHubFunction
dotnet restore
```

### Step 2: Run the Azure Function

```bash
cd EventHubFunction
dotnet build
func start
# OR if you don't have Azure Functions Core Tools:
dotnet run
```

The function will start listening for messages from Event Hub.

### Step 3: Run the Console App

Open a new terminal:

```bash
cd EventHubProducer
dotnet run
```

Follow the prompts to send messages to Event Hub. Example:

```
Event Hub Message Producer
==========================

Connected to Event Hub: sensor-events
Type 'quit' to exit

Enter message count to send (or 'quit' to exit): 5
✓ Successfully sent 5 messages to Event Hub
```

### Step 4: Verify in Cosmos DB

1. Open Azure Portal
2. Navigate to your Cosmos DB account
3. Go to Data Explorer
4. Check the "SensorData" container for the persisted messages

## Project Structure

```
eventhubcosmosdb/
├── EventHubProducer/          # Console application
│   ├── Program.cs             # Main producer logic
│   ├── appsettings.json       # Configuration
│   └── EventHubProducer.csproj
│
├── EventHubFunction/          # Azure Function
│   ├── EventHubTrigger.cs     # Event Hub trigger function
│   ├── Program.cs             # Function host setup
│   ├── host.json              # Function host configuration
│   ├── local.settings.json.template
│   └── EventHubFunction.csproj
│
└── README.md
```

## Message Format

The console app generates sensor data messages with the following format:

```json
{
  "Id": "guid",
  "MessageNumber": 1,
  "Timestamp": "2024-01-01T12:00:00Z",
  "SensorId": "Sensor-5",
  "Temperature": 25.67,
  "Humidity": 65.43
}
```

## How It Works

1. **EventHubProducer**: 
   - Generates simulated sensor data (temperature and humidity)
   - Serializes data to JSON
   - Sends messages to Azure Event Hub in batches

2. **EventHubFunction**:
   - Triggered automatically when messages arrive in Event Hub
   - Deserializes JSON messages
   - Uses Cosmos DB output binding to persist data
   - Logs processing information

3. **Cosmos DB**:
   - Stores messages with automatic indexing
   - Partitioned by `id` for scalability
   - Provides query capabilities for analysis

## Troubleshooting

### Common Issues

1. **Connection String Errors**: Ensure all connection strings are properly configured in appsettings.json and local.settings.json

2. **Function Not Triggering**: 
   - Verify Event Hub connection string has "Listen" permissions
   - Check that the Event Hub name matches exactly

3. **Cosmos DB Write Errors**:
   - Verify Cosmos DB connection string and container name
   - Ensure the container exists with partition key `/id`

4. **Build Errors**: Run `dotnet restore` in both project directories

## Next Steps

To enhance this POC:

- Add error handling and retry policies
- Implement batch processing for better throughput
- Add monitoring and alerting with Application Insights
- Implement message schemas with Azure Schema Registry
- Add data validation and transformation logic
- Deploy to Azure using Azure DevOps or GitHub Actions

## License

This is a demonstration project for educational purposes.
