using Domain.Commons;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DispatchRequest : SorfDeletedEntity, IEntity
{
    public Guid Id { get; set; }

    public string? Description { get; set; }

    public string? FinalDescription { get; set; }

    public int Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid RequestAdminId { get; set; }

    public Guid? ApprovedAdminId { get; set; }

    public Guid? FromStationId { get; set; }

    public Guid ToStationId { get; set; }

    public virtual Staff? ApprovedAdmin { get; set; }

    public virtual ICollection<DispatchRequestStaff> DispatchRequestStaffs { get; set; } = new List<DispatchRequestStaff>();

    public virtual ICollection<DispatchRequestVehicle> DispatchRequestVehicles { get; set; } = new List<DispatchRequestVehicle>();

    public virtual Station? FromStation { get; set; }

    public virtual Staff RequestAdmin { get; set; } = null!;

    public virtual Station ToStation { get; set; } = null!;
}