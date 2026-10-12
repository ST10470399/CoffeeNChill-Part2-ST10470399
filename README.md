# CoffeeNChill — Cloud Development B Part 2

## Project Overview

CoffeeNChill is a cloud-based coffee shop application developed for the Cloud Development B practical assignment. It uses Azure Functions and Azure Storage services to manage menu items, staff documents, and customer orders.

Part 2 extends the Part 1 application with asynchronous order processing using Azure Storage Queues and Azure Table Storage. Docker Compose runs the Azure Functions application alongside the Azurite storage emulator.

## Technologies Used

- C# and .NET
- Azure Functions v4 — isolated worker model
- Azure Storage Queues
- Azure Table Storage
- Azure File Storage
- Azurite Storage Emulator
- Docker and Docker Compose
- Docker Hub
- Postman
- Git and GitHub

## Application Features

### Part 1 — Menu Management

- Create menu items.
- Retrieve all menu items.
- Retrieve menu items by category.
- Update menu items.
- Delete menu items.

### Part 1 — Staff Document Management

- Upload staff documents.
- List stored staff documents.
- Download stored documents.

### Part 2 — Asynchronous Order Processing

- Accept customer orders through an HTTP POST endpoint.
- Validate incoming order details.
- Queue valid orders in the `order-processing-queue` Azure Storage Queue.
- Process queued orders using an Azure Functions queue trigger.
- Store order records in the `Orders` Azure Table.
- Use the order date as the partition key and the order ID as the row key.
- Track order statuses through `Received`, `Preparing`, `Ready`, and `Collected`.
- Log queue processing and order status updates.

### Containerization

- Run the Functions application and Azurite using Docker Compose.
- Use persistent storage for Azurite data.
- Pull the published Part 2 Functions image from Docker Hub.

## API Endpoints

The API base URL for local testing is:

`http://localhost:8080/api`

### Menu

| Method | Endpoint |
|---|---|
| POST | `/menu` |
| GET | `/menu` |
| GET | `/menu/category/{category}` |
| PUT | `/menu/{category}/{id}` |
| DELETE | `/menu/{category}/{id}` |

### Staff Documents

| Method | Endpoint |
|---|---|
| POST | `/documents/upload` |
| GET | `/documents` |
| GET | `/documents/download/{fileName}` |

### Orders

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/orders/queue` | Validate and submit a new order for asynchronous processing |
| PUT | `/orders/{orderDate}/{orderId}/status` | Update an order's status manually |

The queue-processing function consumes orders from `order-processing-queue` and updates their records in the `Orders` table as processing progresses.

## Project Structure

```text
cldv6212-group-2-poe-part-2-group-1/
├── CoffeeNChill.Functions/
│   └── CoffeeNChill.Functions/
│       ├── Models/
│       │   └── Order.cs
│       ├── Services/
│       │   ├── OrderQueueService.cs
│       │   └── OrderTableService.cs
│       ├── OrderFunctions.cs
│       ├── Program.cs
│       └── CoffeeNChill.Functions.csproj
├── docs/
│   ├── CoffeeNChill.postman_collection.json
│   └── CoffeeNChill.postman_environment.json
├── docker-compose.yml
├── .gitignore
└── README.md
```

## Running the Application

Ensure Docker Desktop is running and open a terminal in the project root directory.

Pull the configured images:

```bash
docker compose pull
```

Start the application and Azurite:

```bash
docker compose up -d
```

Check the running containers:

```bash
docker compose ps
```

View the application logs:

```bash
docker compose logs -f
```

To stop the containers:

```bash
docker compose down
```

The Functions API is exposed locally on port `8080`. Azurite provides the local Azure Storage services used by the application.

## API Testing

The Postman collection and environment files are maintained in the `docs` directory.

Testing can cover:

- Menu item creation, retrieval, updates, and deletion.
- Staff document upload, listing, and download.
- Valid and invalid order submissions.
- Queue acceptance and asynchronous processing.
- Order status verification in Azure Storage Explorer.

The order-processing workflow was tested using a sample order. Application logs showed the order progressing through `Received`, `Preparing`, `Ready`, and `Collected`, and the final `Collected` status was verified in Azure Storage Explorer.

## Docker Hub

The published Part 2 Functions image is:

`st10470399nehalroopnarain/coffeenchill-functions:v2.3`

Docker Compose is configured to use this published image for the Functions application.

## Demonstration Video

An unlisted YouTube demonstration will be linked here after recording. It should demonstrate Docker Compose startup, the published Docker Hub image, order submission, queue processing, and order status verification.

**YouTube demonstration:** To be added.

## Code Attribution

Third-party code, documentation, libraries, and other resources must be acknowledged according to the assignment requirements. Supporting references and code attributions should be included in the relevant project documentation.

## Project Status

The Part 1 functionality has been retained and integrated with the Part 2 asynchronous order-processing implementation. The application has been built and tested in Docker with Azurite, and the published Functions image has been used to verify the order-processing lifecycle.

The README and demonstration video should be finalized before submission.