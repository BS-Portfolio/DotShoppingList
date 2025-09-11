# Architektur Pattern - DotShoppingList Projekt

## Architektur-Übersicht

**Hybrid-Architektur:** Frontend mit **MVVM** (Vue.js) und Backend mit **Web API** (ASP.NET Core).

## Frontend (Vue.js) - MVVM Pattern

### **Model**
- **API Types**: TypeScript-Interfaces für Server-Kommunikation
- **Server Data**: Daten vom Backend
- **LocalStorage**: Client-seitige Persistierung

### **ViewModel**
- **Components (Script Teil)**: Reactive data + methods + lokale Logik (`ShoppingList.vue`)
- **Pinia Stores (Global)**: Globale State Management (`useAuthStore`) 
- **Composables**: Wiederverwendbare ViewModel-Logik (`useAuthHelpers.ts`)

### **View**
- **Templates (Vue)**: HTML mit Vue-Direktiven
- **Data Binding**: `{{ }}`, `v-model`, `v-for`

### **MVVM Data Binding**
**Two-Way Data Binding** in Vue.js:

```
MODEL ←→ VIEWMODEL ←→ VIEW
      (Reactive)   (Data Binding)
```

**Funktionsweise:**
- **One-Way (ViewModel → View)**: Änderungen in reactive data aktualisieren automatisch die UI
- **Two-Way (View → ViewModel)**: User-Eingaben in Formularen aktualisieren automatisch die Daten
- **Reactive System**: Vue.js überwacht alle Datenänderungen und triggert UI-Updates
- **Automatic Updates**: Keine manuellen DOM-Manipulationen erforderlich

**Verwendete Direktiven:**
- `{{ }}`: Interpolation für Text-Ausgabe
- `v-model`: Two-Way Binding für Input-Felder
- `v-for`: Listen-Rendering mit automatischen Updates
- `v-if`: Bedingte Darstellung basierend auf Daten

## Backend (ASP.NET Core) - Web API Pattern

### **Controller**
- **ShoppingListApiController.cs**: REST API Endpoints
- **Koordiniert**: Model ↔ Service Layer ↔ Database

### **Service Layer** (Business Logic)
- **DatabaseService.cs**: Datenbank-Operationen
- **AuthenticationService.cs**: Authentifizierung und Autorisierung

### **Model**
- **Entity Models**: Domain-Objekte (ShoppingList, Item, ListUser)
- **DTOs**: Data Transfer Objects (Post/Get/Patch Models)
- **Database Context**: Entity Framework Core

### **Data Layer**
- **SQL Server Database**: Datenpersistierung mit Entity Framework Core

## API-Kommunikation

**MVVM (Frontend) ↔ REST API ↔ Web API (Backend):**

1. **ViewModel** sendet HTTP Request  
2. **Controller** validiert & authentifiziert  
3. **Service Layer** führt Business Logic aus  
4. **Model/Database** liefert Daten  
5. **Controller** sendet JSON Response  
6. **ViewModel** aktualisiert sich reaktiv  
7. **View** zeigt automatisch neue Daten  

## Architektur-Vorteile

- **Frontend MVVM**: Reaktivität, Two-Way Data Binding, deklarative UI
- **Backend Web API**: RESTful Services, JSON-basierte Kommunikation, Service-orientierte Architektur  
- **API-Trennung**: Unabhängige Entwicklung und Skalierung

## Technologien

**Frontend (MVVM):** Vue.js 3, Pinia, TypeScript  
**Backend (Web API):** ASP.NET Core, Entity Framework, SQL Server  
**API:** REST, HTTP/JSON, Bearer Token Authentication
