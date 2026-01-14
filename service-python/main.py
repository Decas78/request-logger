from fastapi import FastAPI
from pydantic import BaseModel
from dotenv import load_dotenv
import pyodbc
import os
import json

app = FastAPI()

load_dotenv()

class APILog(BaseModel):
    method: str
    path: str
    queryString: str | None = None
    bodySizeBytes: int
    latencyMs: int
    responseStatusCode: int
    clientIp: str | None = None
    timestamp: str | None = None

@app.get("/process")
def process(value: int):
    result = value * 2
    return {"result": result} # Double the result before returning

@app.post("/logsimple")
def logsimple(entry: APILog):
    print("Simple log entry:", entry.json())
    save_log_to_db(entry)
    return {"status": "log received"}

def save_log_to_db(log: APILog):
    server = os.getenv("SERVER")
    database = os.getenv("DATABASE")
    username = os.getenv("USERNAME")
    password = os.getenv("PASSWORD")
    driver = "{ODBC Driver 18 for SQL Server}"
    connection_string = f"DRIVER={driver};SERVER={server};DATABASE={database};UID={username};PWD={password};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"

    try:
        with pyodbc.connect(connection_string) as conn:
            with conn.cursor() as cursor:
                insert_query = """
                INSERT INTO Logs (method, path, queryString, bodySizeBytes, latencyMs, responseStatusCode, clientIp, timestamp)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                """
                cursor.execute(insert_query, log.method, log.path, log.queryString, log.bodySizeBytes,
                               log.latencyMs, log.responseStatusCode, log.clientIp, log.timestamp)
                conn.commit()
                print("Log entry saved to database:", log.json())
    except Exception as e:
        print("Error saving log to database:", e)