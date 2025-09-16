using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Biblioteca API",
        Version = "v1",
        Description = "API para gerenciamento de uma biblioteca com livros, autores, gêneros, estudantes e empréstimos",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Clenildon Ferreira",
            Email = "clenildonferreira34@gmail.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Banco de dados deu bom!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao migrar o banco de dados: {ex.Message}");
        }
    }
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();