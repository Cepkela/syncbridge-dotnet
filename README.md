# \# SyncBridge

# 

# A .NET Web API for managing orders received from external systems.

# 

# \## Overview

# 

# SyncBridge is a portfolio project demonstrating how to build a maintainable ASP.NET Core API with persistent storage, input validation, duplicate detection, and automated integration tests.

# 

# \## Features

# 

# \- Create orders through a REST API

# \- Retrieve orders by ID

# \- Validate incoming request data

# \- Prevent duplicate external order IDs

# \- Persist data using Entity Framework Core and SQL Server

# \- Separate business logic into a service layer

# \- Return appropriate HTTP status codes

# \- Run automated API integration tests

# 

# \## Tech Stack

# 

# \- C#

# \- .NET 10

# \- ASP.NET Core Web API

# \- Entity Framework Core

# \- SQL Server

# \- xUnit

# \- ASP.NET Core integration testing

# 

# \## API Endpoints

# 

# | Method | Endpoint | Description |

# |---|---|---|

# | GET | `/api/health` | Check API health |

# | POST | `/api/orders` | Create an order |

# | GET | `/api/orders/{id}` | Retrieve an order by ID |

# 

# \## Example: Create an Order

# 

# Request:

# 

# ```http

# POST /api/orders

# Content-Type: application/json

# ```

# 

# ```json

# {

# &#x20; "externalId": "EXT-1001",

# &#x20; "customerName": "Example Customer",

# &#x20; "totalAmount": 149.99

# }

# ```

# 

# A successful request returns `201 Created`.

# 

# An invalid request returns `400 Bad Request`. An order with an existing external ID returns `409 Conflict`.

# 

# \## Running Locally

# 

# 1\. Install the .NET 10 SDK.

# 2\. Configure the SQL Server connection string in `appsettings.json`.

# 3\. Apply database migrations:

# 

# &#x20;  ```bash

# &#x20;  dotnet ef database update --project SyncBridge.Api

# &#x20;  ```

# 

# 4\. Run the API:

# 

# &#x20;  ```bash

# &#x20;  dotnet run --project SyncBridge.Api

# &#x20;  ```

# 

# 5\. Run the tests from the solution directory:

# 

# &#x20;  ```bash

# &#x20;  dotnet test

# &#x20;  ```

# 

# \## Project Structure

# 

# \- `Controllers/` — HTTP endpoints

# \- `Services/` — business logic

# \- `Models/` — domain models

# \- `Data/` — Entity Framework Core database context

# \- `SyncBridge.Api.Tests/` — automated API tests

# 

# \## Purpose

# 

# This project is being developed to demonstrate practical .NET API development, clean separation of responsibilities, database integration, and reliable handling of invalid and duplicate requests.

