# RabbitMQ Demo with .NET & Docker (RabbitMQDemoWithDocker)

A simple project I built to understand how **RabbitMQ messaging works with .NET**.

The project demonstrates asynchronous communication between an **ASP.NET Core Web API (Producer)** and a **.NET Worker Service (Consumer)** using RabbitMQ.

Everything can be started easily using **Docker Compose**.

## How It Works

The application has three main components:

```text
Client
   |
   | POST /api/Messages
   v
Producer API
   |
   | Publish Message
   v
RabbitMQ
   |
   | demo-queue
   v
Consumer Worker
   |
   | Process Message
   v
  ACK
```

### Producer API

The ASP.NET Core Web API receives an HTTP request and publishes the message to RabbitMQ.

### RabbitMQ

RabbitMQ acts as the message broker.

The message is placed in the `demo-queue` and waits there until a consumer is available to process it.

### Consumer Worker

The .NET Worker Service listens to `demo-queue`.

When a message arrives, the consumer:

1. Receives the message
2. Deserializes it
3. Processes it
4. Sends an acknowledgement (`ACK`) to RabbitMQ

---

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- .NET Worker Service
- RabbitMQ
- RabbitMQ.Client
- Docker
- Docker Compose

---

## Project Structure

```text
RabbitMQDemoWithDocker
│
├── ProducerApi
│   ├── Controllers
│   │   └── MessagesController.cs
│   │
│   ├── Services
│   │   ├── IRabbitMqProducer.cs
│   │   └── RabbitMqProducer.cs
│   │
│   ├── Dockerfile
│   ├── Program.cs
│   └── appsettings.json
│
├── ConsumerWorker
│   ├── Dockerfile
│   ├── Program.cs
│   ├── Worker.cs
│   └── appsettings.json
│
├── Shared
│   └── Models
│       ├── MessageDto.cs
│       └── SendMessageRequest.cs
│
├── docker-compose.yml
├── .dockerignore
└── README.md
```

---

## Prerequisites

To run the complete application, you only need:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

Make sure Docker Desktop is running before starting the application.

You do **not** need to install RabbitMQ manually.

If you want to modify or develop the project locally, you can also install:

- .NET 10 SDK
- Visual Studio 2022 or later

---

## Getting Started

### 1. Clone the Repository

```bash
git clone <your-repository-url>

cd RabbitMQDemoWithDocker
```

Or download the repository as a ZIP file and extract it.

---

### 2. Start the Application

From the project root, run:

```bash
docker compose up --build
```

Docker Compose will automatically:

- Download the RabbitMQ image if required
- Build the Producer API
- Build the Consumer Worker
- Create the Docker network
- Start RabbitMQ
- Start the Producer API
- Start the Consumer Worker

You should have three running containers:

```text
producer-api
rabbitmq-demo
consumer-worker
```

You can verify them using:

```bash
docker ps
```

---

## Send a Message

The Producer API is available at:

```text
http://localhost:5100
```

If Swagger is enabled, open:

```text
http://localhost:5100/swagger/index.html
```

Use:

```http
POST /api/Messages
```

Example request:

```json
{
  "text": "Hello RabbitMQ!"
}
```

Example response:

```json
{
  "message": "Message sent to RabbitMQ successfully.",
  "data": {
    "id": "2e2d82d6-b541-4ae4-8c91-5f3b50c4dce4",
    "text": "Hello RabbitMQ!",
    "createdAt": "2026-07-23T05:03:11Z"
  }
}
```

The message is now published to RabbitMQ.

---

## View Consumer Output

The Consumer Worker runs inside Docker.

To see received messages, run:

```bash
docker logs -f consumer-worker
```

After sending a message, you should see output similar to:

```text
Message received:

Id=2e2d82d6-b541-4ae4-8c91-5f3b50c4dce4
Text=Hello RabbitMQ!
CreatedAt=2026-07-23T05:03:11Z
```

Press:

```text
Ctrl + C
```

to stop following the logs.

This does **not** stop the container.

---

## RabbitMQ Management UI

RabbitMQ includes a Management UI where you can inspect queues, connections, consumers, and messages.

Open:

```text
http://localhost:15672
```

Login:

```text
Username: guest
Password: guest
```

Then go to:

```text
Queues and Streams
    ↓
demo-queue
```

Here you can see:

| Property | Meaning |
|---|---|
| Ready | Messages waiting for a consumer |
| Unacked | Messages delivered but not yet acknowledged |
| Total | Total messages currently in the queue |
| Consumers | Number of consumers listening to the queue |

Because the Consumer Worker processes messages quickly, you may normally see:

```text
Ready: 0
Consumers: 1
```

---

## Test RabbitMQ Queue Behavior

A useful way to understand RabbitMQ is to stop the Consumer and send some messages.

### Stop the Consumer

```bash
docker stop consumer-worker
```

Now send a few messages through the API:

```json
{
  "text": "Message 1"
}
```

```json
{
  "text": "Message 2"
}
```

```json
{
  "text": "Message 3"
}
```

Open RabbitMQ Management:

```text
http://localhost:15672
```

Go to:

```text
Queues and Streams → demo-queue
```

You should see something similar to:

```text
Ready: 3
Consumers: 0
```

The messages are waiting inside RabbitMQ because no consumer is currently available.

Now start the Consumer again:

```bash
docker start consumer-worker
```

Watch the Consumer logs:

```bash
docker logs -f consumer-worker
```

You should see all waiting messages being processed.

RabbitMQ should then return to:

```text
Ready: 0
Consumers: 1
```

This demonstrates one of the main benefits of message queues:

> The Producer and Consumer do not need to be available at exactly the same time.

RabbitMQ can hold messages until the Consumer is ready to process them.

---

## Message Flow

When a request is sent to the API:

```text
POST /api/Messages
        |
        v
MessagesController
        |
        | Create MessageDto
        v
RabbitMqProducer
        |
        | Serialize to JSON / bytes
        v
RabbitMQ
        |
        | Route message
        v
demo-queue
        |
        | Deliver message
        v
ConsumerWorker
        |
        | Deserialize
        | Process
        v
       ACK
```

The Producer does not communicate directly with the Consumer.

RabbitMQ sits between them and handles message delivery.

---

## Docker Setup

The complete application runs using Docker Compose:

```text
                 Docker Compose Network

┌────────────────┐
│  Producer API  │
│                │
│     :8080      │
└───────┬────────┘
        │
        │ rabbitmq:5672
        ▼
┌────────────────┐
│    RabbitMQ    │
│                │
│  demo-queue    │
│                │
│  5672 - AMQP   │
│ 15672 - UI     │
└───────┬────────┘
        │
        ▼
┌────────────────┐
│Consumer Worker │
│                │
│ Receive + ACK  │
└────────────────┘
```

The Producer API is exposed to the host machine as:

```text
localhost:5100
```

RabbitMQ Management is exposed as:

```text
localhost:15672
```

Inside Docker, the Producer and Consumer connect to RabbitMQ using:

```text
rabbitmq:5672
```

Docker Compose automatically creates the network that allows these containers to communicate using their service names.

---

## Useful Docker Commands

Start the complete application:

```bash
docker compose up --build
```

Start in the background:

```bash
docker compose up -d --build
```

Check running containers:

```bash
docker ps
```

View all application logs:

```bash
docker compose logs -f
```

View only Consumer logs:

```bash
docker logs -f consumer-worker
```

View Producer logs:

```bash
docker logs -f producer-api
```

Stop the application:

```bash
docker compose down
```

After changing application code, rebuild using:

```bash
docker compose up --build
```

---

## Concepts Covered

This project helped me understand the basic concepts of RabbitMQ:

- Producer and Consumer
- Message Queues
- Asynchronous Communication
- Message Serialization and Deserialization
- Exchanges and Routing Keys
- Manual Acknowledgements (`ACK`)
- Negative Acknowledgements (`NACK`)
- Queue Durability
- Consumer Availability
- Docker Networking
- Docker Compose

The basic RabbitMQ flow used in this project is:

```text
Producer
    |
    v
Default Exchange
    |
    | Routing Key
    v
demo-queue
    |
    v
Consumer
    |
    v
ACK
```

---

## What I Plan to Explore Next

This is a basic RabbitMQ learning project. Some concepts that can be added next are:

- Direct Exchange
- Fanout Exchange
- Topic Exchange
- Multiple Queues
- Multiple / Competing Consumers
- Retry Mechanism
- Dead Letter Queue (DLQ)
- Publisher Confirms
- Persistent Messages
- Idempotent Consumers
- MassTransit

---

## Why I Built This Project

I created this project to understand RabbitMQ practically instead of only learning the theory.

The main goal was to understand:

- How a Producer publishes a message
- Where the message goes after publishing
- How RabbitMQ stores messages in a queue
- How a Consumer receives messages
- What happens when the Consumer is unavailable
- How acknowledgements work
- How Docker Compose can run the complete messaging environment

This project is intended as a simple starting point for learning **RabbitMQ with .NET and Docker**.
