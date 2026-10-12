# CoffeeNChill – Cloud Development B Part 2

## Project Overview

CoffeeNChill is a cloud-based coffee shop application developed for the Cloud Development B practical assignment. It uses Azure Functions and Azure Storage services to manage menu items, staff documents, and customer orders.

Part 2 extends the existing Part 1 application with asynchronous order processing using Azure Storage Queues and Azure Table Storage. Docker Compose is used to run the application and Azurite storage emulator together.

## Technologies Used

- C# and .NET
- Azure Functions v4 – isolated worker model
- Azure Table Storage
- Azure Storage Queues
- Azure File Storage
- Azurite Storage Emulator
- Docker and Docker Compose
- Docker Hub
- Postman
- Git and GitHub

## Application Features

### Part 1 – Existing Functionality

**Menu Management**
- Create menu items.
- Retrieve all menu items.
- Retrieve menu items by category.
- Update menu items.
- Delete menu items.

**Staff Document Management**
- Upload staff documents.
- List stored staff documents.
- Download stored documents.

### Part 2 – Order Processing

The planned order-processing workflow will:

- Accept customer orders through an HTTP POST endpoint.
- Validate order details before queueing.
- Send valid orders to the `order-processing-queue` Azure Storage Queue.
- Process queued orders using an Azure Functions queue trigger.
- Store orders in the `Orders` Azure Table.
- Use the order date as the partition key and order ID as the row key.
- Track order statuses through Received, Preparing, Ready, and Collected.
- Log processing activity and handle failures.

### Containerization

The application will be configured to run alongside Azurite using Docker Compose, with persistent storage for emulator data and a published Docker Hub image.

## Existing API Endpoints

### Menu

- `POST /api/menu`
- `GET /api/menu`
- `GET /api/menu/category/{category}`
- `PUT /api/menu/{category}/{id}`
- `DELETE /api/menu/{category}/{id}`

### Staff Documents

- `POST /api/documents/upload`
- `GET /api/documents`
- `GET /api/documents/download/{fileName}`

The Part 2 order-processing endpoints and testing instructions will be added after implementation and verification.

## Project Structure

```text
CoffeeNChill-Part2/
├── CoffeeNChill.Functions/
│   ├── CoffeeNChill.Functions.slnx
│   └── CoffeeNChill.Functions/
│       ├── Models/
│       ├── Services/
│       ├── Properties/
│       ├── Program.cs
│       ├── MenuFunctions.cs
│       └── CoffeeNChill.Functions.csproj
├── docs/
│   ├── CoffeeNChill.postman_collection.json
│   ├── CoffeeNChill.postman_environment.json
│   └── Supporting documentation
├── docker-compose.yml
├── .gitignore
└── README.md
```

## Running the Application

The target startup command for the containerized application is:

```bash
docker compose up
```

The final startup instructions and configuration requirements will be documented after the containerized application has been tested.

## API Testing

Postman will be used to test the existing menu and staff-document endpoints and the new order-processing workflow. The collection and environment files are maintained in the `docs` directory.

Testing will cover valid and invalid requests, order submission, queue processing, and order status verification.

## Docker Hub

- **Published Part 2 image:** To be added after the image is built, tested, and published.

## Demonstration Video

An unlisted YouTube demonstration will be linked here after recording. It will show the Docker Compose startup, use of the published image, order submission through Postman, and verification of order processing.

- **YouTube demonstration:** To be added.

## Code Attribution

Third-party code, documentation, libraries, and other resources will be acknowledged according to the assignment requirements. Detailed code attributions and references will be maintained in the separate project documentation.

## Project Status

The existing Part 1 project has been added to the individual repository. Part 2 implementation, integration testing, container publication, and demonstration recording remain in progress.
