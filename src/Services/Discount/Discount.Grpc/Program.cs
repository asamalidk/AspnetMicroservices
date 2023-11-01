using Discount.Grpc.Repositories;
using Discount.Grpc.Services;
using Npgsql;

namespace Discount.Grpc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
            builder.Services.AddAutoMapper(typeof(Program));

            MigrateDb(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<DiscountService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

            app.Run();
        }

        private static void MigrateDb(WebApplicationBuilder builder)
        {
            for (int i = 0; i < 10; i++)
            {
                // Migrate database changes on startup (includes initial db creation)
                using (var scope = builder.Services.BuildServiceProvider().CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var configuration = services.GetRequiredService<IConfiguration>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    try
                    {
                        logger.LogInformation("Migrating postgresql database.");
                        var connectionString = configuration.GetValue<string>("DatabaseSettings:ConnectionString");
                        using var connection = new NpgsqlConnection(connectionString);
                        connection.Open();

                        using var command = new NpgsqlCommand
                        {
                            Connection = connection
                        };

                        command.CommandText = "DROP TABLE IF EXISTS Coupon";
                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY,
                                                                ProductName VARCHAR(24) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
                        command.ExecuteNonQuery();
                        command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
                        command.ExecuteNonQuery();

                        logger.LogInformation("Migrated postgresql database.");
                        break;

                    }
                    catch (NpgsqlException)
                    {
                        logger.LogError("An error occurred while migrating the postgresql database");
                        Thread.Sleep(2000);

                    }
                }
            }
        }
    }
}