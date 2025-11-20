using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.Ticket.Request;
using Application.Dtos.Ticket.Response;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;

namespace Application
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public TicketService(ITicketRepository repo, IUserRepository userRepo,
            IMapper mapper)
        {
            _repo = repo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<PageResult<TicketRes>> GetAllAsync(Guid staffId,
            TicketFilterParams filter, PaginationParams pagination)
        {
            var staff = await _userRepo.GetByIdWithFullInfoAsync(staffId)
                ?? throw new NotFoundException(Message.UserMessage.NotFound);

            // check if request has role name Admin or not
            if (staff.Role.Name.Equals(RoleName.Staff) && filter.Type == (int)TicketType.StaffReport)
                throw new ForbidenException(Message.UserMessage.DoNotHavePermission);

            var page = await _repo.GetAllAsync(filter, pagination);
            var data = _mapper.Map<IEnumerable<TicketRes>>(page.Items);

            return new PageResult<TicketRes>(data, page.PageNumber, page.PageSize, page.Total);
        }

        // ===============
        // for customer
        // ===============

        #region customer

        public async Task<Guid> CreateContactAsync(CreateContactReq req)
        {
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Title = req.Title,
                Description = req.Description,
                Type = (int)TicketType.Contact,
                Status = (int)TicketStatus.Pending
            };

            await _repo.AddAsync(ticket);
            return ticket.Id;
        }

        public async Task<Guid> CreateAsync(Guid? requesterId, CreateTicketReq req)
        {
            var requester = requesterId.HasValue
                ? await _userRepo.GetByIdWithFullInfoAsync(requesterId.Value)
                    ?? throw new NotFoundException(Message.UserMessage.NotFound)
                : null;

            if (requester != null && requester.Role.Name.Equals(RoleName.Admin))
                throw new ForbidenException(Message.UserMessage.DoNotHavePermission);

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Title = req.Title,
                Description = req.Description,
                Type = req.Type,
                Status = (int)TicketStatus.Pending,
                StationId = (requester != null && requester.Staff != null)
                    ? requester.Staff.StationId : null,
                RequesterId = requesterId
            };

            await _repo.AddAsync(ticket);
            return ticket.Id;
        }

        #endregion customer

        // ===============
        // for staff
        // ===============

        #region managerment

        public async Task UpdateAsync(Guid id, UpdateTicketReq req, Guid staffId)
        {
            var ticket = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(Message.TicketMessage.NotFound);

            if (ticket.Status == (int)TicketStatus.Resolved)
                throw new BusinessException(Message.TicketMessage.AlreadyResolved);

            var staff = await _userRepo.GetByIdWithFullInfoAsync(staffId)
                ?? throw new NotFoundException(Message.UserMessage.NotFound);

            switch (staff.Role.Name)
            {
                case RoleName.Admin:
                    {
                        if (ticket.Type == (int)TicketType.CustomerSupport && ticket.Status != (int)TicketStatus.EscalatedToAdmin)
                        {
                            throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
                        }
                        break;
                    }
                case RoleName.Staff:
                    {
                        if (ticket.Type == (int)TicketType.StaffReport
                            || (ticket.Type == (int)TicketType.CustomerSupport && ticket.Status == (int)TicketStatus.EscalatedToAdmin))
                        {
                            throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
                        }
                        break;
                    }
            }

            if (req.Reply is not null)
                ticket.Reply = req.Reply;

            if (req.Status.HasValue)
                ticket.Status = req.Status.Value;

            ticket.AssigneeId = staffId;
            ticket.UpdatedAt = DateTimeOffset.UtcNow;

            await _repo.UpdateAsync(ticket);
        }

        public async Task EscalateToAdminAsync(Guid id)
        {
            var ticket = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(Message.TicketMessage.NotFound);

            if (ticket.Status == (int)TicketStatus.EscalatedToAdmin)
                throw new InvalidOperationException(Message.TicketMessage.AlreadyEscalated);

            ticket.Status = (int)TicketStatus.EscalatedToAdmin;
            await _repo.UpdateAsync(ticket);
        }

        public async Task<PageResult<TicketRes>> GetByCustomerAsync(Guid customerId, int? status, PaginationParams pagination)
        {
            var page = await _repo.GetByCustomerAsync(customerId, status, pagination);
            var data = _mapper.Map<IEnumerable<TicketRes>>(page.Items);
            return new PageResult<TicketRes>(data, page.PageNumber, page.PageSize, page.Total);
        }

        public async Task<PageResult<TicketRes>> GetEscalatedTicketsAsync(PaginationParams pagination)
        {
            var page = await _repo.GetEscalatedAsync(pagination);
            var data = _mapper.Map<IEnumerable<TicketRes>>(page.Items);
            return new PageResult<TicketRes>(data, page.PageNumber, page.PageSize, page.Total);
        }

        #endregion managerment
    }
}