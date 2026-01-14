FROM python:3.12-slim

WORKDIR /app

# Install system dependencies for pyodbc + Microsoft driver
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        curl \
        gnupg \
        unixodbc-dev \
    && curl -sSL https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -o /tmp/prod.deb \
    && dpkg -i /tmp/prod.deb \
    && rm /tmp/prod.deb \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends \
        msodbcsql18 \
        mssql-tools18 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* \
    && echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> /etc/profile

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]