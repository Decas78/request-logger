# API Request Logger

A multiservice logging system where RequestLogger.Api receives requests and processes these whilst
services currently communicate via the internal docker network.

## Problem Statement
"How do you reliably capture, process, and persist HTTP request metadata from a production API without impacting request latency?"

## Architecture

### How Logs Flow
- **RequestLogger.Api** (.NET 8 minimal API): Contains test endpoints for HTTP requests forwarding logs to the python processor service
- **service-python** (FastAPI): Receives structured log entries and processes them (currently just prints to console)

### Why Two Services?
I have chosen to use two API services to decouple logic. .NET will handle validation, authentication and logic. Python will handle logging and persistence via Azure SQL.

## End Goal
A backend logging pipeline where a .NET API captures request and response metadata via middleware, forwards structured logs to a Python processor for validation and persistence, and stores queryable request data in Azure SQL.

## Installation & Run
Ensure to have the following installed:
- .NET SDK 8.0.416
- Python 3.12
- Docker Compose

Open root directory and run the following:
- docker compose build
- docker compose up

## Technologies Used
- Update here