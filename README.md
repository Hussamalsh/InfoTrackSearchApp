# InfoTrack SEO Search Application

## Overview

This project is an application designed to search for specific keywords and URLs in Google and return the positions where the URL appears in the search results. It is developed using .NET Core 8 Web API for the backend and Blazor for the frontend. The application stores search history in a SQL Server database and includes caching to improve performance.

## Features

- Search for keywords and URLs in Google search results.
- Display the positions of the URL in the search results.
- View the history of search results.
- Caching of search results to improve performance.

## Technologies Used

- .NET Core 8 Web API
- Blazor 
- Entity Framework Core
- SQL Server Express
- Dependency Injection
- Logging
- Caching

## Prerequisites

- .NET 8.0 SDK
- SQL Server Express
- Visual Studio 2022 or VS Code

## Setup Instructions

1. **Clone the repository:**

   ```bash
   git clone https://github.com/yourusername/InfoTrackSearchApp.git
   cd InfoTrackSearchApp
   ```

2. **Database Setup:**

  - Ensure SQL Server Express is installed and running.
  - Clone the repository.
  - Navigate to the project directory.
  - Run `dotnet restore`.
  - Update the connection string in `appsettings.json` with your database details.
  - 1- run `dotnet ef migrations add InitialCreate --project InfoTrackSearchData --startup-project InfoTrackSearchAPI`
  - 2- run `dotnet ef database update --project InfoTrackSearchData --startup-project InfoTrackSearchAPI`
  - Run `dotnet run` to start the application.

3. **Configure Connection String:**

   - Update the connection string in `appsettings.json` file of the API project:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=InfoTrackDb;Trusted_Connection=True;Encrypt=true;TrustServerCertificate=True"
   }
   ```

4. **Run the API Project:**

   - Open the solution in Visual Studio or VS Code.
   - Set `InfoTrackSearchAPI` as the startup project.
   - Run the project (Ctrl + F5).

5. **Run the Blazor Project:**

   - Set `InfoTrackSearchBlazor` as the startup project.
   - Run the project (Ctrl + F5).

6. **Access the Application:**

   - Open a web browser and navigate to `https://localhost:5001` (or the port specified in your launch settings).

## Project Structure

- `InfoTrackSearchAPI`: Backend API project.
- `InfoTrackSearchBlazor`: Frontend Blazor project.
- `InfoTrackSearchData`: Data access layer.
- `InfoTrackSearchModel`: Shared models.

## Usage

1. **Perform a Search:**

   - Enter the keyword and URL in the search form and click "Search".
   - The application will display the positions of the URL in the Google search results.

2. **View Search History:**

   - The history of previous searches will be displayed below the search form.
   - You can see the date, keyword, URL, and positions from previous searches.

## Things to Improve

1. **UI Enhancements:**
   - Improve the user interface design for better usability and aesthetics.
   - Add loading indicators and better error messages.

2. **Advanced Search Options:**
   - Allow users to specify additional search parameters (e.g., date range, country-specific searches).

3. **Parallel Searches:**
   - Implement parallel searches across multiple search engines to provide more comprehensive results.

4. **Performance Optimization:**
   - Optimize database queries and improve caching strategies.
   - Implement background processing for long-running search tasks.

5. **Security Enhancements:**
   - Implement user authentication and authorization.
   - Secure sensitive data in transit and at rest.

6. **Comprehensive Testing:**
   - Add more unit tests and integration tests to ensure the application’s reliability.
   - Implement automated testing and continuous integration/continuous deployment (CI/CD) pipelines.


8. **Scalability:**
   - Refactor the application to support horizontal scaling.
   - Use cloud services for better scalability and availability.
9. **Pagination:**
           - We can implement pagination for the search results to improve performance and user experience.


## Additional Configuration

### Logging

- Logging configuration is in `appsettings.json`:
  
  ```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
  ```

### CORS

- CORS is configured to allow any origin, method, and header.

  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddDefaultPolicy(builder =>
      {
          builder.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader();
      });
  });
  ```

### Caching

- Caching settings are configured in `appsettings.json`:

  ```json
  "CacheSettings": {
    "ExpirationMinutes": 30
  }
  ```

## License

This project is licensed under the MIT License.

## Contact

For any questions or suggestions, please contact [hussam@hussam.se](mailto:hussam@hussam.se).
