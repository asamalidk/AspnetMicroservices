
using Discount.API.Repositories;
using Npgsql;

namespace Discount.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            MigrateDb(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

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