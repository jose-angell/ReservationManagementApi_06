# ReservationManagementApi_06

API para la gestión de reservas (Reservation Management API).

Estado del proyecto
- Estado: En desarrollo (prototipo funcional).
- Objetivo: Exponer endpoints REST para crear, consultar y gestionar reservas de recursos.
- Plataforma: .NET 10.

Descripción breve
Este proyecto implementa la lógica y los casos de uso básicos para gestionar recursos y sus reservas. Incluye capas de aplicación, dominio y persistencia (según la estructura del repositorio). Está pensado como backend para integrarse con clientes web o móviles.

Características implementadas (resumen)
- Modelos de dominio para recursos y reservas.
- Casos de uso en la capa Application (p. ej. ResourceUseCase).
- Endpoints API para operaciones CRUD básicas sobre reservas y recursos (pueden estar en desarrollo o incompletos).
- Preparado para pruebas y despliegue local con .NET 10.

Requisitos
- .NET 10 SDK instalado
- (Opcional) SQL Server / base de datos configurada según la configuración del proyecto

Cómo ejecutar (local)
1. Clonar el repositorio
2. Abrir la solución ReservationManagementApi_06.slnx en Visual Studio 2026 o usar dotnet CLI
3. Restaurar paquetes: `dotnet restore`
4. Compilar: `dotnet build`
5. Ejecutar: `dotnet run --project <ruta-al-proyecto-API>`

Notas y siguientes pasos
- Revisar e implementar control de errores y validaciones adicionales.
- Añadir documentación de API (Swagger/OpenAPI) si no está presente.
- Completar pruebas automatizadas y CI.

Contribuciones
Se aceptan contribuciones mediante pull requests. Por favor, describir cambios y añadir pruebas cuando sea posible.

Licencia
Revisar el fichero de licencia del repositorio (si existe) o añadir una según se acuerde.
