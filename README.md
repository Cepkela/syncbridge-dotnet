SyncBridge

A .NET Web API for managing orders received from external systems.

Overview

SyncBridge is a portfolio project demonstrating how to build a maintainable ASP.NET Core API with persistent storage, input validation, duplicate detection, and automated integration tests.

Features

Create orders through a REST API

Retrieve orders by ID

Validate incoming request data

Prevent duplicate external order IDs

Persist data using Entity Framework Core and SQL Server

Separate business logic into a service layer

Return appropriate HTTP status codes

Run automated API integration tests

Tech Stack

C#

.NET 10

ASP.NET Core Web API

Entity Framework Core

SQL Server

xUnit

ASP.NET Core integration testing

API Endpoints

Method

Endpoint

Description

GET

/api/health

Check API health

POST

/api/orders

Create an order

GET

/api/orders/{id}

Retrieve an order by ID

Example: Create an Order

Request:

POST /api/orders
Content-Type: application/json

{
  "externalId": "EXT-1001",
  "customerName": "Example Customer",
  "totalAmount": 149.99
}

A successful request returns 201 Created.

An invalid request returns 400 Bad Request. An order with an existing external ID returns 409 Conflict.

Running Locally

Install the .NET 10 SDK.

Configure the SQL Server connection string in appsettings.json.

Apply database migrations:

dotnet ef database update --project SyncBridge.Api

Run the API:

dotnet run --project SyncBridge.Api

Run the tests from the solution directory:

dotnet test

Project Structure

Controllers/ — HTTP endpoints

Services/ — business logic

Models/ — domain models

Data/ — Entity Framework Core database context

SyncBridge.Api.Tests/ — automated API tests

Purpose

This project is being developed to demonstrate practical .NET API development, clean separation of responsibilities, database integration, and reliable handling of invalid and duplicate requests.