USE green_wheel_db;
GO
/* ============================================================
    SECTION 2 — ROLES
============================================================ */
INSERT INTO roles (name, description)
VALUES
  ('Admin', 'System administrator'),
  ('Staff', 'Station staff'),
  ('Customer', 'Vehicle customer'),
  ('SuperAdmin', 'Super Administrator');
GO
/* ============================================================
    SECTION 3 — STATIONS
============================================================ */
INSERT INTO stations (name, address)
VALUES
  (N'Trạm A', N'123 Quận 3, TP.HCM'),
  (N'Trạm B', N'456 Quận 6, TP.HCM');
GO
/* ============================================================
    SECTION 4 — USERS (BASE ADMINS / STAFFS / 1 CUSTOMER)
============================================================ */
DECLARE @adminRole UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM roles WHERE name='Admin');
DECLARE @staffRole UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM roles WHERE name='Staff');
DECLARE @customerRole UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM roles WHERE name='Customer');
DECLARE @superAdminRole UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM roles WHERE name='SuperAdmin');

INSERT INTO users (first_name, last_name, email, password, phone, sex, role_id)
VALUES
  (N'Nguyễn', N'Admin A', 'adminA@greenwheel.vn',
   '$2a$12$CZ2ikjkipa7p8kDYJN6o7.90TIjpIsswYSMr3iGYJBQQyj8/cgU06', '0901111111', 0, @adminRole),

  (N'Phạm', N'Admin B', 'adminB@greenwheel.vn',
   '$2a$12$CZ2ikjkipa7p8kDYJN6o7.90TIjpIsswYSMr3iGYJBQQyj8/cgU06', '0902222222', 1, @adminRole),

  (N'Trần', N'Staff A', 'staffA@greenwheel.vn',
   '$2a$12$UnyAq2ckOtLYgpDQbNTTje5IPx9cbdTRPw5MB.sDg12OYjygBWJFa', '0902345678', 1, @staffRole),

  (N'Trần', N'Staff B', 'staffB@greenwheel.vn',
   '$2a$12$UnyAq2ckOtLYgpDQbNTTje5IPx9cbdTRPw5MB.sDg12OYjygBWJFa', '0902345670', 1, @staffRole),

  (N'Lê', N'Customer', 'customer@greenwheel.vn',
   '$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe', '0909999999', 0, @customerRole),

  (N'Súp', N'Lơ', 'superAdmin@greenwheel.vn',
   '$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe', '0909999997', 0, @superAdminRole);
GO
/* ============================================================
    SECTION 5 — STAFFS
============================================================ */
DECLARE @adminA UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='adminA@greenwheel.vn');
DECLARE @adminB UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='adminB@greenwheel.vn');
DECLARE @staffA UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='staffA@greenwheel.vn');
DECLARE @staffB UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='staffB@greenwheel.vn');
DECLARE @stationA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%A%');
DECLARE @stationB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%B%');

INSERT INTO staffs (user_id, station_id)
VALUES
  (@adminA, @stationA),
  (@adminB, @stationB),
  (@staffA, @stationA),
  (@staffB, @stationB);
GO
/* ============================================================
    SECTION 6 — BRANDS
============================================================ */
INSERT INTO brands (name, description, country, founded_year)
VALUES
  (N'VinFast', N'Thương hiệu xe điện Việt Nam', N'Việt Nam', 2017);
GO
/* ============================================================
    SECTION 7 — VEHICLE SEGMENTS
============================================================ */
INSERT INTO vehicle_segments (name, description)
VALUES
  (N'Compact', N'Xe nhỏ gọn cho đô thị'),
  (N'SUV', N'Xe gầm cao thể thao đa dụng');
GO
/* ============================================================
    SECTION 9 — VEHICLE COMPONENTS
============================================================ */
INSERT INTO vehicle_components (name, description, damage_fee)
VALUES
  (N'Động cơ điện', N'Bộ phận tạo công suất vận hành', 10000),
  (N'Pin', N'Nguồn năng lượng cho xe', 10000),
  (N'Hệ thống phanh', N'Tăng độ an toàn khi di chuyển', 10000),
  (N'Nội thất', N'Ghế ngồi, màn hình, tiện ích nội thất', 10000);
GO
/* ============================================================
    SECTION 10- VEHICLE MODELS (VF3, VF5, VF6, VF7, VF8)
============================================================ */
DECLARE @brand UNIQUEIDENTIFIER        = (SELECT id FROM brands WHERE name='VinFast');
DECLARE @segSUV UNIQUEIDENTIFIER       = (SELECT id FROM vehicle_segments WHERE name='SUV');
DECLARE @segCompact UNIQUEIDENTIFIER   = (SELECT id FROM vehicle_segments WHERE name='Compact');

/* ---------------- VF3–VF8: INSERT MODELS ---------------- */
INSERT INTO vehicle_models
(name,description,cost_per_day,deposit_fee,
 seating_capacity,number_of_airbags,motor_power,battery_capacity,
 eco_range_km,sport_range_km,brand_id,segment_id,image_url,image_public_id,reservation_fee)
VALUES
-- VF3
(N'VinFast VF 3',N'Mini EV',8000,5000,4,4,50,20,210,180,@brand,@segCompact,
'http://res.cloudinary.com/dsnnghkez/image/upload/v1762873276/models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/main/upwle3e71wfowd5c0oss.jpg',
'models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/main/upwle3e71wfowd5c0oss',10000),

-- VF5
(N'VinFast VF 5',N'Compact SUV điện hạng A',11000,8000,5,4,70,37,300,260,@brand,@segCompact,
'http://res.cloudinary.com/dsnnghkez/image/upload/v1762872756/models/28aa7e24-94f6-460c-82b9-fc1c8363662e/main/nguslcg6tdip8kenx07r.jpg',
'models/28aa7e24-94f6-460c-82b9-fc1c8363662e/main/nguslcg6tdip8kenx07r',10000),

-- VF6
(N'VinFast VF 6',N'EV C-Class',14000,12000,5,6,150,59,380,320,@brand,@segSUV,
'http://res.cloudinary.com/dsnnghkez/image/upload/v1762873059/models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/main/qjroaw6y2ieszesqeqzm.jpg',
'models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/main/qjroaw6y2ieszesqeqzm',10000),

-- VF7
(N'VinFast VF 7',N'EV D-Class',17000,14000,5,6,180,75,420,360,@brand,@segSUV,
'http://res.cloudinary.com/dsnnghkez/image/upload/v1762873454/models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/main/ejl9qomhobnvwqhgccx4.jpg',
'models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/main/ejl9qomhobnvwqhgccx4',10000),

-- VF8
(N'VinFast VF 8',N'EV E-Class',20000,16000,5,8,220,87,480,410,@brand,@segSUV,
'http://res.cloudinary.com/dsnnghkez/image/upload/v1762873366/models/57294a88-6a43-432d-8f0d-370bd1f9fe40/main/jyk2vtm2bsys7rotodbu.jpg',
'models/57294a88-6a43-432d-8f0d-370bd1f9fe40/main/jyk2vtm2bsys7rotodbu',10000);


/* ============================================================
    LOAD MODEL IDs
============================================================ */
DECLARE @mVF3 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 3');
DECLARE @mVF5 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 5');
DECLARE @mVF6 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 6');
DECLARE @mVF7 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 7');
DECLARE @mVF8 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 8');


/* ============================================================
    MODEL IMAGES (VF3–VF8)
============================================================ */
INSERT INTO model_images (url, public_id, model_id) VALUES
-- VF8
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178014/models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/eqc49vffpwnk6jy1bqet.jpg',
 'models/57294a88-6a43-432d-8f0d-370bd1f9fe40/gallery/gptnvechnciutdm0qazz', @mVF8),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873372/models/57294a88-6a43-432d-8f0d-370bd1f9fe40/gallery/pnymkeyuflbdp24h5uf6.jpg',
 'models/57294a88-6a43-432d-8f0d-370bd1f9fe40/gallery/pnymkeyuflbdp24h5uf6', @mVF8),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873371/models/57294a88-6a43-432d-8f0d-370bd1f9fe40/gallery/slebbhl6qo8p68luuju8.jpg',
 'models/57294a88-6a43-432d-8f0d-370bd1f9fe40/gallery/slebbhl6qo8p68luuju8', @mVF8),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873369/models/57294a88-6a43-432d-8f0d-370bd1f9fe40/gallery/tqjxy3kfjwjfzxuin9y4.jpg',
 'models/57294a88-6a43-432d-8f0d-370bd1f9fe40/gallery/tqjxy3kfjwjfzxuin9y4', @mVF8),

-- VF5
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762872762/models/28aa7e24-94f6-460c-82b9-fc1c8363662e/gallery/sgijrmiz7ubwjkoj52py.jpg',
 'models/28aa7e24-94f6-460c-82b9-fc1c8363662e/gallery/sgijrmiz7ubwjkoj52py', @mVF5),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762872758/models/28aa7e24-94f6-460c-82b9-fc1c8363662e/gallery/p1xx6j52pmcghqbcid01.jpg',
 'models/28aa7e24-94f6-460c-82b9-fc1c8363662e/gallery/p1xx6j52pmcghqbcid01', @mVF5),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762872760/models/28aa7e24-94f6-460c-82b9-fc1c8363662e/gallery/mukgqmbp7ysvifzbn7yh.jpg',
 'models/28aa7e24-94f6-460c-82b9-fc1c8363662e/gallery/mukgqmbp7ysvifzbn7yh', @mVF5),

-- VF7
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873464/models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/svxtrm5dtr6ahvvvieak.jpg',
 'models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/svxtrm5dtr6ahvvvieak', @mVF7),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873460/models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/mmr7wimleqpmcezwyb4s.jpg',
 'models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/mmr7wimleqpmcezwyb4s', @mVF7),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873465/models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/ybjliyfx0ec9wgfyoijk.jpg',
 'models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/ybjliyfx0ec9wgfyoijk', @mVF7),
 ('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873458/models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/n3lm6ipstseackbwz8al.jpg',
 'models/21dc600f-367c-4b81-9d3b-11dc3b6a886a/gallery/n3lm6ipstseackbwz8al', @mVF7),

-- VF3
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873283/models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/gallery/krsm95fe7psrq9vbw0xh.jpg',
 'models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/gallery/krsm95fe7psrq9vbw0xh', @mVF3),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873280/models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/gallery/nd0ysefdgpifahx9fpjp.jpg',
 'models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/gallery/nd0ysefdgpifahx9fpjp', @mVF3),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873279/models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/gallery/qtifhw8urgwaycrzjbwh.jpg',
 'models/a926d4aa-c47c-4f0a-b428-5da4e95b753a/gallery/qtifhw8urgwaycrzjbwh', @mVF3),


-- VF6
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873061/models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/gallery/jnxl5djqc5rqlx7i00in.jpg',
 'models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/gallery/jnxl5djqc5rqlx7i00in', @mVF6),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873065/models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/gallery/v2pwwsrfpqtavulaeqbv.jpg',
 'models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/gallery/v2pwwsrfpqtavulaeqbv', @mVF6),
('http://res.cloudinary.com/dsnnghkez/image/upload/v1762873067/models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/gallery/pwaukj8ej6rmx8phh8t5.jpg',
 'models/ffa560be-d42e-401a-9ee8-e5b4b105bbc1/gallery/pwaukj8ej6rmx8phh8t5', @mVF6);


/* ============================================================
    MODEL COMPONENTS (VF3–VF8)
============================================================ */
INSERT INTO model_components(model_id, component_id)
SELECT @mVF3, id FROM vehicle_components
UNION ALL SELECT @mVF5, id FROM vehicle_components
UNION ALL SELECT @mVF6, id FROM vehicle_components
UNION ALL SELECT @mVF7, id FROM vehicle_components
UNION ALL SELECT @mVF8, id FROM vehicle_components;
/* ============================================================
    SECTION 11 — VEHICLES (ALL MODELS)
============================================================ */
DECLARE @sA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%A%');
DECLARE @sB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%B%');
INSERT INTO vehicles (license_plate,status,model_id,station_id)
VALUES
-- VF3
('51C-100.01',0,@mVF3,@sA),
('51C-100.02',0,@mVF3,@sA),
('51C-100.03',0,@mVF3,@sB),
('51C-100.04',0,@mVF3,@sA),
('51C-100.05',0,@mVF3,@sB),

-- VF5
('51C-200.01',0,@mVF5,@sA),
('51C-200.02',0,@mVF5,@sA),
('51C-200.03',0,@mVF5,@sB),
('51C-200.04',0,@mVF5,@sA),
('51C-200.05',0,@mVF5,@sB),

-- VF6
('51C-300.01',0,@mVF6,@sA),
('51C-300.02',0,@mVF6,@sA),
('51C-300.03',0,@mVF6,@sB),
('51C-300.04',0,@mVF6,@sA),
('51C-300.05',0,@mVF6,@sB),

-- VF7
('51C-700.01',0,@mVF7,@sA),
('51C-700.02',0,@mVF7,@sA),
('51C-700.03',0,@mVF7,@sB),
('51C-700.04',0,@mVF7,@sA),
('51C-700.05',0,@mVF7,@sB),

-- VF8 (1 xe)
('51C-800.01',0,@mVF8,@sA);
GO

/* ============================================================
    SECTION 12 — CANCELLED CONTRACTS
============================================================ */
DECLARE @customerUser UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='customer@greenwheel.vn');
DECLARE @handoverStaff UNIQUEIDENTIFIER =
    (SELECT user_id FROM staffs WHERE user_id = (SELECT id FROM users WHERE email='staffA@greenwheel.vn'));

DECLARE @vehA UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate=N'51A-123.01');
DECLARE @vehB UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate=N'51B-456.01');
DECLARE @vehC UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate=N'51A-123.02');
DECLARE @stationA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%A%');
DECLARE @stationB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%B%');
INSERT INTO rental_contracts
(description, notes, start_date, end_date, status, is_signed_by_staff, is_signed_by_customer,
 vehicle_id, customer_id, handover_staff_id, return_staff_id, station_id)
VALUES
(N'Hợp đồng A1', N'Huỷ do thay đổi kế hoạch',
 DATEADD(DAY,-5,SYSDATETIMEOFFSET()), DATEADD(DAY,-3,SYSDATETIMEOFFSET()),
 5,0,0,@vehA,@customerUser,@handoverStaff,@handoverStaff,@stationA),

(N'Hợp đồng B1', N'Huỷ vì không cung cấp đủ giấy tờ',
 DATEADD(DAY,-4,SYSDATETIMEOFFSET()), DATEADD(DAY,-2,SYSDATETIMEOFFSET()),
 5,0,0,@vehB,@customerUser,@handoverStaff,@handoverStaff,@stationB),

(N'Hợp đồng A2', N'Huỷ do không thanh toán tiền cọc',
 DATEADD(DAY,-2,SYSDATETIMEOFFSET()), DATEADD(DAY,-1,SYSDATETIMEOFFSET()),
 5,0,0,@vehC,@customerUser,@handoverStaff,@handoverStaff,@stationA);
GO
/* ============================================================
    SECTION 13 — BUSINESS VARIABLES
============================================================ */
INSERT INTO business_variables (id, created_at, updated_at, [key], [value]) VALUES
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 0, 10000),   -- LateReturnFee
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 1, 10000),   -- CleaningFee
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 2, 0.1),     -- BaseVAT
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 3, 1),       -- MaxLateReturnHours
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 4, 10),      -- BufferDay
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 5, 10);      -- RefundCreationDelayDays
GO
/* ============================================================
    SECTION 14 — PROCEDURE: CREATE INVOICES & DEPOSIT
============================================================ */
CREATE OR ALTER PROCEDURE dbo.__seed_create_invoices
(
    @contract_id      UNIQUEIDENTIFIER,
    @reservation_paid BIT,
    @handover_paid    BIT
)
AS
BEGIN
    /* ===== Lấy vehicle + model để biết deposit ===== */
    DECLARE @vehicle_id UNIQUEIDENTIFIER =
        (SELECT vehicle_id FROM rental_contracts WHERE id = @contract_id);

    DECLARE @model_id UNIQUEIDENTIFIER =
        (SELECT model_id FROM vehicles WHERE id = @vehicle_id);

    DECLARE @deposit DECIMAL(10,2) =
        (SELECT deposit_fee FROM vehicle_models WHERE id = @model_id);

    /* ===== Fixed cost ===== */
    DECLARE @reservation_subtotal DECIMAL(10,2) = 3000.00;
    DECLARE @handover_subtotal    DECIMAL(10,2) = 5000.00;
    DECLARE @reservation_tax_rate DECIMAL(10,2) = 0.00;
    DECLARE @handover_tax_rate    DECIMAL(10,2) = 0.10;

    /* ===== CREATE RESERVATION INVOICE ===== */
    DECLARE @reservation_invoice_id UNIQUEIDENTIFIER = NEWID();
    DECLARE @reservation_paid_amount DECIMAL(10,2) =
        CASE WHEN @reservation_paid = 1 THEN @reservation_subtotal ELSE NULL END;

    INSERT INTO invoices
    (
        id, subtotal, tax, paid_amount, payment_method, notes, status, type, paid_at,
        created_at, updated_at, contract_id
    )
    VALUES
    (
        @reservation_invoice_id,
        @reservation_subtotal,
        @reservation_tax_rate,
        @reservation_paid_amount,
        0,
        N'Reservation invoice',
        CASE WHEN @reservation_paid=1 THEN 1 ELSE 0 END,
        0,
        CASE WHEN @reservation_paid=1 THEN SYSDATETIMEOFFSET() ELSE NULL END,
        SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(),
        @contract_id
    );

    /* ===== CREATE HANDOVER INVOICE ===== */
    DECLARE @handover_invoice_id UNIQUEIDENTIFIER = NEWID();
    DECLARE @handover_paid_amount DECIMAL(10,2) =
        CASE WHEN @handover_paid=1 THEN @handover_subtotal * (1 + @handover_tax_rate) ELSE NULL END;

    INSERT INTO invoices
    (
        id, subtotal, tax, paid_amount, payment_method, notes, status, type, paid_at,
        created_at, updated_at, contract_id
    )
    VALUES
    (
        @handover_invoice_id,
        @handover_subtotal,
        @handover_tax_rate,
        @handover_paid_amount,
        0,
        N'Handover invoice',
        CASE WHEN @handover_paid=1 THEN 1 ELSE 0 END,
        1,
        CASE WHEN @handover_paid=1 THEN SYSDATETIMEOFFSET() ELSE NULL END,
        SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(),
        @contract_id
    );

    /* ===== CREATE DEPOSIT LINKED TO HANDOVER INVOICE ===== */
    INSERT INTO deposits
    (
        id, amount, refunded_at, status,
        created_at, updated_at, deleted_at, invoice_id
    )
    VALUES
    (
        NEWID(),
        @deposit,
        NULL,
        CASE WHEN @handover_paid=1 THEN 1 ELSE 0 END,
        SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), NULL,
        @handover_invoice_id
    );
END;
GO
/* ============================================================
    SECTION 15 — PROCEDURE: CREATE HANDOVER CHECKLIST
============================================================ */
CREATE OR ALTER PROCEDURE dbo.__seed_create_handover_checklist
(
 @staff_id UNIQUEIDENTIFIER,
 @customer_id UNIQUEIDENTIFIER,
 @vehicle_id UNIQUEIDENTIFIER,
 @contract_id UNIQUEIDENTIFIER
)
AS
BEGIN
    DECLARE @chk UNIQUEIDENTIFIER = NEWID();

    INSERT INTO vehicle_checklists
    (id,type,is_signed_by_staff,is_signed_by_customer,
     created_at,updated_at,staff_id,customer_id,vehicle_id,contract_id)
    VALUES
    (
        @chk,
        1,
        1,1,
        SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(),
        @staff_id,@customer_id,@vehicle_id,@contract_id
    );

    /* Insert items cho toàn bộ components */
    INSERT INTO vehicle_checklist_items
    (status, component_id, checklist_id, created_at, updated_at)
    SELECT
        0, id, @chk, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
    FROM vehicle_components;
END;
GO
/* ============================================================
    SECTION 16 — EXTRA CUSTOMER USERS (13 USERS)
============================================================ */
DECLARE @customerRole2 UNIQUEIDENTIFIER = (SELECT id FROM roles WHERE name='Customer');

INSERT INTO users (first_name,last_name,email,password,phone,sex,role_id) VALUES
(N'Duy',N'Case 1 Main','lehoangduy23092005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000001',0,@customerRole2),
(N'Duy',N'Case 1 Sub','lehoangduy23905@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000002',0,@customerRole2),
(N'Duy',N'Case 2 Main','lehoangduy20102005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000003',0,@customerRole2),
(N'Duy',N'Case 2 Sub','hoangduyle.work@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000004',0,@customerRole2),
(N'Huy',N'Case 3 Main','huyngse183274@fpt.edu.vn',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000005',0,@customerRole2),
(N'Huy',N'Case 3 Sub','ngogiahuy.work@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000006',0,@customerRole2),
(N'Đức',N'Case 4 Main','duck05gaming@gmai.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000007',0,@customerRole2),
(N'Đức',N'Case 4 Sub','duck.test.dev.05@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000008',0,@customerRole2),
(N'Huy',N'Cleaning','Huycungbaobinh@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000009',0,@customerRole2),
(N'Huy',N'Warning','Nguyenquanghuy14022005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000010',0,@customerRole2),
(N'Huy',N'Free 1','Quanghuynguyen14022005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000011',0,@customerRole2),
(N'Huy',N'Free 2','Huyquangnguyen14022005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000012',0,@customerRole2),
(N'Huy',N'Free 3','Huytradecoin@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000013',0,@customerRole2);
GO


/* ============================================================
   SECTION 17 — COMPLETED CONTRACTS FOR JAN–NOV 2025 (FINAL WITH INVOICE ITEMS)
============================================================ */

DECLARE @custRole UNIQUEIDENTIFIER = (SELECT id FROM roles WHERE name='Customer');
DECLARE @staffDefault UNIQUEIDENTIFIER = (SELECT TOP 1 user_id FROM staffs);
DECLARE @stationA_MAIN UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations ORDER BY name);

DECLARE @tblUsers TABLE(id UNIQUEIDENTIFIER);
INSERT INTO @tblUsers SELECT id FROM users WHERE role_id=@custRole ORDER BY id;

DECLARE @tblVehicles TABLE(id UNIQUEIDENTIFIER, model_id UNIQUEIDENTIFIER);
INSERT INTO @tblVehicles SELECT id, model_id FROM vehicles ORDER BY id;

DECLARE @uCount INT = (SELECT COUNT(*) FROM @tblUsers);
DECLARE @vCount INT = (SELECT COUNT(*) FROM @tblVehicles);

DECLARE @i INT = 1;
DECLARE @month INT = 1;

WHILE @month <= 11
BEGIN
    DECLARE @u UNIQUEIDENTIFIER = (SELECT id FROM @tblUsers ORDER BY id OFFSET (@i-1) ROWS FETCH NEXT 1 ROWS ONLY);
    DECLARE @v UNIQUEIDENTIFIER = (SELECT id FROM @tblVehicles ORDER BY id OFFSET (@i-1) ROWS FETCH NEXT 1 ROWS ONLY);
    DECLARE @modelId UNIQUEIDENTIFIER = (SELECT model_id FROM @tblVehicles ORDER BY id OFFSET (@i-1) ROWS FETCH NEXT 1 ROWS ONLY);
    DECLARE @depositAmount DECIMAL(18,2) = (SELECT deposit_fee FROM vehicle_models WHERE id = @modelId);

    /* DATE FOR MONTHLY STATS */
    DECLARE @startDate DATETIMEOFFSET =
        DATETIMEOFFSETFROMPARTS(2025, @month, 10, 8, 0, 0, 0, 0, 7, 0);

    DECLARE @endDate DATETIMEOFFSET = DATEADD(DAY, 3, @startDate);

    /* ALL CREATED_AT WILL ALIGN TO MONTH */
    DECLARE @createdAt DATETIMEOFFSET =
        DATETIMEOFFSETFROMPARTS(2025, @month, 10, 9, 0, 0, 0, 0, 7, 0);

    DECLARE @contractId UNIQUEIDENTIFIER = NEWID();

    /* CREATE CONTRACT */
    INSERT INTO rental_contracts
    (id, description, notes, start_date, end_date, status,
     is_signed_by_staff, is_signed_by_customer,
     vehicle_id, customer_id, handover_staff_id, station_id,
     actual_start_date, actual_end_date,
     created_at, updated_at)
    VALUES
    (
        @contractId,
        CONCAT('Completed Contract Month ', @month),
        N'Seed data for statistics',
        @startDate, @endDate,
        4, 1,1,
        @v, @u,
        @staffDefault,
        @stationA_MAIN,
        @startDate, @endDate,
        @createdAt, @createdAt
    );

    /* ==========================
       RESERVATION INVOICE
    ===========================*/
    DECLARE @invRes UNIQUEIDENTIFIER = NEWID();
    DECLARE @resSubtotal DECIMAL(10,2) = 100000;
    DECLARE @resTax DECIMAL(10,2) = 0;
    DECLARE @resPaid DECIMAL(10,2) = @resSubtotal + @resTax;

    INSERT INTO invoices
    (id, contract_id, type, status, subtotal, tax, payment_method,
     paid_amount, notes, created_at, updated_at)
    VALUES
    (
        @invRes, @contractId, 0, 1,
        @resSubtotal, @resTax, 0,
        @resPaid, N'Reservation invoice',
        @createdAt, @createdAt
    );

    /* RESERVATION INVOICE ITEM */
INSERT INTO invoice_items
(id, description, quantity, unit_price, type,
 created_at, updated_at, invoice_id)
VALUES
(
    NEWID(), N'Reservation Fee', 1, @resSubtotal, 0,
    (SELECT created_at FROM invoices WHERE id = @invRes),
    (SELECT updated_at FROM invoices WHERE id = @invRes),
    @invRes
);

    /* ==========================
       HANDOVER INVOICE
    ===========================*/
    DECLARE @invHand UNIQUEIDENTIFIER = NEWID();
    DECLARE @basePrice DECIMAL(18,2) = 300000;
    DECLARE @vat DECIMAL(18,2) = @basePrice * 0.1;
    DECLARE @handPaid DECIMAL(18,2) = @basePrice + @vat;

    INSERT INTO invoices
    (id, contract_id, type, status, subtotal, tax, payment_method,
     paid_amount, notes, created_at, updated_at)
    VALUES
    (
        @invHand, @contractId, 1, 1,
        @basePrice, 0.1, 0,
        @handPaid, N'Handover invoice',
        @createdAt, @createdAt
    );

   /* HANDOVER INVOICE ITEM */
INSERT INTO invoice_items
(id, description, quantity, unit_price, type,
 created_at, updated_at, invoice_id)
VALUES
(
    NEWID(), N'Handover Base Rental', 1, @basePrice, 0,
    (SELECT created_at FROM invoices WHERE id = @invHand),
    (SELECT updated_at FROM invoices WHERE id = @invHand),
    @invHand
);


    /* DEPOSIT LINKED TO HANDOVER */
    INSERT INTO deposits
    (id, invoice_id, amount, status, created_at, updated_at)
    VALUES
    (
        NEWID(), @invHand, @depositAmount,
        1, @createdAt, @createdAt
    );

    /* ==========================
       RETURN INVOICE
    ===========================*/
    DECLARE @invReturn UNIQUEIDENTIFIER = NEWID();
    DECLARE @returnSubtotal DECIMAL(10,2) = 50000;
    DECLARE @returnPaid DECIMAL(10,2) = @returnSubtotal;

    INSERT INTO invoices
    (id, contract_id, type, status, subtotal, tax, payment_method,
     paid_amount, notes, created_at, updated_at)
    VALUES
    (
        @invReturn, @contractId, 2, 1,
        @returnSubtotal, 0, 0,
        @returnPaid, N'Return invoice',
        @createdAt, @createdAt
    );

    INSERT INTO invoice_items
    (id, description, quantity, unit_price, type,
     created_at, updated_at, invoice_id)
    VALUES
    (
        NEWID(), N'Cleaning Fee', 1, @returnSubtotal, 3,
        @createdAt, @createdAt, @invReturn
    );

    /* ==========================
       REFUND INVOICE
    ===========================*/
    DECLARE @invRefund UNIQUEIDENTIFIER = NEWID();

    INSERT INTO invoices
    (id, contract_id, type, status, subtotal, tax, payment_method,
     paid_amount, notes, created_at, updated_at)
    VALUES
    (
        @invRefund, @contractId, 3, 1,
        @depositAmount, 0, 0,
        @depositAmount, N'Refund invoice',
        @createdAt, @createdAt
    );

    INSERT INTO invoice_items
    (id, description, quantity, unit_price, type,
     created_at, updated_at, invoice_id)
    VALUES
    (
        NEWID(), N'Deposit Refund', 1, @depositAmount, 5,
        @createdAt, @createdAt, @invRefund
    );

    /* LOOP */
    SET @i = @i + 1;
    IF @i > @uCount SET @i = 1;
    IF @i > @vCount SET @i = 1;

    SET @month = @month + 1;
END;
Update invoices set subtotal = 100000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 1)
Update invoices set subtotal = 130000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 2)
Update invoices set subtotal = 80000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 3)
Update invoices set subtotal = 180000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 4)													
Update invoices set subtotal = 140000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 5)													
Update invoices set subtotal = 60000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 6)													
Update invoices set subtotal = 200000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 7)												
Update invoices set subtotal = 120000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 8)
Update invoices set subtotal = 100000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 9)
Update invoices set subtotal = 170000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 10)
Update invoices set subtotal = 130000 where id in (SELECT id FROM invoices where contract_id in (SELECT id FROM rental_contracts where status = 4)
													AND MONTH(created_at) = 11)
GO

/* ============================================================
    SECTION 18 — MAIL SCENARIOS (A → F)
============================================================ */
/* ============================================================
    DECLARE CUSTOMER VARIABLES
============================================================ */
DECLARE @u1 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='lehoangduy23092005@gmail.com');
DECLARE @u2 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='lehoangduy23905@gmail.com');
DECLARE @u3 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='lehoangduy20102005@gmail.com');
DECLARE @u4 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='hoangduyle.work@gmail.com');
DECLARE @u5 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='huyngse183274@fpt.edu.vn');
DECLARE @u6 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='ngogiahuy.work@gmail.com');
DECLARE @u7 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='duck05gaming@gmai.com');
DECLARE @u8 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='duck.test.dev.05@gmail.com');
DECLARE @u9 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='Huycungbaobinh@gmail.com');
DECLARE @u10 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='Nguyenquanghuy14022005@gmail.com');
DECLARE @u11 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='Quanghuynguyen14022005@gmail.com');
DECLARE @u12 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='Huyquangnguyen14022005@gmail.com');
DECLARE @u13 UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='Huytradecoin@gmail.com');
DECLARE @vVF3 UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-100.01');
DECLARE @vVF5 UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-200.01');
DECLARE @vVF6 UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-300.01');
DECLARE @vVF7A UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-700.01');
DECLARE @vVF7B UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-700.02');
DECLARE @vVF8 UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-800.01');
DECLARE @staff UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email='staffA@greenwheel.vn');
DECLARE @sA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%A%');
DECLARE @sB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%B%');
/* ===== Scenario A — Payment Pending VF3 ===== */
DECLARE @cA1 UNIQUEIDENTIFIER = NEWID();
DECLARE @cA2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id)
VALUES
(@cA1,'PP VF3 U1',
 '2025-11-03T00:00:00+07:00','2025-11-10T00:00:00+07:00',
 1,0,0,@vVF3,@u1,@staff,@sA),
(@cA2,'PP VF3 U2',
 '2025-11-03T00:00:00+07:00','2025-11-10T00:00:00+07:00',
 1,0,0,@vVF3,@u2,@staff,@sA);

EXEC dbo.__seed_create_invoices @cA1,0,0;
EXEC dbo.__seed_create_invoices @cA2,0,0;

/* ===== Scenario B — VF5 Active + Pending ===== */
DECLARE @cB1 UNIQUEIDENTIFIER = NEWID();
DECLARE @cB2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id,actual_start_date)
VALUES
(@cB1,'Active VF5 U3',
 '2025-11-01T00:00:00+07:00','2025-11-04T11:00:00+07:00',
 2,1,1,@vVF5,@u3,@staff,@sA,'2025-11-01T00:00:00+07:00'),
(@cB2,'PP VF5 U4',
 '2025-11-15T00:00:00+07:00','2025-11-17T00:00:00+07:00',
 1,0,0,@vVF5,@u4,@staff,@sA,NULL);

EXEC dbo.__seed_create_invoices @cB1,1,1;
EXEC dbo.__seed_create_invoices @cB2,0,0;
EXEC dbo.__seed_create_handover_checklist @staff,@u3,@vVF5,@cB1;

UPDATE vehicles SET status=2 WHERE id=@vVF5;

/* ===== Scenario C — VF6 2 Active ===== */
DECLARE @cC1 UNIQUEIDENTIFIER = NEWID();
DECLARE @cC2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id,actual_start_date)
VALUES
(@cC1,'Active VF6 U5',
 '2025-11-01T00:00:00+07:00','2025-11-04T11:00:00+07:00',
 2,1,1,@vVF6,@u5,@staff,@sA,'2025-11-01T00:00:00+07:00'),
(@cC2,'Active VF6 U6',
 '2025-11-15T00:00:00+07:00','2025-11-17T00:00:00+07:00',
 2,1,1,@vVF6,@u6,@staff,@sA,'2025-11-15T00:00:00+07:00');

EXEC dbo.__seed_create_invoices @cC1,1,1;
EXEC dbo.__seed_create_invoices @cC2,1,1;
EXEC dbo.__seed_create_handover_checklist @staff,@u5,@vVF6,@cC1;

UPDATE vehicles SET status=2 WHERE id=@vVF6;

/* ===== Scenario D — VF8 2 Active ===== */
DECLARE @cD1 UNIQUEIDENTIFIER = NEWID();
DECLARE @cD2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id,actual_start_date)
VALUES
(@cD1,'Active VF8 U7',
 '2025-11-01T00:00:00+07:00','2025-11-04T11:00:00+07:00',
 2,1,1,@vVF8,@u7,@staff,@sA,'2025-11-01T00:00:00+07:00'),
(@cD2,'Active VF8 U8',
 '2025-11-15T00:00:00+07:00','2025-11-17T00:00:00+07:00',
 2,1,1,@vVF8,@u8,@staff,@sA,'2025-11-15T00:00:00+07:00');

EXEC dbo.__seed_create_invoices @cD1,1,1;
EXEC dbo.__seed_create_invoices @cD2,1,1;
EXEC dbo.__seed_create_handover_checklist @staff,@u7,@vVF8,@cD1;

UPDATE vehicles SET status=2 WHERE id=@vVF8;

/* ===== Scenario E — VF7A active but actual=null ===== */
DECLARE @cE UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id)
VALUES
(@cE,'Active VF7A U9',
 '2025-10-29T00:00:00+07:00','2025-11-04T09:00:00+07:00',
 2,1,1,@vVF7A,@u9,@staff,@sA);

EXEC dbo.__seed_create_invoices @cE,1,1;
UPDATE vehicles SET status=2 WHERE id=@vVF7A;

/* ===== Scenario F — VF7B active actual_start = start ===== */
DECLARE @cF UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id,actual_start_date)
VALUES
(@cF,'Active VF7B U10',
 '2025-10-29T00:00:00+07:00','2025-11-04T09:00:00+07:00',
 2,1,1,@vVF7B,@u10,@staff,@sA,'2025-10-29T00:00:00+07:00');

EXEC dbo.__seed_create_invoices @cF,1,1;
UPDATE vehicles SET status=2 WHERE id=@vVF7B;
GO
/* ============================================================
    SECTION 19 — DROP TEMP PROCEDURES
============================================================ */
DROP PROCEDURE dbo.__seed_create_invoices;
DROP PROCEDURE dbo.__seed_create_handover_checklist;
GO

