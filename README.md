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

### Abrí la solución con Visual Studio:
- Dsw2025Tpi.sln

### Configurá la cadena de conexión
Edita el archivo appsettings.json dentro del proyecto Dsw2025Tpi.Api:  
<br>JSON<br>  
"ConnectionStrings": {
  "Dsw2025TpiEntities": "Server=(localdb)\\MSSQLLocalDB;Database=Dsw2025Db;Trusted_Connection=True;"
}

### Crear y poblar la base de datos
1. Habilita la Package Manager Console en Visual Studio:
 - Herramientas -> administrador de paquetes NuGet -> Package Manager Console  
2. Ejecuta los siguientes comandos en la Package Manager Console: <br>
   <br>Update-Database -Context Dsw2025TpiContext<br>
   <br>Update-Database -Context AuthenticateContext<br><br>

Al iniciar el proyecto por primera vez, se cargan datos desde Sources/customers.json y luego se cargaran tres roles (Usuario,Admin,Tester) para asignarselos posteriormente a usuarios

### Cómo ejecutar el proyecto  
En Visual Studio:  
1. Establecé Dsw2025Tpi.Api como proyecto de inicio
2. Ejecutá con F5 o Ctrl + F5
3. Se abrirá el browser con Swagger

## EndPoints Disponibles

### Authenticate
| Método | Ruta                          | Descripción                          |Roles Autorizados|
|--------|-------------------------------|--------------------------------------|---------------------------|
| POST    | `/api/auth/login`              | Permite loguearse con usuario y contraseña |no requiere autenticación|
| POST    | `/api/auth/register`         | Registra un nuevo usuario y le asigna un rol        |no requiere autenticación|

### Orders  
| Método | Ruta            | Descripción                                       |Roles Autorizados|
|--------|------------------|---------------------------------------------------|---------------------------|
| POST   | `/api/orders`   | Crea una nueva orden, validando stock             |Usuario,Tester|
|GET|`/api/orders`|Obtiene todas las ordenes existentes de forma paginada, permite filtrado por estado y por cliente |Usuario,Tester,Admin|
|GET|`/api/orders/{id}`|Obtiene una Orden por ID |Usuario,Tester,Admin|
|PUT|`/api/orders/{id}/status`|Actualiza el estado de una orden |Admin,Tester|

### Products

| Método | Ruta                          | Descripción                          |Roles Autorizados|
|--------|-------------------------------|--------------------------------------|---------------------------|
| POST   | `/api/products`              | Agrega un nuevo producto             |Admin,Tester|
| GET    | `/api/products`              | Obtiene todos los productos disponibles |no requiere autenticación|
| GET    | `/api/products/{id}`         | Obtiene un producto por su ID        |no requiere autenticación|
| PUT    | `/api/products/{id}`         | Actualiza los datos de un producto   |Admin,Tester|
| PATCH  | `/api/products/{id}`         | Desactiva un producto                |Admin,Tester|


