## ⚠️ Reglas de trabajo (leer antes de hacer cualquier cosa)

### Generales

1. **Explorar antes de actuar.** Antes de crear, modificar o generar cualquier archivo de código, leer y entender la estructura existente del proyecto — directorios, archivos actuales, namespaces, convenciones de nombres, etc. No asumir: verificar.

2. **Resolver por análisis, no por pregunta.** Antes de consultar al usuario, intentar responder leyendo el proyecto. Solo interactuar con el usuario cuando la duda genuinamente no pueda resolverse analizando el código y los documentos disponibles.

3. **Seguir la arquitectura al pie de la letra.** Todo el código generado debe respetar estrictamente los patrones y convenciones descritos en este documento. No inventar abstracciones nuevas, no saltear capas, no usar ORM.

4. **Una funcionalidad a la vez.** Completar todos los archivos de una funcionalidad (Domain → Persistence → Application → Web) antes de pasar a la siguiente.

5. **No romper lo que ya funciona.** Cualquier cambio a un archivo existente debe preservar el comportamiento actual y los otros endpoints que ya estén implementados.

6. **Verificar existencia antes de implementar.** Antes de implementar un endpoint o funcionalidad, leer el proyecto para determinar si ya existe parcial o completamente. Si ya existe, reportarlo al usuario en lugar de duplicar.

7. **Reportar contradicciones.** Si al explorar el proyecto se encuentra algo que contradice este documento (una convención diferente, un patrón distinto), señalarlo explícitamente antes de continuar.

8. **Listar cambios al terminar.** Al completar una funcionalidad, listar todos los archivos creados y modificados antes de avanzar a la siguiente.

### Sobre paquetes y base de datos

9. **No agregar paquetes NuGet sin aprobación.** Si se necesita un paquete que no está en el proyecto, proponer cuál y por qué, y esperar confirmación del usuario antes de usarlo.

10. **No modificar el esquema de la base de datos bajo ningún concepto.** La estructura de la base de datos está definida y es inmutable. Está terminantemente prohibido generar, sugerir o incluir cualquier DDL (`CREATE TABLE`, `ALTER TABLE`, `DROP`, `CREATE INDEX`, `CREATE TYPE`, etc.). Si una funcionalidad parece requerir un cambio en la DB, detenerse y comunicárselo al usuario — nunca resolverlo por cuenta propia.

### Sobre SQL

11. **Nunca usar `SELECT *`.** Siempre listar las columnas explícitamente con sus alias en `snake_case`.

12. **Siempre usar alias de tabla en JOINs.** Para evitar ambigüedad de columnas en queries con múltiples tablas.

13. **Siempre filtrar `logical_delete = false`.** Todo listado o búsqueda debe excluir registros con borrado lógico, salvo que el contexto lo justifique explícitamente.

### Sobre el dominio

14. **No poner lógica de negocio en handlers.** Si una regla depende del estado de un agregado, pertenece a la entidad de dominio. El handler orquesta, no decide.

15. **No exponer setters públicos en entidades.** Las propiedades de las entidades deben ser `internal set` o `init`. El estado solo cambia a través de métodos con nombre de intención.

### Sobre manejo de errores

16. **Nunca usar excepciones de la BCL para errores de negocio.** Está prohibido usar `throw new Exception(...)`, `throw new InvalidOperationException(...)`, `throw new KeyNotFoundException(...)` o cualquier excepción de la BCL para representar un error de negocio. Siempre crear y lanzar una clase concreta que herede de `DomainException`.

17. **Nunca retornar `null` desde un handler para indicar "no encontrado".** Si un recurso no existe, el handler debe lanzar la excepción de dominio correspondiente (ej. `ProductNotFoundException`). El controller nunca recibe `null` y nunca lo chequea.

### Sobre autenticación y autorización

18. **Validar propiedad del recurso en endpoints de usuario autenticado.** En endpoints que requieren autenticación y son accesibles por usuarios con rol `User` (no Admin), siempre verificar que el recurso pertenece al cliente del token antes de devolverlo o modificarlo. Un cliente nunca puede ver ni operar datos de otro cliente. Esta regla no aplica a endpoints públicos (sin auth) ni a endpoints exclusivos de Admin.

19. **Nunca loggear datos sensibles.** Está prohibido loggear passwords, hashes de passwords, tokens JWT, refresh tokens ni el contenido de `raw_response` de pagos, en ningún nivel de log (ni siquiera Debug o Trace).

### Sobre carrito y órdenes

20. **El checkout siempre lee del carrito persistente.** Al crear una orden (`POST /api/orders`), los items se toman exclusivamente del carrito persistente del cliente en la DB. Nunca del body del request. Si se genera código que lee items del body para crear una orden, es un error.

21. **Las direcciones de una orden siempre son snapshots.** Al crear una orden, siempre insertar un registro en `order_address_snapshot` y guardar su FK en la orden. Nunca guardar una FK directa a la tabla `address` en la orden.

### Sobre calidad del código

22. **Nunca generar código incompleto sin avisarlo.** Si por alguna razón una implementación queda con `TODO`, `FIXME` o un bloque vacío/placeholder, indicarlo explícitamente al usuario antes de entregarlo. No dejar código incompleto sin señalarlo.

23. **Nunca usar `async void`.** Todos los métodos asincrónicos deben retornar `Task` o `Task<T>`. `async void` está prohibido.

24. **Siempre propagar el `CancellationToken`.** En handlers y repositorios, el `CancellationToken` recibido debe propagarse a todos los métodos de Npgsql y operaciones asincrónicas. No ignorarlo ni descartarlo.

### Sobre seguridad

25. **Nunca interpolar variables en SQL.** Está terminantemente prohibido construir queries con interpolación de strings o concatenación (`$"SELECT ... WHERE id = {id}"`). Todo valor externo va como parámetro nombrado con `AddParameter(...)`.

26. **Nunca exponer campos sensibles a usuarios no Admin.** Campos como `raw_response` de pagos, `password_hash`, tokens internos u otros datos de infraestructura no deben aparecer en respuestas de endpoints accesibles por usuarios con rol `User`.

---

---

# SECCIÓN 1 — ARQUITECTURA Y GUÍA DE DESARROLLO

> Contenido completo de `ARCHITECTURE.md`

# Guía de Arquitectura y Desarrollo

Documentación técnica de una arquitectura base para APIs REST en .NET, combinando **Clean Architecture**, **CQRS con MediatR** y **acceso a datos directo sobre ADO.NET** (sin ORM). Este documento sirve como referencia para entender el diseño y como guía paso a paso para iniciar un proyecto desde cero o agregar nuevas funcionalidades siguiendo convenciones consistentes.

---

## 1. Visión general

La arquitectura propuesta es una API REST en **.NET 10** inspirada en los principios de **Clean Architecture**, combinada con el patrón **CQRS** (implementado con MediatR) y acceso a datos vía **ADO.NET sobre PostgreSQL** (usando `Npgsql`, sin ORM). El objetivo es mantener las capas de la aplicación desacopladas, con responsabilidades claras y reglas de dependencia estrictas.

> **Aclaración importante**: esta es una **variante pragmática** de Clean Architecture, no la versión canónica. La diferencia más relevante: las **interfaces de repositorio viven en `Persistence`**, no en `Domain` ni `Application` (la versión ortodoxa colocaría los puertos en una capa interna y dejaría la implementación en una externa). Se eligió este trade-off para evitar una capa adicional de abstracción y porque el equipo no contempla cambiar de tecnología de persistencia. Si en algún momento se necesita esa portabilidad, el refactor consistirá en mover las interfaces a Application/Domain e invertir la dependencia.

Una decisión central de diseño: **el dominio es rico, no anémico**. Las entidades encapsulan estado y comportamiento, protegen sus invariantes a través de métodos con nombre de intención, y se construyen mediante factory methods que validan reglas. Los handlers de CQRS orquestan, no implementan reglas de negocio. Ver sección 2.1 para los detalles.

### 1.1 Tecnologías principales

| Capa / Propósito | Tecnología |
|---|---|
| Runtime | .NET 10 |
| Web | ASP.NET Core (Kestrel + Controllers) |
| Mediación CQRS | MediatR 14 |
| Base de datos | PostgreSQL vía Npgsql |
| Autenticación | JWT Bearer + Refresh Tokens |
| Hashing de contraseñas | BCrypt.Net-Next |
| Logging | Serilog (consola + archivo JSON) |
| Documentación | Swashbuckle (Swagger/OpenAPI) |
| Compresión | Gzip |
| Rate limiting | `System.Threading.RateLimiting` |

### 1.2 Estructura de la solución

La solución se organiza en **seis proyectos** con dependencias estrictamente unidireccionales:

```
Web (Host)
   └──> Application
           ├──> Persistence
           │       ├──> DataAccess
           │       ├──> Domain
           │       └──> Utils
           ├──> Domain
           └──> Utils

DataAccess ──> Utils
Domain     ──> Utils
Utils      (núcleo, sin dependencias de negocio)
```

Reglas clave de dependencia:
- **`Domain` no depende de nadie** (salvo `Utils`).
- **`DataAccess` no depende de nadie** salvo `Utils`: es una abstracción genérica sobre PostgreSQL, no conoce el negocio.
- **`Persistence` depende de `Domain` y `DataAccess`** (necesita las entidades para hidratarlas y los contratos de DB para ejecutarlas), pero **no depende de `Application` ni de `Web`**.
- **`Application` depende de `Persistence` y `Domain`** (consume las interfaces de repositorio que viven en `Persistence`).
- **`Web` depende solo de `Application`** (no toca `Persistence` ni `Domain` directamente; los DTOs que devuelve son los de `Application`).

Convención recomendada para namespaces y assemblies: usar un prefijo común (por ejemplo `MyApp.Domain`, `MyApp.Application`, etc.) para que el escaneo de ensamblados por reflexión pueda filtrarlos fácilmente. En los `.csproj`:

```xml
<AssemblyName>MyApp.$(MSBuildProjectName)</AssemblyName>
<RootNamespace>MyApp.$(MSBuildProjectName)</RootNamespace>
```

---

## 2. Capas y responsabilidades

### 2.1 Domain

Contiene las **entidades del negocio**, **value objects**, **constantes de dominio** y **excepciones de dominio**. Es la única capa sin dependencias de librerías externas de negocio.

#### El dominio NO es anémico

Esta es una regla central de la arquitectura: **las entidades de dominio no son DTOs ni POCOs con setters públicos**. Son objetos que encapsulan estado *y* comportamiento. Las invariantes y reglas de negocio viven dentro de la entidad, no esparcidas en handlers o repositorios.

Reglas mínimas que toda entidad debe respetar:

- **Setters privados o `init`-only.** El estado solo cambia a través de métodos con nombre de intención (`Cancel()`, `ChangePrice(...)`, `MarkAsShipped()`).
- **Nada de constructores públicos sin lógica.** Una entidad nace a través de un **factory method estático** (`Order.Create(...)`) que valida invariantes antes de devolverla. Esto garantiza que no exista una instancia en estado inválido.
- **Las reglas de negocio viven dentro de la entidad**, no en el handler. Si "una orden cancelada no se puede volver a cancelar", esa regla la verifica `Order.Cancel()`, no el `CancelOrderCommandHandler`.
- **Constructor sin argumentos `internal` para hidratación desde la base**: los repositorios necesitan instanciar la entidad antes de poblarla con `ICDataReader`. Debe ser `internal` (no `private`) para que la capa `Persistence` pueda invocarlo vía `InternalsVisibleTo` sin reflexión. Este constructor existe solo para ese propósito y no debe usarse desde el código de negocio.

Ejemplo — `Domain/Products/Product.cs`:
```csharp
namespace MyApp.Domain.Products;

public class Product {
    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public decimal Price { get; internal set; }
    public bool Active { get; internal set; }
    public DateTime CreatedAt { get; internal set; }

    // Constructor para hidratación desde repositorio (uso exclusivo de la capa Persistence vía InternalsVisibleTo).
    internal Product() { }

    // Factory method: única forma de crear un Product nuevo.
    public static Product Create(string name, string? description, decimal price) {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidProductNameException();
        if (price < 0)
            throw new InvalidProductPriceException(price);

        return new Product {
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Comportamiento: cambiar precio respetando invariantes.
    public void ChangePrice(decimal newPrice) {
        if (newPrice < 0)
            throw new InvalidProductPriceException(newPrice);
        if (!Active)
            throw new InactiveProductException(Id);

        Price = newPrice;
    }

    public void Deactivate() {
        if (!Active) return; // idempotente
        Active = false;
    }

    // Setter "interno" para que el repositorio asigne el Id retornado por la DB.
    internal void AssignId(int id) => Id = id;
}
```

> **Nota sobre `internal set`**: las propiedades son `internal set`, no `private set`, porque el `Map` del repositorio (en el assembly `Persistence`) necesita escribirlas para hidratar la entidad. Esto requiere `[InternalsVisibleTo("MyApp.Persistence")]` en el `.csproj` de `Domain`. El nivel `internal` es seguro en la práctica: el código de negocio vive en `Application`/`Web` (otros assemblies) y por lo tanto sigue obligado a usar los métodos con nombre de intención (`ChangePrice`, `Cancel`, etc.). Solo el código dentro del propio assembly `Domain` y el de `Persistence` (al que se le da acceso explícito) pueden tocar los setters, lo cual es exactamente lo que se quiere.

Ejemplo más expresivo — `Domain/Orders/Order.cs`:
```csharp
namespace MyApp.Domain.Orders;

public class Order {
    private readonly List<OrderItem> _items = new();

    public int Id { get; internal set; }
    public int CustomerId { get; internal set; }
    public DateTime Date { get; internal set; }
    public OrderStatus Status { get; internal set; }
    public decimal Total { get; internal set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    internal Order() { } // hidratación

    public static Order Create(int customerId, DateTime date, IEnumerable<OrderItem> items) {
        var list = items.ToList();
        if (list.Count == 0)
            throw new EmptyOrderException();

        var order = new Order {
            CustomerId = customerId,
            Date = date,
            Status = OrderStatus.Pending,
        };
        order._items.AddRange(list);
        order.Total = order._items.Sum(i => i.Quantity * i.UnitPrice);
        return order;
    }

    public void Cancel() {
        if (Status == OrderStatus.Cancelled)
            throw new OrderAlreadyCancelledException(Id);
        if (Status == OrderStatus.Shipped)
            throw new OrderCannotBeCancelledAfterShippingException(Id);

        Status = OrderStatus.Cancelled;
    }

    public void MarkAsShipped() {
        if (Status != OrderStatus.Pending)
            throw new OrderInvalidTransitionException(Id, Status, OrderStatus.Shipped);
        Status = OrderStatus.Shipped;
    }

    internal void AssignId(int id) => Id = id;
}
```

#### Value objects

Los conceptos del negocio que tienen reglas propias (`Email`, `Money`, `Price`, `Quantity`) se modelan como **value objects** (records inmutables con validación en el constructor o en un factory). Esto evita repetir validaciones en 10 lugares y le da significado al tipo.

```csharp
public record Email {
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Email Create(string raw) {
        if (string.IsNullOrWhiteSpace(raw) || !raw.Contains('@'))
            throw new InvalidEmailException(raw);
        return new Email(raw.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}
```

#### Excepciones de dominio

Subcarpeta — `Domain/Exceptions`. Aquí viven la base abstracta y **una clase concreta por cada error de negocio identificable**.

##### Regla central: `DomainException` es `abstract` y nunca se lanza directamente

`DomainException` es la **clase base abstracta** del sistema de errores de negocio. Define el contrato (`HttpStatusCode`, `ErrorType`, mensaje) pero **nunca se instancia**.

```csharp
namespace MyApp.Domain.Exceptions;

public abstract class DomainException : Exception {
    public HttpStatusCode StatusCode { get; }
    public string ErrorType { get; }

    protected DomainException(HttpStatusCode statusCode, string errorType, string message)
        : base(message) {
        StatusCode = statusCode;
        ErrorType = errorType;
    }
}
```

> **Prohibido en el código de negocio**:
> ```csharp
> // MAL — el compilador lo impide, pero además sería un antipatrón:
> throw new DomainException(HttpStatusCode.UnprocessableEntity,
>     "InvalidName", "Customer name is required.");
> ```
>
> **Correcto**: crear una clase concreta que herede de `DomainException` y lanzarla.
> ```csharp
> throw new InvalidCustomerNameException();
> ```

##### Por qué clases concretas

Las razones son las mismas que justifican un dominio rico en lugar de POCOs anémicos:

1. **Cada error de negocio tiene un nombre.** `InvalidCustomerNameException` documenta la situación; un `throw new DomainException(...)` con un string mágico `"InvalidName"` la oculta.
2. **El status code y el `ErrorType` viven en un solo lugar.** Si el día de mañana "nombre inválido" pasa de `422` a `400`, se cambia en una clase. Con strings sueltos hay que hacer grep en todo el proyecto.
3. **`catch` específicos son posibles.** `catch (CustomerNotFoundException)` permite reaccionar a un caso concreto sin parsear strings.
4. **Los tests son legibles.** `Assert.Throws<OrderAlreadyCancelledException>(() => order.Cancel())` se entiende solo.
5. **No hay duplicación de literales.** `"AlreadyCancelled"` escrito en 5 lugares es 5 oportunidades de typo. Una clase es una sola fuente de verdad.

##### Estructura de las clases concretas

Cada excepción concreta vive en `Domain/<Feature>/Exceptions/` (próxima al agregado al que pertenece) y sigue este patrón:

```csharp
namespace MyApp.Domain.Customers.Exceptions;

public sealed class InvalidCustomerNameException : DomainException {
    public InvalidCustomerNameException()
        : base(HttpStatusCode.UnprocessableEntity,
               "InvalidCustomerName",
               "Customer name is required.") { }
}
```

```csharp
namespace MyApp.Domain.Customers.Exceptions;

public sealed class CustomerNotFoundException : DomainException {
    public CustomerNotFoundException(int id)
        : base(HttpStatusCode.NotFound,
               "CustomerNotFound",
               $"Customer #{id} not found.") { }
}
```

```csharp
namespace MyApp.Domain.Orders.Exceptions;

public sealed class OrderAlreadyCancelledException : DomainException {
    public OrderAlreadyCancelledException(int orderId)
        : base(HttpStatusCode.Conflict,
               "OrderAlreadyCancelled",
               $"Order #{orderId} is already cancelled.") { }
}
```

```csharp
namespace MyApp.Domain.Orders.Exceptions;

public sealed class OrderInvalidTransitionException : DomainException {
    public OrderInvalidTransitionException(int orderId, OrderStatus from, OrderStatus to)
        : base(HttpStatusCode.Conflict,
               "OrderInvalidTransition",
               $"Order #{orderId} cannot transition from {from} to {to}.") { }
}
```

Convenciones:
- **`sealed`** salvo que tenga sentido extender (raro).
- **Nombre que termina en `Exception`** y describe el caso del negocio.
- **Constructor sin argumentos** cuando el mensaje es estático; constructor con parámetros cuando hay datos contextuales (ids, valores).
- **El mensaje y el `ErrorType` se fijan dentro de la clase**, no se reciben desde afuera.
- **Ubicación**: junto al agregado dueño de la regla, en `Domain/<Feature>/Exceptions/`.

##### `ModelValidationException`

`ModelValidationException` también extiende `DomainException` (no es abstracta porque transporta un diccionario dinámico de errores de input HTTP):

```csharp
public sealed class ModelValidationException : DomainException {
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ModelValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(HttpStatusCode.UnprocessableEntity, "ValidationError", "One or more validation errors occurred.") {
        Errors = errors;
    }
}
```

Esta es la única `DomainException` concreta que se instancia con datos variables, y solo la lanza el `ModelStateValidationFilter`.

##### Cómo se usan desde el dominio

Las entidades lanzan **excepciones concretas** cuando se intenta una operación inválida. El handler no necesita validar antes de invocar el método: confía en que la entidad protege sus invariantes y deja que la excepción suba al `ExceptionMiddleware`.

```csharp
public void Cancel() {
    if (Status == OrderStatus.Cancelled)
        throw new OrderAlreadyCancelledException(Id);
    if (Status == OrderStatus.Shipped)
        throw new OrderCannotBeCancelledAfterShippingException(Id);

    Status = OrderStatus.Cancelled;
}
```

#### Constantes y enums de dominio

Cuando un valor semántico se comparte entre capas, vive en `Domain`. Para estados se prefiere un `enum` (`OrderStatus.Pending`) sobre constantes string. Si por razones de persistencia se necesitan strings (lookup tables), se mantienen como `public const string` en una clase estática (ej. `OrderStatusNames.Pending = "pending"`) que handlers y repositorios pueden referenciar sin duplicar literales.

#### Qué NO va en una entidad

- Lógica de presentación, formateo de respuestas HTTP, mapeo a DTOs.
- Acceso a base de datos, llamadas a servicios externos, IO en general.
- Dependencias de MediatR, ASP.NET Core, Serilog, etc.

Si una regla necesita IO (ej. "no se puede crear una orden si el cliente está bloqueado, lo cual requiere consultar la DB"), esa orquestación va en el handler — pero el chequeo final ("este cliente está bloqueado") lo hace el dominio sobre los datos ya cargados.

### 2.2 Persistence

Contiene:
- **Interfaces de repositorio** (`I<Entidad>Repository`) en `Persistence/<Feature>/Interfaces/`. Estas son los contratos que consume `Application`.
- **Implementaciones** en `Persistence/<Feature>/Repositories/`.
- **Filtros de consulta** (records) en `Persistence/<Feature>/Interfaces/<Feature>Filter.cs`.
- **Servicios de infraestructura con IO** (ej. servicios de autenticación que tocan DB, emisores de tokens). Sus interfaces también viven aquí y `Application` las consume del mismo modo que los repositorios.
- **Helpers compartidos** en `Persistence/Shared/` (paginación, filtros base).

Los repositorios se registran automáticamente vía el atributo `[Injectable(ServiceLifetime.Scoped)]` (ver sección 5).

### 2.3 DataAccess

Abstracción delgada sobre ADO.NET / Npgsql. Define tres interfaces clave:

| Interface | Responsabilidad |
|---|---|
| `ICConnection` | Conexión y transacciones (`Connect`, `Disconnect`, `BeginTransaction`, `CommitTransaction`, `CancelTransaction`, `CreateCommand`). |
| `ICCommand` | Comando parametrizado con helpers: `ExecuteCommandQuery`, `ExecuteCommandExists`, `ExecuteCommandNonQuery`, `ExecuteGetValue<T>`, `ExecuteSelect<T>`, `ExecuteSelectList<T>`. |
| `ICDataReader` | Lector con `GetValue<T>(string alias)` que maneja nullables, `DateOnly` y chequeo de columnas. |

Esta capa **no conoce** nada de negocio: es una abstracción genérica sobre PostgreSQL. El resto del sistema trabaja contra sus interfaces, nunca contra `NpgsqlConnection` directamente.

### 2.4 Application

Aloja el **código de casos de uso** siguiendo CQRS con MediatR. La separación entre lecturas y escrituras se materializa en **dos clases distintas por cada caso de uso** — nunca se mezclan en una sola jerarquía compartida. Por cada caso de uso hay dos archivos:

- **Para mutaciones** (Commands): `XxxCommand.cs` (un `record` que implementa `IRequest<TResponse>`) + `XxxCommandHandler.cs` (un `class` que implementa `IRequestHandler<XxxCommand, TResponse>`).
- **Para lecturas** (Queries): `XxxQuery.cs` (un `record` que implementa `IRequest<TResponse>`) + `XxxQueryHandler.cs` (un `class` que implementa `IRequestHandler<XxxQuery, TResponse>`).

Los Commands y las Queries son **tipos completamente independientes**: no comparten clase base ni interfaz común más allá de `IRequest<>`. Esto refuerza la separación CQRS a nivel de tipos: el compilador impide que un handler de comando se confunda con un handler de query, y permite que cada lado evolucione con sus propios behaviors, validaciones y políticas (por ejemplo, `IPagedQuery` aplica solo a queries).

También contiene los **DTOs** (records con un método estático `From(<Entidad>)`), los **pipeline behaviors** de MediatR, y utilidades de paginación (`PagedResult<T>`, extensiones de mapeo).

### 2.5 Web (Host)

Capa de presentación. Contiene:
- **Controllers** ligeros que solo hacen `_mediator.Send(new XxxCommand(...))` y mapean el request.
- **Request models** con `DataAnnotations` para validación (se validan en el filtro `ModelStateValidationFilter`).
- **Middlewares** para conexión DB, excepciones, logging, cabeceras de seguridad.
- **Filters** para validación y envoltorio de respuesta.
- **Configurations** dividida en dos: `Builder/` (sobre `IHostApplicationBuilder`) y `Application/` (sobre `WebApplication`).
- **Program.cs** minimalista (un solo statement encadenado que delega en extension methods).

### 2.6 Utils

Núcleo compartido. Aloja:
- `InjectableAttribute`: marca clases para auto-registro en DI.
- `AppDomainExtensions.GetProjectAssemblies()`: busca los DLLs del proyecto por prefijo en el directorio base.
- `ServiceCollectionExtensions.AddInjectables()`: escanea los assemblies y registra todas las clases con `[Injectable]`.

---

## 3. Patrones arquitectónicos aplicados

### 3.1 Clean Architecture (variante pragmática)

Las dependencias apuntan en una sola dirección: `Web → Application → Persistence → DataAccess`, con `Domain` como hoja sin dependencias de negocio. A diferencia de la versión ortodoxa, las **interfaces de repositorio viven en `Persistence`** (no en una capa interna), por lo que `Application` depende directamente de `Persistence`. Es un trade-off explícito (ver sección 1).

### 3.2 CQRS con MediatR

Cada acción de usuario se modela como una petición inmutable (`record`) que MediatR enruta al handler registrado. CQRS aquí no es solo una convención de nombres: implica **dos clases distintas e independientes** según la naturaleza de la operación, sin ninguna jerarquía común que las una.

- **Commands** (mutaciones): clase propia para cambios de estado. Ejemplos: `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`. Devuelven el resultado de la operación (DTO creado, booleano de éxito, etc.).
- **Queries** (lecturas): clase propia para consultas. Ejemplos: `GetProductsQuery`, `GetProductByIdQuery`. Nunca mutan estado; pueden implementar marker interfaces específicas como `IPagedQuery`.

```csharp
// Command — clase propia, lado de escritura.
public record CreateProductCommand(string Name, decimal Price)
    : IRequest<ProductDto>;

// Query — clase propia, lado de lectura. Sin relación de tipos con el Command.
public record GetProductsQuery(int? CategoryId, int Page, int PageSize)
    : IRequest<PagedResult<ProductDto>>, IPagedQuery;
```

La regla es estricta: **nunca** se reutiliza una misma clase para representar a la vez una intención de lectura y una de escritura, ni se introduce una clase base abstracta común (`RequestBase`, `OperationBase`, etc.) entre Commands y Queries. Cada lado debe poder evolucionar de forma independiente — distintos pipeline behaviors, distintas políticas de caché, distintas decisiones futuras (por ejemplo, mover queries a un read model separado) — sin arrastrar al otro.

### 3.3 Repository Pattern

Cada agregado del dominio tiene una interfaz `I<Entidad>Repository` con los métodos de persistencia que el dominio necesita. Las implementaciones están aisladas detrás de estas interfaces.

### 3.4 Unit of Work manual vía transacciones

Para operaciones que tocan varios agregados, el handler inyecta `ICConnection` y llama manualmente `BeginTransaction` / `CommitTransaction` / `CancelTransaction`. La transacción no vive en el repositorio sino en el caso de uso.

### 3.5 Pipeline behaviors

Cross-cutting concerns que se aplican a todas las peticiones MediatR. Un ejemplo común es un `PageValidationBehavior` que valida que `Page >= 1` para cualquier petición que implemente `IPagedQuery`.

### 3.6 Middleware chain

Orden recomendado en el `Configure` del `WebApplication`:

1. HSTS + HTTPS redirection
2. CORS
3. Response compression + Rate limiter
4. `ExceptionMiddleware` — atrapa todo lo que no se manejó
5. `DbConnectionMiddleware` — abre y cierra la conexión por request
6. Swagger
7. Autenticación + Autorización
8. Health checks
9. Controllers

### 3.7 Convention-over-configuration para DI

El atributo `[Injectable(ServiceLifetime.Xxx)]` elimina la necesidad de registrar cada servicio manualmente. El sistema inspecciona los assemblies del proyecto (filtrados por prefijo), busca clases decoradas y las registra contra todas sus interfaces que estén en un namespace del proyecto.

---

## 4. Flujo de una petición (end-to-end)

Secuencia completa para un `GET /api/products/5`:

1. **Kestrel** recibe el request.
2. **Middlewares** ejecutan en orden: HSTS + HTTPS redirection → CORS → Compression → RateLimiter → **ExceptionMiddleware** → **DbConnectionMiddleware** (llama `connection.Connect()`).
3. **JWT Bearer** valida el token, popula `HttpContext.User`.
4. **Routing** encuentra `ProductsController.GetById(int id)`.
5. **ModelStateValidationFilter** valida el modelo; si inválido, lanza `ModelValidationException`.
6. El controller invoca `_mediator.Send(new GetProductByIdQuery(id))`.
7. **MediatR** ejecuta el pipeline de behaviors (si la query implementa marker interfaces) y despacha a `GetProductByIdQueryHandler`.
8. El handler llama al repositorio: `_repository.GetByIdAsync(id)`.
9. El repositorio usa `ICConnection.CreateCommand()`, agrega parámetros, ejecuta `ExecuteSelect<Product>(Map)`, retorna `Product?`.
10. El handler mapea `Product` → `ProductDto` vía `ProductDto.From(product)`.
11. El controller retorna el DTO.
12. **ResponseFilter** envuelve el resultado en un `Response(StatusCode, Message, Data)`.
13. **DbConnectionMiddleware** cierra la conexión en el `finally`.
14. Gzip comprime la salida y se envía al cliente.

Si en cualquier paso hay una excepción, `ExceptionMiddleware` la convierte en un `ApiProblemDetails` (RFC 7807) con tipo, título, status, detail, instance, traceId y timestamp.

---

## 5. Inyección de dependencias automática

La pieza central es el atributo `InjectableAttribute`:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InjectableAttribute : Attribute {
    public ServiceLifetime Lifetime { get; }
    public InjectableAttribute(ServiceLifetime lifetime) { Lifetime = lifetime; }
}
```

El método `AddInjectables` (en `Utils/DI/ServiceCollectionExtensions.cs`) se llama desde la configuración del builder y ejecuta este algoritmo:

1. Enumera los archivos `<Prefix>.*.dll` en el directorio de ejecución.
2. Carga cada assembly.
3. Busca tipos no abstractos con `[Injectable]`.
4. Para cada tipo:
   - Obtiene sus interfaces cuyo namespace empiece con el prefijo del proyecto.
   - Si tiene al menos una, registra el tipo contra **cada** interface.
   - Si no tiene ninguna, registra el tipo contra sí mismo.

Convención: todos los repositorios y servicios que quieran DI usan `[Injectable(ServiceLifetime.Scoped)]` (o `Singleton` si son stateless puros, como un generador de JWT).

**Implicación importante**: si una clase tiene múltiples interfaces bajo el prefijo, se registra contra todas. Para repositorios esto no es problema porque cada uno implementa una sola interfaz.

---

## 6. Manejo de la conexión y transacciones

### 6.1 Scope de la conexión

La conexión está registrada como **Scoped**. El `DbConnectionMiddleware` llama `Connect()` al inicio del request y `Disconnect()` en el `finally`. Todos los repositorios que reciban `ICConnection` en su constructor dentro del mismo request comparten la misma conexión física.

```csharp
public async Task InvokeAsync(HttpContext context, ICConnection connection) {
    await connection.Connect();
    try {
        await _next(context);
    } finally {
        await connection.Disconnect();
    }
}
```

### 6.2 Transacciones

La transacción es opt-in y vive a nivel de **handler** (no de repositorio). Patrón canónico (con dominio rico):

```csharp
// El dominio valida y construye el agregado completo en memoria.
var order = Order.Create(request.CustomerId, request.Date, request.Items
    .Select(i => OrderItem.Create(i.ProductId, i.Quantity, i.UnitPrice)));

await _connection.BeginTransaction();
try {
    await _orderRepository.CreateAsync(order); // persiste cabecera + items
    foreach (var item in order.Items) {
        await _stockRepository.DeductAsync(item.ProductId, item.Quantity);
    }
    await _connection.CommitTransaction();
    return OrderDto.From(order);
} catch {
    await _connection.CancelTransaction();
    throw;
}
```

El handler abre transacción, orquesta llamadas a múltiples repositorios, y cierra. Las validaciones de negocio ya ocurrieron al construir `Order` y `OrderItem` — si algún dato era inválido la transacción ni siquiera empieza.

Cuando hay una transacción activa, `CCommand` la usa automáticamente:

```csharp
public CCommand(CConnection connection) {
    _command = new NpgsqlCommand { Connection = connection.ConnectionDB };
    if (connection.Transaction != null)
        _command.Transaction = connection.Transaction;
}
```

---

## 7. Acceso a datos: convenciones con `ICConnection` / `ICCommand`

### 7.1 Mapeo de filas

Cada repositorio define un método estático `Map(T obj, ICDataReader rs)` que conoce los alias de columna:

```csharp
private static void Map(Product obj, ICDataReader rs) {
    obj.Id = rs.GetValue<int>("id");
    obj.Name = rs.GetValue<string>("name");
    obj.Price = rs.GetValue<decimal>("price");
}
```

Los alias siguen `snake_case` porque así vienen de PostgreSQL. El mapeador no debe hacer validación: si el tipo no encaja, `ICDataReader.GetValue<T>` devuelve `default(T)`.

#### Mapeo y dominio rico

Dado que las entidades tienen setters `internal` (sección 2.1), el repositorio necesita acceso a esos setters para hidratar. La técnica estándar es **`InternalsVisibleTo`** declarado en el `.csproj` de cada proyecto Domain:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="MyApp.Persistence" />
</ItemGroup>
```

Con esa declaración, el `Map` del repositorio puede instanciar la entidad vía su constructor `internal Product()` y poblar las propiedades `internal set`. El contrato sigue siendo el mismo desde el código de negocio (que vive en `Application`/`Web`, otros assemblies): **no hay forma de mutar una entidad sin pasar por un método con nombre de intención**.

Una alternativa más explícita —pero mucho más verbosa y con la misma garantía de seguridad— es exponer un único método `internal void Hydrate(...)` que reciba todos los campos. No se recomienda salvo en entidades con muchos campos donde la lista de setters individuales se vuelve tediosa de mantener.

### 7.2 Métodos de ejecución

| Método | Uso |
|---|---|
| `ExecuteSelect<T>(Map)` | Devuelve la primera fila como `T?`. Usar para `GetById`, `Create RETURNING`, etc. |
| `ExecuteSelectList<T>(Map)` | Devuelve `List<T>`. |
| `ExecuteCommandQuery(Action<ICDataReader>)` | Itera todas las filas manualmente. Útil cuando se arma una estructura compleja (ej. cabecera + detalle). |
| `ExecuteCommandExists()` | Booleano: ¿hay al menos una fila? |
| `ExecuteCommandNonQuery()` | Para `INSERT/UPDATE/DELETE` sin `RETURNING`. Devuelve true si afectó filas. |
| `ExecuteGetValue<T>("alias")` | Lee un valor escalar por alias. Usar con `SELECT ... AS alias`. |

### 7.3 Parámetros

Siempre con `AddParameter("name", value)`, nunca por concatenación de string:

```csharp
cmd.AddParameter("id", id);
cmd.AddParameter("description", (object?)description ?? DBNull.Value);
```

Para strings nullables, usar el cast `(object?)value ?? DBNull.Value` para evitar que Npgsql interprete `null` como ausencia del parámetro.

### 7.4 Paginación: `PaginationHelper`

Patrón estándar para listados paginados. La SQL debe incluir `COUNT(*) OVER() AS total_count`:

```csharp
return await PaginationHelper.FetchPagedAsync<Product>(
    _connection,
    "SELECT id, name, price, COUNT(*) OVER() AS total_count FROM products ORDER BY id",
    null,           // applyParams opcional
    Map,
    filter.Page,
    filter.PageSize);
```

El helper:
- Añade `LIMIT @pageSize OFFSET @offset` al final.
- Si el resultado está vacío y la página es > 1, reintenta en la página 1 (fallback).
- Devuelve `PagedData<T>(Items, TotalCount, EffectivePage)`.

Para filtros con múltiples condiciones WHERE, el patrón es construir un helper privado `BuildWhere(filter)` que devuelve una tupla `(string where, List<Action<ICCommand>> applyParams)`:

```csharp
private static (string where, List<Action<ICCommand>> applyParams) BuildWhere(ProductsFilter filter) {
    var conditions = new List<string>();
    var actions = new List<Action<ICCommand>>();

    if (filter.CategoryId.HasValue) {
        conditions.Add("p.category_id = @catId");
        actions.Add(cmd => cmd.AddParameter("catId", filter.CategoryId.Value));
    }
    if (filter.Active.HasValue) {
        conditions.Add("p.active = @active");
        actions.Add(cmd => cmd.AddParameter("active", filter.Active.Value));
    }

    var where = conditions.Count > 0 ? " WHERE " + string.Join(" AND ", conditions) : "";
    return (where, actions);
}
```

Y luego:
```csharp
cmd => { foreach (var apply in applyParams) apply(cmd); }
```

### 7.5 CTEs para crear + hacer side-effect atómico

Muchos `CreateAsync` / `UpdateAsync` pueden usar CTEs de PostgreSQL para hacer la inserción y devolver el id (u otros valores generados por la DB) y a la vez ejecutar inserts derivados, en una sola query. Ejemplo: insertar un producto **y** registrar su precio inicial en una tabla de histórico a la vez:

```sql
WITH ins AS (
    INSERT INTO products (name, price) VALUES (@name, @price) RETURNING id
),
ph AS (
    INSERT INTO price_history (product_id, price, reason)
    SELECT id, @price, 'Initial price' FROM ins
)
SELECT id FROM ins
```

El repositorio lee el `id` con `ExecuteGetValue<int>("id")` y se lo asigna al agregado en memoria con `AssignId`. **Regla**: el agregado canónico que el handler devuelve es **siempre el que construyó el dominio** (y al que el repositorio le asignó el Id), nunca un objeto rehidratado por el repositorio en el mismo `Create`. Esto evita tener dos versiones del mismo agregado convivendo, y mantiene la garantía de que el objeto que circula por la aplicación es el que pasó por el factory method.

Esta técnica evita viajes extra al servidor de base de datos y mantiene atomicidad sin abrir una transacción explícita.

---

## 8. Autenticación y autorización

### 8.1 Flujo

1. El usuario hace `POST /api/auth/login` con email/password.
2. El servicio de autenticación busca el usuario, verifica la contraseña con `BCrypt.Net.BCrypt.Verify` (paquete NuGet `BCrypt.Net-Next`, que comparte el namespace `BCrypt.Net` con el paquete legado; usar siempre `BCrypt.Net-Next` que es el mantenido).
3. Genera un **access token** JWT (60 min por defecto) y un **refresh token** (GUID, 30 días).
4. Persiste el refresh en la tabla `refresh_tokens`.
5. Devuelve `AuthResponseDto { Token, RefreshToken, ExpiresIn }`.

### 8.2 Refresh

`POST /api/auth/refresh` con `{ token, refreshToken }`: valida el refresh en DB (no revocado, no expirado, usuario activo), lo marca como revocado, y emite un par nuevo. Rotación estricta.

### 8.3 Logout

Marca el refresh token como `revoked = true`. El access token sigue siendo válido hasta su expiración natural (stateless JWT).

### 8.4 Autorización

Se usa `[Authorize]` y `[Authorize(Roles = "Admin")]` a nivel de controller o acción. El claim de rol se llena desde el JWT con `ClaimTypes.Role`.

Ejemplo de autorización compuesta (solo admin o el propio usuario):

```csharp
private bool IsAdminOrOwner(int resourceUserId) {
    var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    if (role == "Admin") return true;
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    return userId == resourceUserId.ToString();
}
```

---

## 9. Manejo de errores

### 9.1 Estrategia

Los controllers **no atrapan** excepciones. Lanzan y confían en `ExceptionMiddleware`. Convenciones:

| Situación | Qué lanzar |
|---|---|
| Recurso no encontrado | Una clase concreta por agregado: `throw new CustomerNotFoundException(id)` |
| Credenciales inválidas | `throw new InvalidCredentialsException()` |
| Validación de input HTTP | Se lanza automáticamente `ModelValidationException` desde el filter |
| Conflicto de estado / invariante violada | Lanzada por la **entidad de dominio** como una clase concreta (ej. `OrderAlreadyCancelledException(id)`) |
| Argumento inválido en input | Cubierto por DataAnnotations → `ModelValidationException` |
| Error de negocio con status específico | Clase concreta que herede de `DomainException` (ej. `CustomerBlockedException`) |

**Regla**: las reglas de negocio se expresan como **clases concretas que heredan de `DomainException`** lanzadas desde la entidad, no como `throw new DomainException(...)` directos ni como excepciones de la BCL (`KeyNotFoundException`, `InvalidOperationException`) desde el handler. Esto da:
- `DomainException` es **abstracta** y el compilador impide instanciarla directamente.
- Tipos propios del dominio en lugar de strings mágicos como `"NotFound"` repartidos por el código.
- Imposible confundir un "orden no existe" del negocio con un `KeyNotFoundException` que lanzó internamente otra librería.
- Status HTTP y `errorType` consistentes porque viven dentro de la clase de excepción, no en cada call site.
- `catch` específicos posibles cuando un caso de uso necesita reaccionar a una situación particular.

### 9.2 `ExceptionMiddleware`

Mapea cada tipo de excepción a un `ApiProblemDetails` (wrapper sobre `ProblemDetails` de ASP.NET Core con `traceId`, `timestamp`, `errors`, etc.) usando `ProblemDetailsFactory`. Registra en Serilog según severidad:

- `DomainException` con 5xx → `Log.Error`
- `DomainException` con 4xx → `Log.Warning` (incluye `ModelValidationException`, que hereda de `DomainException` y devuelve 422)
- Cualquier otra excepción no manejada → `Log.Error`

### 9.3 `ResponseFilter`

Envuelve la respuesta exitosa en un objeto uniforme:
```json
{
  "statusCode": "OK",
  "message": "Success",
  "data": { ... }
}
```

Nota: si hubo una excepción el filtro no actúa; el middleware genera el ProblemDetails directamente.

---

## 10. Convenciones de código

### 10.1 Nombres

- Interfaces: `I<Nombre>` (`IProductRepository`, `IAuthService`).
- Repositorios: `<Entidad>Repository`.
- Handlers: `<NombreComando>Handler`.
- DTOs: `<Entidad>Dto` (record con `From(<Entidad>)` estático).
- Requests HTTP: `<Accion><Entidad>Request` o `<Entidad>Request`.
- Filtros: `<Entidad>Filter` (record en la capa Persistence).
- Commands/Queries: `<Verbo><Entidad>Command` / `Query`.

### 10.2 Namespaces

Siempre se usan **file-scoped namespaces**:
```csharp
namespace MyApp.Application.Products.CreateProduct;
```

### 10.3 DTOs

Siempre son `record` con un método estático `From`:
```csharp
public record ProductDto(int Id, string Name, decimal Price) {
    public static ProductDto From(Product p) => new(p.Id, p.Name, p.Price);
}
```

Cuando el DTO tiene hijos (ej. una cabecera con lista de items), el `From` hace la proyección:
```csharp
Items: entity.Items.Select(ItemDto.From).ToList()
```

### 10.4 Commands y Queries son records

```csharp
public record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;

public record GetProductsQuery(int? CategoryId, int Page, int PageSize)
    : IRequest<PagedResult<ProductDto>>, IPagedQuery;
```

Las queries paginadas implementan `IPagedQuery` para activar `PageValidationBehavior`.

### 10.5 Nullability

`<Nullable>enable</Nullable>` en todos los `.csproj`. Los repositorios devuelven `T?` cuando el recurso puede no existir. Los handlers, al detectar `null`, lanzan la excepción de dominio correspondiente (`CustomerNotFoundException(id)`, `OrderNotFoundException(id)`, etc.) — nunca `KeyNotFoundException` ni excepciones genéricas de la BCL. Los controllers no chequean `null`: confían en que el handler ya tradujo "no encontrado" a una excepción de dominio.

### 10.6 Llaves y formato

Estilo consistente a lo largo del código:
- Llave de apertura en la misma línea.
- Indentación con tabs o espacios (elegir uno y mantenerlo).
- Un solo archivo por clase pública.

---

## 11. Cómo agregar una nueva funcionalidad — receta paso a paso

Supongamos que queremos agregar una entidad `Customer` con CRUD completo.

### Paso 1 — Entidad en `Domain`

Crear `Domain/Customers/Customer.cs` con factory method y setters privados:
```csharp
namespace MyApp.Domain.Customers;

public class Customer {
    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string? Email { get; internal set; }
    public DateTime CreatedAt { get; internal set; }

    internal Customer() { } // hidratación desde repositorio

    public static Customer Create(string name, string? email) {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCustomerNameException();

        return new Customer {
            Name = name.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? email) {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCustomerNameException();

        Name = name.Trim();
        Email = email?.Trim().ToLowerInvariant();
    }

    internal void AssignId(int id) => Id = id;
}
```

Notar:
- Ningún setter público.
- `Create` valida y normaliza (trim, lowercase). Esa lógica nunca se duplica en handlers.
- `Update` agrupa la mutación en un método con intención clara.
- `AssignId` es `internal` para que solo lo use la capa Persistence al mapear el `RETURNING id` de la DB.

### Paso 1.5 — Excepciones de dominio para esta entidad

Por **cada error de negocio** que la entidad pueda lanzar, crear una clase concreta que herede de `DomainException`. **Nunca** lanzar `new DomainException(...)` directamente desde el código (de hecho `DomainException` es `abstract` — el compilador lo impide). Ver sección 2.1 para el detalle del patrón.

Para `Customer` se necesitan al menos:

`Domain/Customers/Exceptions/InvalidCustomerNameException.cs`:
```csharp
namespace MyApp.Domain.Customers.Exceptions;

public sealed class InvalidCustomerNameException : DomainException {
    public InvalidCustomerNameException()
        : base(HttpStatusCode.UnprocessableEntity,
               "InvalidCustomerName",
               "Customer name is required.") { }
}
```

`Domain/Customers/Exceptions/CustomerNotFoundException.cs`:
```csharp
namespace MyApp.Domain.Customers.Exceptions;

public sealed class CustomerNotFoundException : DomainException {
    public CustomerNotFoundException(int id)
        : base(HttpStatusCode.NotFound,
               "CustomerNotFound",
               $"Customer #{id} not found.") { }
}
```

A medida que aparezcan más reglas para `Customer`, cada una recibe su clase concreta (`DuplicateCustomerEmailException`, `CustomerBlockedException`, etc.). Si una regla aparece y no encaja en ninguna excepción existente, se crea una nueva — no se reusa una "genérica".

### Paso 2 — Filtro (si hay listado paginado)

Crear `Persistence/Customers/Interfaces/CustomersFilter.cs`:
```csharp
namespace MyApp.Persistence.Customers.Interfaces;
public record CustomersFilter(string? Search, int Page, int PageSize);
```

O reutilizar `PagedFilter` si solo necesitas paginación sin filtros extra.

### Paso 3 — Interface del repositorio

Crear `Persistence/Customers/Interfaces/ICustomerRepository.cs`:
```csharp
using MyApp.Domain.Customers;
using MyApp.Persistence.Shared;

namespace MyApp.Persistence.Customers.Interfaces;

public interface ICustomerRepository {
    Task<PagedData<Customer>> GetAllAsync(CustomersFilter filter);
    Task<Customer?> GetByIdAsync(int id);
    Task CreateAsync(Customer customer);   // recibe la entidad ya construida y validada
    Task UpdateAsync(Customer customer);   // recibe la entidad ya mutada; lanza CustomerNotFoundException si no existe
    Task DeleteAsync(int id);              // lanza CustomerNotFoundException si no existe
}
```

El repositorio **no recibe parámetros sueltos** (`string name, string? email`): recibe la entidad de dominio. Esto refuerza que las invariantes ya se aplicaron antes de llegar a la persistencia y elimina la tentación de que el repositorio "decida" qué guardar.

### Paso 4 — Implementación del repositorio

Crear `Persistence/Customers/Repositories/CustomerRepository.cs`:
```csharp
using MyApp.DataAccess.Interfaces;
using MyApp.Domain.Customers;
using MyApp.Persistence.Customers.Interfaces;
using MyApp.Persistence.Shared;
using MyApp.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Persistence.Customers.Repositories;

[Injectable(ServiceLifetime.Scoped)]
public class CustomerRepository : ICustomerRepository {
    private readonly ICConnection _connection;

    public CustomerRepository(ICConnection connection) {
        _connection = connection;
    }

    private static void Map(Customer obj, ICDataReader rs) {
        obj.Id = rs.GetValue<int>("id");
        obj.Name = rs.GetValue<string>("name");
        obj.Email = rs.GetValue<string?>("email");
        obj.CreatedAt = rs.GetValue<DateTime>("created_at");
    }

    public async Task<PagedData<Customer>> GetAllAsync(CustomersFilter filter) {
        var sql = @"
            SELECT id, name, email, created_at, COUNT(*) OVER() AS total_count
            FROM customers
            WHERE (@search IS NULL OR name ILIKE '%' || @search || '%')
            ORDER BY id";
        return await PaginationHelper.FetchPagedAsync<Customer>(
            _connection, sql,
            cmd => cmd.AddParameter("search", (object?)filter.Search ?? DBNull.Value),
            Map, filter.Page, filter.PageSize);
    }

    public async Task<Customer?> GetByIdAsync(int id) {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT id, name, email, created_at FROM customers WHERE id = @id";
        cmd.AddParameter("id", id);
        return await cmd.ExecuteSelect<Customer>(Map);
    }

    public async Task CreateAsync(Customer customer) {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO customers (name, email, created_at)
            VALUES (@name, @email, @createdAt)
            RETURNING id";
        cmd.AddParameter("name", customer.Name);
        cmd.AddParameter("email", (object?)customer.Email ?? DBNull.Value);
        cmd.AddParameter("createdAt", customer.CreatedAt);

        var newId = await cmd.ExecuteGetValue<int>("id");
        customer.AssignId(newId);
    }

    public async Task UpdateAsync(Customer customer) {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE customers SET name = @name, email = @email
            WHERE id = @id";
        cmd.AddParameter("id", customer.Id);
        cmd.AddParameter("name", customer.Name);
        cmd.AddParameter("email", (object?)customer.Email ?? DBNull.Value);

        var affected = await cmd.ExecuteCommandNonQuery();
        if (!affected)
            throw new CustomerNotFoundException(customer.Id);
    }

    public async Task DeleteAsync(int id) {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM customers WHERE id = @id";
        cmd.AddParameter("id", id);
        var affected = await cmd.ExecuteCommandNonQuery();
        if (!affected)
            throw new CustomerNotFoundException(id);
    }
}
```

Notar `[Injectable(ServiceLifetime.Scoped)]`: esto basta para que aparezca en el contenedor.

### Paso 5 — DTO en `Application`

Crear `Application/Customers/CustomerDto.cs`:
```csharp
using MyApp.Domain.Customers;
namespace MyApp.Application.Customers;

public record CustomerDto(int Id, string Name, string? Email, DateTime CreatedAt) {
    public static CustomerDto From(Customer c) => new(c.Id, c.Name, c.Email, c.CreatedAt);
}
```

### Paso 6 — Commands/Queries y Handlers

Un archivo por par. Ejemplo para `Create`:

`Application/Customers/CreateCustomer/CreateCustomerCommand.cs`:
```csharp
using MyApp.Application.Customers;
using MediatR;
namespace MyApp.Application.Customers.CreateCustomer;

public record CreateCustomerCommand(string Name, string? Email) : IRequest<CustomerDto>;
```

`Application/Customers/CreateCustomer/CreateCustomerCommandHandler.cs`:
```csharp
using MyApp.Application.Customers;
using MyApp.Domain.Customers;
using MyApp.Persistence.Customers.Interfaces;
using MediatR;

namespace MyApp.Application.Customers.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto> {
    private readonly ICustomerRepository _repository;
    public CreateCustomerCommandHandler(ICustomerRepository repository) => _repository = repository;

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken) {
        // El dominio decide si los datos son válidos. El handler no valida reglas de negocio.
        var customer = Customer.Create(request.Name, request.Email);
        await _repository.CreateAsync(customer);
        return CustomerDto.From(customer);
    }
}
```

Notar el flujo: el handler **no** valida que el nombre no esté vacío ni que el email tenga formato — eso lo hace `Customer.Create`. El handler solo orquesta. Si mañana aparece otro caso de uso que también crea customers (importación masiva, registro vía API externa), todos pasan por el mismo factory method y aplican las mismas reglas.

Para el Update el patrón es análogo:

```csharp
public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken ct) {
    var customer = await _repository.GetByIdAsync(request.Id)
        ?? throw new CustomerNotFoundException(request.Id);

    customer.Update(request.Name, request.Email); // la entidad valida y muta

    await _repository.UpdateAsync(customer);
    return CustomerDto.From(customer);
}
```

Para la query paginada, recordar implementar `IPagedQuery`:
```csharp
public record GetCustomersQuery(string? Search, int Page, int PageSize)
    : IRequest<PagedResult<CustomerDto>>, IPagedQuery;
```

Y el handler usa `PagedDataExtensions.ToPagedResult(...)`:
```csharp
var filter = new CustomersFilter(request.Search, request.Page, request.PageSize);
var data = await _repository.GetAllAsync(filter);
return data.ToPagedResult(filter.PageSize, CustomerDto.From);
```

### Paso 7 — Request model en la capa Web

Crear `Web/Controllers/Customers/CustomerRequest.cs`:
```csharp
using System.ComponentModel.DataAnnotations;
namespace MyApp.Web.Controllers.Customers;

public class CustomerRequest {
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, MaxLength(100)]
    public string? Email { get; set; }
}
```

### Paso 8 — Controller

Crear `Web/Controllers/Customers/CustomersController.cs`:
```csharp
using MyApp.Application.Customers;
using MyApp.Application.Customers.CreateCustomer;
using MyApp.Application.Customers.DeleteCustomer;
using MyApp.Application.Customers.GetCustomerById;
using MyApp.Application.Customers.GetCustomers;
using MyApp.Application.Customers.UpdateCustomer;
using MyApp.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Web.Controllers.Customers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CustomersController : ControllerBase {
    private readonly IMediator _mediator;
    public CustomersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<PagedResult<CustomerDto>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) {
        return await _mediator.Send(new GetCustomersQuery(search, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<CustomerDto> GetById(int id) {
        // Si no existe, el handler lanza CustomerNotFoundException.
        return await _mediator.Send(new GetCustomerByIdQuery(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<CustomerDto> Create([FromBody] CustomerRequest request) {
        return await _mediator.Send(new CreateCustomerCommand(request.Name, request.Email));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<CustomerDto> Update(int id, [FromBody] CustomerRequest request) {
        // Si no existe, el handler lanza CustomerNotFoundException.
        return await _mediator.Send(new UpdateCustomerCommand(id, request.Name, request.Email));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task Delete(int id) {
        // Si no existe, el repositorio lanza CustomerNotFoundException.
        await _mediator.Send(new DeleteCustomerCommand(id));
    }
}
```

### Paso 9 — Tabla en la base de datos

Este enfoque **no usa migraciones** (no hay EF Core). La creación de tablas se hace manualmente:
```sql
CREATE TABLE customers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
```

### Paso 10 — ¡No se necesita registrar nada más!

Gracias al escaneo por `[Injectable]` y a que MediatR escanea assemblies con `RegisterServicesFromAssemblies`, el repositorio y los handlers quedan disponibles automáticamente en la próxima ejecución.

---

## 12. Operaciones multi-agregado con transacciones

Cuando un caso de uso toca más de un agregado, el patrón es:

1. **Construir/cargar los agregados aplicando reglas del dominio** (factory methods, métodos de mutación). Las validaciones ocurren *antes* de tocar la DB.
2. Inyectar los repositorios **y** `ICConnection` en el handler.
3. Iniciar transacción.
4. Ejecutar las operaciones, cada una a su repositorio correspondiente.
5. Commit; en catch, rollback y rethrow.
6. Mapear el agregado a DTO (no hace falta releer de la DB: el agregado en memoria ya tiene el estado final).

Ejemplo — crear una orden que además descuenta stock:

```csharp
public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken) {
    // 1. El dominio construye el agregado válido. Si algo falla, lanza DomainException
    //    y nunca llegamos a abrir transacción.
    var items = request.Items
        .Select(i => OrderItem.Create(i.ProductId, i.Quantity, i.UnitPrice))
        .ToList();
    var order = Order.Create(request.CustomerId, request.Date, items);

    await _connection.BeginTransaction();
    try {
        await _orderRepository.CreateAsync(order); // inserta cabecera + items, asigna Id

        foreach (var item in order.Items) {
            await _stockRepository.RegisterMovementAsync(
                StockMovementTypes.Sale,
                item.ProductId,
                -item.Quantity,
                order.Id,
                $"Order #{order.Id}");
        }

        await _connection.CommitTransaction();
        return OrderDto.From(order);
    } catch {
        await _connection.CancelTransaction();
        throw;
    }
}
```

Las constantes de tipos de movimiento viven en `Domain` y se usan como contrato entre handler y repositorio (que las traduce vía lookup `SELECT id FROM movement_types WHERE name = @type`).

**Antipatrón a evitar**: hacer la validación dentro de la transacción.

```csharp
// MAL: si el primer item es inválido, ya abrimos transacción para nada.
// Peor: si la validación se olvida en algún caso de uso, los datos llegan
// inconsistentes a la DB.
await _connection.BeginTransaction();
foreach (var item in request.Items) {
    if (item.Quantity <= 0) throw new InvalidOperationException("...");
    await _orderRepository.AddItemAsync(...);
}
```

Construir el agregado primero garantiza que la transacción solo se abra cuando los datos son válidos según el dominio.

---

## 13. Seguridad y privacidad

### 13.1 Datos sensibles en logs

Un atributo `[IsSensitiveInformation]` puede marcar propiedades que nunca deben loguearse en texto plano. El `LoggingMiddleware` las reemplaza por un marcador como `[SENSITIVE_INFORMATION]` al serializar request bodies.

Ejemplo — un request de login:
```csharp
public class LoginRequest {
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [IsSensitiveInformation]
    public string Password { get; set; } = string.Empty;
}
```

### 13.2 Headers de seguridad

Un `SecurityHeadersMiddleware` agrega HSTS, CSP, X-Frame-Options, etc. en producción. Se puede dejar en la pipeline comentado durante desarrollo y activar en producción.

### 13.3 Tokens sensibles en auth

Los refresh tokens se persisten en DB y se revocan en cada rotación para prevenir replay. Los hashes de contraseña usan BCrypt.

### 13.4 Rate limiting

Configuración recomendada: 100 requests/minuto por IP. Si se excede, devuelve `429 Too Many Requests`.

---

## 14. Validación

### 14.1 Validación declarativa

Con `DataAnnotations` en los request models: `[Required]`, `[MaxLength(N)]`, `[EmailAddress]`, `[Range(min,max)]`, `[MinLength(N)]`.

### 14.2 Filter que convierte a excepción

`ModelStateValidationFilter` corre antes de la acción. Si `ModelState.IsValid` es false, construye un diccionario de errores y lanza `ModelValidationException`, que `ExceptionMiddleware` traduce a `422 Unprocessable Entity` con problem details.

### 14.3 Validación de paginación

`PageValidationBehavior` (pipeline de MediatR) verifica `Page >= 1` para cualquier request que implemente `IPagedQuery`. No se duplica en cada handler.

### 14.4 Reglas de negocio

**Las reglas de negocio viven en la entidad de dominio**, no en el handler. El handler orquesta (cargar, invocar, persistir) pero no decide. Esta es la consecuencia directa de tener un dominio rico (sección 2.1).

> **Aclaración: orquestación vs. regla de negocio.** Verificar que un agregado **exista** antes de operarlo (`var customer = await _repository.GetByIdAsync(id) ?? throw new CustomerNotFoundException(id);`) es **orquestación**, no regla de negocio: el handler está cargando lo que necesita y traduciendo "no hay registro" al lenguaje del dominio. La regla de negocio sería, por ejemplo, "no se puede actualizar un cliente bloqueado" — y eso *sí* va dentro del método de la entidad. La distinción es: si la decisión depende del **estado** del agregado, es regla de negocio (entidad); si depende de su **presencia**, es orquestación (handler).

Patrón correcto — cancelar una orden:

```csharp
// Handler: orquesta. No conoce las reglas.
public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken ct) {
    var order = await _repository.GetByIdAsync(request.Id)
        ?? throw new OrderNotFoundException(request.Id);

    order.Cancel(); // toda la regla de negocio vive aquí adentro

    await _repository.UpdateAsync(order);
    return Unit.Value;
}
```

```csharp
// Entidad: dueña de la regla.
public void Cancel() {
    if (Status == OrderStatus.Cancelled)
        throw new OrderAlreadyCancelledException(Id);
    if (Status == OrderStatus.Shipped)
        throw new OrderCannotBeCancelledAfterShippingException(Id);

    Status = OrderStatus.Cancelled;
}
```

Antipatrón a evitar — reglas dispersas en el handler:

```csharp
// MAL: el handler conoce el modelo de transiciones de estado.
// El día que aparezca un nuevo handler que también cancele, las reglas se duplican
// (y se desincronizan).
var original = await _repository.GetByIdAsync(request.Id);
if (original == null)
    throw new KeyNotFoundException($"Order #{request.Id} not found.");
if (original.Cancelled)
    throw new InvalidOperationException($"Order #{request.Id} is already cancelled.");
original.Cancelled = true;
await _repository.UpdateAsync(original);
```

La diferencia no es solo estética: con las reglas en la entidad, cualquier flujo que termine llamando `order.Cancel()` aplica las mismas validaciones automáticamente. Con las reglas en el handler, cada nuevo caso de uso es una oportunidad para introducir una inconsistencia.

Cuando una regla requiere datos que la entidad no tiene cargados (ej. "no se puede crear una orden si el cliente está bloqueado"), el handler hace la consulta y le pasa el resultado al dominio:

```csharp
var customer = await _customerRepository.GetByIdAsync(request.CustomerId)
    ?? throw new CustomerNotFoundException(request.CustomerId);

// El chequeo final lo hace la entidad sobre los datos que recibe.
var order = Order.Create(customer, request.Date, items);
```

---

## 15. Configuración

### 15.1 `appsettings.json`

Secretos se mantienen fuera del repositorio. El `.gitignore` excluye `appsettings.*.json` excepto el base. En el base viven solo las llaves vacías:

```json
{
  "ConnectionStrings": { "value": "" },
  "Auth": {
    "Secret": "",
    "ValidAudience": "",
    "ValidIssuer": "",
    "AccessTokenExpirationMinutes": 0,
    "RefreshTokenExpirationDays": 0
  }
}
```

Valores reales se suministran vía `appsettings.Development.json`, variables de entorno, o el mecanismo de user-secrets.

### 15.2 Arranque (Program.cs)

`Program.cs` es minimalista: un único statement encadenado que delega toda la configuración a extension methods modulares.

```csharp
WebApplication.CreateBuilder(args)
              .Configure()
              .Build()
              .Configure()
              .Run();
```

El primer `.Configure()` es sobre `WebApplicationBuilder` (extension method). El segundo es sobre `WebApplication` (otro extension method). Cada uno agrupa su propia configuración en módulos (CORS, JSON, Swagger, Auth, Controllers, etc.).

---

## 16. Checklist al agregar funcionalidad

Antes de considerar "listo" un caso de uso nuevo:

1. [ ] Entidad en `Domain` **con setters privados, factory method `Create(...)` y métodos de mutación con nombre de intención**. Sin setters públicos, sin constructores públicos vacíos.
2. [ ] Invariantes y reglas de negocio **dentro de la entidad** (lanzando `DomainException`), no en el handler ni el repositorio.
3. [ ] Value objects para conceptos con reglas propias (`Email`, `Money`, etc.) en lugar de strings/decimales sueltos.
4. [ ] Interface de repositorio en `Persistence/<Feature>/Interfaces` **que recibe la entidad completa** en `Create`/`Update`, no parámetros sueltos.
5. [ ] Implementación con `[Injectable(ServiceLifetime.Scoped)]` en `Persistence/<Feature>/Repositories`.
6. [ ] DTO en `Application/<Feature>` con método estático `From`.
7. [ ] Command/Query como `record` implementando `IRequest<TResponse>` (y `IPagedQuery` si corresponde).
8. [ ] Handler implementando `IRequestHandler<T, TResponse>` que **orquesta** (carga, invoca métodos del dominio, persiste). No valida reglas de negocio.
9. [ ] Request model en la capa Web con DataAnnotations (validación de input, no de dominio).
10. [ ] Controller con `[Authorize]` mínimo + `[Authorize(Roles="Admin")]` en endpoints sensibles.
11. [ ] SQL con parámetros (`@name`), nunca concatenación.
12. [ ] `null` a DB → `(object?)value ?? DBNull.Value`.
13. [ ] Paginación con `COUNT(*) OVER() AS total_count` y `PaginationHelper.FetchPagedAsync`.
14. [ ] Operaciones multi-tabla → construir el agregado primero, *luego* abrir transacción en el handler.
15. [ ] Errores → lanzar **clases concretas que heredan de `DomainException`** desde el dominio. Nunca `throw new DomainException(...)` directo (es `abstract`), nunca excepciones genéricas de la BCL (`KeyNotFoundException`, `InvalidOperationException`), nunca `IActionResult`.
16. [ ] **Una clase de excepción por cada error de negocio identificable** en `Domain/<Feature>/Exceptions/`. `sealed`, nombre que termine en `Exception`, mensaje y `ErrorType` fijados dentro de la clase.
17. [ ] Datos sensibles en request → `[IsSensitiveInformation]`.
18. [ ] Tabla SQL creada en la base de datos.
19. [ ] `InternalsVisibleTo` configurado entre Domain y Persistence si las entidades usan `internal set` para hidratación.

---

## 17. Trampas conocidas / gotchas

- **`ICConnection` es Scoped**: dos repositorios en el mismo request comparten la conexión, pero entre requests diferentes son instancias distintas. No guardar estado.
- **`CCommand` consulta la transacción al construirse**: si inicias una transacción *después* de crear el comando, ese comando no la usará. Siempre `BeginTransaction` → `CreateCommand` → operar.
- **MediatR (v12+)** requiere `RegisterServicesFromAssemblies(params Assembly[])` para registrar handlers (la API anterior basada en `AddMediatR(typeof(...))` quedó obsoleta). El código carga los assemblies vía `AppDomain.GetProjectAssemblies()`, no asume que estén ya cargados.
- **`DomainException` es `abstract`**: no se puede instanciar directamente, hay que crear (o usar) una subclase concreta. Si el compilador te tira `Cannot create an instance of the abstract type 'DomainException'`, ese es el motivo: creá la clase concreta correspondiente en `Domain/<Feature>/Exceptions/`.
- **`DomainException` con status 4xx se loggea como Warning, no Error**. Si lanzás una `DomainException` con 500 se promueve a Error automáticamente.
- **`ModelValidationException` hereda de `DomainException`** pero tiene su propio payload `Errors` — el middleware la trata por separado en el switch.
- **El `ResponseFilter` envuelve todo**: si necesitas retornar algo sin envolver (ej. un binario o stream), el filtro lo romperá; considerar retornar `FileResult` directamente o bypassear el filtro para ese endpoint.
- **Escaneo de assemblies por prefijo**: asegurate de que `AssemblyName` en todos los `.csproj` siga la convención, de lo contrario el auto-registro DI no encontrará las clases.

---

## 18. Resumen visual de flujo por feature

```
HTTP Request
    │
    ▼
[Middleware chain] ─ CORS ─ Compression ─ RateLimiter ─ Exception ─ DbConnection ─ Auth
    │
    ▼
[Controller] ──────────▶ _mediator.Send(Command/Query)
    │                              │
    │                              ▼
    │                    [Pipeline Behaviors]
    │                              │  (PageValidationBehavior, etc.)
    │                              ▼
    │                         [Handler]
    │                              │
    │                              ▼
    │                        [Repository]
    │                              │
    │                              ▼
    │                  [ICConnection / ICCommand]
    │                              │
    │                              ▼
    │                          PostgreSQL
    │                              │
    │                              ▼
    │                       Domain Entity
    │                              ▲
    │                      (Map con ICDataReader)
    │                              │
    │                              ▼
    │                      DTO.From(entity)
    │                              │
    ◀──────────────────────────────┘
    │
    ▼
[ResponseFilter] envuelve en { statusCode, message, data }
    │
    ▼
HTTP Response (gzip)
```

Si algo falla en cualquier punto, el `ExceptionMiddleware` toma control y emite un `ApiProblemDetails` con formato estándar RFC 7807 + extensiones (`traceId`, `timestamp`, `errors`).

---

# SECCIÓN 2 — ENDPOINTS DE LA API

> Contenido completo de `coleccionaloya_endpoints.md`

# ColeccionaloYa - Documentación de Endpoints API

Documentación completa de la API REST para el proyecto ColeccionaloYa, un e-commerce especializado en productos de colección (cómics, mangas, packs y objetos coleccionables).

---

## Índice

1. [Autenticación](#1-autenticación)
2. [Roles](#2-roles)
3. [Clientes](#3-clientes)
4. [Direcciones](#4-direcciones)
5. [Productos (catálogo general)](#5-productos-catálogo-general)
6. [Libros (cómics/mangas)](#6-libros-cómicsmangas)
7. [Packs](#7-packs)
8. [Autores](#8-autores)
9. [Géneros](#9-géneros)
10. [Tags / Franquicias](#10-tags--franquicias)
11. [Galería de productos](#11-galería-de-productos)
12. [Menú de navegación](#12-menú-de-navegación)
13. [Carrito](#13-carrito)
14. [Órdenes](#14-órdenes)
15. [Cupones](#15-cupones)
16. [Descuentos por producto](#16-descuentos-por-producto)
17. [Stock](#17-stock)
18. [Wishlist](#18-wishlist)
19. [Dashboard / Reportes](#19-dashboard--reportes)
20. [Upload de archivos](#20-upload-de-archivos)
21. [Pagos](#21-pagos-integración-con-pasarela)

---

## 1. Autenticación

Gestiona el ciclo completo de autenticación: registro, inicio y cierre de sesión, refresco de tokens y cambio de contraseña. Los tokens JWT se firman con HS256; los refresh tokens son GUIDs persistidos en la tabla `refresh_tokens` con `expires_at` y flag `revoked`.

> **Convenciones:**
> - Todas las respuestas exitosas con body se devuelven envueltas por `ResponseFilter` en la estructura `{ statusCode: "OK", message: "Success", data: <payload> }`. Los bloques `ts` de este documento representan el contenido del campo `data`.
> - Los endpoints cuyo handler no retorna valor (`Task`) devuelven `204 No Content` sin envoltura.
> - El cliente envía el `token` (access token) en el header `Authorization: Bearer <token>` en endpoints protegidos.
> - El `refreshToken` se usa únicamente contra `/api/Auth/refresh` para obtener un nuevo par de tokens. En cada refresh, el anterior queda revocado (rotación).
> - Las contraseñas se envían en texto plano por HTTPS y se hashean con BCrypt en backend (almacenadas en `client.password_hash`).

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/Auth/register` | Registra un nuevo cliente con rol `User` por defecto. No devuelve tokens: tras el registro el cliente debe hacer login explícito. | No | No |

**Parámetros** (body):
```ts
{
  name: string,                        // máx 50 chars
  lastname: string,                    // máx 50 chars
  email: string,                       // único en el sistema, formato válido
  password: string                     // mín 8 chars; se hashea con BCrypt en backend
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: el email ya está registrado, o no existe el rol `User` en la tabla `roles`.
- `422 Unprocessable Entity`: el body no cumple las validaciones (`email` vacío/no válido, `password` < 8 chars, campos requeridos ausentes). Respuesta con diccionario `errors` por campo.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/Auth/login` | Autentica al cliente con email + password y devuelve el par de tokens (access + refresh). | No | No |

**Parámetros** (body):
```ts
{
  email: string,                       // requerido, formato email válido
  password: string                     // requerido, mín 6 chars
}
```

**Respuesta** `200 OK`:
```ts
{
  token: string,                       // JWT HS256. Claims: NameIdentifier (id_client), Email, Role
  refreshToken: string,                // GUID; expira según Auth:RefreshTokenExpirationDays
  expiresIn: long                      // segundos hasta expiración del access token
}
```

**Errores posibles:**
- `401 Unauthorized`: email no registrado, usuario inactivo (`active = false`) o password incorrecto.
- `422 Unprocessable Entity`: el body no cumple las validaciones (`email` vacío/no válido, `password` vacío o < 6 chars). Respuesta con diccionario `errors` por campo.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/Auth/refresh` | Intercambia un `refreshToken` válido por un nuevo par de tokens. El refresh anterior queda revocado (rotación). | No | No |

**Parámetros** (body):
```ts
{
  token: string,                       // access token actual (puede estar expirado)
  refreshToken: string                 // refresh token emitido previamente
}
```

**Respuesta** `200 OK`:
```ts
{
  token: string,                       // nuevo JWT
  refreshToken: string,                // nuevo refresh token (el anterior queda revoked = true)
  expiresIn: long                      // segundos hasta expiración del nuevo access token
}
```

**Errores posibles:**
- `401 Unauthorized`: el `refreshToken` no existe, está revocado (`revoked = true`), expiró (`expires_at <= NOW()`) o el usuario asociado está inactivo.
- `422 Unprocessable Entity`: `token` o `refreshToken` ausentes/vacíos en el body.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/Auth/logout` | Revoca el `refreshToken` indicado (lo marca `revoked = true`). El `token` de acceso seguirá siendo válido hasta expirar naturalmente. | Sí | No |

**Parámetros** (body):
```ts
{
  refreshToken: string                 // requerido, token a revocar
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `401 Unauthorized`: header `Authorization` ausente, JWT inválido o expirado.
- `422 Unprocessable Entity`: `refreshToken` ausente/vacío en el body.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/Auth/change-password` | Cambia la contraseña del cliente autenticado. Requiere la contraseña actual como confirmación. Al ejecutarse, todos los `refresh_tokens` del cliente quedan revocados (incluido el de la sesión actual), forzando un nuevo login en todos los dispositivos. | Sí | No |

**Parámetros** (body):
```ts
{
  currentPassword: string,             // requerido
  newPassword: string                  // requerido, mín 8 chars, debe ser distinta a la actual
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `401 Unauthorized`: header `Authorization` ausente, JWT inválido/expirado, o `currentPassword` incorrecta.
- `409 Conflict`: la `newPassword` coincide con la contraseña actual.
- `422 Unprocessable Entity`: el body no cumple las validaciones (`currentPassword` o `newPassword` vacíos, `newPassword` < 8 chars). Respuesta con diccionario `errors` por campo.

---

## 2. Roles

Todos los endpoints de esta sección requieren autenticación y rol Admin.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/roles` | Lista todos los roles disponibles en el sistema. |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
[
  {
    id: int,
    name: string,            // ej: "Admin", "User"
    description: string      // puede ser null
  }
]
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/roles/:id` | Obtiene el detalle de un rol específico. |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id: int,
  name: string,
  description: string        // puede ser null
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/roles` | Crea un nuevo rol para asignar a usuarios. |

**Parámetros** (body):
```ts
{
  name: string,              // requerido, máx 50 chars
  description?: string       // opcional
}
```

**Respuesta** `201 Created`:
```ts
{
  id: int,
  name: string,
  description: string
}
```

**Errores posibles:**
- `409 Conflict`: ya existe un rol con ese `name`.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| PUT | `/api/roles/:id` | Actualiza el nombre o descripción de un rol existente. |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  name?: string,             // máx 50 chars
  description?: string
}
```

**Respuesta** `200 OK`:
```ts
{
  id: int,
  name: string,
  description: string
}
```

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| DELETE | `/api/roles/:id` | Elimina un rol del sistema (si no tiene usuarios asociados). |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: el rol tiene usuarios asignados y no puede eliminarse.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 3. Clientes

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/clients` | Lista todos los clientes con paginación y filtros (vista administrativa). | Sí | Sí |

**Parámetros** (query):
```ts
{
  page?: int,            // default: 1
  limit?: int,           // default: 20
  search?: string,       // busca en name, lastname y email
  role_id?: int,         // filtra por rol
  active?: boolean       // filtra por estado activo/inactivo
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_client: int,
      name: string,
      lastname: string,
      email: string,
      phone: string,
      id_address_delivery: int,    // puede ser null
      id_address_order: int,       // puede ser null
      role_id: int,
      active: boolean,
      creation_date: datetime
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/clients/:id` | Obtiene el detalle completo de un cliente incluyendo sus direcciones. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  client: {
    id_client: int,
    name: string,
    lastname: string,
    email: string,
    phone: string,
    id_address_delivery: int,    // puede ser null
    id_address_order: int,       // puede ser null
    role_id: int,
    active: boolean,
    creation_date: datetime
  },
  addresses: [
    {
      id_address: int,
      id_client: int,
      state: string,
      type: 'house' | 'apartment' | 'work',
      observations: string,
      apartment: string,
      corner: string,
      door_number: string,
      floor: string,
      name: string,
      neighborhood: string,
      street: string
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/clients/me` | Obtiene el perfil del cliente autenticado. | Sí | No |

**Parámetros:** ninguno (se usa el token del header).

**Respuesta** `200 OK`: misma estructura que `/api/clients/:id`.

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/clients` | Crea un nuevo cliente asignándole un rol (uso administrativo). Para auto-registro desde el frontend público usar `/api/auth/register`. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string,              // máx 50 chars
  lastname: string,          // máx 50 chars
  email: string,             // único en el sistema
  phone: string,             // máx 10 chars
  password: string,          // se hashea en backend
  role_id: int
}
```

**Respuesta** `201 Created`:
```ts
{
  id_client: int,
  name: string,
  lastname: string,
  email: string,
  phone: string,
  role_id: int,
  active: boolean,             // true por default
  creation_date: datetime
}
```

**Errores posibles:**
- `409 Conflict`: el `email` ya está registrado.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/clients/:id` | Actualiza los datos de un cliente, incluyendo rol y direcciones predefinidas. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  name?: string,
  lastname?: string,
  email?: string,
  phone?: string,
  role_id?: int,
  active?: boolean,
  id_address_delivery?: int,
  id_address_order?: int
}
```

**Respuesta** `200 OK`: cliente actualizado (misma estructura que `POST /api/clients`).

**Errores posibles:**
- `409 Conflict`: el nuevo `email` ya está en uso por otro cliente.
- `422 Unprocessable Entity`: `id_address_delivery` o `id_address_order` no pertenecen al cliente.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/clients/me` | Permite al cliente autenticado actualizar su propio perfil. | Sí | No |

**Parámetros** (body):
```ts
{
  name?: string,
  lastname?: string,
  phone?: string,
  id_address_delivery?: int,
  id_address_order?: int
}
```

**Respuesta** `200 OK`: cliente actualizado.

**Errores posibles:**
- `422 Unprocessable Entity`: `id_address_delivery` o `id_address_order` no pertenecen al cliente.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PATCH | `/api/clients/:id/activate` | Activa la cuenta de un cliente previamente desactivada. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`: cliente actualizado con `active: true`.

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PATCH | `/api/clients/:id/deactivate` | Desactiva la cuenta de un cliente sin eliminarla. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`: cliente actualizado con `active: false`.

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/clients/:id` | Elimina lógicamente un cliente (mantiene histórico de órdenes). | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 4. Direcciones

Todos los endpoints requieren autenticación. No requieren rol Admin (los clientes gestionan sus propias direcciones).

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/clients/:id_client/addresses` | Lista todas las direcciones de un cliente específico. |

**Parámetros** (path):
```ts
{
  id_client: int
}
```

**Respuesta** `200 OK`:
```ts
[
  {
    id_address: int,
    id_client: int,
    state: string,
    type: 'house' | 'apartment' | 'work',
    observations: string,
    apartment: string,
    corner: string,
    door_number: string,
    floor: string,
    name: string,
    neighborhood: string,
    street: string
  }
]
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/addresses/:id` | Obtiene el detalle de una dirección por su ID. |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`: misma estructura que un item del listado anterior.

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/clients/:id_client/addresses` | Agrega una nueva dirección al cliente (casa, trabajo, etc.). |

**Parámetros** (path + body):
```ts
// Path
{
  id_client: int
}

// Body
{
  state: string,                                  // máx 20 chars (departamento)
  type: 'house' | 'apartment' | 'work',
  observations?: string,                          // máx 200 chars
  apartment?: string,                             // máx 20 chars
  corner?: string,                                // máx 100 chars
  door_number?: string,                           // máx 10 chars
  floor?: string,                                 // máx 50 chars
  name: string,                                   // máx 50 chars (nombre/alias de la dirección)
  neighborhood: string,                           // máx 100 chars
  street: string                                  // máx 200 chars
}
```

**Respuesta** `201 Created`: dirección creada (misma estructura que el listado).

**Errores posibles:**
- `403 Forbidden`: el `id_client` de la URL no corresponde al usuario autenticado (salvo Admin).
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| PUT | `/api/addresses/:id` | Actualiza los datos de una dirección existente. |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body — todos los campos son opcionales (mismas restricciones que en POST)
{
  state?: string,
  type?: 'house' | 'apartment' | 'work',
  observations?: string,
  apartment?: string,
  corner?: string,
  door_number?: string,
  floor?: string,
  name?: string,
  neighborhood?: string,
  street?: string
}
```

**Respuesta** `200 OK`: dirección actualizada.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| DELETE | `/api/addresses/:id` | Elimina lógicamente una dirección del cliente. |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: la dirección está referenciada como `id_address_delivery` o `id_address_order` predeterminada del cliente.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 5. Productos (catálogo general)

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/products` | Lista el catálogo de productos con filtros avanzados, búsqueda, ordenamiento y agrupadores especiales (destacados, novedades, en oferta, relacionados). Único endpoint de búsqueda/listado de productos del sistema. | No | No |

**Parámetros** (query):
```ts
{
  page?: int,                                                                       // default: 1
  limit?: int,                                                                      // default: 20
  search?: string,                                                                  // busca en name y short_description
  type?: 'book' | 'pack' | 'object',                                                // filtra por tipo de producto
  type_book?: 'comic' | 'manga',                                                    // solo aplica si type='book'
  tags?: int[],                                                                     // ids de tags (incluye franquicias)
  genres?: int[],                                                                   // ids de géneros (solo libros)
  authors?: int[],                                                                  // ids de autores (solo libros)
  franchise?: int,                                                                  // id de tag con is_franchise=true
  min_price?: decimal,
  max_price?: decimal,
  in_stock?: boolean,                                                               // true = solo con stock disponible
  on_sale?: boolean,                                                                // true = solo con descuento activo
  featured?: boolean,                                                               // products.is_featured = true
  new_arrivals?: boolean,                                                           // ordena por creation_date desc (últimos 30 días)
  related_to?: int,                                                                 // id de un producto: devuelve productos similares
  sort?: 'price_asc' | 'price_desc' | 'newest' | 'name' | 'best_sellers'            // default: 'newest'
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_product: int,
      name: string,
      short_description: string,
      price: decimal,
      type: 'book' | 'pack' | 'object',
      weight: decimal,
      is_featured: boolean,
      creation_date: date,
      cover_url: string,                       // imagen de product_gallery con menor `order` o null
      current_stock: int,                      // sum(stock.input) - sum(stock.output) para el producto
      discount: {                              // null si no hay descuento activo
        id_discount: int,
        porcentage: decimal,
        valid_from: datetime,
        valid_until: datetime,
        final_price: decimal                   // price * (1 - porcentage/100)
      } | null,
      type_book: 'comic' | 'manga' | null      // solo si type='book'
    }
  ],
  total: int,
  page: int,
  limit: int,
  filters_applied: {                           // eco de los filtros aplicados
    type: string | null,
    tags: int[],
    genres: int[],
    authors: int[],
    min_price: decimal | null,
    max_price: decimal | null,
    in_stock: boolean | null,
    on_sale: boolean | null
  }
}
```

**Errores posibles:**
- `400 Bad Request`: query params con formato inválido (ej: `min_price` no numérico, `tags` mal formado, `sort` fuera de los valores permitidos).

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/products/:id` | Obtiene el detalle completo de un producto con galería, tags, stock y descuentos. | No | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  product: {
    id_product: int,
    name: string,
    short_description: string,
    long_description: string,
    price: decimal,
    type: 'book' | 'pack' | 'object',
    weight: decimal,
    is_featured: boolean,
    creation_date: date
  },
  gallery: [
    {
      id_gallery: int,
      id_product: int,
      order: int,
      url: string
    }
  ],
  tags: [
    {
      id_tag: int,
      name: string,
      is_franchise: boolean
    }
  ],
  discount: {                            // null si no hay descuento activo
    id_discount: int,
    porcentage: decimal,
    valid_from: datetime,
    valid_until: datetime,
    final_price: decimal
  } | null,
  stock: {
    current_stock: int,                  // sum(input) - sum(output)
    last_movement_date: datetime | null
  }
}
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/products` | Crea un nuevo producto genérico en el catálogo (usar `/api/books` o `/api/packs` para esos tipos). | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string,                                   // máx 100 chars
  short_description: string,                      // máx 100 chars
  long_description: string,
  price: decimal,
  type: 'book' | 'pack' | 'object',
  weight: decimal,                                // en kg
  is_featured?: boolean                           // default: false
}
```

**Respuesta** `201 Created`:
```ts
{
  id_product: int,
  name: string,
  short_description: string,
  long_description: string,
  price: decimal,
  type: 'book' | 'pack' | 'object',
  weight: decimal,
  is_featured: boolean,
  creation_date: date
}
```

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/products/:id` | Actualiza los datos de un producto existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body — todos los campos opcionales
{
  name?: string,
  short_description?: string,
  long_description?: string,
  price?: decimal,
  type?: 'book' | 'pack' | 'object',
  weight?: decimal,
  is_featured?: boolean
}
```

**Respuesta** `200 OK`: producto actualizado.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/products/:id` | Elimina lógicamente un producto del catálogo (`logical_delete = true`). | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: el producto está incluido en órdenes activas (pendientes/en proceso) o compone packs activos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 6. Libros (cómics/mangas)

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/books/:id` | Obtiene el detalle de un libro con sus autores y géneros. | No | No |

**Parámetros** (path):
```ts
{
  id: int      // id_product del libro
}
```

**Respuesta** `200 OK`:
```ts
{
  book: {
    id_product: int,
    type_book: 'comic' | 'manga'
  },
  product: {                       // datos generales del producto asociado
    id_product: int,
    name: string,
    short_description: string,
    long_description: string,
    price: decimal,
    weight: decimal,
    is_featured: boolean,
    creation_date: date
  },
  authors: [
    {
      id_author: int,
      name: string
    }
  ],
  genres: [
    {
      id_genre: int,
      name: string
    }
  ]
}
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/books` | Crea un nuevo libro (cómic o manga) con sus autores y géneros. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string,
  short_description: string,
  long_description: string,
  price: decimal,
  weight: decimal,
  type_book: 'comic' | 'manga',
  author_ids: int[],            // ids de autores a vincular
  genre_ids: int[],             // ids de géneros a vincular
  is_featured?: boolean         // default: false
}
```

**Respuesta** `201 Created`:
```ts
{
  id_product: int,
  name: string,
  type_book: 'comic' | 'manga',
  authors: [{ id_author: int, name: string }],
  genres: [{ id_genre: int, name: string }]
}
```

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `author_ids` o `genre_ids` no existe.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/books/:id` | Actualiza los datos de un libro existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body — todos los campos opcionales
{
  name?: string,
  short_description?: string,
  long_description?: string,
  price?: decimal,
  weight?: decimal,
  type_book?: 'comic' | 'manga'
}
```

**Respuesta** `200 OK`: libro actualizado.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/books/:id/authors` | Agrega uno o varios autores a un libro. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  author_ids: int[]
}
```

**Respuesta** `200 OK`:
```ts
{
  id_product: int,
  authors: [{ id_author: int, name: string }]
}
```

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `author_ids` no existe.
- `409 Conflict`: uno o más autores ya estaban vinculados al libro.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/books/:id/authors/:id_author` | Desvincula un autor específico de un libro. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int,            // id_product del libro
  id_author: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `404 Not Found`: el libro o el autor no existe, o no están vinculados entre sí.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/books/:id/genres` | Agrega uno o varios géneros a un libro. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  genre_ids: int[]
}
```

**Respuesta** `200 OK`:
```ts
{
  id_product: int,
  genres: [{ id_genre: int, name: string }]
}
```

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `genre_ids` no existe.
- `409 Conflict`: uno o más géneros ya estaban vinculados al libro.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/books/:id/genres/:id_genre` | Desvincula un género específico de un libro. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int,
  id_genre: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `404 Not Found`: el libro o el género no existe, o no están vinculados entre sí.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 7. Packs

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/packs/:id` | Obtiene el detalle de un pack con los productos que lo componen y cantidades. | No | No |

**Parámetros** (path):
```ts
{
  id: int      // id_product del pack
}
```

**Respuesta** `200 OK`:
```ts
{
  pack: {
    id_product: int,             // id del producto-pack
    product_count: int,          // cantidad total de items en el pack
    creation_date: datetime,
    name: string,
    short_description: string,
    long_description: string,
    price: decimal,
    weight: decimal
  },
  products: [
    {
      product: {
        id_product: int,
        name: string,
        price: decimal,
        type: 'book' | 'pack' | 'object',
        cover_url: string
      },
      quantity: int              // cantidad de unidades de ese producto en el pack
    }
  ]
}
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/packs` | Crea un nuevo pack agrupando productos con cantidades específicas. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string,
  short_description: string,
  long_description: string,
  price: decimal,
  weight: decimal,
  items: [
    {
      id_product: int,
      quantity: int
    }
  ]
}
```

**Respuesta** `201 Created`:
```ts
{
  id_product: int,               // id del producto-pack creado
  product_count: int,
  creation_date: datetime,
  items: [
    { id_product: int, quantity: int }
  ]
}
```

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `items[].id_product` no existe o es de tipo `pack` (no se permite anidar packs).
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/packs/:id` | Actualiza los datos generales de un pack existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body — todos los campos opcionales
{
  name?: string,
  short_description?: string,
  long_description?: string,
  price?: decimal,
  weight?: decimal
}
```

**Respuesta** `200 OK`: pack actualizado.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/packs/:id/products` | Agrega un producto al pack con una cantidad determinada. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  id_product: int,
  quantity: int
}
```

**Respuesta** `200 OK`: pack actualizado con la nueva composición.

**Errores posibles:**
- `422 Unprocessable Entity`: el producto no existe o es de tipo `pack`.
- `409 Conflict`: el producto ya forma parte del pack (usar PUT para actualizar cantidad).
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/packs/:id/products/:id_product` | Modifica la cantidad de un producto dentro de un pack. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int,
  id_product: int
}

// Body
{
  quantity: int
}
```

**Respuesta** `200 OK`: pack actualizado.

**Errores posibles:**
- `404 Not Found`: el pack o el producto no existe, o el producto no forma parte del pack.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/packs/:id/products/:id_product` | Remueve un producto de la composición del pack. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int,
  id_product: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `404 Not Found`: el pack o el producto no existe, o el producto no forma parte del pack.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 8. Autores

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/authors` | Lista todos los autores con búsqueda y paginación. | No | No |

**Parámetros** (query):
```ts
{
  search?: string,    // busca en name
  page?: int,         // default: 1
  limit?: int         // default: 20
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_author: int,
      name: string
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `400 Bad Request`: `page` o `limit` con formato inválido.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/authors/:id` | Obtiene el detalle de un autor. | No | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id_author: int,
  name: string
}
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/authors` | Registra un nuevo autor en el sistema. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string      // máx 100 chars
}
```

**Respuesta** `201 Created`:
```ts
{
  id_author: int,
  name: string
}
```

**Errores posibles:**
- `409 Conflict`: ya existe un autor con ese `name`.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/authors/:id` | Actualiza el nombre de un autor existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  name: string
}
```

**Respuesta** `200 OK`:
```ts
{
  id_author: int,
  name: string
}
```

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/authors/:id` | Elimina un autor del sistema (si no tiene libros asociados). | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: el autor tiene libros asociados.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 9. Géneros

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/genres` | Lista todos los géneros literarios disponibles. | No | No |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
[
  {
    id_genre: int,
    name: string
  }
]
```

**Errores posibles:**
- `500 Internal Server Error`: error inesperado en el servidor.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/genres/:id` | Obtiene el detalle de un género específico. | No | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id_genre: int,
  name: string
}
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/genres` | Crea un nuevo género literario. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string      // máx 20 chars
}
```

**Respuesta** `201 Created`:
```ts
{
  id_genre: int,
  name: string
}
```

**Errores posibles:**
- `409 Conflict`: ya existe un género con ese `name`.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/genres/:id` | Actualiza el nombre de un género. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  name: string
}
```

**Respuesta** `200 OK`:
```ts
{
  id_genre: int,
  name: string
}
```

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/genres/:id` | Elimina un género del sistema. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: el género tiene libros asociados.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 10. Tags / Franquicias

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/tags` | Lista todas las etiquetas, pudiendo filtrar solo franquicias. | No | No |

**Parámetros** (query):
```ts
{
  is_franchise?: boolean,    // true = solo franquicias, false = solo tags no-franquicia
  search?: string            // busca en name
}
```

**Respuesta** `200 OK`:
```ts
[
  {
    id_tag: int,
    name: string,
    is_franchise: boolean
  }
]
```

**Errores posibles:**
- `400 Bad Request`: `is_franchise` con formato inválido (debe ser `true` o `false`).

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/tags/:id` | Obtiene el detalle de un tag o franquicia. | No | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id_tag: int,
  name: string,
  is_franchise: boolean
}
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/tags` | Crea un nuevo tag o franquicia. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string,              // máx 20 chars
  is_franchise?: boolean     // default: false
}
```

**Respuesta** `201 Created`:
```ts
{
  id_tag: int,
  name: string,
  is_franchise: boolean
}
```

**Errores posibles:**
- `409 Conflict`: ya existe un tag con ese `name`.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/tags/:id` | Actualiza los datos de un tag existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  name?: string,
  is_franchise?: boolean
}
```

**Respuesta** `200 OK`: tag actualizado.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/tags/:id` | Elimina un tag del sistema. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: el tag está asociado a productos o es usado por ítems del menú.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/products/:id/tags` | Asigna uno o varios tags a un producto. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int       // id_product
}

// Body
{
  tag_ids: int[]
}
```

**Respuesta** `200 OK`:
```ts
{
  id_product: int,
  tags: [
    { id_tag: int, name: string, is_franchise: boolean }
  ]
}
```

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `tag_ids` no existe.
- `409 Conflict`: uno o más tags ya estaban asignados al producto.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/products/:id/tags/:id_tag` | Desvincula un tag específico de un producto. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int,         // id_product
  id_tag: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `404 Not Found`: el producto o el tag no existe, o no están vinculados entre sí.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 11. Galería de productos

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/products/:id/gallery` | Lista todas las imágenes de la galería de un producto, ordenadas por `order` ascendente. | No | No |

**Parámetros** (path):
```ts
{
  id: int      // id_product
}
```

**Respuesta** `200 OK`:
```ts
[
  {
    id_gallery: int,
    id_product: int,
    order: int,
    url: string
  }
]
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/products/:id/gallery` | Agrega una nueva imagen a la galería de un producto. Acepta archivo binario o URL ya existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int      // id_product
}

// Body — uno de los dos modos:
// Modo A: archivo binario (multipart/form-data)
{
  file: file,           // binario de imagen
  order?: int           // si se omite, se asigna al final
}

// Modo B: URL ya alojada (application/json)
{
  url: string,
  order?: int
}
```

**Respuesta** `201 Created`:
```ts
{
  id_gallery: int,
  id_product: int,
  order: int,
  url: string
}
```

**Errores posibles:**
- `413 Payload Too Large`: el archivo supera el tamaño máximo permitido.
- `415 Unsupported Media Type`: el tipo de archivo no es una imagen soportada.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/products/:id/gallery/reorder` | Reordena masivamente las imágenes de la galería de un producto. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int      // id_product
}

// Body
{
  items: [
    {
      id_gallery: int,
      order: int
    }
  ]
}
```

**Respuesta** `200 OK`: galería completa con el nuevo orden.
```ts
[
  {
    id_gallery: int,
    id_product: int,
    order: int,
    url: string
  }
]
```

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `items[].id_gallery` no pertenece a la galería del producto.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/gallery/:id` | Elimina una imagen de la galería. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int      // id_gallery
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 12. Menú de navegación

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/menu` | Devuelve el árbol completo de navegación del sitio con sus sub-items. | No | No |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
[
  {
    id_menu: int,
    name: string,
    id_tag: int,                       // tag asociado al ítem
    order: int,
    id_menu_referenced: int | null,    // null si es item raíz
    is_category_filter: boolean,
    children: [
      // misma estructura recursiva
    ]
  }
]
```

**Errores posibles:**
- `500 Internal Server Error`: error inesperado en el servidor.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/menu/:id` | Obtiene el detalle de un ítem de menú. | No | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id_menu: int,
  name: string,
  id_tag: int,
  order: int,
  id_menu_referenced: int | null,
  is_category_filter: boolean
}
```

**Errores posibles:**
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/menu` | Crea un nuevo ítem de menú de navegación. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string,                        // máx 20 chars
  id_tag: int,                         // tag al que apunta el ítem
  order: int,
  id_menu_referenced?: int,            // padre en el árbol; null/omitido = ítem raíz
  is_category_filter?: boolean         // default: false
}
```

**Respuesta** `201 Created`:
```ts
{
  id_menu: int,
  name: string,
  id_tag: int,
  order: int,
  id_menu_referenced: int | null,
  is_category_filter: boolean
}
```

**Errores posibles:**
- `422 Unprocessable Entity`: el `id_tag` no existe o el `id_menu_referenced` no es válido.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/menu/:id` | Actualiza un ítem de menú existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body — todos los campos opcionales
{
  name?: string,
  id_tag?: int,
  order?: int,
  id_menu_referenced?: int,
  is_category_filter?: boolean
}
```

**Respuesta** `200 OK`: ítem de menú actualizado.

**Errores posibles:**
- `422 Unprocessable Entity`: `id_menu_referenced` apunta a un ítem inexistente o generaría una referencia circular.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/menu/reorder` | Reordena masivamente los ítems del menú de navegación. | Sí | Sí |

**Parámetros** (body):
```ts
{
  items: [
    {
      id_menu: int,
      order: int
    }
  ]
}
```

**Respuesta** `200 OK`: árbol completo del menú con el nuevo orden (misma estructura que `GET /api/menu`).

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `items[].id_menu` no existe.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/menu/:id` | Elimina un ítem de menú de navegación. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `409 Conflict`: el ítem tiene sub-ítems que lo referencian como `id_menu_referenced`.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 13. Carrito

Todos los endpoints requieren autenticación. El carrito es persistente por cliente (un solo carrito activo).

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/cart` | Obtiene el contenido actual del carrito con subtotales y totales calculados. |

**Parámetros:** ninguno (se usa el token del header).

**Respuesta** `200 OK`:
```ts
{
  id_cart: int,
  id_client: int,
  id_coupon: int | null,
  creation_date: datetime,
  updated_date: datetime,
  items: [
    {
      id_product: int,
      name: string,
      cover_url: string,
      price: decimal,                  // precio unitario actual
      discounted_price: decimal,       // precio unitario con descuento aplicado
      quantity: int,
      subtotal: decimal,               // discounted_price * quantity
      added_date: datetime,
      current_stock: int               // disponible (para validación)
    }
  ],
  subtotal: decimal,                   // suma de subtotales de todos los items
  discounts: decimal,                  // descuentos por producto + cupón
  taxes: decimal,                      // IVA / impuestos calculados
  total: decimal,                      // subtotal - discounts + taxes
  coupon: {                            // null si no hay cupón aplicado
    id_coupon: int,
    name: string,
    token: string,
    porcentage: decimal
  } | null
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/cart/items` | Agrega un producto al carrito con su cantidad. Si el producto ya está, suma la cantidad. |

**Parámetros** (body):
```ts
{
  id_product: int,
  quantity: int       // > 0
}
```

**Respuesta** `200 OK`: carrito actualizado (misma estructura que `GET /api/cart`).

**Errores posibles:**
- `404 Not Found`: el `id_product` no existe o está eliminado.
- `409 Conflict`: stock insuficiente para la cantidad solicitada.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción |
|---|---|---|
| PUT | `/api/cart/items/:id_product` | Modifica la cantidad de un producto ya agregado al carrito. |

**Parámetros** (path + body):
```ts
// Path
{
  id_product: int
}

// Body
{
  quantity: int       // > 0; usar DELETE para quitar
}
```

**Respuesta** `200 OK`: carrito actualizado.

**Errores posibles:**
- `404 Not Found`: el producto no está en el carrito.
- `409 Conflict`: stock insuficiente para la cantidad solicitada.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| DELETE | `/api/cart/items/:id_product` | Remueve un producto específico del carrito. |

**Parámetros** (path):
```ts
{
  id_product: int
}
```

**Respuesta** `200 OK`: carrito actualizado (sin el item eliminado).

**Errores posibles:**
- `404 Not Found`: el producto no está en el carrito.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| DELETE | `/api/cart` | Vacía completamente el carrito del usuario. |

**Parámetros:** ninguno.

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/cart/validate` | Valida el carrito verificando stock disponible y precios actualizados antes del checkout. |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
{
  valid: boolean,
  issues: [
    {
      id_product: int,
      name: string,
      issue_type: 'out_of_stock' | 'insufficient_stock' | 'price_changed' | 'product_unavailable',
      message: string,
      requested_quantity: int,         // cantidad pedida en carrito
      available_stock: int | null,     // stock disponible (null si no aplica)
      old_price: decimal | null,
      new_price: decimal | null
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/cart/apply-coupon` | Aplica un cupón de descuento al carrito actual. |

**Parámetros** (body):
```ts
{
  coupon_token: string      // token del cupón (ver tabla coupon.token)
}
```

**Respuesta** `200 OK`:
```ts
{
  cart: { /* misma estructura que GET /api/cart */ },
  discount_applied: decimal       // monto descontado por el cupón
}
```

**Errores posibles:**
- `404 Not Found`: el `coupon_token` no existe.
- `409 Conflict`: el cupón está inactivo, expirado o no asignado al cliente.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción |
|---|---|---|
| DELETE | `/api/cart/coupon` | Remueve el cupón aplicado al carrito. |

**Parámetros:** ninguno.

**Respuesta** `200 OK`: carrito actualizado sin cupón.

**Errores posibles:**
- `404 Not Found`: el carrito no tiene ningún cupón aplicado.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

## 14. Órdenes

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/orders` | Lista todas las órdenes del sistema con filtros (vista administrativa). | Sí | Sí |

**Parámetros** (query):
```ts
{
  page?: int,                                                                                                   // default: 1
  limit?: int,                                                                                                  // default: 20
  status?: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned',
  date_from?: date,
  date_to?: date,
  id_client?: int
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_order: int,
      creation_date: date,
      id_client: int,
      client_name: string,                           // join con client
      client_email: string,
      id_coupon: int | null,
      status: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned',
      tracking: string,
      subtotal: decimal,
      taxes: decimal,
      total: decimal,
      items_count: int                               // suma de order_line.quantity
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/orders/me` | Lista las órdenes del cliente autenticado. | Sí | No |

**Parámetros** (query):
```ts
{
  page?: int,    // default: 1
  limit?: int,   // default: 20
  status?: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned'
}
```

**Respuesta** `200 OK`: misma estructura que `GET /api/orders` (filtrada por cliente del token).

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/orders/:id` | Obtiene el detalle completo de una orden con líneas, cliente, direcciones snapshot y cupón. | Sí | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  order: {
    id_order: int,
    creation_date: date,
    id_client: int,
    id_coupon: int | null,
    taxes: decimal,
    status: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned',
    tracking: string,
    observations: string,
    cancel_reason: string | null,
    return_reason: string | null,
    subtotal: decimal,
    total: decimal
  },
  lines: [
    {
      id_product: int,
      product_name: string,
      cover_url: string,
      quantity: int,
      price: decimal,                  // precio unitario al momento de la orden
      line_total: decimal              // price * quantity
    }
  ],
  client: {
    id_client: int,
    name: string,
    lastname: string,
    email: string,
    phone: string
  },
  addresses: {
    delivery: {                        // snapshot inmutable de la dirección al momento de la orden
      id_snapshot: int,
      state: string,
      type: 'house' | 'apartment' | 'work',
      observations: string,
      apartment: string,
      corner: string,
      door_number: string,
      floor: string,
      name: string,
      neighborhood: string,
      street: string
    },
    billing: { /* misma estructura que delivery */ } | null
  },
  coupon: {                            // null si la orden no usó cupón
    id_coupon: int,
    name: string,
    token: string,
    porcentage: decimal
  } | null
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/orders/:id/tracking` | Consulta el estado de envío y el historial de cambios de estado de una orden. | Sí | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  tracking: string,                    // número de seguimiento
  status: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned',
  history: [
    {
      id: int,
      status: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned',
      reason: string | null,
      changed_by: int | null,          // id_client del usuario que hizo el cambio (null si es sistema)
      date: datetime
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/orders` | Crea una nueva orden a partir del carrito del cliente autenticado (proceso de checkout). Los items se toman del carrito persistente, no del body. Al confirmar la orden, el carrito se vacía. | Sí | No |

**Parámetros** (body):
```ts
{
  id_delivery_address: int,            // id_address del cliente para entrega
  id_order_address: int,               // id_address del cliente para facturación
  observations?: string                // máx 200 chars
}
```

**Respuesta** `201 Created`: orden completa (misma estructura que `GET /api/orders/:id`).

**Errores posibles:**
- `409 Conflict`: el carrito está vacío, hay productos sin stock, o precios desactualizados (invocar `/api/cart/validate` primero).
- `422 Unprocessable Entity`: `id_delivery_address` o `id_order_address` no pertenecen al cliente.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PATCH | `/api/orders/:id/status` | Cambia el estado de una orden (confirmed, processing, shipped, delivered, etc.). | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  status: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned',
  reason?: string                      // se guarda en order_status_history.reason
}
```

**Respuesta** `200 OK`: orden actualizada (misma estructura que `GET /api/orders/:id`).

**Errores posibles:**
- `409 Conflict`: transición de estado no permitida (ej: de `delivered` a `pending`).
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PATCH | `/api/orders/:id/cancel` | Cancela una orden liberando el stock reservado. | Sí | No |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  reason: string                       // máx 500 chars; se guarda en order.cancel_reason
}
```

**Respuesta** `200 OK`: orden actualizada con `status='cancelled'`.

**Errores posibles:**
- `409 Conflict`: la orden ya fue cancelada, entregada o devuelta y no puede cancelarse.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PATCH | `/api/orders/:id/tracking` | Actualiza el número de seguimiento del envío. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  tracking: string                     // máx 20 chars
}
```

**Respuesta** `200 OK`: orden actualizada.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/orders/:id/return` | Registra la devolución de una orden entregada. | Sí | No |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  reason: string                       // máx 500 chars; se guarda en order.return_reason
}
```

**Respuesta** `200 OK`: orden actualizada con `status='returned'`.

**Errores posibles:**
- `409 Conflict`: la orden no está en estado `delivered` y no puede marcarse como devuelta.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/orders/:id/invoice` | Genera y descarga la factura de la orden en PDF. | Sí | No |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`: archivo PDF binario.
- `Content-Type: application/pdf`
- `Content-Disposition: attachment; filename="invoice-<id>.pdf"`

**Errores posibles:**
- `409 Conflict`: la orden está en estado `pending` o `cancelled` y no admite factura.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 15. Cupones

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/coupons` | Lista todos los cupones del sistema (vista administrativa). | Sí | Sí |

**Parámetros** (query):
```ts
{
  is_active?: boolean,
  search?: string,        // busca en name y token
  page?: int,             // default: 1
  limit?: int             // default: 20
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_coupon: int,
      name: string,
      token: string,
      description: string,
      creation_date: datetime,
      valid_from: datetime | null,
      valid_until: datetime | null,
      porcentage: decimal,
      is_active: boolean,
      assigned_clients_count: int    // cantidad de clientes con el cupón asignado
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/coupons/:id` | Obtiene el detalle de un cupón y los clientes asignados. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  coupon: {
    id_coupon: int,
    name: string,
    token: string,
    description: string,
    creation_date: datetime,
    valid_from: datetime | null,
    valid_until: datetime | null,
    porcentage: decimal,
    is_active: boolean
  },
  clients: [
    {
      id_client: int,
      name: string,
      lastname: string,
      email: string
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/coupons/me` | Lista los cupones disponibles para el cliente autenticado. | Sí | No |

**Parámetros:** ninguno (se usa el token del header).

**Respuesta** `200 OK`:
```ts
[
  {
    id_coupon: int,
    name: string,
    token: string,
    description: string,
    valid_from: datetime | null,
    valid_until: datetime | null,
    porcentage: decimal
  }
]
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/coupons/validate` | Valida si un código de cupón es aplicable al cliente actual. | Sí | No |

**Parámetros** (body):
```ts
{
  token: string
}
```

**Respuesta** `200 OK`:
```ts
{
  valid: boolean,
  reason: string | null,               // ej: "expired", "not_assigned", "inactive" — null si valid=true
  coupon: {                            // null si no es válido
    id_coupon: int,
    name: string,
    token: string,
    porcentage: decimal,
    valid_from: datetime | null,
    valid_until: datetime | null
  } | null,
  discount: decimal                    // 0 si no es válido
}
```

**Errores posibles:**
- `404 Not Found`: el `token` no corresponde a ningún cupón.
- `409 Conflict`: el cupón está inactivo, expirado o no está asignado al cliente.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/coupons` | Crea un nuevo cupón de descuento. | Sí | Sí |

**Parámetros** (body):
```ts
{
  name: string,                        // máx 20 chars
  token: string,                       // máx 10 chars; debe ser único
  description: string,
  valid_from?: datetime,               // null = válido desde la creación
  valid_until?: datetime,              // null = sin fecha de expiración
  porcentage: decimal                  // 0-100
}
```

**Respuesta** `201 Created`:
```ts
{
  id_coupon: int,
  name: string,
  token: string,
  description: string,
  creation_date: datetime,
  valid_from: datetime | null,
  valid_until: datetime | null,
  porcentage: decimal,
  is_active: boolean                   // false por default; activar con PATCH activate
}
```

**Errores posibles:**
- `409 Conflict`: ya existe un cupón con ese `token`.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PUT | `/api/coupons/:id` | Actualiza los datos de un cupón existente. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body — todos los campos opcionales
{
  name?: string,
  description?: string,
  valid_from?: datetime,
  valid_until?: datetime,
  porcentage?: decimal
}
```

**Respuesta** `200 OK`: cupón actualizado.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PATCH | `/api/coupons/:id/activate` | Activa un cupón para que pueda ser utilizado. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`: cupón actualizado con `is_active: true`.

**Errores posibles:**
- `409 Conflict`: el cupón ya está activo o ha expirado.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| PATCH | `/api/coupons/:id/deactivate` | Desactiva un cupón impidiendo su uso. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`: cupón actualizado con `is_active: false`.

**Errores posibles:**
- `409 Conflict`: el cupón ya está inactivo.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/coupons/:id/clients` | Asigna un cupón a uno o varios clientes específicos. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  client_ids: int[]
}
```

**Respuesta** `200 OK`:
```ts
{
  id_coupon: int,
  clients: [
    { id_client: int, name: string, lastname: string, email: string }
  ]
}
```

**Errores posibles:**
- `422 Unprocessable Entity`: alguno de los `client_ids` no existe.
- `409 Conflict`: uno o más clientes ya tenían el cupón asignado.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/coupons/:id/clients/:id_client` | Desvincula un cupón de un cliente. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int,
  id_client: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `404 Not Found`: el cupón o el cliente no existe, o el cupón no estaba asignado al cliente.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| DELETE | `/api/coupons/:id` | Elimina lógicamente un cupón del sistema. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 16. Descuentos por producto

Todos los endpoints requieren autenticación y rol Admin.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/discounts` | Lista todos los descuentos del sistema con filtros. |

**Parámetros** (query):
```ts
{
  is_active?: boolean,
  id_product?: int,       // filtra por producto
  page?: int,             // default: 1
  limit?: int             // default: 20
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_discount: int,
      id_product: int,
      product_name: string,
      porcentage: decimal,
      creation_date: datetime,
      valid_from: datetime | null,
      valid_until: datetime | null,
      is_active: boolean
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/discounts/:id` | Obtiene el detalle de un descuento específico. |

**Parámetros** (path):
```ts
{
  id: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id_discount: int,
  id_product: int,
  product_name: string,
  porcentage: decimal,
  creation_date: datetime,
  valid_from: datetime | null,
  valid_until: datetime | null,
  is_active: boolean
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/discounts` | Crea un nuevo descuento aplicable a un producto. |

**Parámetros** (body):
```ts
{
  id_product: int,
  porcentage: decimal,                 // 0-100
  valid_from?: datetime,               // null = válido desde la creación
  valid_until?: datetime               // null = sin fecha de expiración
}
```

**Respuesta** `201 Created`:
```ts
{
  id_discount: int,
  id_product: int,
  porcentage: decimal,
  creation_date: datetime,
  valid_from: datetime | null,
  valid_until: datetime | null,
  is_active: boolean                   // false por default
}
```

**Errores posibles:**
- `404 Not Found`: el `id_product` no existe.
- `409 Conflict`: el producto ya tiene un descuento activo que se solapa con el rango de fechas.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| PUT | `/api/discounts/:id` | Actualiza los datos de un descuento existente. |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body — todos los campos opcionales
{
  porcentage?: decimal,
  valid_from?: datetime,
  valid_until?: datetime
}
```

**Respuesta** `200 OK`: descuento actualizado.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| PATCH | `/api/discounts/:id/status` | Activa o desactiva un descuento según la acción indicada. |

**Parámetros** (path + body):
```ts
// Path
{
  id: int
}

// Body
{
  action: 'activate' | 'deactivate'
}
```

**Respuesta** `200 OK`: descuento actualizado con `is_active` correspondiente.

**Errores posibles:**
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 17. Stock

Todos los endpoints requieren autenticación y rol Admin.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/stock` | Lista los movimientos de stock con filtros por producto y fecha. |

**Parámetros** (query):
```ts
{
  id_product?: int,
  date_from?: date,
  date_to?: date,
  page?: int,             // default: 1
  limit?: int             // default: 20
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_stock: int,
      id_product: int,
      product_name: string,
      date: datetime,
      input: int,                                       // unidades ingresadas (>=0)
      output: int,                                      // unidades retiradas (>=0)
      id_order: int | null,                             // null si el movimiento no proviene de una orden
      type: 'entry' | 'exit' | 'adjustment',
      reason: string | null
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/stock/product/:id_product` | Obtiene el stock actual de un producto específico. |

**Parámetros** (path):
```ts
{
  id_product: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id_product: int,
  current_stock: int,                  // sum(input) - sum(output)
  total_input: int,                    // suma histórica de ingresos
  total_output: int,                   // suma histórica de egresos
  last_movement_date: datetime | null
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/stock/product/:id_product/movements` | Obtiene el historial de movimientos de stock de un producto. |

**Parámetros** (path + query):
```ts
// Path
{
  id_product: int
}

// Query
{
  date_from?: date,
  date_to?: date,
  page?: int,             // default: 1
  limit?: int             // default: 20
}
```

**Respuesta** `200 OK`:
```ts
{
  movements: [
    {
      id_stock: int,
      date: datetime,
      input: int,
      output: int,
      id_order: int | null,
      type: 'entry' | 'exit' | 'adjustment',
      reason: string | null,
      running_balance: int             // stock acumulado tras este movimiento
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/stock/movement` | Registra un movimiento de stock. El backend asigna `input` (cuando `type='entry'` o `type='adjustment'` con `quantity` positivo) u `output` (cuando `type='exit'` o `type='adjustment'` con `quantity` negativo) según el tipo. La validación queda centralizada en el backend. |

**Parámetros** (body):
```ts
{
  id_product: int,
  type: 'entry' | 'exit' | 'adjustment',
  quantity: int,                       // positivo para entradas; positivo o negativo para ajustes
  date: datetime,
  id_order?: int,                      // opcional: vincular el movimiento a una orden
  reason?: string                      // máx 500 chars
}
```

**Respuesta** `201 Created`:
```ts
{
  id_stock: int,
  id_product: int,
  date: datetime,
  input: int,
  output: int,
  id_order: int | null,
  type: 'entry' | 'exit' | 'adjustment',
  reason: string | null
}
```

**Errores posibles:**
- `404 Not Found`: el `id_product` o `id_order` no existe.
- `409 Conflict`: el movimiento resultaría en stock negativo (solo aplica a `exit` y `adjustment` negativos).
- `422 Unprocessable Entity`: `quantity` es cero, o el signo no coincide con el `type` (ej: `entry` con `quantity` negativo).
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

## 18. Wishlist

Todos los endpoints requieren autenticación. No requieren rol Admin (cada cliente gestiona su propia wishlist).

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/wishlist` | Lista los productos en la lista de deseos del cliente autenticado. |

**Parámetros:** ninguno (se usa el token del header).

**Respuesta** `200 OK`:
```ts
[
  {
    id_product: int,
    name: string,
    short_description: string,
    price: decimal,
    type: 'book' | 'pack' | 'object',
    cover_url: string,
    current_stock: int,
    discount: {                        // null si no hay descuento activo
      id_discount: int,
      porcentage: decimal,
      final_price: decimal
    } | null
  }
]
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/wishlist/:id_product` | Agrega un producto a la lista de deseos. |

**Parámetros** (path):
```ts
{
  id_product: int
}
```

**Respuesta** `201 Created`:
```ts
{
  id_client: int,
  id_product: int
}
```

**Errores posibles:**
- `404 Not Found`: el `id_product` no existe.
- `409 Conflict`: el producto ya está en la wishlist del cliente.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| DELETE | `/api/wishlist/:id_product` | Remueve un producto de la lista de deseos. |

**Parámetros** (path):
```ts
{
  id_product: int
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `404 Not Found`: el producto no está en la wishlist del cliente.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/wishlist/check/:id_product` | Verifica si un producto está en la wishlist del cliente (útil para UI). |

**Parámetros** (path):
```ts
{
  id_product: int
}
```

**Respuesta** `200 OK`:
```ts
{
  in_wishlist: boolean
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 19. Dashboard / Reportes

Todos los endpoints de esta sección requieren autenticación (`Auth = Sí`) y rol Admin (`Admin = Sí`). Para mantener la tabla legible, se omiten esas dos columnas y la respuesta se documenta en detalle abajo de cada endpoint.

> **Convención de fechas:** todos los rangos de fecha (`date_from`, `date_to`) son **inclusivos**. Si no se especifican, el endpoint devuelve datos de los **últimos 30 días** por defecto. El parámetro `compare_with_previous` (boolean, opcional, default `false`) habilita la comparación contra el período anterior de igual duración.

> **Convención de moneda:** todos los importes se devuelven en `UYU` (moneda base del sistema), como `decimal` con 2 posiciones.

---

### 18.1 KPIs generales

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/summary` | Métricas resumidas del negocio en un rango de fechas, con comparativa opcional contra el período anterior. |

**Parámetros** (query):
```ts
{
  date_from?: date,                    // default: hace 30 días
  date_to?: date,                      // default: hoy
  compare_with_previous?: boolean      // default: false
}
```

**Respuesta** `200 OK`:
```ts
{
  total_sales: decimal,           // suma de order.total de órdenes en estados confirmed|processing|shipped|delivered
  total_sales_previous: decimal,  // mismo cálculo en el período inmediatamente anterior (solo si compare_with_previous=true)
  total_sales_variation: decimal, // variación porcentual respecto al período anterior (ej: 12.5 = +12.5%)
  orders_count: int,              // cantidad de órdenes válidas (no canceladas) en el período
  orders_count_previous: int,
  orders_count_variation: decimal,
  new_clients: int,               // clientes con creation_date dentro del período
  new_clients_previous: int,
  new_clients_variation: decimal,
  avg_ticket: decimal,            // ticket promedio = total_sales / orders_count
  avg_ticket_previous: decimal,
  avg_ticket_variation: decimal,
  total_units_sold: int,          // suma de order_line.quantity en órdenes válidas del período
  active_clients: int,            // clientes que realizaron al menos 1 orden válida en el período
  conversion_rate: decimal,       // % de carritos que terminaron en orden confirmada
  date_from: date,                // fecha de inicio efectiva del período consultado
  date_to: date                   // fecha de fin efectiva del período consultado
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.2 Ventas e ingresos

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/sales` | Evolución temporal de ventas agrupadas por día, semana o mes. Devuelve un punto por cada unidad de tiempo en el rango. |

**Parámetros** (query):
```ts
{
  period?: 'day' | 'week' | 'month',           // default: 'day'
  date_from?: date,                             // default: hace 30 días
  date_to?: date,                               // default: hoy
  status?: 'all' | 'valid' | 'delivered'        // default: 'valid'
                                                // 'all' = todas | 'valid' = no canceladas | 'delivered' = solo entregadas
}
```

**Respuesta** `200 OK`:
```ts
{
  period: 'day' | 'week' | 'month',
  series: [
    {
      date: date,             // fecha del bucket (inicio del día, semana o mes según period)
      label: string,          // etiqueta legible: "2026-04-18", "Sem 16 2026", "Abr 2026"
      total: decimal,         // suma de order.total en ese bucket
      subtotal: decimal,      // suma de order.subtotal en ese bucket
      taxes: decimal,         // suma de order.taxes en ese bucket
      orders: int,            // cantidad de órdenes en ese bucket
      units: int              // suma de unidades vendidas (order_line.quantity)
    }
  ],
  totals: {
    total: decimal,           // suma de toda la serie
    orders: int,
    units: int,
    avg_per_bucket: decimal   // promedio de ventas por bucket no vacío
  }
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/sales-by-weekday` | Distribución de ventas por día de la semana (lunes a domingo). |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      weekday: int,           // 1=Lunes, 7=Domingo (ISO)
      weekday_name: string,   // "Lunes", "Martes", ...
      orders: int,
      total: decimal,
      avg_order_value: decimal,
      units: int
    }
  ],
  best_day: string,           // nombre del día con más ingresos
  worst_day: string
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/sales-by-category` | Ventas agrupadas por tipo de producto (`book`, `pack`, `object`) y subtipo de libro. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  by_type: [
    {
      type: 'book' | 'pack' | 'object',
      orders_count: int,      // órdenes que contienen al menos 1 producto de este tipo
      units_sold: int,        // total de unidades vendidas de este tipo
      revenue: decimal,       // ingresos generados (sum de order_line.price * quantity)
      revenue_share: decimal  // % del total de ingresos del período
    }
  ],
  by_book_type: [
    {
      type_book: 'comic' | 'manga',
      units_sold: int,
      revenue: decimal,
      revenue_share: decimal  // % sobre el total de ingresos de libros
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/sales-by-genre` | Ventas agrupadas por género literario (solo libros). |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date,      // default: hoy
  limit?: int          // default: 10
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_genre: int,
      genre_name: string,
      units_sold: int,
      revenue: decimal,
      revenue_share: decimal,
      books_count: int        // cantidad de libros distintos del género vendidos
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/sales-by-franchise` | Ventas agrupadas por franquicia (tags con `is_franchise=true`). |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date,      // default: hoy
  limit?: int          // default: 10
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_tag: int,
      franchise_name: string,
      units_sold: int,
      revenue: decimal,
      revenue_share: decimal,
      products_count: int     // productos distintos de la franquicia vendidos
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/sales-by-author` | Ventas agrupadas por autor (solo libros). |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date,      // default: hoy
  limit?: int          // default: 10
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_author: int,
      author_name: string,
      units_sold: int,
      revenue: decimal,
      books_count: int
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.3 Productos

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/top-products` | Productos más vendidos en un período. |

**Parámetros** (query):
```ts
{
  limit?: int,                              // default: 10
  date_from?: date,                         // default: hace 30 días
  date_to?: date,                           // default: hoy
  type?: 'book' | 'pack' | 'object'         // opcional: filtra por tipo de producto
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_product: int,
      name: string,
      type: 'book' | 'pack' | 'object',
      cover_url: string,           // imagen de product_gallery con menor `order` o null si no tiene
      price: decimal,              // precio actual del producto
      quantity_sold: int,          // unidades vendidas en el período
      revenue: decimal,            // ingresos generados
      orders_count: int,           // órdenes distintas que lo incluyen
      current_stock: int           // stock disponible actualmente (input - output acumulado)
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/worst-products` | Productos con peor performance (menos vendidos pero con stock disponible). Útil para detectar inventario estancado. |

**Parámetros** (query):
```ts
{
  limit?: int,         // default: 10
  date_from?: date,    // default: hace 30 días
  date_to?: date,      // default: hoy
  min_stock?: int      // default: 1 — solo incluye productos con stock >= min_stock
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_product: int,
      name: string,
      type: 'book' | 'pack' | 'object',
      price: decimal,
      current_stock: int,
      quantity_sold: int,         // 0 o muy bajo en el período
      days_since_last_sale: int,  // días desde la última venta (null si nunca se vendió)
      stock_value: decimal        // current_stock * price (capital inmovilizado)
    }
  ],
  total_stuck_value: decimal      // suma de stock_value de todos los productos del listado
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/never-sold-products` | Productos del catálogo que nunca registraron una venta. |

**Parámetros** (query):
```ts
{
  limit?: int,    // default: 50
  page?: int      // default: 1
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_product: int,
      name: string,
      type: 'book' | 'pack' | 'object',
      price: decimal,
      creation_date: date,        // cuándo se cargó al catálogo
      days_in_catalog: int,       // días desde creation_date
      current_stock: int,
      stock_value: decimal,
      is_featured: boolean
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/products-trending` | Productos con mayor crecimiento de ventas comparando dos períodos consecutivos. |

**Parámetros** (query):
```ts
{
  period_days?: int,    // default: 7 — duración (en días) de cada uno de los dos períodos a comparar
  limit?: int           // default: 10
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_product: int,
      name: string,
      type: 'book' | 'pack' | 'object',
      cover_url: string,
      sales_current: int,         // unidades vendidas en el período actual
      sales_previous: int,        // unidades vendidas en el período anterior de igual duración
      growth_units: int,          // sales_current - sales_previous
      growth_percentage: decimal, // % de crecimiento
      revenue_current: decimal,
      revenue_previous: decimal
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/product-performance/:id_product` | Performance detallada de un producto específico (vista de drill-down). |

**Parámetros** (path + query):
```ts
// Path
{
  id_product: int       // requerido
}

// Query
{
  date_from?: date,     // default: hace 30 días
  date_to?: date        // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  product: {
    id_product: int,
    name: string,
    type: 'book' | 'pack' | 'object',
    price: decimal,
    cover_url: string
  },
  metrics: {
    units_sold: int,
    revenue: decimal,
    orders_count: int,
    avg_order_quantity: decimal,    // promedio de unidades por orden
    current_stock: int,
    stock_turnover_days: int,       // días estimados para vender el stock actual al ritmo del período
    times_in_wishlist: int          // cantidad de clientes que lo tienen en wishlist
  },
  sales_evolution: [
    {
      date: date,
      units: int,
      revenue: decimal
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

### 18.4 Clientes

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/top-clients` | Clientes con mayor gasto en un período. |

**Parámetros** (query):
```ts
{
  limit?: int,         // default: 10
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_client: int,
      name: string,
      lastname: string,
      email: string,
      total_spent: decimal,        // suma de order.total en órdenes válidas del período
      orders_count: int,           // cantidad de órdenes válidas en el período
      avg_ticket: decimal,         // total_spent / orders_count
      units_purchased: int,        // unidades totales compradas
      last_order_date: date,       // fecha de la última orden
      client_since: date           // creation_date del cliente
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/clients-summary` | Resumen agregado de la base de clientes. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  total_clients: int,             // clientes totales con role=User y active=true
  new_clients: int,               // clientes registrados en el período
  active_clients: int,            // clientes con al menos 1 orden válida en el período
  inactive_clients: int,          // total_clients - active_clients
  recurrent_clients: int,         // clientes con 2+ órdenes en el período
  one_time_buyers: int,           // clientes con exactamente 1 orden en el período
  recurrence_rate: decimal,       // recurrent_clients / active_clients
  avg_orders_per_client: decimal,
  avg_lifetime_value: decimal,    // promedio histórico de gasto por cliente activo
  churn_rate: decimal             // % de clientes activos en período anterior que no compraron en este
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/clients-acquisition` | Evolución temporal de adquisición de nuevos clientes. |

**Parámetros** (query):
```ts
{
  period?: 'day' | 'week' | 'month',    // default: 'day'
  date_from?: date,                      // default: hace 30 días
  date_to?: date                         // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  period: 'day' | 'week' | 'month',
  series: [
    {
      date: date,
      label: string,
      new_clients: int,
      converted_clients: int      // de los nuevos, cuántos hicieron al menos 1 orden
    }
  ],
  totals: {
    new_clients: int,
    converted_clients: int,
    conversion_rate: decimal     // % de nuevos clientes que convirtieron
  }
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/clients-segmentation` | Segmentación RFM (Recency, Frequency, Monetary) de los clientes. Útil para campañas de marketing. |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
{
  segments: [
    {
      segment: 'champions' | 'loyal' | 'potential_loyalists' | 'new_customers' | 'promising' | 'need_attention' | 'about_to_sleep' | 'at_risk' | 'cant_lose_them' | 'hibernating' | 'lost',
      label: string,              // descripción legible
      clients_count: int,
      total_revenue: decimal,
      avg_recency_days: decimal,  // promedio de días desde la última compra del segmento
      avg_frequency: decimal,     // promedio de órdenes por cliente del segmento
      avg_monetary: decimal       // gasto promedio por cliente del segmento
    }
  ],
  total_clients_segmented: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/clients-retention` | Análisis de cohortes de retención: % de clientes que vuelven a comprar mes a mes desde su primera compra. |

**Parámetros** (query):
```ts
{
  cohort_count?: int    // default: 6 — cantidad de cohortes mensuales hacia atrás a analizar
}
```

**Respuesta** `200 OK`:
```ts
{
  cohorts: [
    {
      cohort_month: string,       // ej: "2026-04"
      initial_clients: int,       // clientes con primera compra en ese mes
      retention: [
        {
          month_offset: int,      // 0=mes inicial, 1=mes siguiente, etc.
          clients_returning: int,
          retention_rate: decimal // % sobre initial_clients
        }
      ]
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.5 Órdenes

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/orders-by-status` | Distribución de órdenes por estado en un período. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      status: 'pending' | 'confirmed' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'returned',
      count: int,
      total_value: decimal,       // suma de order.total de órdenes en este estado
      percentage: decimal         // % sobre el total de órdenes del período
    }
  ],
  total_orders: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/orders-funnel` | Embudo de conversión desde carrito a entrega. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  steps: [
    {
      step: 'cart_created' | 'cart_with_items' | 'order_placed' | 'order_paid' | 'order_shipped' | 'order_delivered',
      label: string,
      count: int,
      drop_off_from_previous: decimal  // % de pérdida respecto al paso anterior
    }
  ],
  overall_conversion_rate: decimal     // delivered / cart_created
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/orders-fulfillment-time` | Tiempo promedio entre cambios de estado de las órdenes (eficiencia operativa). |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  avg_pending_to_confirmed_hours: decimal,
  avg_confirmed_to_processing_hours: decimal,
  avg_processing_to_shipped_hours: decimal,
  avg_shipped_to_delivered_hours: decimal,
  avg_total_fulfillment_hours: decimal,    // pending → delivered
  total_orders_analyzed: int,
  slowest_step: string,                    // paso con mayor tiempo promedio
  bottleneck_warning: boolean              // true si algún paso supera umbrales esperados
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/orders-cancellation` | Análisis de cancelaciones: cantidad, valor perdido y razones. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  cancelled_count: int,
  cancelled_value: decimal,            // valor total de las órdenes canceladas
  cancellation_rate: decimal,          // % sobre el total de órdenes
  by_reason: [
    {
      reason: string,                  // texto agrupado de cancel_reason
      count: int,
      total_value: decimal
    }
  ],
  by_status_at_cancellation: [
    {
      status: string,                  // estado en el que se canceló
      count: int
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/orders-returns` | Análisis de devoluciones: cantidad, valor y motivos principales. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  returned_count: int,
  returned_value: decimal,
  return_rate: decimal,                // % sobre órdenes entregadas
  top_returned_products: [
    {
      id_product: int,
      product_name: string,
      returns_count: int,
      return_rate: decimal             // % de las veces que se vendió y fue devuelto
    }
  ],
  by_reason: [
    {
      reason: string,
      count: int
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/orders-by-region` | Distribución geográfica de órdenes por barrio/departamento (usa `order_address_snapshot.neighborhood` y `state`). |

**Parámetros** (query):
```ts
{
  date_from?: date,                            // default: hace 30 días
  date_to?: date,                              // default: hoy
  group_by?: 'neighborhood' | 'state'          // default: 'neighborhood'
}
```

**Respuesta** `200 OK`:
```ts
{
  group_by: 'neighborhood' | 'state',
  data: [
    {
      region: string,                  // nombre del barrio o departamento
      orders_count: int,
      total_revenue: decimal,
      unique_clients: int,
      avg_ticket: decimal
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.6 Stock e inventario

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/stock-alerts` | Productos con stock crítico que requieren reposición. |

**Parámetros** (query):
```ts
{
  threshold?: int    // default: 5 — productos con stock <= threshold se consideran críticos
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_product: int,
      name: string,
      type: 'book' | 'pack' | 'object',
      cover_url: string,
      current_stock: int,              // input - output acumulado de la tabla stock
      threshold: int,                  // umbral configurado
      avg_daily_sales: decimal,        // promedio de ventas diarias en últimos 30 días
      days_until_stockout: int,        // current_stock / avg_daily_sales (null si sin ventas)
      severity: 'critical' | 'low' | 'warning'
    }
  ],
  total_alerts: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/stock-summary` | Resumen general del inventario. |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
{
  total_products: int,                 // productos activos en catálogo
  total_units_in_stock: int,           // suma de stock disponible de todos los productos
  total_inventory_value: decimal,      // suma de (current_stock * price)
  out_of_stock_products: int,          // productos con stock = 0
  low_stock_products: int,             // productos por debajo del threshold
  overstock_products: int,             // productos con stock > 100 (configurable)
  by_type: [
    {
      type: 'book' | 'pack' | 'object',
      products_count: int,
      units: int,
      value: decimal
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/stock-movements-summary` | Resumen de movimientos de stock en un período (entradas vs salidas). |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  total_inputs: int,                   // suma de stock.input en el período
  total_outputs: int,                  // suma de stock.output en el período
  net_change: int,                     // total_inputs - total_outputs
  by_type: [
    {
      type: 'entry' | 'exit' | 'adjustment',
      movements_count: int,
      units: int
    }
  ],
  by_day: [
    {
      date: date,
      inputs: int,
      outputs: int,
      net: int
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/stock-rotation` | Productos con mayor y menor rotación de inventario. |

**Parámetros** (query):
```ts
{
  limit?: int,         // default: 10
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  fastest_rotation: [
    {
      id_product: int,
      name: string,
      units_sold: int,
      avg_stock: decimal,              // promedio de stock disponible en el período
      rotation_rate: decimal           // units_sold / avg_stock
    }
  ],
  slowest_rotation: [
    {
      id_product: int,
      name: string,
      units_sold: int,
      avg_stock: decimal,
      rotation_rate: decimal,
      days_in_stock: int               // días promedio que una unidad permanece en stock
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.7 Carritos abandonados

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/abandoned-carts` | Carritos que no se convirtieron en órdenes (con items y antigüedad). |

**Parámetros** (query):
```ts
{
  min_hours?: int,     // default: 24 — horas mínimas de inactividad para considerar abandonado
  page?: int,          // default: 1
  limit?: int          // default: 20
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_cart: int,
      id_client: int,
      client_name: string,
      client_email: string,
      items_count: int,                // cantidad de productos distintos
      total_units: int,                // suma de cart_item.quantity
      estimated_value: decimal,        // suma de (price actual * quantity)
      hours_since_update: int,         // horas desde updated_date
      created_at: datetime,
      updated_at: datetime
    }
  ],
  total: int,
  total_value_lost: decimal            // suma de estimated_value de TODOS los carritos abandonados
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/cart-conversion` | Tasa de conversión carrito → orden, con desglose. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  carts_created: int,                  // carritos creados en el período
  carts_with_items: int,               // carritos con al menos 1 cart_item
  carts_converted: int,                // carritos que generaron una orden
  carts_abandoned: int,
  conversion_rate: decimal,            // carts_converted / carts_with_items
  abandonment_rate: decimal,
  avg_items_in_converted: decimal,     // promedio de items en carritos que convirtieron
  avg_items_in_abandoned: decimal      // promedio de items en carritos que no convirtieron
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.8 Pagos

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/payments-summary` | Resumen de pagos por estado y método. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  by_status: [
    {
      status: 'pending' | 'approved' | 'rejected' | 'refunded' | 'cancelled' | 'in_process',
      count: int,
      total_amount: decimal,
      percentage: decimal
    }
  ],
  by_provider: [
    {
      provider: string,                // ej: "mercadopago"
      count: int,
      total_amount: decimal,
      success_rate: decimal            // approved / (approved + rejected)
    }
  ],
  by_payment_method: [
    {
      payment_method: string,          // ej: "credit_card", "debit_card", "cash"
      count: int,
      total_amount: decimal
    }
  ],
  total_approved: decimal,
  total_refunded: decimal,
  net_revenue: decimal                 // total_approved - total_refunded
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/payments-failures` | Análisis de pagos rechazados o cancelados. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date,      // default: hoy
  limit?: int          // default: 20 — cantidad de fallos recientes a listar en `recent_failures`
}
```

**Respuesta** `200 OK`:
```ts
{
  total_failures: int,
  total_failed_value: decimal,
  failure_rate: decimal,               // fallidos / total intentos
  recent_failures: [
    {
      id_payment: int,
      id_order: int,
      client_email: string,
      amount: decimal,
      status: 'rejected' | 'cancelled',
      provider: string,
      payment_method: string,
      creation_date: datetime
    }
  ],
  recovered_count: int,                // pagos rechazados cuya orden luego fue pagada con éxito
  recovery_rate: decimal               // recovered / total_failures
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.9 Cupones y descuentos

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/coupons-usage` | Estadísticas de uso de cupones. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  total_coupons_active: int,
  total_uses: int,                     // órdenes con id_coupon != null en el período
  total_discount_given: decimal,       // suma de descuentos aplicados (subtotal * porcentage)
  avg_discount_per_order: decimal,
  by_coupon: [
    {
      id_coupon: int,
      name: string,
      token: string,
      porcentage: decimal,
      uses: int,
      total_discount: decimal,
      total_revenue_with_coupon: decimal,   // ingresos generados por órdenes con este cupón
      unique_clients: int
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/discounts-impact` | Impacto de descuentos por producto en ventas. |

**Parámetros** (query):
```ts
{
  date_from?: date,    // default: hace 30 días
  date_to?: date       // default: hoy
}
```

**Respuesta** `200 OK`:
```ts
{
  active_discounts: int,
  products_on_sale: int,               // productos con descuento activo
  total_revenue_discounted: decimal,   // ingresos por productos en oferta
  total_units_discounted: int,
  avg_lift: decimal,                   // % de aumento promedio en ventas vs período sin descuento
  by_discount: [
    {
      id_discount: int,
      id_product: int,
      product_name: string,
      porcentage: decimal,
      valid_from: datetime,
      valid_until: datetime,
      units_sold_with_discount: int,
      revenue_with_discount: decimal,
      lift_vs_baseline: decimal        // % de cambio respecto a ventas previas al descuento
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.10 Wishlist

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/wishlist-popular` | Productos más agregados a wishlists (interés sin compra). |

**Parámetros** (query):
```ts
{
  limit?: int    // default: 10
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_product: int,
      name: string,
      type: 'book' | 'pack' | 'object',
      cover_url: string,
      price: decimal,
      wishlist_count: int,             // clientes que lo tienen en wishlist
      units_sold: int,                 // ventas históricas
      wish_to_sale_ratio: decimal,     // wishlist_count / units_sold (alto = interés alto, ventas bajas)
      current_stock: int,
      has_active_discount: boolean
    }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.11 Búsquedas y catálogo

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/catalog-summary` | Resumen del estado del catálogo. |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
{
  total_products: int,                 // logical_delete = false
  total_books: int,
  total_packs: int,
  total_objects: int,
  total_comics: int,
  total_mangas: int,
  featured_products: int,              // is_featured = true
  products_with_discount: int,
  products_without_image: int,         // sin entradas en product_gallery
  total_authors: int,
  total_genres: int,
  total_franchises: int,               // tags con is_franchise=true
  total_tags: int,
  avg_price: decimal,
  avg_price_by_type: [
    { type: string, avg_price: decimal }
  ]
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

### 18.12 Operacional / tiempo real

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/realtime-summary` | Métricas en tiempo real (últimas 24h y "ahora mismo"). |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
{
  orders_last_24h: int,
  revenue_last_24h: decimal,
  new_clients_last_24h: int,
  active_carts_now: int,               // carritos con updated_date en última hora
  pending_orders: int,                 // órdenes en estado 'pending'
  processing_orders: int,
  shipped_orders: int,                 // pendientes de entrega
  pending_payments: int,
  stock_alerts: int,                   // productos por debajo del threshold
  last_order: {
    id_order: int,
    client_name: string,
    total: decimal,
    creation_date: date
  } | null
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| GET | `/api/dashboard/pending-actions` | Lista accionable de tareas pendientes para el admin. |

**Parámetros:** ninguno.

**Respuesta** `200 OK`:
```ts
{
  orders_to_confirm: int,              // status = 'pending'
  orders_to_process: int,              // status = 'confirmed'
  orders_to_ship: int,                 // status = 'processing'
  orders_to_track: int,                // status = 'shipped' con tracking vacío o sin actualizar
  payments_to_sync: int,               // pagos in_process > 1 hora
  stock_critical: int,
  total_actions: int                   // suma de todo lo anterior
}
```


**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

## 20. Upload de archivos

Todos los endpoints requieren autenticación y rol Admin.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/upload/image` | Sube una imagen al almacenamiento y devuelve su URL pública. |

**Parámetros** (body, `multipart/form-data`):
```ts
{
  file: file       // imagen (jpg, png, webp, etc.)
}
```

**Respuesta** `201 Created`:
```ts
{
  url: string,           // URL pública de la imagen
  filename: string,      // nombre del archivo en el storage
  size: int              // tamaño en bytes
}
```

**Errores posibles:**
- `413 Payload Too Large`: el archivo supera el tamaño máximo permitido.
- `415 Unsupported Media Type`: el tipo de archivo no es una imagen soportada.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| POST | `/api/upload/images` | Sube múltiples imágenes en una sola petición. |

**Parámetros** (body, `multipart/form-data`):
```ts
{
  files: file[]    // múltiples imágenes
}
```

**Respuesta** `201 Created`:
```ts
[
  {
    url: string,
    filename: string,
    size: int
  }
]
```

**Errores posibles:**
- `413 Payload Too Large`: uno o más archivos superan el tamaño máximo permitido.
- `415 Unsupported Media Type`: algún archivo no es una imagen soportada.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción |
|---|---|---|
| DELETE | `/api/upload/:filename` | Elimina un archivo del almacenamiento. |

**Parámetros** (path):
```ts
{
  filename: string
}
```

**Respuesta** `204 No Content` (sin body).

**Errores posibles:**
- `404 Not Found`: el archivo no existe en el almacenamiento.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## 21. Pagos (integración con pasarela)

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/payments` | Lista todos los pagos del sistema con filtros (vista administrativa). Útil para conciliación, reportes y auditoría. | Sí | Sí |

**Parámetros** (query):
```ts
{
  page?: int,                                                                                          // default: 1
  limit?: int,                                                                                         // default: 20
  status?: 'pending' | 'approved' | 'rejected' | 'refunded' | 'cancelled' | 'in_process',
  id_order?: int,
  id_client?: int,
  provider?: string,                                                                                   // ej: "mercadopago"
  date_from?: date,
  date_to?: date
}
```

**Respuesta** `200 OK`:
```ts
{
  data: [
    {
      id_payment: int,
      id_order: int,
      external_id: string | null,                                                                       // id en la pasarela
      preference_id: string | null,
      status: 'pending' | 'approved' | 'rejected' | 'refunded' | 'cancelled' | 'in_process',
      amount: decimal,
      currency: string,                                                                                  // "UYU" por default
      payment_method: string | null,                                                                     // ej: "credit_card"
      provider: string | null,                                                                            // ej: "mercadopago"
      creation_date: datetime,
      updated_date: datetime,
      client_email: string                                                                                // join con order → client
    }
  ],
  total: int,
  page: int,
  limit: int
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/payments/:id_payment` | Obtiene el detalle completo de un pago específico, incluyendo el `raw_response` de la pasarela. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id_payment: int
}
```

**Respuesta** `200 OK`:
```ts
{
  payment: {
    id_payment: int,
    id_order: int,
    external_id: string | null,
    preference_id: string | null,
    status: 'pending' | 'approved' | 'rejected' | 'refunded' | 'cancelled' | 'in_process',
    amount: decimal,
    currency: string,
    payment_method: string | null,
    provider: string | null,
    creation_date: datetime,
    updated_date: datetime,
    raw_response: string | null         // JSON crudo de la pasarela como texto
  },
  order: {
    id_order: int,
    creation_date: date,
    status: string,
    total: decimal,
    id_client: int,
    client_name: string,
    client_email: string
  }
}
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/orders/:id_order/payments` | Lista todos los intentos de pago asociados a una orden (incluye reintentos y pagos rechazados previos). | Sí | No |

**Parámetros** (path):
```ts
{
  id_order: int
}
```

**Respuesta** `200 OK`:
```ts
[
  {
    id_payment: int,
    id_order: int,
    external_id: string | null,
    preference_id: string | null,
    status: 'pending' | 'approved' | 'rejected' | 'refunded' | 'cancelled' | 'in_process',
    amount: decimal,
    currency: string,
    payment_method: string | null,
    provider: string | null,
    creation_date: datetime,
    updated_date: datetime
  }
]
```

**Errores posibles:**
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/payments/create-preference` | Genera una preferencia de pago en la pasarela (ej: MercadoPago) para una orden y crea el registro `payment` en estado `pending`. | Sí | No |

**Parámetros** (body):
```ts
{
  id_order: int
}
```

**Respuesta** `201 Created`:
```ts
{
  preference_id: string,         // id de la preferencia en la pasarela
  init_point: string,            // URL para redirigir al usuario al checkout
  id_payment: int                // id del registro `payment` creado en local
}
```

**Errores posibles:**
- `404 Not Found`: la orden no existe.
- `409 Conflict`: la orden ya tiene un pago aprobado o está cancelada.
- `502 Bad Gateway`: error al comunicarse con la pasarela de pago.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/payments/webhook` | Endpoint público que recibe notificaciones de la pasarela de pago. Actualiza el estado del `payment` y dispara cambio de estado de la `order` correspondiente. | No | No |

**Parámetros** (body): payload variable según el proveedor. Para MercadoPago suele ser:
```ts
{
  type: string,                  // ej: "payment"
  data: {
    id: string                   // external_id del pago en la pasarela
  },
  // ... otros campos del proveedor
}
```

**Respuesta** `200 OK`: cuerpo vacío o `{ received: true }` (depende del proveedor).

**Errores posibles:**
- `400 Bad Request`: firma del webhook inválida o payload malformado.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| GET | `/api/payments/:id_order/status` | Consulta el estado del último pago asociado a una orden. | Sí | No |

**Parámetros** (path):
```ts
{
  id_order: int
}
```

**Respuesta** `200 OK`:
```ts
{
  status: 'pending' | 'approved' | 'rejected' | 'refunded' | 'cancelled' | 'in_process' | 'no_payment',
  payment_details: {                   // null si no hay ningún payment para la orden
    id_payment: int,
    external_id: string | null,
    amount: decimal,
    currency: string,
    payment_method: string | null,
    provider: string | null,
    creation_date: datetime,
    updated_date: datetime
  } | null
}
```

**Errores posibles:**
- `404 Not Found`: la orden no existe.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/payments/:id_payment/refund` | Solicita el reembolso de un pago aprobado a través de la pasarela. Cambia el estado a `refunded` si la operación es exitosa. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id_payment: int
}

// Body
{
  amount?: decimal,              // opcional; default: monto total del pago
  reason?: string
}
```

**Respuesta** `200 OK`:
```ts
{
  id_payment: int,
  status: 'refunded',
  amount: decimal,
  refunded_amount: decimal,
  updated_date: datetime
}
```

**Errores posibles:**
- `409 Conflict`: el pago no está en estado `approved` y no puede reembolsarse.
- `422 Unprocessable Entity`: el `amount` solicitado supera el monto del pago.
- `502 Bad Gateway`: error al comunicarse con la pasarela de pago.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/payments/:id_payment/cancel` | Cancela un pago en estado `pending` o `in_process` antes de que sea aprobado. | Sí | Sí |

**Parámetros** (path + body):
```ts
// Path
{
  id_payment: int
}

// Body
{
  reason?: string
}
```

**Respuesta** `200 OK`:
```ts
{
  id_payment: int,
  status: 'cancelled',
  updated_date: datetime
}
```

**Errores posibles:**
- `409 Conflict`: el pago no está en estado `pending` o `in_process`.
- `502 Bad Gateway`: error al comunicarse con la pasarela de pago.
- `400 Bad Request`: datos faltantes, formato inválido o tipos incorrectos.
- `422 Unprocessable Entity`: la entidad no cumple reglas de negocio (ej: referencias inexistentes, valores fuera de rango).
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/payments/:id_payment/retry` | Genera una nueva preferencia de pago para una orden cuyo pago anterior fue rechazado o cancelado. Crea un nuevo registro `payment` vinculado a la misma orden. | Sí | No |

**Parámetros** (path):
```ts
{
  id_payment: int        // id del payment fallido a reintentar
}
```

**Respuesta** `201 Created`:
```ts
{
  preference_id: string,
  init_point: string,
  id_payment: int        // id del NUEVO payment creado
}
```

**Errores posibles:**
- `409 Conflict`: el pago anterior no está en un estado que permita reintento (`rejected` o `cancelled`).
- `502 Bad Gateway`: error al comunicarse con la pasarela de pago.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el recurso no pertenece al usuario autenticado.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

| Método | Path | Descripción | Auth | Admin |
|---|---|---|---|---|
| POST | `/api/payments/:id_payment/sync` | Fuerza la consulta del estado del pago directamente a la pasarela y actualiza el registro local. Útil cuando el webhook falló. | Sí | Sí |

**Parámetros** (path):
```ts
{
  id_payment: int
}
```

**Respuesta** `200 OK`:
```ts
{
  id_payment: int,
  status: 'pending' | 'approved' | 'rejected' | 'refunded' | 'cancelled' | 'in_process',
  external_id: string | null,
  updated_date: datetime,
  changed: boolean       // true si el estado cambió respecto al anterior
}
```

**Errores posibles:**
- `502 Bad Gateway`: error al comunicarse con la pasarela de pago.
- `401 Unauthorized`: token ausente, inválido o expirado.
- `403 Forbidden`: el usuario autenticado no tiene rol `Admin`.
- `404 Not Found`: el recurso referenciado en la URL no existe o fue eliminado lógicamente.

---

## Notas generales

### Columnas

- **Auth**: indica si el endpoint requiere que el usuario esté autenticado (envío de header `Authorization: Bearer <token>`).
- **Admin**: indica si además de estar autenticado, el usuario debe tener el rol `Admin`. Si está en `No`, cualquier usuario autenticado puede ejecutarlo (sujeto a validación de propiedad del recurso: por ejemplo, un cliente solo puede ver sus propias órdenes, aunque el endpoint no sea admin-only).

### Convenciones

- **Autenticación**: Header `Authorization: Bearer <token>` en endpoints protegidos.
- **Paginación**: `page` (default 1), `limit` (default 20). Respuesta con `{ data, total, page, limit }`.
- **Filtros**: siempre como query params.
- **Bodies**: JSON excepto uploads (multipart/form-data).
- **Fechas**: formato ISO 8601.
- **Códigos HTTP**: `200` OK, `201` Created, `204` No Content, `400` Bad Request, `401` Unauthorized, `403` Forbidden, `404` Not Found, `409` Conflict, `413` Payload Too Large, `415` Unsupported Media Type, `422` Unprocessable Entity, `500` Internal Server Error, `502` Bad Gateway.
- **Logical delete**: los DELETE en entidades con `logical_delete` marcan el flag, no borran físicamente.

### Manejo de errores

Todos los endpoints pueden devolver los siguientes errores de forma implícita, por lo que **no se listan individualmente** en cada uno:

- `500 Internal Server Error`: error inesperado en el servidor (bug, caída de BD, etc.).
- `503 Service Unavailable`: el servicio está temporalmente fuera de línea (mantenimiento, sobrecarga).
- `429 Too Many Requests`: el cliente excedió el rate limit configurado.

El formato estándar de respuesta de error es:
```ts
{
  error: {
    code: string,              // código simbólico, ej: "VALIDATION_ERROR", "NOT_FOUND", "UNAUTHORIZED"
    message: string,           // mensaje legible para el usuario final
    details?: [                // opcional: solo para errores de validación (400/422)
      {
        field: string,         // nombre del campo que falla
        issue: string          // descripción del problema
      }
    ],
    timestamp: datetime,
    path: string               // endpoint que generó el error
  }
}
```

### Significado de los códigos HTTP en este API

- **`400 Bad Request`**: error de formato del cliente (JSON malformado, tipos incorrectos, campos obligatorios ausentes).
- **`401 Unauthorized`**: falta el token, está mal firmado o expiró. El cliente debe re-autenticarse.
- **`403 Forbidden`**: el token es válido pero el usuario no tiene permisos para la operación (rol insuficiente o recurso ajeno).
- **`404 Not Found`**: el recurso identificado en la URL no existe (o fue eliminado lógicamente).
- **`409 Conflict`**: la operación entra en conflicto con el estado actual del recurso (duplicados, transiciones de estado inválidas, dependencias rotas).
- **`413 Payload Too Large`**: el body/archivo excede el tamaño máximo permitido.
- **`415 Unsupported Media Type`**: el `Content-Type` del body no es el esperado (ej: se esperaba JSON y llegó XML).
- **`422 Unprocessable Entity`**: el payload es sintácticamente correcto pero viola reglas de negocio (ej: referencia a una entidad inexistente, cantidades fuera de rango).
- **`502 Bad Gateway`**: error al comunicarse con un servicio externo (típicamente la pasarela de pago).

### Tipos de datos usados

- `int`: número entero
- `decimal`: número con decimales (precios, porcentajes, pesos)
- `string`: cadena de texto
- `boolean`: verdadero/falso
- `date`: fecha (YYYY-MM-DD)
- `datetime`: fecha y hora (ISO 8601)
- `enum`: valor de una lista cerrada (se indican los valores posibles)
- `int[]`: array de enteros
- `file`: archivo binario (multipart/form-data)
- `array<{...}>`: array de objetos con estructura específica

---

# SECCIÓN 3 — ESQUEMA DE BASE DE DATOS

> Contenido completo de `coleccionaloya.sql`

```sql
CREATE TABLE "public"."address" (
  "id_address" serial4,
  "id_client" int4 NOT NULL,
  "state" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "type" "public"."type_address" NOT NULL,
  "observations" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "apartment" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "corner" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "door_number" varchar(10) COLLATE "pg_catalog"."default" NOT NULL,
  "floor" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "neighborhood" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "street" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "logical_delete" bool NOT NULL DEFAULT false
);

CREATE TABLE "public"."author" (
  "id_author" serial4,
  "name" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
);

CREATE TABLE "public"."book" (
  "type" "public"."type_book" NOT NULL,
  "id_product" int4 NOT NULL
);

CREATE TABLE "public"."book_author" (
  "id_book" int4 NOT NULL,
  "id_author" int4 NOT NULL
);

CREATE TABLE "public"."book_genre" (
  "id_book" int4 NOT NULL,
  "id_genre" int4 NOT NULL
);

CREATE TABLE "public"."cart" (
  "id_cart" serial4,
  "id_client" int4 NOT NULL,
  "id_coupon" int4,
  "creation_date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "public"."cart_item" (
  "id_cart" int4 NOT NULL,
  "id_product" int4 NOT NULL,
  "quantity" int4 NOT NULL,
  "added_date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "public"."client" (
  "id_client" serial4,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "lastname" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "email" text COLLATE "pg_catalog"."default" NOT NULL,
  "phone" varchar(10) COLLATE "pg_catalog"."default" NOT NULL,
  "id_address_delivery" int4,
  "id_address_order" int4,
  "logical_delete" bool NOT NULL DEFAULT false,
  "password_hash" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "role_id" int4 NOT NULL,
  "active" bool NOT NULL DEFAULT true,
  "creation_date" timestamp(6) NOT NULL DEFAULT now()
);

CREATE TABLE "public"."coupon" (
  "id_coupon" serial4,
  "name" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "token" varchar(10) COLLATE "pg_catalog"."default" NOT NULL,
  "description" text COLLATE "pg_catalog"."default" NOT NULL,
  "creation_date" timestamp(3) NOT NULL,
  "valid_from" timestamp(3),
  "valid_until" timestamp(3),
  "porcentage" numeric(5,2) NOT NULL,
  "is_active" bool NOT NULL DEFAULT false,
  "logical_delete" bool NOT NULL DEFAULT false
);

CREATE TABLE "public"."coupon_client" (
  "id_client" int4 NOT NULL,
  "id_coupon" int4 NOT NULL
);

CREATE TABLE "public"."discount" (
  "id_discount" serial4,
  "id_product" int4,
  "porcentage" numeric(5,2) NOT NULL,
  "creation_date" timestamp(3) NOT NULL,
  "valid_from" timestamp(3),
  "valid_until" timestamp(3),
  "is_active" bool NOT NULL DEFAULT false,
  "logical_delete" bool NOT NULL DEFAULT false
);

CREATE TABLE "public"."genre" (
  "id_genre" serial4,
  "name" varchar(20) COLLATE "pg_catalog"."default" NOT NULL
);

CREATE TABLE "public"."idempotency_keys" (
  "id" serial4,
  "idempotency_key" varchar(128) COLLATE "pg_catalog"."default" NOT NULL,
  "id_client" int4,
  "endpoint" text COLLATE "pg_catalog"."default" NOT NULL,
  "request_hash" varchar(64) COLLATE "pg_catalog"."default" NOT NULL,
  "status" varchar(20) COLLATE "pg_catalog"."default" NOT NULL DEFAULT 'processing'::character varying,
  "status_code" int2,
  "response_body" text COLLATE "pg_catalog"."default",
  "created_at" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "expires_at" timestamp(3) NOT NULL DEFAULT (CURRENT_TIMESTAMP + '24:00:00'::interval)
);

CREATE TABLE "public"."log_backend" (
  "id" serial4,
  "date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "ip" text COLLATE "pg_catalog"."default" NOT NULL,
  "user_agent" text COLLATE "pg_catalog"."default" NOT NULL,
  "protocol" text COLLATE "pg_catalog"."default" NOT NULL,
  "query_parameters" text COLLATE "pg_catalog"."default" NOT NULL,
  "body" text COLLATE "pg_catalog"."default" NOT NULL,
  "status_response" text COLLATE "pg_catalog"."default" NOT NULL,
  "response_time" text COLLATE "pg_catalog"."default" NOT NULL,
  "id_user" text COLLATE "pg_catalog"."default",
  "email" text COLLATE "pg_catalog"."default",
  "endpoint" text COLLATE "pg_catalog"."default" NOT NULL,
  "http_method" text COLLATE "pg_catalog"."default" NOT NULL
);

CREATE TABLE "public"."menu" (
  "id_menu" serial4,
  "name" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "id_tag" int4 NOT NULL,
  "order" int4 NOT NULL,
  "id_menu_referenced" int4,
  "is_category_filter" bool NOT NULL DEFAULT false
);

CREATE TABLE "public"."order" (
  "id_order" serial4,
  "creation_date" date NOT NULL,
  "id_client" int4 NOT NULL,
  "id_coupon" int4,
  "taxes" numeric(12,2) NOT NULL,
  "status" "public"."type_order_status" NOT NULL,
  "tracking" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "observations" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "cancel_reason" varchar(500) COLLATE "pg_catalog"."default",
  "return_reason" varchar(500) COLLATE "pg_catalog"."default",
  "subtotal" numeric(12,2),
  "total" numeric(12,2),
  "id_delivery_snapshot" int4,
  "id_order_snapshot" int4
);

CREATE TABLE "public"."order_address_snapshot" (
  "id_snapshot" serial4,
  "id_address_original" int4,
  "state" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "type" "public"."type_address" NOT NULL,
  "observations" varchar(200) COLLATE "pg_catalog"."default",
  "apartment" varchar(20) COLLATE "pg_catalog"."default",
  "corner" varchar(100) COLLATE "pg_catalog"."default",
  "door_number" varchar(10) COLLATE "pg_catalog"."default",
  "floor" varchar(50) COLLATE "pg_catalog"."default",
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "neighborhood" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "street" varchar(200) COLLATE "pg_catalog"."default" NOT NULL,
  "creation_date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "public"."order_line" (
  "id_order" int4 NOT NULL,
  "id_product" int4 NOT NULL,
  "quantity" int4 NOT NULL,
  "price" numeric(12,2) NOT NULL
);

CREATE TABLE "public"."order_status_history" (
  "id" serial4,
  "id_order" int4 NOT NULL,
  "status" "public"."type_order_status" NOT NULL,
  "reason" varchar(500) COLLATE "pg_catalog"."default",
  "changed_by" int4,
  "date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "public"."pack" (
  "product_count" int4 NOT NULL,
  "creation_date" timestamp(3) NOT NULL,
  "id_product" int4 NOT NULL
);

CREATE TABLE "public"."pack_product" (
  "id_pack" int4 NOT NULL,
  "id_product" int4 NOT NULL,
  "quantity" int4 NOT NULL
);

CREATE TABLE "public"."payment" (
  "id_payment" serial4,
  "id_order" int4 NOT NULL,
  "external_id" varchar(100) COLLATE "pg_catalog"."default",
  "preference_id" varchar(100) COLLATE "pg_catalog"."default",
  "status" "public"."payment_status" NOT NULL DEFAULT 'pending'::payment_status,
  "amount" numeric(12,2) NOT NULL,
  "currency" varchar(10) COLLATE "pg_catalog"."default" NOT NULL DEFAULT 'UYU'::character varying,
  "payment_method" varchar(50) COLLATE "pg_catalog"."default",
  "provider" varchar(50) COLLATE "pg_catalog"."default",
  "creation_date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_date" timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "raw_response" text COLLATE "pg_catalog"."default"
);

CREATE TABLE "public"."product" (
  "id_product" serial4,
  "name" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "short_description" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "long_description" text COLLATE "pg_catalog"."default" NOT NULL,
  "price" numeric(12,2) NOT NULL,
  "creation_date" date NOT NULL,
  "type" "public"."product_type" NOT NULL,
  "weight" numeric(12,2) NOT NULL,
  "logical_delete" bool NOT NULL DEFAULT false,
  "is_featured" bool NOT NULL DEFAULT false
);

CREATE TABLE "public"."product_gallery" (
  "id_gallery" serial4,
  "id_product" int4 NOT NULL,
  "order" int4 NOT NULL,
  "url" text COLLATE "pg_catalog"."default" NOT NULL
);

CREATE TABLE "public"."refresh_tokens" (
  "id" serial4,
  "id_client" int4 NOT NULL,
  "token" varchar(512) COLLATE "pg_catalog"."default" NOT NULL,
  "expires_at" timestamp(6) NOT NULL,
  "revoked" bool NOT NULL DEFAULT false,
  "created_at" timestamp(6) NOT NULL DEFAULT now()
);

CREATE TABLE "public"."roles" (
  "id" serial4,
  "name" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "description" text COLLATE "pg_catalog"."default"
);

CREATE TABLE "public"."stock" (
  "id_stock" serial4,
  "id_product" int4 NOT NULL,
  "date" timestamp(3) NOT NULL,
  "input" int4 NOT NULL,
  "output" int4 NOT NULL,
  "id_order" int4,
  "type" "public"."stock_movement_type" NOT NULL,
  "reason" varchar(500) COLLATE "pg_catalog"."default"
);

CREATE TABLE "public"."tag" (
  "id_tag" serial4,
  "name" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "is_franchise" bool NOT NULL DEFAULT false
);

CREATE TABLE "public"."tag_product" (
  "id_tag" int4 NOT NULL,
  "id_product" int4 NOT NULL
);

CREATE TABLE "public"."wishlist" (
  "id_client" int4 NOT NULL,
  "id_product" int4 NOT NULL
);


-- ----------------------------
-- Foreign Keys structure for table address
-- ----------------------------
ALTER TABLE "public"."address" ADD CONSTRAINT "address_id_client_fkey" FOREIGN KEY ("id_client") REFERENCES "public"."client" ("id_client") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."book" ADD CONSTRAINT "book_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."book_author" ADD CONSTRAINT "book_author_id_author_fkey" FOREIGN KEY ("id_author") REFERENCES "public"."author" ("id_author") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."book_author" ADD CONSTRAINT "book_author_id_book_fkey" FOREIGN KEY ("id_book") REFERENCES "public"."book" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."book_genre" ADD CONSTRAINT "book_genre_id_book_fkey" FOREIGN KEY ("id_book") REFERENCES "public"."book" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."book_genre" ADD CONSTRAINT "book_genre_id_genre_fkey" FOREIGN KEY ("id_genre") REFERENCES "public"."genre" ("id_genre") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."client" ADD CONSTRAINT "id_address_delivery_fkey" FOREIGN KEY ("id_address_delivery") REFERENCES "public"."address" ("id_address") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."client" ADD CONSTRAINT "id_address_order_fkey" FOREIGN KEY ("id_address_order") REFERENCES "public"."address" ("id_address") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."coupon_client" ADD CONSTRAINT "coupon_client_id_client_fkey" FOREIGN KEY ("id_client") REFERENCES "public"."client" ("id_client") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."coupon_client" ADD CONSTRAINT "coupon_client_id_coupon_fkey" FOREIGN KEY ("id_coupon") REFERENCES "public"."coupon" ("id_coupon") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."discount" ADD CONSTRAINT "discount_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "public"."menu" ADD CONSTRAINT "menu_id_menu_referenced_fkey" FOREIGN KEY ("id_menu_referenced") REFERENCES "public"."menu" ("id_menu") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "public"."menu" ADD CONSTRAINT "menu_id_tag_fkey" FOREIGN KEY ("id_tag") REFERENCES "public"."tag" ("id_tag") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."order" ADD CONSTRAINT "order_id_client_fkey" FOREIGN KEY ("id_client") REFERENCES "public"."client" ("id_client") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."order" ADD CONSTRAINT "order_id_delivery_snapshot_fkey" FOREIGN KEY ("id_delivery_snapshot") REFERENCES "public"."order_address_snapshot" ("id_snapshot") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."order" ADD CONSTRAINT "order_id_order_snapshot_fkey" FOREIGN KEY ("id_order_snapshot") REFERENCES "public"."order_address_snapshot" ("id_snapshot") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."order_address_snapshot" ADD CONSTRAINT "order_address_snapshot_id_address_original_fkey" FOREIGN KEY ("id_address_original") REFERENCES "public"."address" ("id_address") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "public"."order_line" ADD CONSTRAINT "order_line_id_order_fkey" FOREIGN KEY ("id_order") REFERENCES "public"."order" ("id_order") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."order_line" ADD CONSTRAINT "order_line_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."pack" ADD CONSTRAINT "pack_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."pack_product" ADD CONSTRAINT "pack_product_id_pack_fkey" FOREIGN KEY ("id_pack") REFERENCES "public"."pack" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."pack_product" ADD CONSTRAINT "pack_product_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."product_gallery" ADD CONSTRAINT "product_gallery_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."stock" ADD CONSTRAINT "stock_id_order_fkey" FOREIGN KEY ("id_order") REFERENCES "public"."order" ("id_order") ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE "public"."stock" ADD CONSTRAINT "stock_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."tag_product" ADD CONSTRAINT "tag_product_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."tag_product" ADD CONSTRAINT "tag_product_id_tag_fkey" FOREIGN KEY ("id_tag") REFERENCES "public"."tag" ("id_tag") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."wishlist" ADD CONSTRAINT "wishlist_id_client_fkey" FOREIGN KEY ("id_client") REFERENCES "public"."client" ("id_client") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "public"."wishlist" ADD CONSTRAINT "wishlist_id_product_fkey" FOREIGN KEY ("id_product") REFERENCES "public"."product" ("id_product") ON DELETE RESTRICT ON UPDATE CASCADE;
```
