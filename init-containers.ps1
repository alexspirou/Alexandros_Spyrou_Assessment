docker compose down -v --remove-orphans --rmi all
docker compose up -d --build

docker ps
docker inspect -f '{{.State.Health.Status}}' novibet-wallet-sqlserver
docker logs novibet-wallet-sqlserver --tail 20
docker logs novibet-wallet-redis --tail 20