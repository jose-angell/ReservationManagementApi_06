# ReservationManagementApi_06

API para la gestión de reservas (Reservation Management API).

Estado actual
- Plataforma: .NET 10
- Estado: En desarrollo (prototipo funcional)
- Cambios recientes relevantes:
  - Migración del uso de DateTimeOffset a DateTime en entidades, DTOs, controladores, casos de uso y tests (propiedades StartDateTime, EndDateTime, CreatedAt y parámetros relacionados).
  - Migraciones adaptadas para usar columnas tipo `timestamp` en lugar de `timestamptz` cuando procedía. Revisar la carpeta `Migrations`.

Descripción breve
Este proyecto implementa la lógica necesaria para gestionar recursos y reservas, con capas de dominio, aplicación y persistencia. Está pensado como backend para clientes web o móviles.

Características implementadas (resumen)
- Modelos de dominio para recursos y reservas.
- Casos de uso en la capa Application (ResourceUseCase, ReservationUseCase, ...).
- Endpoints REST para recursos y reservas.
- Pruebas unitarias y de aplicación (ver carpeta `ReservationManagementApi_06.Tests`).

Requisitos
- .NET 10 SDK
- Base de datos PostgreSQL (opcional para ejecución local según configuración)

Cómo ejecutar localmente
1. Restaurar paquetes: `dotnet restore`
2. Compilar: `dotnet build`
3. Ejecutar la API: `dotnet run --project ReservationManagementApi_06`

Pruebas (resumen y cómo obtenerlo)
- Ejecutar todas las pruebas: `dotnet test`
- Generar resultados en formato TRX: `dotnet test --logger "trx;LogFileName=test_results.trx"`
- Ejecutar con salida concisa en PowerShell: `dotnet test --no-build --logger "console;verbosity=minimal"`

Estado actual de las pruebas
- Nota: las pruebas no se han ejecutado automáticamente durante la última modificación por este agente. Para obtener un resumen actualizado (total de pruebas, pasadas, fallidas):
  1) Desde la raíz del repositorio ejecuta `dotnet test`.
  2) Revisa la salida en consola o abre `test_results.trx` con un visor compatible.

Puntos a revisar tras la migración DateTimeOffset -> DateTime
- Asegurarse de que las comparaciones de fechas usan DateTime.UtcNow y que DateTime.Kind se establece cuando sea necesario.
- Comprobar serialización JSON y formatos de timestamp si hay clientes que dependan de un formato concreto.
- Revisar y aplicar migraciones de base de datos si se requiere cambiar el esquema existente.

Contribuciones
- Abrir pull requests con descripciones claras y pruebas cuando sea posible.

Repositorio
- https://github.com/jose-angell/ReservationManagementApi_06

