# Workout Diary
This is an application which I am developing for personal development where I create a workout diary that allows me to track my own workouts. Being a gym enthusiast means I always keep track of what workouts I have done with the associated weight, reps, and sets.

---

## ✅ Requirements
- **.NET SDK 10** (projects target `net10.0`)
- **Docker** with Compose (or **Podman** with Compose plugin)
- Optional: an IDE such as **Visual Studio** or **VS Code**

> **Important:** The project's required services (database, etc.) are defined in `compose.yaml` at the repository root. You must run Docker Compose (or Podman Compose) before running the API or integration tests because those resources are hosted locally in containers.

---

## 🔧 Build
- From the repository root:

```bash
# Build the solution
dotnet build workout-diary.sln
```

---

## ▶️ Run (Local Development)
1. Start required services (from repository root):

```bash
docker compose up -d
```

2. Run the API project:

```bash
dotnet run --project src/Api
```

For hot reload during development:

```bash
dotnet watch --project src/Api run
```

To stop the containers:

```bash
docker compose down
```

---

## 🧪 Tests
- Run all tests:

```bash
dotnet test
```

- Run a single test project:

```bash
dotnet test test/Api.IntegrationTests
dotnet test test/Infrastructure.IntegrationTests
```

> ⚠️ **Note:** Integration tests depend on the services started by Docker Compose (database, etc.). Ensure you run `docker compose up -d` before running integration tests to avoid failures.

---

## 💡 Troubleshooting
- If integration tests fail with database connection errors, verify the containers are running and the DB has been seeded.
---
