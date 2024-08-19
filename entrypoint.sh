#!/bin/bash

# Detect if the OS is Linux or Windows
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
  export CosmosDb__Account="https://cosmosdb:8081"
  export AzureStorage__ConnectionString="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite:10000/devstoreaccount1;QueueEndpoint=http://azurite:10001/devstoreaccount1;"
  export BankSimulator__BaseUrl="http://bank_simulator:8080"
elif [[ "$OSTYPE" == "msys" ]]; then
  export CosmosDb__Account="https://host.docker.internal:8081"
  export AzureStorage__ConnectionString="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10010/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10011/devstoreaccount1;"
  export BankSimulator__BaseUrl="http://host.docker.internal:8080"
fi

docker-compose up -d
