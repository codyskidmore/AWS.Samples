using Amazon.SQS;
using Sqs.Consumer.Client;

var builder = WebApplication.CreateBuilder(args);

// See https://github.com/codyskidmore/OptionsExample
builder.Services.Configure<QueueOptions>(
    builder.Configuration.GetSection(nameof(QueueOptions)));

builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddHostedService<SqsConsumerService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();

return;