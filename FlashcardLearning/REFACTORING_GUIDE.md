# ??? REPOSITORY PATTERN & SERVICE LAYER - REFACTORING GUIDE

## ? HOÀN THÀNH REFACTORING

### ?? T?ng Quan

D? án ?ã ???c refactor t? **Fat Controllers** sang ki?n trúc **3-Layer Architecture**:
- **Repository Layer** (Data Access)
- **Service Layer** (Business Logic)
- **Controller Layer** (Presentation)

---

## ?? Ki?n Trúc M?i

```
???????????????????????????????????????????????????????
?                  HTTP REQUEST                        ?
???????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????
?              CONTROLLER LAYER                        ?
?  (DecksController)                                   ?
?  - Nh?n HTTP Request                                 ?
?  - G?i Service                                       ?
?  - Tr? v? HTTP Response                              ?
???????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????
?              SERVICE LAYER                           ?
?  (IDeckService -> DeckService)                       ?
?  - Business Logic                                    ?
?  - Validation                                        ?
?  - Authorization Logic                               ?
?  - DTO Mapping                                       ?
???????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????
?              REPOSITORY LAYER                        ?
?  (IDeckRepository -> DeckRepository)                 ?
?  - Database Queries                                  ?
?  - CRUD Operations                                   ?
?  - Entity Framework Core                             ?
???????????????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????????????
?                  DATABASE                            ?
???????????????????????????????????????????????????????
```

---

## ?? Files Created

### 1. Generic Repository
```
? Repositories/IRepository.cs              - Generic interface
? Repositories/Repository.cs               - Generic implementation
```

### 2. Deck Repository
```
? Repositories/IDeckRepository.cs          - Deck-specific interface
? Repositories/DeckRepository.cs           - Deck-specific implementation
```

### 3. Folder Repository
```
? Repositories/IFolderRepository.cs        - Folder interface
? Repositories/FolderRepository.cs         - Folder implementation
```

### 4. Deck Service
```
? Services/IDeckService.cs                 - Service interface
? Services/DeckService.cs                  - Service implementation
```

### 5. Updated Files
```
? Controllers/DecksController.cs           - Refactored to thin controller
? Program.cs                               - DI configuration
```

---

## ?? Implementation Details

### 1?? Generic Repository Pattern

**Interface: `IRepository<T>`**
```csharp
public interface IRepository<T> where T : class
{
    // Query
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    // Command
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    
    // Utility
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountAsync();
}
```

**Benefits:**
- ? DRY (Don't Repeat Yourself)
- ? Consistent CRUD operations
- ? Easy to extend

---

### 2?? Specialized Deck Repository

**Interface: `IDeckRepository : IRepository<Deck>`**
```csharp
public interface IDeckRepository : IRepository<Deck>
{
    // Domain-specific queries
    Task<Deck?> GetDeckWithFlashcardsAsync(Guid deckId);
    Task<Deck?> GetDeckWithDetailsAsync(Guid deckId);
    Task<IEnumerable<Deck>> GetDecksAccessibleByUserAsync(Guid userId, bool isAdmin);
    
    // Business queries
    Task<bool> IsUserOwnerAsync(Guid deckId, Guid userId);
    Task<bool> CanUserAccessAsync(Guid deckId, Guid userId, bool isAdmin);
}
```

**Key Features:**
- ? K? th?a t? Generic Repository
- ? Thêm queries ph?c t?p v?i Include/Join
- ? Encapsulate business queries

---

### 3?? Deck Service Layer

**Interface: `IDeckService`**
```csharp
public interface IDeckService
{
    // Query operations
    Task<IEnumerable<DeckResponse>> GetDecksForUserAsync(Guid userId, bool isAdmin);
    Task<DeckDetailResponse?> GetDeckByIdAsync(Guid deckId, Guid currentUserId, bool isAdmin);
    
    // Command operations
    Task<DeckResponse> CreateDeckAsync(CreateDeckRequest request, Guid userId);
    Task<bool> UpdateDeckAsync(Guid deckId, UpdateDeckRequest request, Guid userId, bool isAdmin);
    Task<bool> DeleteDeckAsync(Guid deckId, Guid userId, bool isAdmin);
    
    // Validation
    Task<bool> CanUserAccessDeckAsync(Guid deckId, Guid userId, bool isAdmin);
    Task<bool> CanUserModifyDeckAsync(Guid deckId, Guid userId, bool isAdmin);
}
```

**Responsibilities:**
- ? All business logic
- ? Validation & authorization
- ? Entity ? DTO mapping
- ? Orchestrate multiple repositories
- ? Exception handling

**Important Rules:**
- ? Service KHÔNG ???c g?i DbContext tr?c ti?p
- ? Service ch? g?i Repository
- ? Service tr? v? DTO, không tr? v? IActionResult

---

### 4?? Thin Controller

**DecksController (Refactored)**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DecksController : ControllerBase
{
    private readonly IDeckService _deckService;

    public DecksController(IDeckService deckService)
    {
        _deckService = deckService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeckResponse>>> GetDecks()
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();
        
        var decks = await _deckService.GetDecksForUserAsync(userId, isAdmin);
        
        return Ok(decks);
    }
    
    // ... more endpoints
}
```

**Characteristics:**
- ? Thin & clean
- ? Only inject Service (không inject Repository/DbContext)
- ? Only handle HTTP concerns
- ? Delegate all logic to Service

---

## ?? Dependency Injection (Program.cs)

```csharp
// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Specialized Repositories
builder.Services.AddScoped<IDeckRepository, DeckRepository>();
builder.Services.AddScoped<IFolderRepository, FolderRepository>();

// Services
builder.Services.AddScoped<IDeckService, DeckService>();
```

**Lifecycle: `AddScoped`**
- Created once per HTTP request
- Disposed at end of request
- Perfect for database operations

---

## ?? Comparison: Before vs After

### **BEFORE (Fat Controller):**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Deck>>> GetDecks()
{
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
    
    if (string.IsNullOrEmpty(currentUserId)) 
        return Unauthorized();
    
    var query = _context.Decks
        .Include(d => d.Flashcards)
        .AsQueryable();
    
    if (currentUserRole != UserRoles.Admin)
    {
        query = query.Where(d => 
            d.UserId.ToString() == currentUserId || 
            d.IsPublic == true);
    }
    
    return await query
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync();
}
```

**Problems:**
- ? Controller tr?c ti?p g?i DbContext
- ? Business logic trong Controller
- ? Khó test
- ? Khó tái s? d?ng logic

---

### **AFTER (Thin Controller + Service + Repository):**

**Controller:**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<DeckResponse>>> GetDecks()
{
    try
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();
        
        var decks = await _deckService.GetDecksForUserAsync(userId, isAdmin);
        
        return Ok(decks);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = ex.Message });
    }
}
```

**Service:**
```csharp
public async Task<IEnumerable<DeckResponse>> GetDecksForUserAsync(Guid userId, bool isAdmin)
{
    var decks = await _deckRepository.GetDecksAccessibleByUserAsync(userId, isAdmin);
    
    return decks.Select(deck => new DeckResponse
    {
        Id = deck.Id,
        Title = deck.Title,
        // ... mapping
    });
}
```

**Repository:**
```csharp
public async Task<IEnumerable<Deck>> GetDecksAccessibleByUserAsync(Guid userId, bool isAdmin)
{
    if (isAdmin)
        return await _dbSet.Include(d => d.Flashcards).ToListAsync();
    
    return await _dbSet
        .Include(d => d.Flashcards)
        .Where(d => d.UserId == userId || d.IsPublic)
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync();
}
```

**Benefits:**
- ? Separation of Concerns
- ? Easy to test (mock Service/Repository)
- ? Reusable business logic
- ? Clean code

---

## ?? Testing Benefits

### Unit Test Service
```csharp
public class DeckServiceTests
{
    private readonly Mock<IDeckRepository> _mockDeckRepo;
    private readonly Mock<IFolderRepository> _mockFolderRepo;
    private readonly DeckService _service;

    public DeckServiceTests()
    {
        _mockDeckRepo = new Mock<IDeckRepository>();
        _mockFolderRepo = new Mock<IFolderRepository>();
        _service = new DeckService(_mockDeckRepo.Object, _mockFolderRepo.Object);
    }

    [Fact]
    public async Task CreateDeck_WithInvalidFolder_ThrowsException()
    {
        // Arrange
        var request = new CreateDeckRequest { FolderId = Guid.NewGuid() };
        _mockFolderRepo.Setup(x => x.ExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateDeckAsync(request, Guid.NewGuid()));
    }
}
```

---

## ?? Next Steps - Refactor Other Controllers

### Recommended Order:

1. **FoldersController**
   - Create `IFolderService` + `FolderService`
   - Already have `IFolderRepository` + `FolderRepository`

2. **FlashcardsController**
   - Create `IFlashcardRepository` + `FlashcardRepository`
   - Create `IFlashcardService` + `FlashcardService`

3. **UsersController**
   - Create `IUserRepository` + `UserRepository`
   - Create `IUserService` + `UserService`

4. **AuthController**
   - Create `IAuthService` + `AuthService`
   - Use `IUserRepository`

5. **StudySessionsController**
   - Create `IStudySessionRepository` + `StudySessionRepository`
   - Create `IStudySessionService` + `StudySessionService`

---

## ?? Best Practices

### DO ?
- ? Keep controllers thin
- ? Put all business logic in Service
- ? Use DTOs for API responses
- ? Use async/await everywhere
- ? Handle exceptions in Service
- ? Use dependency injection
- ? Write unit tests for Services

### DON'T ?
- ? Call DbContext from Controller
- ? Call Repository from Controller
- ? Put business logic in Repository
- ? Return IActionResult from Service
- ? Use `new` keyword for dependencies
- ? Expose entities directly to API

---

## ?? Benefits Achieved

### Separation of Concerns
- ? Controller: HTTP handling
- ? Service: Business logic
- ? Repository: Data access

### Testability
- ? Mock repositories in Service tests
- ? Mock services in Controller tests
- ? No database needed for unit tests

### Maintainability
- ? Changes isolated to specific layers
- ? Easy to add new features
- ? Clear responsibilities

### Reusability
- ? Business logic can be reused
- ? Generic repository for all entities
- ? DI makes everything flexible

---

## ?? How to Use

### 1. Implement a new feature:
```
1. Create Repository (if needed)
2. Create Service with business logic
3. Update Controller to use Service
4. Register in Program.cs
```

### 2. Test a feature:
```
1. Mock Repository in Service test
2. Test business logic
3. Mock Service in Controller test (optional)
```

### 3. Debug:
```
1. Check Controller for HTTP errors
2. Check Service for business logic errors
3. Check Repository for database errors
```

---

## ?? Resources

- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Service Layer Pattern](https://martinfowler.com/eaaCatalog/serviceLayer.html)
- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)

---

## ? Summary

**Before:** Fat Controllers (all-in-one)
**After:** 3-Layer Architecture (clean & maintainable)

**Status:** ? Decks module fully refactored
**Next:** Refactor remaining controllers

**Ready for:** Testing & Production

---

*Last Updated: 2024-11-26*
*Version: 1.0*
*Author: Backend Architect*
