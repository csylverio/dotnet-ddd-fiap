using FiapEcommerce.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddDomainRepositories()
    .AddOrderDomainServices()
    .AddPaymentInfrastructure()
    .AddBehavioralPatterns();

var app = builder.Build();
app.UseDomainEventSubscribers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
