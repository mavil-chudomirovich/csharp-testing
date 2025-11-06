using API.Filters;
using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Invoice.Request;
using Application.Dtos.Momo.Request;
using Application.Dtos.Payment.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
namespace API.Controllers
{
    /// <summary>
    /// Manages invoice operations such as creation, payment, and retrieval.
    /// </summary>
    [Route("api/invoices")]
    [ApiController]
    public class InvoiceController(IMomoService momoService,
        IInvoiceService invoiceItemService,
        IMemoryCache cache,
        IUserService userService
            ) : ControllerBase
    {
        private readonly IMomoService _momoService = momoService;
        private readonly IInvoiceService _invoiceService = invoiceItemService;
        private readonly IMemoryCache _cache = cache;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Receives and verifies the MoMo payment callback (IPN) to update invoice status.
        /// </summary>
        /// <param name="req">MoMo IPN request containing payment and order information.</param>
        /// <returns>Result message indicating whether the callback was processed successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Invalid signature.</response>
        /// <response code="404">Invoice not found.</response>
        [AllowAnonymous]
        [HttpPost("payment-callback/momo")]
        public async Task<IActionResult> UpdateInvoiceMomoPayment([FromBody] MomoIpnReq req)
        {
            await _momoService.VerifyMomoIpnReq(req);
            var lastDashIndex = req.OrderId.LastIndexOf('-');
            await _invoiceService.UpdateInvoiceMomoPayment(req, Guid.Parse(req.OrderId.Substring(0, lastDashIndex)));
            return Ok(new { resultCode = 0, message = "Received" });
        }

        /// <summary>
        /// Retrieves an invoice by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the invoice.</param>
        /// <returns>Invoice details if found.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Invoice not found.</response>

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(Guid id)
        {
            var invoiceView = await _invoiceService.GetInvoiceById(id, true, true);
            return Ok(invoiceView);
        }

        /// <summary>
        /// Processes payment for the specified invoice based on its type and payment method.
        /// </summary>
        /// <param name="id">The unique identifier of the invoice.</param>
        /// <param name="paymentReq">Payment request containing payment method, amount, and fallback URL.</param>
        /// <returns>Success message or payment link depending on the payment method.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Invoice not found.</response>
        /// <response code="500">Invoice type error.</response>

        [HttpPut("{id}/payment")]
        [RoleAuthorize(RoleName.Staff, RoleName.Customer)]
        public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] PaymentReq paymentReq)
        {
            //kiểm tra phải hoá đơn của nó không
            var userId = Guid.Parse(HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sid)!.Value.ToString());
            var invoice = await _invoiceService.GetRawInvoiceById(id, true, true);
            var roles = _cache.Get<List<Domain.Entities.Role>>(Common.SystemCache.AllRoles)
                ?? throw new NotFoundException(Message.RoleMessage.NotFound);
            var userInDB = await _userService.GetByIdAsync(userId)
                ?? throw new NotFoundException(Message.UserMessage.NotFound);
            var userRole = roles.FirstOrDefault(r => r.Id == userInDB.Role!.Id)!.Name;
            if (userRole == RoleName.Customer)
            {
                if (invoice.Contract.CustomerId != userId)
                    throw new BusinessException(Message.InvoiceMessage.ForbiddenInvoiceAccess);
            }

            if (invoice.Status == (int)InvoiceStatus.Cancelled || invoice.Status == (int)InvoiceStatus.Paid) 
                throw new BusinessException(Message.InvoiceMessage.ThisInvoiceWasPaidOrCancel);
            if(paymentReq.PaymentMethod == (int)PaymentMethod.Cash)
            {
                if (paymentReq.Amount == null) throw new BadRequestException(Message.InvoiceMessage.AmountRequired);
                
                switch (invoice.Type)
                {
                    case (int)InvoiceType.Handover:
                        await _invoiceService.PayHandoverInvoiceManual(invoice, (decimal)paymentReq.Amount);
                        break;
                    case (int)InvoiceType.Return:
                        await _invoiceService.PayReturnInvoiceManual(invoice, (decimal)paymentReq.Amount);
                        break;
                    case (int)InvoiceType.Reservation:
                        await _invoiceService.PayReservationInvoiceManual(invoice, (decimal)paymentReq.Amount);
                        break;
                    case (int)InvoiceType.Refund:
                        await _invoiceService.PayRefundInvoiceManual(invoice, (decimal)paymentReq.Amount);
                        break;
                    default:
                        throw new Exception(Message.InvoiceMessage.InvalidInvoiceType);
                       
                }

                return Ok();
            }
            string link = invoice.Type switch
            {
                (int)InvoiceType.Handover => await _invoiceService.PayHandoverInvoiceOnline(invoice, paymentReq.FallbackUrl),
                (int)InvoiceType.Reservation => await _invoiceService.PayReservationInvoiceOnline(invoice, paymentReq.FallbackUrl),
                (int)InvoiceType.Return => await _invoiceService.PayReturnInvoiceOnline(invoice, paymentReq.FallbackUrl),
                //(int)InvoiceType.Refund => await _invoiceService.PayRefundInvoiceOnline(invoice, paymentReq.FallbackUrl),
                _ => throw new Exception(Message.InvoiceMessage.InvalidInvoiceType),
            };
            return Ok(new { link });
        }

        /// <summary>
        /// Retrieves all invoices with pagination support.
        /// </summary>
        /// <param name="pagination">Pagination parameters for page number and page size.</param>
        /// <returns>List of invoices with pagination metadata.</returns>
        /// <response code="200">Success.</response>

        [RoleAuthorize(RoleName.Staff)]
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices([FromQuery] PaginationParams pagination)
        {
            var result = await _invoiceService.GetAllInvoicesAsync(pagination);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new invoice with the provided information.
        /// </summary>
        /// <param name="req">Request containing invoice details such as contract, amount, and type.</param>
        /// <returns>Information about the created invoice.</returns>
        /// <response code="201">Invoice created successfully.</response>
        /// <response code="400">Invalid invoice data.</response>
        /// <response code="404">Related entity not found (e.g., contract or customer).</response>
        [RoleAuthorize(RoleName.Staff)]
        [HttpPost()]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceReq req)
        {
            await _invoiceService.CreateAsync(req);
            return Created();
        }


        /// <summary>
        /// Updates an existing invoice with new information.
        /// </summary>
        /// <param name="id">The unique identifier of the invoice to update.</param>
        /// <param name="req">Request containing updated invoice details.</param>
        /// <returns>Success message if the invoice is updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Invoice not found.</response>
        /// <response code="400">Invalid invoice data.</response>
        [RoleAuthorize(RoleName.Staff)]
        [HttpPut("{id}")]
        public async Task<IActionResult>UpdateInvoice(Guid id, UpdateInvoiceReq req)
        {
            await _invoiceService.UpdateAsync(id, req);
            return Ok();
        }


        /// <summary>
        /// Updates the notes field of a specific invoice.
        /// </summary>
        /// <param name="id">The unique identifier of the invoice to update.</param>
        /// <param name="notes">The new notes content to be saved.</param>
        /// <returns>Success message if the notes are updated successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="404">Invoice not found.</response>
        /// <response code="400">Invalid notes data.</response>
        [HttpPut("{id}/notes")]
        public async Task<IActionResult>UpdateInvoiceNotes(Guid id, string notes)
        {
            await _invoiceService.UpdateNoteAsync(id, notes);
            return Ok();
        }

        /// <summary>
        /// Uploads the main image for a specific invoice.
        /// </summary>
        /// <param name="id">The unique identifier of the invoice to attach the image to.</param>
        /// <param name="file">The image file to be uploaded (multipart/form-data).</param>
        /// <returns>Returns the uploaded image URL and invoice ID if successful.</returns>
        /// <response code="200">Image uploaded successfully.</response>
        /// <response code="400">Invalid file or request data.</response>
        /// <response code="404">Invoice not found.</response>
        [HttpPut("{id}/image")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadMainImage([FromRoute] Guid id, [FromForm(Name = "file")] IFormFile file)
        {
            var imageUrl = await _invoiceService.UploadImageAsync(id, file);
            return Ok(new { data = new { id, imageUrl }, message = Message.CloudinaryMessage.UploadSuccess });
        }


        /// <summary>
        /// Deletes the main image of a specific invoice model.
        /// </summary>
        /// <param name="id">The unique identifier of the model whose image will be deleted.</param>
        /// <returns>Success message if the image is deleted successfully.</returns>
        /// <response code="200">Image deleted successfully.</response>
        /// <response code="404">Image or model not found.</response>
        [HttpDelete("{id}/image")]
        public async Task<IActionResult> DeleteMainImage([FromRoute] Guid id)
        {
            await _invoiceService.DeleteImageAsync(id);
            return Ok(new { message = Message.CloudinaryMessage.DeleteSuccess });
        }
    }
}