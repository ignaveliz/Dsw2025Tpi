# Trabajo Práctico Integrador
## Desarrollo de Software
### Backend

## Integrantes del grupo
- **Veliz Ignacio Martin** - Legajo 56571
- **Rizo Avalos Nadia Enoa** - Legajo 56199
- **Soria Leandro Miguel** - Legajo 56068

## Instrucciones para configurar y ejecutar el proyecto

### Requisitos previos:
- .NET 8
- SQL Server
- Visual Studio 2022

### Cloná el repositorio 
- git clone https://github.com/ignaveliz/Dsw2025Tpi.git
- cd Dsw2025Tpi

### Configurá la cadena de conexión
Edita el archivo appsettings.json dentro del proyecto Dsw2025Tpi.Api:  
<br>JSON<br>  
"ConnectionStrings": {
  "Dsw2025TpiEntities": "Server=(localdb)\\MSSQLLocalDB;Database=Dsw2025Db;Trusted_Connection=True;"
}

### Crear y poblar la base de datos
1. Aplica la migracion ejecutando el siguiente comando en la Package Manager Console:  
   <br>Update-Database<br><br>
2. Al iniciar el proyecto por primera vez, se cargan datos desde Sources/customers.json

### Cómo ejecutar el proyecto  
En Visual Studio:  
1. Establecé Dsw2025Tpi.Api como proyecto de inicio
2. Ejecutá con F5 o Ctrl + F5
3. Se abrirá el browser con Swagger

## EndPoints Disponibles
### Products

| Método | Ruta                          | Descripción                          |
|--------|-------------------------------|--------------------------------------|
| GET    | `/api/products`              | Obtiene todos los productos disponibles |
| GET    | `/api/products/{id}`         | Obtiene un producto por su ID        |
| POST   | `/api/products`              | Agrega un nuevo producto             |
| PUT    | `/api/products/{id}`         | Actualiza los datos de un producto   |
| PATCH  | `/api/products/{id}`         | Desactiva un producto                |

### Orders  
| Método | Ruta            | Descripción                                       |
|--------|------------------|---------------------------------------------------|
| POST   | `/api/orders`   | Crea una nueva orden, validando stock             |
