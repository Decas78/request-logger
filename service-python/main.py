from fastapi import FastAPI
from pydantic import BaseModel
import json

app = FastAPI()

class APILog(BaseModel):
    method: str
    path: str
    queryString: str | None = None
    bodySizeBytes: int
    timestamp: str | None = None

@app.get("/process")
def process(value: int):
    result = value * 2
    return {"result": result} # Double the result before returning

@app.post("/logsimple")
def logsimple(entry: APILog):
    print("Simple log entry:", entry.json())