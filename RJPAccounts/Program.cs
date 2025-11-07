using MassTransit;
using RJPAccounts.Services;
using RJPProject.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<AccountsTblSettings>(
    builder.Configuration.GetSection("AccountsTbl"));
builder.Services.Configure<TransactionsTblSettings>(
    builder.Configuration.GetSection("TransactionsTbl"));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AccountsService>();
builder.Services.AddScoped<TransactionsServices>();

//MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NewAccountConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("get-account-queue", e =>
        {
            e.ConfigureConsumer<NewAccountConsumer>(context);
        });
    });
});

//CORS to communicate with API from Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyMethod()
               .AllowCredentials()
               .SetIsOriginAllowed(host => true)
               .AllowAnyHeader());
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
