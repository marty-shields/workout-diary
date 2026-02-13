#!/bin/bash
cd src/Infrastructure/
dotnet ef migrations add $1 -- "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=workouts;"