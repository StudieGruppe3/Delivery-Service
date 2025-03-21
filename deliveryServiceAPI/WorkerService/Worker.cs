using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using deliveryServiceAPI.Repository; // Sørg for, at namespace er korrekt

namespace deliveryServiceAPI.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly BookingRepository _repository;

        public Worker(ILogger<Worker> logger, BookingRepository repository)
        {
            _logger = logger;
            _repository = repository;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "shipment_queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started, waiting for messages...");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var booking = JsonSerializer.Deserialize<BookingDTO>(message);

                // Store to repository (if needed)
                _repository.Put(booking);
                _logger.LogInformation($"Received and stored booking: {message}");

                // Write to CSV
                string csvFilePath = "/app/data/DeliveryPlan-20240924.csv"; // Shared Docker volume path
                bool fileExists = File.Exists(csvFilePath);

                try
                {
                    using (var writer = new StreamWriter(csvFilePath, append: true))
                    {
                        if (!fileExists)
                        {
                            writer.WriteLine("OrderId,CustomerName,Address,Deadline");
                        }

                        string line = $"{booking.OrderId},{booking.CustomerName},{booking.Address},{booking.Deadline:yyyy-MM-dd HH:mm}";
                        writer.WriteLine(line);
                    }

                    _logger.LogInformation($"Booking written to CSV: {booking.OrderId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write booking to CSV");
                }
            };

            _channel.BasicConsume(queue: "shipment_queue",
                                  autoAck: true,
                                  consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Worker stopped");
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
