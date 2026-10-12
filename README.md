# CLDV6212 Group 2 POE Part 1

## CoffeeNChill Canteen Management System

## Group Members

* Nehal – Project setup, Azurite and Docker
* Rahul – Menu management
* Emile – Staff documents and Postman

## Setup

### Azurite

Run Azurite using Docker:

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

Azurite is used for local Azure Storage.

### Azure Functions

Open the project in Visual Studio and run the Functions project.

The Functions project is configured to use the local Azurite storage connection.

### Docker

Build the Functions image:

```bash
docker build -t <dockerhub_username>/coffeenchill-functions:v1.0 .
```

Run the Functions container:

```bash
docker run -p 7071:80 <dockerhub_username>/coffeenchill-functions:v1.0
```

## Endpoints

### Menu

* POST `/api/menu`
* GET `/api/menu`
* GET `/api/menu/category/{category}`
* PUT `/api/menu/{category}/{id}`
* DELETE `/api/menu/{category}/{id}`

### Staff Documents

* POST `/api/documents/upload`
* GET `/api/documents`
* GET `/api/documents/download/{fileName}`

## Postman

The Postman collection is located in:

`/docs`

## Group Work

**Nehal**

* Project setup
* Azurite configuration
* Docker setup

**Rahul**

* MenuItems table
* Menu CRUD functions

**Emile**

* Azure Blob Storage
* Staff document functions
* Postman testing

## Video

YouTube video:
`<Unlisted YouTube link>`
