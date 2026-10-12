# CoffeeNChill – Cloud Development B Part 2

## Project Overview

CoffeeNChill is a cloud-based coffee shop application developed as part of the Cloud Development B practical assignment. The project uses Azure Functions and Azure Storage services to manage menu items, staff documents, and customer orders.

Part 2 extends the existing Part 1 application by introducing an asynchronous order-processing workflow using Azure Storage Queues and Azure Table Storage. Docker Compose is used to run the application and its storage emulator together.

## Project Objectives

- Maintain the existing Part 1 functionality.
- Manage menu items using Azure Table Storage.
- Upload, list, and download staff documents using Azure File Storage.
- Accept customer orders through an HTTP-triggered Azure Function.
- Place valid orders into an Azure Storage Queue for asynchronous processing.
- Process queued orders and store their details in Azure Table Storage.
- Track order statuses through the processing lifecycle.
- Containerize the application and storage emulator using Docker Compose.
- Test the application's endpoints using Postman.
- Publish the application image to Docker Hub.

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
- Update existing menu items.
- Delete menu items.

**Staff Document Management**
- Upload staff documents.
- List stored staff documents.
- Download stored documents.

### Part 2 – Order Processing

**Order Submission**
- Accept order details through an HTTP POST endpoint.
- Validate required order information.
- Submit valid orders to the `order-processing-queue` Azure Storage Queue.

**Asynchronous Order Processing**
- Consume messages from the order-processing queue.
- Validate and process queued orders.
- Store order records in the `Orders` Azure Table.
- Use the order date as the partition key and the order ID as the row key.
- Track order status transitions:
  - Received
  - Preparing
  - Ready
  - Collected
- Log processing activity and handle failures.

**Containerization**
- Run the Azure Functions application and Azurite through Docker Compose.
- Persist emulator data using a Docker volume.
- Use the published Docker Hub application image.
- Start the application stack with a single Docker Compose command.

## Project Structure

```text
CoffeeNChill-Part2-Individual/
├── CoffeeNChill.Functions/
│   └── CoffeeNChill.Functions/
│       ├── Models/
│       ├── Services/
│       ├── MenuFunctions.cs
│       ├── OrderFunctions.cs
│       ├── Program.cs
│       └── CoffeeNChill.Functions.csproj
├── docs/
│   └── Postman collection and testing resources
├── docker-compose.yml
├── Dockerfile
└── README.md
```

*The structure above will be updated to reflect the final repository layout.*

## Running the Application

The application is intended to run using Docker Compose, with Azurite providing local Azure Storage emulation.

Once the Docker configuration and published image are finalized, the application will be started from the repository root using:

```bash
docker compose up
```

The exact setup instructions, configuration requirements, endpoints, and verification steps will be documented here after the containerized application has been tested successfully.

## API Testing

Postman will be used to test the application's HTTP endpoints, including the existing menu and staff-document operations and the new order-processing workflow.

The Postman collection will be stored in the `docs` directory. Tests will cover valid requests, invalid input, order submission, and order status verification.

## Docker Hub

The published Docker Hub image will be linked here once the final Part 2 image has been built, tested, and pushed.

- **Docker Hub image:** To be added after publication.

## Demonstration Video

An unlisted YouTube demonstration will be linked here once the final implementation has been tested. The demonstration will cover starting the application using Docker Compose, pulling the published image, submitting an order through Postman, and verifying order processing and status changes.

- **YouTube demonstration:** To be added.

## Code Attribution

Third-party code, documentation, libraries, and other resources used in this project will be acknowledged in accordance with the assignment requirements. Detailed code attributions and supporting references will be maintained in the project's separate documentation.

## Project Status

Implementation and testing are in progress. This README will be updated as the features, container configuration, Postman collection, and demonstration are completed.
