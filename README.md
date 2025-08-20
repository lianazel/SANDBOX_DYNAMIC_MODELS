# SANDBOX_DYNAMIC_MODELS

## Overview
This repository contains:
- **SANDBOX_DYNAMIC_MODELS** (Class Library): a tiny runtime engine that generates entities using `Reflection.Emit` (IL at runtime) and configures an EF Core **dynamic `DbContext`**.

## Key ideas
- Dynamic types are created at runtime (no static POCOs).
- Properties (get/set) are emitted in **IL** using `Reflection.Emit`.
- EF Core is configured on the fly:
  - PK = `ID_Auto` (SQLite autoincrement)
  - Business IDs as **GUIDs** (`ID_Constructeur`, `ID_TypePropulsion`)
  - Relationships target **alternate keys** via `HasPrincipalKey(...)`.

## Tech stack
- .NET 9
- EF Core 9
- SQLite

  ## Notes
 - This repo is a learning sandbox, not production-ready.
 -  If you want to reuse the library elsewhere, you can later split it into a dedicated repo or publish to a private/public NuGet feed.

## To test 
- Create a console project
- Add a existing project ( "File" / "Add" /  "Existing Project" /  chosse "SANDBOX_DYNAMIC_MODELS.csproj" 
- In class "Program.cs" of your console projet :
   MD_DEMO.Run(); // Simple demo orchestration
   System.Console.WriteLine("\nPress any key to close...");
   System.Console.ReadKey();
           
