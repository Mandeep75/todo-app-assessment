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

---

## Frontend

### Tech Stack

| Layer     | Technology             |
| --------- | ---------------------- |
| Framework | Angular 21, TypeScript |
| State     | Signals                |
| HTTP      | Angular HttpClient     |
| Tests     | Jasmine, Karma         |

### Prerequisites

- [Node.js](https://nodejs.org/) v20+
- [Angular CLI](https://angular.io/cli): `npm install -g @angular/cli`

### Running the Frontend

> Make sure the backend API is running first.

```bash
cd frontend
npm install
ng serve
```

The app will be available at `http://localhost:4200`.

### Running Frontend Tests

```bash
cd frontend
ng test
```

This launches Karma in Chrome and runs all 21 unit tests.

### Architecture Notes

- **Standalone components** follow the Angular 21 recommended approach — no NgModules needed.
- **Signals** are used for reactive state management (`todos`, `isLoading`, `errorMessage`, computed `remainingCount` and `completedCount`).
- **Service layer** — all HTTP logic is in `TodoService`, the component never calls the API directly.
- **Environment files** — API URL is configurable per environment, not hardcoded in the component.
- **trackBy** — used in `*ngFor` for performance, avoids re-rendering the entire list on every change.
