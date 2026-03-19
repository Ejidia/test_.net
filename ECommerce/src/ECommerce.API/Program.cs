using ECommerce.Core.Models;
using ECommerce.Core.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sample users data
var users = new List<User>
{
    new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe", Role = UserRole.Customer },
    new User { Id = 2, Email = "jane@example.com", FirstName = "Jane", LastName = "Smith", Role = UserRole.Admin },
    new User { Id = 3, Email = "bob@example.com", FirstName = "Bob", LastName = "Johnson", Role = UserRole.Customer }
};

// GET /users - Get all users
app.MapGet("/users", () => users)
.WithName("GetUsers")
.WithOpenApi();

// GET /users/{id} - Get user by ID
app.MapGet("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
})
.WithName("GetUser")
.WithOpenApi();

// POST /users - Create new user
app.MapPost("/users", (User user) =>
{
    user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
    user.CreatedAt = DateTime.UtcNow;
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser")
.WithOpenApi();

// PUT /users/{id} - Update user
app.MapPut("/users/{id}", (int id, User updatedUser) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();
    
    user.Email = updatedUser.Email;
    user.FirstName = updatedUser.FirstName;
    user.LastName = updatedUser.LastName;
    user.PhoneNumber = updatedUser.PhoneNumber;
    user.Role = updatedUser.Role;
    user.IsActive = updatedUser.IsActive;
    user.UpdatedAt = DateTime.UtcNow;
    
    return Results.Ok(user);
})
.WithName("UpdateUser")
.WithOpenApi();

// DELETE /users/{id} - Delete user
app.MapDelete("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();
    
    users.Remove(user);
    return Results.NoContent();
})
.WithName("DeleteUser")
.WithOpenApi();

app.Run();
