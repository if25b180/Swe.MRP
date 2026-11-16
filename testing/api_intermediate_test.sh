#!/usr/bin/env bash

# This script was written with the values from the postman collection
# Imported into bruno (Free and open source Postman alternative) -> bruno can generate curl commands :)

# /media/{mediaId} might need to be updated after running once,
# as it is serial and the counter doesn't reset after deleting
MEDIA_ID=1

# Register User
curl --request POST \
  --url http://localhost:8080/api/users/register \
  --header 'Content-Type: application/json' \
  --data '{
  "username": "user1",
  "password": "pass123"
}'

# Login User
curl --request POST \
  --url http://localhost:8080/api/users/login \
  --header 'Content-Type: application/json' \
  --data '{
  "username": "user1",
  "password": "pass123"
}'

# Create Media Entry
curl --request POST \
  --url http://localhost:8080/api/media \
  --header 'Content-Type: application/json' \
  --data '{
  "title": "Inception",
  "description": "Sci-fi thriller",
  "mediaType": "movie",
  "releaseYear": 2010,
  "genres": [
    "sci-fi",
    "thriller"
  ],
  "ageRestriction": 12
}'

# Update Media Entry
curl --request PUT \
  --url http://localhost:8080/api/media/$MEDIA_ID \
  --header 'Content-Type: application/json' \
  --data '{
  "title": "Inception updated",
  "description": "Updated description",
  "mediaType": "movie",
  "releaseYear": 2010,
  "genres": [
    "sci-fi",
    "action"
  ],
  "ageRestriction": 16
}'

# Get Media by ID
curl --request GET \
  --url http://localhost:8080/api/media/$MEDIA_ID

# Delete Media Entry
curl --request DELETE \
  --url http://localhost:8080/api/media/$MEDIA_ID
