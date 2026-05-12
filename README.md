\# Todo App



A full-stack Todo List application built with Angular (frontend) and .NET 10 Web API (backend).



> \*\*Note:\*\* Frontend and E2E test documentation will be added as those layers are completed.



\---



\## Backend



\### Tech Stack



| Layer | Technology |

|-------|-----------|

| API | .NET 10 Web API, C# |

| Storage | In-memory (thread-safe `ConcurrentDictionary`) |

| API Docs | Swashbuckle (Swagger UI) |

| Tests | xUnit, Moq, FluentAssertions |



\### Prerequisites



\- \[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)



\### Running the API



```bash

cd backend/TodoApi

dotnet restore

dotnet run

```



Swagger UI will open automatically at `http://localhost:5000/swagger`.



\### Running Backend Tests



```bash

cd backend/TodoApi.Tests

dotnet restore

dotnet test

```



\### API Endpoints



| Method | Endpoint | Description |

|--------|----------|-------------|

| GET | `/api/todos` | Get all todos |

| POST | `/api/todos` | Create a new todo |

| GET | `/api/todos/{id}` | Get todo by ID |

| DELETE | `/api/todos/{id}` | Delete a todo |

| PATCH | `/api/todos/{id}/toggle` | Toggle completion status |



\### Architecture Notes



\- \*\*Repository pattern\*\* decouples data access from business logic, making the storage layer swappable (e.g. to Entity Framework) without touching controllers.

\- \*\*Singleton repository\*\* ensures in-memory state persists across requests.

\- \*\*CORS\*\* is configured to allow requests from `http://localhost:4200`.

\- \*\*DTOs\*\* separate the API contract from the domain model.

