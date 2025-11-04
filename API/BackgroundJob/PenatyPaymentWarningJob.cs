using Application.Abstractions;
using Quartz;

namespace API.BackgroundJob
{
    public class PenatyPaymentWarningJob : IJob
    {
        private readonly IInvoiceService _service;
        private readonly ILogger<PenatyPaymentWarningJob> _logger;
        public PenatyPaymentWarningJob(IInvoiceService service, ILogger<PenatyPaymentWarningJob> logger)
        {
            _service = service;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Quartz penaty refund invoice warning job started...");
            await _service.WarningRefundInvoiceAsync();
        }
    }
}
