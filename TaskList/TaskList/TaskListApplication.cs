Console.WriteLine("Do you want to work with Command Prompt \"cmd\" or the web API \"api\"?");
string input;
while (true)
{
    input = Console.ReadLine();
    if (input == "cmd" || input == "api")
    {
        break;
    }
    Console.WriteLine("Invalid input. Please enter either \"cmd\" or \"api\".");
}

if (input == "cmd")
{
    TaskList.TaskList.Main(args);
}
else
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddMemoryCache();
    builder.Services.AddControllers(); // Register controllers

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers(); // Map controllers

    app.Run();
}