# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-09-08

### Features

- Added Microsoft SQL Database to store user and shopping list data
- Added ASP .NET Web API to implement the business logic and provide controlled access to the database
- Added Database Provider ADO.NET as the mediator between the API and the database
- Added Database scheme
- Added the Class diagram for the API
- Added github workflow to deploy the API as Azure Web App
- Added activity diagram to depict the deployment workflow

## [0.1.0] - 2025-09-10

### Features

- Added Entities
- Added DbContext
- Added Initial Migration

## [0.1.1] - 2025-09-18

### Features

- Added Repositories and their respective Interfaces
- Added Services and their respective Interfaces
- Added Unit of Work and its Interface
- Added new return types for the Services
- Added new migrations
- Added Constants to appsettings.json

### Changes
- Edited Entities minorly
- Edited DbContext minorly
- Edite: reorganized DTOs

## [0.1.2] - 2025-09-19

### Features
- Added AppAuthenticationService and its Interface
- Added AppAuthenticationMiddleware
- Added global exception handling



