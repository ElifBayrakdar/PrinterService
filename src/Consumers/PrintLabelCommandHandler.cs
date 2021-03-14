using System;
using System.Threading.Tasks;
using BoxService;
using MassTransit;

namespace PrinterService.Consumers
{
    public class PrintLabelCommandHandler:IConsumer<PrintLabelCommand>
    {
        public async Task Consume(ConsumeContext<PrintLabelCommand> context)
        {
            await Task.Delay(5000);
            var message = context.Message;
            Console.WriteLine($"Çıktınız alınıyor... Barkodunuz: {message.Label}");
        }
    }
}