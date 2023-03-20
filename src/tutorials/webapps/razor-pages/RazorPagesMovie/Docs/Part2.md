# Add a model to a Razor Pages app in ASP.NET Core

In this tutorial, classes are added for managing movies in a database. The app's model classes use Entity Framework Core (EF Core) to work with the database. EF Core is an object-relational mapper (O/RM) that simplifies data access. You write the model classes first, and EF Core creates the database.

The model classes are known as POCO classes (from "Plain-Old CLR Objects") because they don't have a dependency on EF Core. They define the properties of the data that are stored in the database.

Add a data model

Add a folder named Models.
Add a class to the Models folder named Movie.cs.
Add the following properties to the Movie class:

```C#

Copy
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public class Movie
{
    public int Id { get; set; }
    public string? Title { get; set; }
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public decimal Price { get; set; }
}
```

The Movie class contains:

- An ID field to provide a primary key for the database.

- A [DataType] attribute to specify the type of data in the ReleaseDate field. With this attribute:

    - The user is not required to enter time information in the date field.
    - Only the date is displayed, not time information.

- The question mark after string indicates that the property is nullable. For more information, see Nullable reference types.


### Add NuGet packages and EF tools
Run the following .NET CLI commands:

```bash
dotnet tool uninstall --global dotnet-aspnet-codegenerator
dotnet tool install --global dotnet-aspnet-codegenerator
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SQLite
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

nb: `To scaffold controllers and views using models, install Entity Framework core packages and try again: Microsoft.EntityFrameworkCore.Tools`
The preceding commands add:

- The command-line interface (CLI) tools for EF Core
- The aspnet-codegenerator scaffolding tool.
- Design time tools for EF Core
- The EF Core SQLite provider, which installs the EF Core package as a dependency.
- Packages needed for scaffolding: Microsoft.`VisualStudio.Web.CodeGeneration.Design` and `Microsoft.EntityFrameworkCore.SqlServer`.  

For guidance on multiple environment configuration that permits an app to configure its database contexts by environment, see [Use multiple environments in ASP.NET Core.](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-7.0#environment-based-startup-class-and-methods)



[DataAnnotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) are covered in a later tutorial.

Build the project to verify there are no compilation errors.

### Scaffold the movie model
In this section, the movie model is scaffolded. That is, the scaffolding tool produces pages for Create, Read, Update, and Delete (CRUD) operations for the movie model.

- Open a command shell to the project directory, which contains the Program.cs and .csproj files. Run the following command:

```bash

dotnet aspnet-codegenerator razorpage -m Movie -dc RazorPagesMovie.Data.RazorPagesMovieContext -udl -outDir Pages/Movies --referenceScriptLibraries -sqlite

```

The following table details the ASP.NET Core code generator options.

Option	Description
-m	The name of the model.
-dc	The DbContext class to use including namespace.
-udl	Use the default layout.
-outDir	The relative output folder path to create the views.
--referenceScriptLibraries	Adds _ValidationScriptsPartial to Edit and Create pages
Use the -h option to get help on the dotnet aspnet-codegenerator razorpage command:

.NET CLI

Copy
dotnet aspnet-codegenerator razorpage -h
For more information, see dotnet aspnet-codegenerator.

Use SQLite for development, SQL Server for production
When SQLite is selected, the template generated code is ready for development. The following code shows how to select the SQLite connection string in development and SQL Server in production.

C#

Copy
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("RazorPagesMovieContext")));
}
else
{
    builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionMovieContext")));
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
The preceding code doesn't call UseDeveloperExceptionPage in development because WebApplication calls UseDeveloperExceptionPage in development mode.

Files created and updated
The scaffold process creates the following files:

Pages/Movies: Create, Delete, Details, Edit, and Index.
Data/RazorPagesMovieContext.cs
The created files are explained in the next tutorial.

The scaffold process adds the following highlighted code to the Program.cs file:

Visual Studio
Visual Studio Code / Visual Studio for Mac
C#

Copy
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("RazorPagesMovieContext") ?? throw new InvalidOperationException("Connection string 'RazorPagesMovieContext' not found.")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
The Program.cs changes are explained later in this tutorial.


Create the initial database schema using EF's migration feature
The migrations feature in Entity Framework Core provides a way to:

Create the initial database schema.
Incrementally update the database schema to keep it in sync with the app's data model. Existing data in the database is preserved.
Visual Studio
Visual Studio Code
Visual Studio for Mac
Right-click the RazorPagesMovie.csproj project, and then select Open in Integrated Terminal.

The Terminal window opens with the command prompt at the project directory, which contains the Program.cs and .csproj files.

Run the following .NET CLI commands:

.NET CLI

Copy
dotnet ef migrations add InitialCreate
dotnet ef database update
The migrations command generates code to create the initial database schema. The schema is based on the model specified in DbContext. The InitialCreate argument is used to name the migrations. Any name can be used, but by convention a name is selected that describes the migration.

The update command runs the Up method in migrations that have not been applied. In this case, update runs the Up method in the Migrations/<time-stamp>_InitialCreate.cs file, which creates the database.

 Note

For SQLite, column type for the Price field is set to TEXT. This is resolved in a later step.

The following warning is displayed, which is addressed in a later step:

No type was specified for the decimal column 'Price' on entity type 'Movie'. This will cause values to be silently truncated if they do not fit in the default precision and scale. Explicitly specify the SQL server column type that can accommodate all the values using 'HasColumnType()'.

Examine the context registered with dependency injection
ASP.NET Core is built with dependency injection. Services, such as the EF Core database context, are registered with dependency injection during application startup. Components that require these services (such as Razor Pages) are provided via constructor parameters. The constructor code that gets a database context instance is shown later in the tutorial.

The scaffolding tool automatically created a database context and registered it with the dependency injection container. The following highlighted code is added to the Program.cs file by the scaffolder:

Visual Studio
Visual Studio Code / Visual Studio for Mac
C#

Copy
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("RazorPagesMovieContext") ?? throw new InvalidOperationException("Connection string 'RazorPagesMovieContext' not found.")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
The data context RazorPagesMovieContext:

Derives from Microsoft.EntityFrameworkCore.DbContext.
Specifies which entities are included in the data model.
Coordinates EF Core functionality, such as Create, Read, Update and Delete, for the Movie model.
C#

Copy
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Data
{
    public class RazorPagesMovieContext : DbContext
    {
        public RazorPagesMovieContext (DbContextOptions<RazorPagesMovieContext> options)
            : base(options)
        {
        }

        public DbSet<RazorPagesMovie.Models.Movie> Movie { get; set; } = default!;
    }
}
The preceding code creates a DbSet<Movie> property for the entity set. In Entity Framework terminology, an entity set typically corresponds to a database table. An entity corresponds to a row in the table.

The name of the connection string is passed in to the context by calling a method on a DbContextOptions object. For local development, the Configuration system reads the connection string from the appsettings.json file.


Test the app
Run the app and append /Movies to the URL in the browser (http://localhost:port/movies).

If you receive the following error:

Console

Copy
SqlException: Cannot open database "RazorPagesMovieContext-GUID" requested by the login. The login failed.
Login failed for user 'User-name'.
You missed the migrations step.

Test the Create New link.

Create page

 Note

You may not be able to enter decimal commas in the Price field. To support jQuery validation for non-English locales that use a comma (",") for a decimal point and for non US-English date formats, the app must be globalized. For globalization instructions, see this GitHub issue.

Test the Edit, Details, and Delete links.

The next tutorial explains the files created by scaffolding.