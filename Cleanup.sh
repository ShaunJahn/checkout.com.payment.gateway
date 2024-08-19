docker-compose down --volumes --remove-orphans
rm -rf ./azurite
rm -rf ./data/cosmosdb
rm -rf ./seq-data
rm -rf ./test-results
rm -rf ./logs
echo "Cleanup completed!"