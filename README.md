# System Info
A multiservice logging system where ReuqestLogger.Api recieves requests and processes these whilst
Services currently communicate via the internal docker network

# Why two Services?
I have chosen to use two API services to seperate logic. .NET will handle validation, authentication and logic. Python will handle logging and future analytics

# How Logs flow
- **RequestLogger.Api** (.NET 8 minimal API): Acts as middleware that intercepts all HTTP requests, logs metadata, and forwards logs to the processor service
- **service-python** (FastAPI): Receives structured log entries and processes them (currently just prints to console)

# Problem statement
“How do you reliably capture, process, and persist HTTP request metadata from a production API without impacting request latency?”

# End Goal
A backend logging pipeline where a .NET API captures request and response metadata via middleware, forwards structured logs to a Python processor for validation and persistence, and stores queryable request data in Azure SQL.