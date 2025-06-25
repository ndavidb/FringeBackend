# Fringe Solution Architecture  

## Overview  
The **Fringe** solution consists of four projects:  

1. **Fringe.API** – API layer with controllers and configurations.  
2. **Fringe.Domain** – Defines **Entities**, **DTOs**, and **DbContext** for Entity Framework.  
3. **Fringe.Repository** – Handles data access logic.  
4. **Fringe.Service** – Manages business logic.  

## Project References  
- **Fringe.Repository** → References **Fringe.Domain**  
- **Fringe.Service** → References **Fringe.Domain** and **Fringe.Repository**  
- **Fringe.API** → References **Fringe.Service** (not **Fringe.Repository** directly)  

### Avoiding Circular References  
- **Fringe.Domain** remains independent, containing only models and DbContext.  
- Dependencies flow **one way**: **Domain → Repository → Service → API**.  

This structure ensures **separation of concerns** and prevents **circular dependencies**.