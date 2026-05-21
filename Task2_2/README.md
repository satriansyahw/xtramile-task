# WeatherApp (.NET 10 Blazor WASM & Web API)

This is a modern weather application built using **.NET 10**. It has a **Blazor WebAssembly** frontend that runs directly in the client's browser, and an **ASP.NET Core Web API** backend. The system integrates with the OpenWeatherMap API for live data, but includes a built-in deterministic fallback that generates mock weather data if the API key is missing, unauthorized, or offline.

---

## How the System Flows

Here is a quick overview of how the components communicate:

1. **User Interaction**: The user selects a country and city on the Blazor WebAssembly frontend dashboard.
2. **API Request**: The frontend sends an HTTP request (`GET /api/weather/{city}`) to the Web API backend.
3. **Validation & Database Check**: The backend verifies if the requested city exists in the local EF Core database.
4. **Weather Fetching**: The backend attempts to fetch live data from the OpenWeatherMap API using the configured API Key.
5. **Fallback Safety**: If no API key is set, if the key is invalid (401 Unauthorized), or if the connection fails, the backend automatically generates and returns realistic, deterministic mock weather data.

---

## API Endpoints

The Web API backend exposes three REST endpoints. Every response is wrapped in a standard **Result Pattern** (`Result` or `Result<T>`) containing metadata about whether the operation succeeded or failed.

### 1. Get Countries
- **Route**: `GET /api/countries`
- **Purpose**: Retrieves the list of available countries to populate the first dropdown on the dashboard.
- **Responses**:
  - **Success (`200 OK`)**:
    ```json
    {
      "isSuccess": true,
      "errorMessage": "",
      "value": [
        {
          "id": 1,
          "code": "ID",
          "name": "Indonesia"
        },
        {
          "id": 2,
          "code": "US",
          "name": "United States"
        }
      ]
    }
    ```
  - **Server Error (`500 Internal Server Error`)**:
    ```json
    {
      "isSuccess": false,
      "errorMessage": "An unexpected server error occurred. Please try again later."
    }
    ```

---

### 2. Get Cities by Country
- **Route**: `GET /api/countries/{countryCode}/cities`
- **Purpose**: Retrieves the list of seeded cities for a specific country to populate the second dropdown on the dashboard.
- **Route Parameters**:
  - `countryCode` (string, required): The ISO 2-letter country code (e.g., `ID`, `US`, `JP`).
- **Responses**:
  - **Success (`200 OK`)**:
    ```json
    {
      "isSuccess": true,
      "errorMessage": "",
      "value": [
        {
          "id": 1,
          "name": "Jakarta",
          "countryCode": "ID"
        },
        {
          "id": 2,
          "name": "Surabaya",
          "countryCode": "ID"
        }
      ]
    }
    ```
  - **Validation Error (`400 Bad Request`)**:
    If the `countryCode` parameter fails validation rules (e.g., empty or invalid format):
    ```json
    {
      "isSuccess": false,
      "errorMessage": "Validation failed",
      "errors": [
        "Country code is required and must be exactly 2 letters."
      ]
    }
    ```
  - **Server Error (`500 Internal Server Error`)**:
    ```json
    {
      "isSuccess": false,
      "errorMessage": "An unexpected server error occurred. Please try again later."
    }
    ```

---

### 3. Get Weather by City Name
- **Route**: `GET /api/weather/{cityName}`
- **Purpose**: Fetches the weather data for the specified city.
- **Route Parameters**:
  - `cityName` (string, required): The name of the city (e.g., `Jakarta`, `Tokyo`, `New York`).
- **Responses**:
  - **Success (`200 OK`)**:
    ```json
    {
      "isSuccess": true,
      "errorMessage": "",
      "value": {
        "location": "Jakarta, Indonesia",
        "timeUTC": "2026-05-21 10:30:45 UTC",
        "wind": "12.5 mph, 180°",
        "visibility": "6.2 miles",
        "skyConditions": "Cloudy",
        "temperatureFahrenheit": 77.0,
        "temperatureCelsius": 25.0,
        "dewPoint": 21.5,
        "relativeHumidity": 60,
        "pressure": 1013
      }
    }
    ```
  - **Validation Error (`400 Bad Request`)**:
    If the city name parameter fails validation rules (e.g., empty):
    ```json
    {
      "isSuccess": false,
      "errorMessage": "Validation failed",
      "errors": [
        "City name is required."
      ]
    }
    ```
  - **Not Found (`404 Not Found`)**:
    If the city does not exist in the database:
    ```json
    {
      "isSuccess": false,
      "errorMessage": "City 'Gotham' not found in our records."
    }
    ```
  - **Server Error (`500 Internal Server Error`)**:
    ```json
    {
      "isSuccess": false,
      "errorMessage": "An unexpected server error occurred. Please try again later."
    }
    ```

---

## Project Structure

The solution is divided into four main projects:

- **WeatherApp.Shared**: Shared models, DTOs, and validation logic. Keeping these here ensures that both the frontend and backend use the exact same models, preventing integration mismatches.
- **WeatherApp.Api**: The backend server. It exposes REST API endpoints, manages the in-memory database, handles external HTTP requests, and runs the fallback weather engine.
  - **Key Files**:
    - [WeatherService.cs](file:../WeatherApp.Api/Infrastructure/Services/WeatherService.cs) - Main weather fetching, conversion, and fallback logic.
    - [WeatherController.cs](file:../WeatherApp.Api/Controllers/WeatherController.cs) - API controller exposing the weather endpoint to the frontend.
- **WeatherApp.Web**: The Blazor WebAssembly frontend. This UI runs client-side in the user's browser.
- **WeatherApp.Tests**: The offline testing suite using xUnit and Moq to cover all backend endpoints, validation logic, and services.

---

## Setup and Configuration

### Prerequisites
- **.NET 10.0 SDK** (run `dotnet --version` in your terminal to check).
- **Visual Studio 2022** (v17.12 or newer) or **VS Code**.

### Configuration

#### Backend API Configuration
To configure live weather data, open [appsettings.json](file:../WeatherApp.Api/appsettings.json) in the `WeatherApp.Api` project and add your OpenWeatherMap key:
```json
"WeatherSettings": {
  "ApiKey": "YOUR_API_KEY_HERE"
}
```

#### Frontend Client Configuration
The API URLs used by the frontend are read dynamically from configuration. To edit them, open [appsettings.json](file:../WeatherApp.Web/wwwroot/appsettings.json) in the `WeatherApp.Web/wwwroot` folder:
```json
{
  "ApiSettings": {
    "BaseUrlHttps": "https://localhost:7228",
    "BaseUrlHttp": "http://localhost:5006"
  }
}
```

#### Offline Fallback Mechanism
If the `ApiKey` is empty, if the API returns a `401 Unauthorized` status (due to an expired or invalid key), or if network requests fail, the application will automatically fall back to generating realistic mock data. This means the app will work out-of-the-box without any manual configuration.

---

## How to Run the App

You can run the app from your terminal at the solution root folder (where `WeatherApp.slnx` is located).

### 1. Build the Solution
Compile all projects and download dependencies:
```bash
dotnet build
```

### 2. Run the Backend API
```bash
dotnet run --project WeatherApp.Api
```
The server will start, and you can access the interactive Swagger documentation at: `https://localhost:7228/swagger`

### 3. Run the Frontend Client
In a new terminal window, run:
```bash
dotnet run --project WeatherApp.Web
```
Once started, open your browser and navigate to: `https://localhost:7162`

---

## Running Automated Tests

All tests are designed to run 100% offline with zero external dependencies. Execute them with:
```bash
dotnet test
```

---

## Architectural Choices & Trade-offs

### 1. Blazor WASM Frontend + Web API Backend
We decided to decouple the client UI from the backend services rather than building a monolithic server-rendered app.

- **Benefits**:
  - **Shared Code**: DTOs and validation classes are defined once in `WeatherApp.Shared` and used by both client and server.
  - **Security**: Sensitive configuration keys (like the OpenWeatherMap `ApiKey`) stay securely on the backend server and are never exposed to the client's browser.
  - **Client-Side Efficiency**: Rendering workloads are offloaded to the user's browser, reducing hosting costs and server resource usage.
  - **Flexibility**: The backend is client-agnostic. We can reuse the same Web API to power a mobile app or a third-party service in the future.
- **Trade-offs**:
  - **Initial Load Time**: Because it runs entirely client-side, the browser has to download the .NET WebAssembly runtime (`.wasm` and `.dll` files) on the first load, making the initial startup slightly slower than a traditional server-rendered app.
  - **SEO Considerations**: Client-side rendering is harder for older search engines to index without setting up pre-rendering or SSR (Server-Side Rendering).

### 2. Direct DbContext Injection (No Repository Pattern)
Following Modern C# and EF Core best practices, we inject the `WeatherDbContext` directly into our services rather than introducing an abstraction layer like a Generic Repository.

- **Why**: EF Core's `DbSet` is already a repository, and `DbContext` is a Unit of Work. Creating another layer over them usually results in boilerplate code that limits EF Core's query capabilities (like `Include` and projection optimization) without providing any real benefits.
