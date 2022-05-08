# Getting Started

## Database Provisioning

The following instructions are used for provisioning the CosmosDB database on th localhost:

```
az cosmosdb database create --db-name Zytonomy --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081"

az cosmosdb collection create --db-name Zytonomy --collection-name Core --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081" --partition-key-path /PartitionKey
```

## Configuration Notes
|Setting|Notes|
|--|--|
|`AzureWebJobsStorage`|This setting should use the local storage for development otherwise it will not signal correctly.|
|`ContentSourceStorage`|This setting must use the remote storage even for development because the URL needs to be visible to Q&A Maker|

## Running

### Start Azurite

To run with Azurite, perform the following commands:

```
cd api/zytonomy.api/azurite
azurite -s                    # To start in silent mode; it's noisy otherwise.
```

It is necessary to run this before starting the functions runtime.

### Start the Runtime

Execute the following commands to start the backend.

```
cd api/zytonomy.api
dotnet restore                # Restore nuget packages (1st run)
dotnet build                  # Build to make sure everything is OK (1st run)
func start                    # Start the runtime
```

You may need to reload VS Code after `dotnet restore`.  Use `CTRL+SHIFT+P` and type "Reload" to find the command.

To start with hot reload

```
dotnet watch msbuild /t:RunFunctions
```

### Start the Frontend

To start the front, end, run:

```
cd web
yarn                          # Restore packages (1st run)
yarn dev                      # Start the runtime
```

The application should be available at `http://localhost:3000`

### CosmosDB Name

The CosmosDB name is configured in two places:

1. In the `local.settings.json`, the `Env_Suffix` variable holds an environment variable name.
2. When the value is present, it will be appended in `CosmosGateway.cs` to create the database name.

This allows development against a shared CosmosDB instance if a local one is not available (macOS)