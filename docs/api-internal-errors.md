# Catálogo de excepciones internas de la API

> Generado automáticamente desde las clases que heredan de `DomainException` en el proyecto `Domain`.
> Son los **errores de negocio** que la API lanza de forma controlada. El `ExceptionMiddleware` las
> traduce a una respuesta `ApiProblemDetails` (RFC 7807) con esta forma (camelCase):

```json
{
  "type": "<errorType>",          // código estable y único, p.ej. "OrderNotFound"
  "title": "<nombreEstadoHTTP>",  // p.ej. "NotFound", "Conflict"
  "status": <httpStatus>,         // p.ej. 404
  "detail": "<mensaje>",          // texto descriptivo de la excepción
  "instance": "<path>",           // endpoint que originó el error
  "stackTrace": "<...>",          // stack trace (presente en la serialización actual)
  "traceId": "<id>",
  "timestamp": "<utc>",
  "errors": { "campo": ["..."] }  // SOLO en el caso de validación (errorType = "Validation", 422)
}
```

El campo **`type`** (= `errorType`) es el identificador estable para manejar cada caso programáticamente.
Los textos con `{...}` son plantillas: en runtime se reemplazan por valores concretos (ids, nombres, etc.).

**Total: 96 excepciones de negocio (DomainException).**

## Códigos HTTP usados

| HTTP | Significado |
|:---:|---|
| 400 | Bad Request — formato/tipos inválidos |
| 401 | Unauthorized — token ausente / inválido / expirado |
| 403 | Forbidden — sin permisos o recurso ajeno |
| 404 | Not Found — recurso inexistente |
| 409 | Conflict — conflicto de estado / duplicado / dependencia |
| 413 | Payload Too Large — archivo demasiado grande |
| 415 | Unsupported Media Type — tipo de archivo no soportado |
| 422 | Unprocessable Entity — viola reglas de negocio |
| 502 | Bad Gateway — error con la pasarela de pago |

## Autenticación / Clientes

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `InvalidCredentialsException` | 401 | `InvalidCredentials` | Invalid email or password. |
| `InvalidRefreshTokenException` | 401 | `InvalidRefreshToken` | The refresh token is invalid, expired, or revoked. |
| `ClientNotFoundException` | 404 | `ClientNotFound` | The client was not found. |
| `DefaultUserRoleNotFoundException` | 409 | `DefaultUserRoleNotFound` | The default user role is not configured in the system. |
| `EmailAlreadyRegisteredException` | 409 | `EmailAlreadyRegistered` | The email address is already registered. |
| `SamePasswordException` | 409 | `SamePassword` | The new password must be different from the current password. |
| `InvalidClientReferenceException` | 422 | `InvalidClientReference` | One or more of the referenced clients do not exist. |

## Roles

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `RoleNotFoundException` | 404 | `RoleNotFound` | Role #{id} not found. |
| `DuplicateRoleNameException` | 409 | `DuplicateRoleName` | A role with the name '{name}' already exists. |
| `RoleInUseException` | 409 | `RoleInUse` | Role #{id} cannot be deleted because it is assigned to one or more clients. |

## Direcciones

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `AddressAccessDeniedException` | 403 | `AddressAccessDenied` | You do not have permission to access this address. |
| `AddressNotFoundException` | 404 | `AddressNotFound` | Address #{id} not found. |

## Productos

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `ProductNotFoundException` | 404 | `ProductNotFound` | Product #{id} not found. |
| `ProductTagNotLinkedException` | 404 | `ProductTagNotLinked` | Tag #{tagId} is not linked to product #{productId}. |
| `ProductInUseException` | 409 | `ProductInUse` | Product #{id} cannot be deleted because it is part of active orders or composes active packs. |
| `ProductTagAlreadyLinkedException` | 409 | `ProductTagAlreadyLinked` | One or more tags are already assigned to the product. |
| `InvalidProductPriceException` | 422 | `InvalidProductPrice` | Product price '{price}' is invalid; it must be zero or greater. |
| `InvalidProductTypeException` | 422 | `InvalidProductType` | Product type '{type}' is invalid; allowed values are 'book', 'pack' or 'object'. |
| `InvalidProductWeightException` | 422 | `InvalidProductWeight` | Product weight '{weight}' is invalid; it must be zero or greater. |

## Libros

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `BookAuthorNotLinkedException` | 404 | `BookAuthorNotLinked` | Author #{authorId} is not linked to book #{bookId}. |
| `BookGenreNotLinkedException` | 404 | `BookGenreNotLinked` | Genre #{genreId} is not linked to book #{bookId}. |
| `BookNotFoundException` | 404 | `BookNotFound` | Book #{id} not found. |
| `BookAuthorAlreadyLinkedException` | 409 | `BookAuthorAlreadyLinked` | One or more authors are already linked to the book. |
| `BookGenreAlreadyLinkedException` | 409 | `BookGenreAlreadyLinked` | One or more genres are already linked to the book. |
| `InvalidBookTypeException` | 422 | `InvalidBookType` | Book type '{type}' is invalid; allowed values are 'comic' or 'manga'. |

## Packs

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `PackItemNotFoundException` | 404 | `PackItemNotFound` | Product #{productId} is not part of pack #{packId}. |
| `PackNotFoundException` | 404 | `PackNotFound` | Pack #{id} not found. |
| `PackItemAlreadyExistsException` | 409 | `PackItemAlreadyExists` | Product #{productId} is already part of pack #{packId}. |
| `InvalidPackItemException` | 422 | `InvalidPackItem` | One or more products do not exist or are packs (packs cannot be nested). |

## Autores

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `AuthorNotFoundException` | 404 | `AuthorNotFound` | Author #{id} not found. |
| `AuthorInUseException` | 409 | `AuthorInUse` | Author #{id} cannot be deleted because it is linked to one or more books. |
| `DuplicateAuthorNameException` | 409 | `DuplicateAuthorName` | An author with the name '{name}' already exists. |
| `InvalidAuthorReferenceException` | 422 | `InvalidAuthorReference` | One or more of the referenced authors do not exist. |

## Géneros

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `GenreNotFoundException` | 404 | `GenreNotFound` | Genre #{id} not found. |
| `DuplicateGenreNameException` | 409 | `DuplicateGenreName` | A genre with the name '{name}' already exists. |
| `GenreInUseException` | 409 | `GenreInUse` | Genre #{id} cannot be deleted because it is linked to one or more books. |
| `InvalidGenreReferenceException` | 422 | `InvalidGenreReference` | One or more of the referenced genres do not exist. |

## Tags / Franquicias

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `TagNotFoundException` | 404 | `TagNotFound` | Tag #{id} not found. |
| `DuplicateTagNameException` | 409 | `DuplicateTagName` | A tag with the name '{name}' already exists. |
| `TagInUseException` | 409 | `TagInUse` | Tag #{id} cannot be deleted because it is associated with products or used by menu items. |
| `InvalidTagReferenceException` | 422 | `InvalidTagReference` | One or more of the referenced tags do not exist. |

## Galería

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `GalleryImageNotFoundException` | 404 | `GalleryImageNotFound` | Gallery image #{id} not found. |
| `GalleryImageSourceRequiredException` | 422 | `GalleryImageSourceRequired` | Either an image file or an image url must be provided. |
| `GalleryReorderMismatchException` | 422 | `GalleryReorderMismatch` | One or more gallery images do not belong to the product. |

## Menú

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `MenuItemNotFoundException` | 404 | `MenuItemNotFound` | Menu item #{id} not found. |
| `MenuHasChildrenException` | 409 | `MenuHasChildren` | Menu item #{id} cannot be deleted because other items reference it as parent. |
| `InvalidMenuReferenceException` | 422 | `InvalidMenuReference` | The referenced tag or parent menu item does not exist. |
| `MenuCircularReferenceException` | 422 | `MenuCircularReference` | The requested parent reference would create a circular menu structure. |

## Carrito

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `CartCouponNotAppliedException` | 404 | `CartCouponNotApplied` | The cart does not have a coupon applied. |
| `CartItemNotFoundException` | 404 | `CartItemNotFound` | Product #{productId} is not in the cart. |
| `CartEmptyException` | 409 | `CartEmpty` | The cart is empty. |
| `InsufficientStockException` | 409 | `InsufficientStock` | Insufficient stock for product #{productId}. |
| `InvalidCartQuantityException` | 422 | `InvalidCartQuantity` | Cart item quantity must be greater than zero. |

## Cupones

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `CouponClientNotAssignedException` | 404 | `CouponClientNotAssigned` | Coupon #{couponId} is not assigned to client #{clientId}. |
| `CouponNotFoundException` | 404 | `CouponNotFound` | Coupon #{id} not found. |
| `CouponTokenNotFoundException` | 404 | `CouponTokenNotFound` | Coupon with token '{token}' not found. |
| `CouponAlreadyActiveException` | 409 | `CouponAlreadyActive` | Coupon #{id} is already active. |
| `CouponAlreadyInactiveException` | 409 | `CouponAlreadyInactive` | Coupon #{id} is already inactive. |
| `CouponClientAlreadyAssignedException` | 409 | `CouponClientAlreadyAssigned` | One or more clients already have the coupon assigned. |
| `CouponExpiredException` | 409 | `CouponExpired` | Coupon #{id} has expired. |
| `CouponNotUsableException` | 409 | `CouponNotUsable` | The coupon is inactive, expired or not assigned to the client. |
| `DuplicateCouponTokenException` | 409 | `DuplicateCouponToken` | A coupon with token '{token}' already exists. |
| `InvalidCouponPercentageException` | 422 | `InvalidCouponPercentage` | Coupon percentage '{porcentage}' is invalid; it must be between 0 and 100. |

## Descuentos

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `DiscountNotFoundException` | 404 | `DiscountNotFound` | Discount #{id} not found. |
| `DiscountOverlapException` | 409 | `DiscountOverlap` | Product #{productId} already has an active discount overlapping the given date range. |
| `InvalidDiscountActionException` | 422 | `InvalidDiscountAction` | Discount action '{action}' is invalid; allowed values are 'activate' or 'deactivate'. |
| `InvalidDiscountPercentageException` | 422 | `InvalidDiscountPercentage` | Discount percentage '{porcentage}' is invalid; it must be between 0 and 100. |

## Stock

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `NegativeStockException` | 409 | `NegativeStock` | The movement would result in negative stock for product #{productId}. |
| `InvalidStockQuantityException` | 422 | `InvalidStockQuantity` | Stock movement quantity cannot be zero. |
| `InvalidStockSignException` | 422 | `InvalidStockSign` | The quantity sign does not match the movement type '{type}'. |
| `InvalidStockTypeException` | 422 | `InvalidStockType` | Stock movement type '{type}' is invalid; allowed values are 'entry', 'exit' or 'adjustment'. |

## Wishlist

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `WishlistItemNotFoundException` | 404 | `WishlistItemNotFound` | Product #{productId} is not in the wishlist. |
| `WishlistItemAlreadyExistsException` | 409 | `WishlistItemAlreadyExists` | Product #{productId} is already in the wishlist. |

## Órdenes

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `OrderAccessDeniedException` | 403 | `OrderAccessDenied` | The order does not belong to the authenticated user. |
| `OrderNotFoundException` | 404 | `OrderNotFound` | Order #{id} not found. |
| `EmptyOrderException` | 409 | `EmptyOrder` | An order cannot be created without items. |
| `OrderCannotBeCancelledException` | 409 | `OrderCannotBeCancelled` | Order #{orderId} cannot be cancelled in its current status. |
| `OrderCannotBeReturnedException` | 409 | `OrderCannotBeReturned` | Order #{orderId} cannot be returned because it is not in 'delivered' status. |
| `OrderInvalidTransitionException` | 409 | `OrderInvalidTransition` | Order #{orderId} cannot transition from '{from}' to '{to}'. |
| `OrderInvoiceNotAvailableException` | 409 | `OrderInvoiceNotAvailable` | Order #{orderId} is pending or cancelled and does not support an invoice. |
| `InvalidOrderAddressException` | 422 | `InvalidOrderAddress` | The delivery or billing address does not belong to the client. |
| `InvalidOrderStatusException` | 422 | `InvalidOrderStatus` | Order status '{status}' is invalid. |

## Pagos

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `PaymentAccessDeniedException` | 403 | `PaymentAccessDenied` | The payment does not belong to the authenticated user. |
| `PaymentNotFoundException` | 404 | `PaymentNotFound` | Payment #{id} not found. |
| `OrderAlreadyPaidException` | 409 | `OrderAlreadyPaid` | Order #{orderId} already has an approved payment or is cancelled. |
| `PaymentNotCancellableException` | 409 | `PaymentNotCancellable` | Payment #{id} is not in 'pending' or 'in_process' status. |
| `PaymentNotRefundableException` | 409 | `PaymentNotRefundable` | Payment #{id} is not in 'approved' status and cannot be refunded. |
| `PaymentNotRetryableException` | 409 | `PaymentNotRetryable` | Payment #{id} is not in 'rejected' or 'cancelled' status. |
| `InvalidRefundAmountException` | 422 | `InvalidRefundAmount` | The refund amount for payment #{id} exceeds the payment amount. |
| `PaymentGatewayException` | 502 | `PaymentGatewayError` | Error communicating with the payment gateway: {detail} |

## Archivos / Upload

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `UploadedFileNotFoundException` | 404 | `FileNotFound` | The file '{fileName}' does not exist in storage. |
| `PayloadTooLargeException` | 413 | `PayloadTooLarge` | The file exceeds the maximum allowed size of {maxMB} MB. |
| `UnsupportedImageTypeException` | 415 | `UnsupportedMediaType` | The file '{fileName}' is not a supported image type. |

## Idempotencia

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `IdempotencyKeyInFlightException` | 409 | `IdempotencyKeyInFlight` | A request with this Idempotency-Key is already being processed. |
| `IdempotencyKeyMismatchException` | 422 | `IdempotencyKeyMismatch` | This Idempotency-Key was already used with a different request body. |

## Validación / Compartidas

| Excepción | HTTP | errorType | Mensaje |
|---|:---:|---|---|
| `ModelValidationException` | 422 | `Validation` | One or more validation errors occurred |

## Excepciones genéricas (fallback del middleware)

Además de las `DomainException` anteriores, el `ExceptionMiddleware` traduce estas excepciones no controladas:

| Excepción .NET | HTTP | Detalle |
|---|:---:|---|
| `ArgumentNullException` | 400 | `Required parameter '{param}' was not provided` |
| `ArgumentException` | 400 | Mensaje de la excepción |
| `UnauthorizedAccessException` | 401 | No autorizado |
| `KeyNotFoundException` | 404 | Recurso no encontrado |
| `InvalidOperationException` | 409 | Mensaje de la excepción |
| `ValidationException` (DataAnnotations) | 422 | `One or more validation errors occurred.` |
| *(cualquier otra no controlada)* | 500 | `An error occurred while processing your request.` |

