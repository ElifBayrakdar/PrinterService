using System;
using System.Threading.Tasks;
using BoxService;
using MassTransit;

namespace PrinterService.Consumers
{
    public class PrintLabelCommandHandler:IConsumer<PrintLabelCommand>
    {
        public Task Consume(ConsumeContext<PrintLabelCommand> context)
        {
            var message = context.Message;
            Console.WriteLine($"Çıktınız alınıyor... Barkodunuz: {message.Label}");
            
            return Task.CompletedTask;
        }
    }
}