#!/bin/bash

echo "========================================="
echo "  TICKETMASTER SYSTEM STATUS"
echo "========================================="
echo ""

echo "📊 MYSQL DATABASE - Seat Inventory:"
echo "-----------------------------------"
docker exec systemdesign-mysql-db-1 mysql -uroot -prootpassword -e "USE ticketdb; SELECT * FROM Seats;" 2>/dev/null
echo ""

echo "🔐 REDIS - Active Locks:"
echo "-----------------------------------"
LOCKS=$(docker exec systemdesign-redis-cache-1 redis-cli KEYS "lock:seat:*")
if [ -z "$LOCKS" ]; then
    echo "No active seat locks"
else
    echo "$LOCKS" | while read -r key; do
        if [ ! -z "$key" ]; then
            VALUE=$(docker exec systemdesign-redis-cache-1 redis-cli GET "$key")
            TTL=$(docker exec systemdesign-redis-cache-1 redis-cli TTL "$key")
            echo "  $key -> Held by: $VALUE (Expires in: ${TTL}s)"
        fi
    done
fi
echo ""

echo "💾 REDIS - Cache Status:"
echo "-----------------------------------"
CACHE=$(docker exec systemdesign-redis-cache-1 redis-cli GET "all_seats")
if [ -z "$CACHE" ]; then
    echo "Cache is empty (will be populated on next GET request)"
else
    echo "Cache exists:"
    echo "$CACHE" | python3 -m json.tool 2>/dev/null || echo "$CACHE"
fi
echo ""

echo "🐳 DOCKER CONTAINERS:"
echo "-----------------------------------"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep systemdesign
echo ""

echo "🌐 API ENDPOINTS:"
echo "-----------------------------------"
echo "  Swagger UI: http://localhost:5277/swagger"
echo "  GET Seats:  curl http://localhost:5277/api/ticket/seats"
echo "  Book Seat:  curl -X POST 'http://localhost:5277/api/ticket/book?seatId=A1&userId=UserX'"
echo ""
