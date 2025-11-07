-- Database
USE master
IF DB_ID('green_wheel_db') IS NOT NULL
    DROP DATABASE green_wheel_db;
GO
CREATE DATABASE green_wheel_db
GO
USE green_wheel_db
GO
SET QUOTED_IDENTIFIER ON;
GO

-- Table

CREATE TABLE [roles] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [name] nvarchar(20) NOT NULL,
    [description] nvarchar(100) NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset
)
GO

CREATE TABLE [stations] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [name] nvarchar(100) NOT NULL,
    [address] nvarchar(255) NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset
)
GO

CREATE TABLE [users] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [first_name] nvarchar(50) NOT NULL,
    [last_name] nvarchar(50) NOT NULL,
    [email] varchar(255),
    [password] nvarchar(255),
    [phone] varchar(15),

    [sex] int, -- Male, Female
    [date_of_birth] datetimeoffset,
    
    [avatar_url] nvarchar(500),
    [avatar_public_id] nvarchar(255),
	[is_google_linked] bit NOT NULL DEFAULT 0,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,
    
    [role_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_users_roles FOREIGN KEY ([role_id]) REFERENCES [roles]([id])
)
GO
CREATE INDEX idx_users_role_id ON users (role_id);
GO

CREATE TABLE [staffs] (
    [user_id] uniqueidentifier PRIMARY KEY,
    [deleted_at] datetimeoffset,
    
    [station_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_staff_users FOREIGN KEY ([user_id]) REFERENCES [users]([id]),
    CONSTRAINT fk_staff_stations FOREIGN KEY ([station_id]) REFERENCES [stations]([id])
)
GO
CREATE INDEX idx_staffs_station_id ON staffs (station_id);
GO

CREATE TABLE [tickets] (
    [id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [title] nvarchar(255) NOT NULL,
    [description] nvarchar(max) NOT NULL,
    [reply] nvarchar(max),
    [status] INT NOT NULL DEFAULT 0,
    [type] INT NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [station_id] uniqueidentifier,
    [requester_id] uniqueidentifier,
    [assignee_id] uniqueidentifier,

    CONSTRAINT fk_tickets_stations FOREIGN KEY ([station_id]) REFERENCES [stations]([id]),
    CONSTRAINT fk_tickets_users FOREIGN KEY ([requester_id]) REFERENCES [users]([id]),
    CONSTRAINT fk_tickets_staffs FOREIGN KEY ([assignee_id]) REFERENCES [staffs]([user_id])
);
GO
CREATE INDEX idx_tickets_station_id ON tickets (station_id);
CREATE INDEX idx_tickets_requester_id ON tickets (requester_id);
CREATE INDEX idx_tickets_assignee_id ON tickets (assignee_id);
GO

CREATE TABLE [refresh_tokens] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [token] varchar(max) NOT NULL,
    [issued_at] datetimeoffset NOT NULL,
    [expires_at] datetimeoffset NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [is_revoked] bit NOT NULL DEFAULT (0),
    
    [user_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_refresh_users FOREIGN KEY ([user_id]) REFERENCES [users]([id])
)
GO
CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens (user_id);
GO

CREATE TABLE [driver_licenses] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [number] nvarchar(20) NOT NULL,
    [class] int NOT NULL, -- 0: B1, 1: B, 2: C1, 3: C, 4: D1, 5: D2, 6: D, 7: BE, 8: C1E, 9: CE, 10: D1E, 11: D2E, 12: DE
    [full_name] nvarchar(100) NOT NULL,
    [nationality] nvarchar(50) NOT NULL,
    [sex] int NOT NULL DEFAULT 0, -- Male, Female
    [date_of_birth] datetimeoffset NOT NULL,
    [expires_at] datetimeoffset NOT NULL,
    [front_image_url] nvarchar(500) NOT NULL,
    [front_image_public_id] nvarchar(255) NOT NULL,
	[back_image_url] nvarchar(500) NOT NULL,
    [back_image_public_id] nvarchar(255) NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,
    
    [user_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_driver_users FOREIGN KEY ([user_id]) REFERENCES [users]([id])
)
GO
CREATE INDEX idx_driver_licenses_user_id ON driver_licenses (user_id);
GO

CREATE TABLE [citizen_identities] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [number] nvarchar(20) NOT NULL,
    [full_name] nvarchar(100) NOT NULL,
    [nationality] nvarchar(50) NOT NULL,
    [sex] int NOT NULL DEFAULT 0, -- Male, Female
    [date_of_birth] datetimeoffset NOT NULL,
    [expires_at] datetimeoffset NOT NULL,
    [front_image_url] nvarchar(500) NOT NULL,
    [front_image_public_id] nvarchar(255) NOT NULL,
	[back_image_url] nvarchar(500) NOT NULL,
    [back_image_public_id] nvarchar(255) NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,
    
    [user_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_citizen_users FOREIGN KEY ([user_id]) REFERENCES [users]([id])
)
GO
CREATE INDEX idx_citizen_identities_user_id ON citizen_identities (user_id);
GO

CREATE TABLE [station_feedbacks] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [content] nvarchar(max),
    [rating] int NOT NULL,
    
    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [customer_id] uniqueidentifier NOT NULL,
    [station_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_feedback_users FOREIGN KEY ([customer_id]) REFERENCES [users]([id]),
    CONSTRAINT fk_feedback_stations FOREIGN KEY ([station_id]) REFERENCES [stations]([id])
)
GO
CREATE INDEX idx_station_feedbacks_customer_id ON station_feedbacks (customer_id);
CREATE INDEX idx_station_feedbacks_station_id ON station_feedbacks (station_id);
GO

CREATE TABLE [brands] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [name] nvarchar(50) NOT NULL,
    [description] nvarchar(255) NOT NULL,
    [country] nvarchar(50) NOT NULL,
    [founded_year] int NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset
)
GO

CREATE TABLE [vehicle_segments] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [name] nvarchar(50) NOT NULL,
    [description] nvarchar(255) NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset
)
GO

CREATE TABLE [vehicle_models] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [name] nvarchar(100) NOT NULL,
    [description] nvarchar(255) NOT NULL,
    [cost_per_day] decimal(10,2) NOT NULL,
    [deposit_fee] decimal(10, 2) NOT NULL,
    [reservation_fee] decimal(10, 2) NOT NULL,
    [seating_capacity] int NOT NULL,
    [number_of_airbags] int NOT NULL,
    [motor_power] decimal(5,1) NOT NULL,
    [battery_capacity] decimal(6,2) NOT NULL,
    [eco_range_km] decimal(6,1) NOT NULL,
    [sport_range_km] decimal(6,1) NOT NULL,
	[image_url] nvarchar(500),
    [image_public_id] nvarchar(255),

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,
    
    [brand_id] uniqueidentifier NOT NULL,
    [segment_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_model_brands FOREIGN KEY ([brand_id]) REFERENCES [brands]([id]),
    CONSTRAINT fk_model_segments FOREIGN KEY ([segment_id]) REFERENCES [vehicle_segments]([id])
)
GO
CREATE INDEX idx_vehicle_models_brand_id ON vehicle_models (brand_id);
CREATE INDEX idx_vehicle_models__segment_id ON vehicle_models (segment_id);
GO

CREATE TABLE [model_images] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [url] nvarchar(500) NOT NULL UNIQUE,
    [public_id] nvarchar(255) NOT NULL,

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [model_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_model_images_vehicle_models FOREIGN KEY ([model_id]) REFERENCES [vehicle_models]([id])
);
GO
CREATE INDEX idx_model_images_model_id ON model_images (model_id);
GO

CREATE TABLE [vehicle_components] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [name] nvarchar(100) NOT NULL,
    [description] nvarchar(255) NOT NULL,
    [damage_fee] decimal(18, 2) NOT NULL,
    
    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset
);
GO

CREATE TABLE [model_components] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [model_id] uniqueidentifier NOT NULL,
    [component_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_model_components_vehicle_models FOREIGN KEY ([model_id]) REFERENCES [vehicle_models]([id]),
    CONSTRAINT fk_model_components_vehicle_components FOREIGN KEY ([component_id]) REFERENCES [vehicle_components]([id])
);
GO
CREATE INDEX idx_model_components_model_id ON model_components (model_id);
CREATE INDEX idx_model_components_component_id ON model_components (component_id);
GO

CREATE TABLE [vehicles] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [license_plate] nvarchar(15) NOT NULL UNIQUE,
    [status] int NOT NULL DEFAULT 0, -- 0: Available, 1: Unavailable, 2: Pending

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,
    
    [model_id] uniqueidentifier NOT NULL,
    [station_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_vehicles_models FOREIGN KEY ([model_id]) REFERENCES [vehicle_models]([id]),
    CONSTRAINT fk_vehicles_stations FOREIGN KEY ([station_id]) REFERENCES [stations]([id])
);
GO
CREATE INDEX idx_vehicles_model_id ON vehicles (model_id);
CREATE INDEX idx_vehicles_vehicles ON vehicles (station_id);
GO

CREATE TABLE [rental_contracts] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [description] nvarchar(MAX) NOT NULL,
    [notes] nvarchar(255),
    [start_date] datetimeoffset NOT NULL,
    [actual_start_date] datetimeoffset,
    [end_date] datetimeoffset NOT NULL,
    [actual_end_date] datetimeoffset,
    [is_signed_by_staff] bit NOT NULL DEFAULT (0),
    [is_signed_by_customer] bit NOT NULL DEFAULT (0),
    [status] int NOT NULL DEFAULT 0, -- RequestPending, PaymentPending, Active, Completed, Cancelled

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [vehicle_id] uniqueidentifier,
    [customer_id] uniqueidentifier NOT NULL,
    [handover_staff_id] uniqueidentifier,
    [return_staff_id] uniqueidentifier,
    [station_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_rental_contracts_vehicles FOREIGN KEY ([vehicle_id]) REFERENCES [vehicles]([id]),
    CONSTRAINT fk_rental_contracts_customers FOREIGN KEY ([customer_id]) REFERENCES [users]([id]),
    CONSTRAINT fk_rental_contracts_handover_staffs FOREIGN KEY ([handover_staff_id]) REFERENCES [staffs]([user_id]),
    CONSTRAINT fk_rental_contracts_return_staffs FOREIGN KEY ([return_staff_id]) REFERENCES [staffs]([user_id]),
    CONSTRAINT fk_rental_contracts_stations FOREIGN KEY ([station_id]) REFERENCES [stations]([id])
);
GO
CREATE INDEX idx_rental_contracts_vehicle_id ON rental_contracts (vehicle_id);
CREATE INDEX idx_rental_contracts_customer_id ON rental_contracts (customer_id);
CREATE INDEX idx_rental_contracts_handover_staff_id ON rental_contracts (handover_staff_id);
CREATE INDEX idx_rental_contracts_return_staff_id ON rental_contracts (return_staff_id);
CREATE INDEX idx_rental_contracts_station_id ON rental_contracts (station_id);
GO

CREATE TABLE [vehicle_checklists] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [type] int NOT NULL DEFAULT 0,
    [is_signed_by_staff] bit NOT NULL DEFAULT (0),
    [is_signed_by_customer] bit NOT NULL DEFAULT (0),

    [maintained_until] datetimeoffset,
    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [staff_id] uniqueidentifier NOT NULL,
    [customer_id] uniqueidentifier NULL,
    [vehicle_id] uniqueidentifier NOT NULL,
    [contract_id] uniqueidentifier,

    CONSTRAINT fk_vehicle_checklists_staffs FOREIGN KEY ([staff_id]) REFERENCES [staffs]([user_id]),
    CONSTRAINT fk_vehicle_checklists_users FOREIGN KEY ([customer_id]) REFERENCES [users]([id]),
    CONSTRAINT fk_vehicle_checklists_vehicles FOREIGN KEY ([vehicle_id]) REFERENCES [vehicles]([id]),
    CONSTRAINT fk_vehicle_checklists_contracts FOREIGN KEY ([contract_id]) REFERENCES [rental_contracts]([id])
);
GO
CREATE INDEX idx_vehicle_checklists_staff_id ON vehicle_checklists (staff_id);
CREATE INDEX idx_vehicle_checklists_customer_id ON vehicle_checklists (customer_id);
CREATE INDEX idx_vehicle_checklists_vehicle_id ON vehicle_checklists (vehicle_id);
CREATE INDEX idx_vehicle_checklists_contract_id ON vehicle_checklists (contract_id);
GO

CREATE TABLE [vehicle_checklist_items] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [notes] nvarchar(255),
    [status] int NOT NULL DEFAULT 0, -- Good, Minor, Moderate, Severe, Totaled
    [image_url] nvarchar(500),
    [image_public_id] nvarchar(255),

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [component_id] uniqueidentifier NOT NULL,
    [checklist_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_vehicle_checklist_items_components FOREIGN KEY ([component_id]) REFERENCES [vehicle_components]([id]),
    CONSTRAINT fk_vehicle_checklist_items_checklists FOREIGN KEY ([checklist_id]) REFERENCES [vehicle_checklists]([id])
);
GO
CREATE UNIQUE INDEX idx_vehicle_checklist_items_image_url ON vehicle_checklist_items (image_url) WHERE image_url IS NOT NULL;
CREATE INDEX idx_vehicle_checklist_items_component_id ON vehicle_checklist_items (component_id);
CREATE INDEX idx_vehicle_checklist_items_checklist_id ON vehicle_checklist_items (checklist_id);
GO

CREATE TABLE [invoices] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [subtotal] decimal(10,2) NOT NULL,
    [tax] decimal(10,2) NOT NULL,
    [paid_amount] decimal(10,2),

    [payment_method] int NOT NULL, -- 0: Cash, 1: MomoWallet
    [notes] nvarchar(255),
    [status] int NOT NULL DEFAULT 0, -- Pending, Paid, Cancelled
    [type] int NOT NULL DEFAULT 0,

    [paid_at] datetimeoffset,

	[image_url] nvarchar(500),
    [image_public_id] nvarchar(255),

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [contract_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_invoices_contracts FOREIGN KEY ([contract_id]) REFERENCES [rental_contracts]([id])
);
GO
CREATE INDEX idx_invoices_contract_id ON invoices (contract_id);
GO

CREATE TABLE [invoice_items] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
	[description] nvarchar(100),
    [quantity] int NOT NULL DEFAULT 1,
    [unit_price] decimal(10,2) NOT NULL,
    [type] int NOT NULL, -- 0: BaseRental, 1: Damage, 2: LateReturn, 3: Cleaning, 4: Penalty, 5: Other

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [invoice_id] uniqueidentifier NOT NULL,
    [checklist_item_id] uniqueidentifier,

    CONSTRAINT fk_invoice_items_invoices FOREIGN KEY ([invoice_id]) REFERENCES [invoices]([id]),
    CONSTRAINT fk_invoice_items_checklist_items FOREIGN KEY ([checklist_item_id]) REFERENCES [vehicle_checklist_items]([id])
);
GO
CREATE INDEX idx_invoice_items_invoice_id ON invoice_items (invoice_id);
CREATE UNIQUE INDEX idx_invoice_items_checklist_item_id ON invoice_items (checklist_item_id) WHERE checklist_item_id IS NOT NULL;
GO

CREATE TABLE [deposits] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [amount] decimal(10,2) NOT NULL,
    [refunded_at] datetimeoffset,
    [status] int NOT NULL DEFAULT 0, -- Pending, Paid, Refunded, Forfeited

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,
    
    [invoice_id] uniqueidentifier UNIQUE NOT NULL,

    CONSTRAINT fk_deposits_invoices FOREIGN KEY ([invoice_id]) REFERENCES [invoices]([id]),
    CONSTRAINT uq_deposits_invoice_id UNIQUE ([invoice_id])
);
GO

CREATE TABLE [dispatch_requests] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [description] nvarchar(max),
    [final_description] nvarchar(max),
    [status] int NOT NULL DEFAULT 0, -- Pending, Approved, Rejected, Received

    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,
    
    [request_admin_id] uniqueidentifier NOT NULL,
    [approved_admin_id] uniqueidentifier,
    [from_station_id] uniqueidentifier,
    [to_station_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_dispatch_requests_request_admins FOREIGN KEY ([request_admin_id]) REFERENCES [staffs]([user_id]),
    CONSTRAINT fk_dispatch_requests_approved_admins FOREIGN KEY ([approved_admin_id]) REFERENCES [staffs]([user_id]),
    CONSTRAINT fk_dispatch_requests_from_stations FOREIGN KEY ([from_station_id]) REFERENCES [stations]([id]),
    CONSTRAINT fk_dispatch_requests_to_stations FOREIGN KEY ([to_station_id]) REFERENCES [stations]([id])
);
GO
CREATE INDEX idx_dispatch_requests_request_admin_id ON dispatch_requests (request_admin_id);
CREATE INDEX idx_dispatch_requests_approved_admin_id ON dispatch_requests (approved_admin_id);
CREATE INDEX idx_dispatch_requests_from_station_id ON dispatch_requests (from_station_id);
CREATE INDEX idx_dispatch_requests_to_station_id ON dispatch_requests (to_station_id);
GO

CREATE TABLE [dispatch_request_staffs] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [dispatch_request_id] uniqueidentifier NOT NULL,
    [staff_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_dispatch_request_staffs_dispatch_requests FOREIGN KEY ([dispatch_request_id]) REFERENCES [dispatch_requests]([id]),
    CONSTRAINT fk_dispatch_request_staffs_staffs FOREIGN KEY ([staff_id]) REFERENCES [staffs]([user_id])
);
GO
CREATE INDEX idx_dispatch_request_staffs_dispatch_request_id ON dispatch_request_staffs (dispatch_request_id);
CREATE INDEX idx_dispatch_request_staffs_staff_id ON dispatch_request_staffs (staff_id);
GO

CREATE TABLE [dispatch_request_vehicles] (
    [id] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    [created_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [updated_at] datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [deleted_at] datetimeoffset,

    [dispatch_request_id] uniqueidentifier NOT NULL,
    [vehicle_id] uniqueidentifier NOT NULL,

    CONSTRAINT fk_dispatch_request_vehicles_dispatch_requests FOREIGN KEY ([dispatch_request_id]) REFERENCES [dispatch_requests]([id]),
    CONSTRAINT fk_dispatch_request_vehicles_vehicles FOREIGN KEY ([vehicle_id]) REFERENCES [vehicles]([id])
);
GO
CREATE INDEX idx_dispatch_request_vehicles_dispatch_request_id ON dispatch_request_vehicles (dispatch_request_id);
CREATE INDEX idx_dispatch_request_vehicles_vehicle_id ON dispatch_request_vehicles (vehicle_id);
GO

CREATE TABLE [business_variables] (
    [id] UNIQUEIDENTIFIER NOT NULL,
    [created_at] DATETIMEOFFSET NOT NULL,
    [updated_at] DATETIMEOFFSET NOT NULL,
    [key] int NOT NULL,
    [value] DECIMAL(18,2) NOT NULL,
    CONSTRAINT [PK_business_variables] PRIMARY KEY ([id])
);

GO