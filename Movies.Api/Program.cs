using Movies.Application;
using Movies.Application.Database;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    var config = builder.Configuration;
    
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    builder.Services.AddApplication();
    builder.Services.AddDatabase(config.GetConnectionString("Default"));
}

WebApplication app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    DbInitializer dbInitializer = app.Services.GetRequiredService<DbInitializer>();
    await dbInitializer.InitalizeAsync();
    
    app.Run();
}