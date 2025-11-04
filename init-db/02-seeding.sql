USE green_wheel_db;
GO

-- ============================================
-- ROLES
-- ============================================
INSERT INTO roles (name, description)
VALUES
  ('Admin', 'System administrator'),
  ('Staff', 'Station staff'),
  ('Customer', 'Vehicle customer');
GO
-- ============================================
-- STATIONS
-- ============================================
INSERT INTO stations (name, address)
VALUES
  (N'Trạm A', N'123 Quận 3, TP.HCM'),
  (N'Trạm B', N'456 Quận 6, TP.HCM');
GO

-- ============================================
-- USERS (2 admins, 1 staff, 1 customer)
-- ============================================
DECLARE @adminRole UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM roles WHERE name='Admin');
DECLARE @staffRole UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM roles WHERE name='Staff');
DECLARE @customerRole UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM roles WHERE name='Customer');

INSERT INTO users (first_name, last_name, email, password, phone, sex, role_id)
VALUES
  (N'Nguyễn', N'Admin A', 'adminA@greenwheel.vn', '$2a$12$CZ2ikjkipa7p8kDYJN6o7.90TIjpIsswYSMr3iGYJBQQyj8/cgU06', '0901111111', 0, @adminRole),
  (N'Phạm', N'Admin B', 'adminB@greenwheel.vn', '$2a$12$CZ2ikjkipa7p8kDYJN6o7.90TIjpIsswYSMr3iGYJBQQyj8/cgU06', '0902222222', 1, @adminRole),
  (N'Trần', N'Staff A', 'staffA@greenwheel.vn', '$2a$12$UnyAq2ckOtLYgpDQbNTTje5IPx9cbdTRPw5MB.sDg12OYjygBWJFa', '0902345678', 1, @staffRole),
  (N'Trần', N'Staff B', 'staffB@greenwheel.vn', '$2a$12$UnyAq2ckOtLYgpDQbNTTje5IPx9cbdTRPw5MB.sDg12OYjygBWJFa', '0902345670', 1, @staffRole),
  (N'Lê', N'Customer', 'customer@greenwheel.vn', '$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe', '0909999999', 0, @customerRole);
GO

-- ============================================
-- STAFFS
-- ============================================
DECLARE @adminA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email='adminA@greenwheel.vn');
DECLARE @adminB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email='adminB@greenwheel.vn');
DECLARE @staffA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email='staffA@greenwheel.vn');
DECLARE @staffB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email='staffB@greenwheel.vn');
DECLARE @stationA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%Trạm A%');
DECLARE @stationB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%Trạm B%');

INSERT INTO staffs (user_id, station_id)
VALUES
  (@adminA, @stationA),
  (@adminB, @stationB),
  (@staffA, @stationA),
  (@staffB, @stationB);
GO

-- ============================================
-- BRANDS
-- ============================================
INSERT INTO brands (name, description, country, founded_year)
VALUES
  (N'VinFast', N'Thương hiệu xe điện Việt Nam', N'Việt Nam', 2017);
GO

-- ============================================
-- VEHICLE SEGMENTS
-- ============================================
INSERT INTO vehicle_segments (name, description)
VALUES
  (N'Compact', N'Xe nhỏ gọn cho đô thị'),
  (N'SUV', N'Xe gầm cao thể thao đa dụng');
GO

-- ============================================
-- VEHICLE MODELS
-- ============================================
DECLARE @brandVinfast UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM brands WHERE name='VinFast');
DECLARE @segmentSUV UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM vehicle_segments WHERE name='SUV');
DECLARE @segmentCompact UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM vehicle_segments WHERE name='Compact');

INSERT INTO vehicle_models (name, description, cost_per_day, deposit_fee, reservation_fee, seating_capacity, number_of_airbags, motor_power,
battery_capacity, eco_range_km, sport_range_km, brand_id, segment_id,image_url,image_public_id, reservation_fee)
VALUES
  (N'VinFast VF 5', N'Compact SUV điện hạng A', 11000, 8000, 10000, 5, 4, 70.0, 37.0, 300.0, 260.0, @brandVinfast, @segmentCompact,
  'http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178633/models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/main/fchalezgggotfpm1biue.jpg',
  'models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/main/fchalezgggotfpm1biue', 10000);
GO

-- ============================================
-- VEHICLE COMPONENTS
-- ============================================
INSERT INTO vehicle_components (name, description, damage_fee)
VALUES
  (N'Động cơ điện', N'Bộ phận tạo công suất vận hành', 10000),
  (N'Pin', N'Nguồn năng lượng cho xe', 10000),
  (N'Hệ thống phanh', N'Tăng độ an toàn khi di chuyển', 10000),
  (N'Nội thất', N'Ghế ngồi, màn hình, tiện ích nội thất', 10000);
GO

-- ============================================
-- MODEL COMPONENTS
-- ============================================
INSERT INTO model_components (model_id, component_id)
SELECT m.id, c.id
FROM vehicle_models m
CROSS JOIN vehicle_components c;
GO

-- ============================================
-- VEHICLES
-- ============================================
DECLARE @modelVf5 UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM vehicle_models WHERE name=N'VinFast VF 5');
DECLARE @stationA2 UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%Trạm A%');
DECLARE @stationB2 UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%Trạm B%');

INSERT INTO vehicles (license_plate, status, model_id, station_id)
VALUES
(N'51A-123.01', 0, @modelVf5, @stationA2),
(N'51A-123.02', 0, @modelVf5, @stationA2),
(N'51B-456.01', 0, @modelVf5, @stationB2),
(N'51B-456.02', 0, @modelVf5, @stationB2);
GO

-- ============================================
-- RENTAL CONTRACTS (3 hợp đồng bị huỷ - status = 5)
-- ============================================
DECLARE @customerUser UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email='customer@greenwheel.vn');
DECLARE @handoverStaff UNIQUEIDENTIFIER = (SELECT TOP 1 user_id FROM staffs WHERE user_id = (SELECT id FROM users WHERE email='staff@greenwheel.vn'));
DECLARE @adminA2 UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email='adminA@greenwheel.vn');
DECLARE @adminB2 UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email='adminB@greenwheel.vn');
DECLARE @stationA3 UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%Trạm A%');
DECLARE @stationB3 UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations WHERE name LIKE N'%Trạm B%');
DECLARE @vehA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM vehicles WHERE license_plate=N'51A-123.01');
DECLARE @vehB UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM vehicles WHERE license_plate=N'51B-456.01');
DECLARE @vehC UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM vehicles WHERE license_plate=N'51A-123.02');

INSERT INTO rental_contracts
(description, notes, start_date, end_date, status, is_signed_by_staff, is_signed_by_customer,
 vehicle_id, customer_id, handover_staff_id, return_staff_id, station_id)
VALUES
-- ❌ Hợp đồng bị huỷ 1
(N'Hợp đồng A1', N'Huỷ do thay đổi kế hoạch',
 DATEADD(DAY, -5, SYSDATETIMEOFFSET()), DATEADD(DAY, -3, SYSDATETIMEOFFSET()),
 5, 0, 0, @vehA, @customerUser, @handoverStaff, @adminA2, @stationA3),

-- ❌ Hợp đồng bị huỷ 2
(N'Hợp đồng B1', N'Huỷ vì không cung cấp đủ giấy tờ',
 DATEADD(DAY, -4, SYSDATETIMEOFFSET()), DATEADD(DAY, -2, SYSDATETIMEOFFSET()),
 5, 0, 0, @vehB, @customerUser, @handoverStaff, @adminB2, @stationB3),

-- ❌ Hợp đồng bị huỷ 3
(N'Hợp đồng A2', N'Huỷ do không thanh toán tiền cọc',
 DATEADD(DAY, -2, SYSDATETIMEOFFSET()), DATEADD(DAY, -1, SYSDATETIMEOFFSET()),
 5, 0, 0, @vehC, @customerUser, @handoverStaff, @adminA2, @stationA3);
GO

INSERT INTO business_variables (id, created_at, updated_at, [key], [value]) VALUES
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 0, 10000),   -- LateReturnFee
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 1, 10000),   -- CleaningFee
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 2, 0.1),      -- BaseVAT (10%)
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 3, 1),        -- MaxLateReturnHours
(NEWID(), SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 4, 10);       -- RentalContractBufferDay

GO

USE green_wheel_db;
GO

/* ===================================================================
   PROC 1: Tạo Reservation + Handover Invoice (có deposit + tax 0.1)
=================================================================== */
CREATE OR ALTER PROCEDURE dbo.__seed_create_invoices
(
    @contract_id      UNIQUEIDENTIFIER,
    @reservation_paid BIT,
    @handover_paid    BIT
)
AS
BEGIN
    /* ============================================================
       1) Lấy vehicle + model để biết deposit_fee
    ============================================================ */
    DECLARE @vehicle_id UNIQUEIDENTIFIER =
        (SELECT vehicle_id FROM rental_contracts WHERE id = @contract_id);

    DECLARE @model_id UNIQUEIDENTIFIER =
        (SELECT model_id FROM vehicles WHERE id = @vehicle_id);

    DECLARE @deposit DECIMAL(10,2) =
        (SELECT deposit_fee FROM vehicle_models WHERE id = @model_id);


    /* ============================================================
       2) Chuẩn bị các giá trị cấu hình cố định
    ============================================================ */
    DECLARE @reservation_subtotal DECIMAL(10,2) = 3000.00;
    DECLARE @handover_subtotal    DECIMAL(10,2) = 5000.00;

    DECLARE @reservation_tax_rate DECIMAL(10,2) = 0.00;
    DECLARE @handover_tax_rate    DECIMAL(10,2) = 0.10;


    /* ============================================================
       3) RESERVATION INVOICE
    ============================================================ */
    DECLARE @reservation_invoice_id UNIQUEIDENTIFIER = NEWID();
    DECLARE @reservation_paid_at DATETIMEOFFSET =
        CASE WHEN @reservation_paid = 1 THEN SYSDATETIMEOFFSET() ELSE NULL END;

    DECLARE @reservation_paid_amount DECIMAL(10,2) =
        CASE WHEN @reservation_paid = 1
            THEN @reservation_subtotal * (1 + @reservation_tax_rate)
            ELSE NULL
        END;

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
        0,  -- Cash
        N'Reservation invoice',
        CASE WHEN @reservation_paid = 1 THEN 1 ELSE 0 END, -- InvoiceStatus
        0,  -- InvoiceType.Reservation
        @reservation_paid_at,
        SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(),
        @contract_id
    );


    /* ============================================================
       4) HANDOVER INVOICE
    ============================================================ */
    DECLARE @handover_invoice_id UNIQUEIDENTIFIER = NEWID();
    DECLARE @handover_paid_at DATETIMEOFFSET =
        CASE WHEN @handover_paid = 1 THEN SYSDATETIMEOFFSET() ELSE NULL END;

    DECLARE @handover_paid_amount DECIMAL(10,2) =
        CASE WHEN @handover_paid = 1
            THEN @handover_subtotal * (1 + @handover_tax_rate)
            ELSE NULL
        END;

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
        0,  -- Cash
        N'Handover invoice',
        CASE WHEN @handover_paid = 1 THEN 1 ELSE 0 END,
        1,  -- InvoiceType.Handover
        @handover_paid_at,
        SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(),
        @contract_id
    );


    /* ============================================================
       5) DEPOSIT (GẮN VỚI HANDOVER INVOICE)
       deposit.status:
         0 = Pending
         1 = Paid
         2 = Refunded
         3 = Forfeited
    ============================================================ */
    DECLARE @deposit_status INT =
        CASE WHEN @handover_paid = 1 THEN 1 ELSE 0 END;

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
        @deposit_status,
        SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), NULL,
        @handover_invoice_id
    );
END;
GO
/* ===================================================================
   PROC 2: Tạo checklist handover (full component)
=================================================================== */
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
    (@chk,1,1,1,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET(),
     @staff_id,@customer_id,@vehicle_id,@contract_id);

    INSERT INTO vehicle_checklist_items
    (status, component_id, checklist_id, created_at, updated_at)
    SELECT
        0, id, @chk, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
    FROM vehicle_components;
END;
GO
/* ============================================================
   1) USERS — 13 tài khoản (role Customer)
============================================================ */
DECLARE @customerRole UNIQUEIDENTIFIER =
 (SELECT id FROM roles WHERE name='Customer');

INSERT INTO users (first_name,last_name,email,password,phone,sex,role_id) VALUES
(N'Duy',N'Case 1 Main','lehoangduy23092005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000001',0,@customerRole),
(N'Duy',N'Case 1 Sub','lehoangduy23905@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000002',0,@customerRole),
(N'Duy',N'Case 2 Main','lehoangduy20102005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000003',0,@customerRole),
(N'Duy',N'Case 2 Sub','hoangduyle.work@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000004',0,@customerRole),
(N'Huy',N'Case 3 Main','huyngse183274@fpt.edu.vn',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000005',0,@customerRole),
(N'Huy',N'Case 3 Sub','ngogiahuy.work@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000006',0,@customerRole),
(N'Đức',N'Case 4 Main','duck05gaming@gmai.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000007',0,@customerRole),
(N'Đức',N'Case 4 Sub','duck.test.dev.05@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000008',0,@customerRole),
(N'Huy',N'Cleaning','Huycungbaobinh@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000009',0,@customerRole),
(N'Huy',N'Warning','Nguyenquanghuy14022005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000010',0,@customerRole),
(N'Huy',N'Free 1','Quanghuynguyen14022005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000011',0,@customerRole),
(N'Huy',N'Free 2','Huyquangnguyen14022005@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000012',0,@customerRole),
(N'Huy',N'Free 3','Huytradecoin@gmail.com',
'$2a$12$EF0KCPRK/mIt16yJtjCL1u/R5K0NXE7Mu9Q0s1WLX.iNOVrNEtXYe','0903000013',0,@customerRole);


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


/* ============================================================
   2) MODELS VF3–VF8 (brand VinFast)
============================================================ */
DECLARE @brand UNIQUEIDENTIFIER = (SELECT id FROM brands WHERE name='VinFast');
DECLARE @segSUV UNIQUEIDENTIFIER = (SELECT id FROM vehicle_segments WHERE name='SUV');
DECLARE @segCompact UNIQUEIDENTIFIER = (SELECT id FROM vehicle_segments WHERE name='Compact');

INSERT INTO vehicle_models
(name,description,cost_per_day,deposit_fee,reservation_fee,
 seating_capacity,number_of_airbags,motor_power,battery_capacity,
 eco_range_km,sport_range_km,brand_id,segment_id,image_url,image_public_id, reservation_fee)
VALUES
(N'VinFast VF 3',N'Mini EV',8000,5000,3000,4,4,50,20,210,180,@brand,@segCompact,
'http://res.cloudinary.com/dk5pwoag4/image/upload/v1762177952/models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/main/lwlrnpldyuvzo5krm0te.jpg',
'models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/main/lwlrnpldyuvzo5krm0te', 10000),
(N'VinFast VF 6',N'EV C-Class',14000,12000,4000,5,6,150,59,380,320,@brand,@segSUV,
'http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178284/models/a65f0887-35a4-4058-af15-b7600581c8eb/main/xxg5dlpy9vvji0dwoydg.jpg',
'models/a65f0887-35a4-4058-af15-b7600581c8eb/main/xxg5dlpy9vvji0dwoydg', 10000),
(N'VinFast VF 7',N'EV D-Class',17000,14000,5000,5,6,180,75,420,360,@brand,@segSUV,
'http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178229/models/0d223468-cff9-41ea-8178-915361a6f6d9/main/tf6nvfsnylfb7ypfvjso.jpg',
'models/0d223468-cff9-41ea-8178-915361a6f6d9/main/tf6nvfsnylfb7ypfvjso', 10000),
(N'VinFast VF 8',N'EV E-Class',20000,16000,6000,5,8,220,87,480,410,@brand,@segSUV,
'http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178011/models/a00b3abd-c55d-4fc0-8be5-1ac002894533/main/xjohfofnhldb86xtdnua.jpg',
'models/a00b3abd-c55d-4fc0-8be5-1ac002894533/main/xjohfofnhldb86xtdnua', 10000);


DECLARE @mVF3 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 3');
DECLARE @mVF5 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 5');
DECLARE @mVF6 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 6');
DECLARE @mVF7 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 7');
DECLARE @mVF8 UNIQUEIDENTIFIER = (SELECT id FROM vehicle_models WHERE name=N'VinFast VF 8');

/*=================================
Add model image
===================================*/
INSERT INTO model_images
(url, public_id, model_id)
VALUES
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178014/models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/eqc49vffpwnk6jy1bqet.jpg',
'models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/eqc49vffpwnk6jy1bqet', @mVF8),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178016/models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/ilhydecz2mkisgqigqek.jpg',
'models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/ilhydecz2mkisgqigqek', @mVF8),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178018/models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/tv7tdyilae5ymi1a71em.jpg',
'models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/tv7tdyilae5ymi1a71em', @mVF8),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178012/models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/cnyonqogdifzzsoapdqh.jpg',
'models/a00b3abd-c55d-4fc0-8be5-1ac002894533/gallery/cnyonqogdifzzsoapdqh', @mVF8),

('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178643/models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/gallery/dwqz0c8zs3kiqhqsquip.jpg',
'models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/gallery/dwqz0c8zs3kiqhqsquip', @mVF5),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178635/models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/gallery/v3h8y8wonmmveenokkog.jpg',
'models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/gallery/v3h8y8wonmmveenokkog', @mVF5),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178637/models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/gallery/mrofru0puwkhngjk4fjy.jpg',
'models/bf85ac59-53bf-4068-b3c3-2ba2bf1d92a2/gallery/mrofru0puwkhngjk4fjy', @mVF5),

('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178234/models/0d223468-cff9-41ea-8178-915361a6f6d9/gallery/aj0uzlvkbioe27uf7hw0.jpg',
'models/0d223468-cff9-41ea-8178-915361a6f6d9/gallery/aj0uzlvkbioe27uf7hw0', @mVF7),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178233/models/0d223468-cff9-41ea-8178-915361a6f6d9/gallery/o1pineohtqwc7idd4bwf.jpg',
'models/0d223468-cff9-41ea-8178-915361a6f6d9/gallery/o1pineohtqwc7idd4bwf', @mVF7),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178231/models/0d223468-cff9-41ea-8178-915361a6f6d9/gallery/duiptyjfvqeb1jwf6ztl.jpg',
'models/0d223468-cff9-41ea-8178-915361a6f6d9/gallery/duiptyjfvqeb1jwf6ztl', @mVF7),

('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762177953/models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/l65cyxycn86x502wbqds.jpg',
'models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/l65cyxycn86x502wbqds', @mVF3),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762177911/models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/vhnsky5itnbealgd1uxk.jpg',
'models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/vhnsky5itnbealgd1uxk', @mVF3),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762177957/models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/civitlwrdl6qee6vpupy.jpg',
'models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/civitlwrdl6qee6vpupy', @mVF3),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762177910/models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/mwdivdhgi588jksg73kq.jpg',
'models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/mwdivdhgi588jksg73kq', @mVF3),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762177955/models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/kihoczys0riovnm0ihyl.jpg',
'models/3012ad0b-03ea-4913-8ce8-b340f0b0c6cd/gallery/kihoczys0riovnm0ihyl', @mVF3),

('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178286/models/a65f0887-35a4-4058-af15-b7600581c8eb/gallery/lihtugg9wa5ijyses9d7.jpg',
'models/a65f0887-35a4-4058-af15-b7600581c8eb/gallery/lihtugg9wa5ijyses9d7', @mVF6),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178289/models/a65f0887-35a4-4058-af15-b7600581c8eb/gallery/lpvunjzdpmhh3wakmyay.jpg',
'models/a65f0887-35a4-4058-af15-b7600581c8eb/gallery/lpvunjzdpmhh3wakmyay', @mVF6),
('http://res.cloudinary.com/dk5pwoag4/image/upload/v1762178287/models/a65f0887-35a4-4058-af15-b7600581c8eb/gallery/umxw8qb93ubz9tu8179d.jpg',
'models/a65f0887-35a4-4058-af15-b7600581c8eb/gallery/umxw8qb93ubz9tu8179d', @mVF6);

/* ============================================================
   3) Gán component cho model mới
============================================================ */
INSERT INTO model_components (model_id, component_id)
SELECT @mVF3, id FROM vehicle_components
UNION ALL SELECT @mVF6, id FROM vehicle_components
UNION ALL SELECT @mVF7, id FROM vehicle_components
UNION ALL SELECT @mVF8, id FROM vehicle_components;


/* ============================================================
   4) Vehicles  (5 xe mỗi model – VF8 chỉ 1 xe)
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

/* ============================================================
   EXTRA SEED: Completed contracts for each month Jan–Nov 2025
============================================================ */

DECLARE @custRole UNIQUEIDENTIFIER = (SELECT id FROM roles WHERE name='Customer');
DECLARE @staffDefault UNIQUEIDENTIFIER = (SELECT TOP 1 user_id FROM staffs);
DECLARE @stationA UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM stations ORDER BY name);

-- Lấy danh sách user Customer
DECLARE @tblUsers TABLE(id UNIQUEIDENTIFIER);
INSERT INTO @tblUsers
SELECT id FROM users WHERE role_id=@custRole ORDER BY id;

-- Lấy danh sách xe (ưu tiên xe available)
DECLARE @tblVehicles TABLE(id UNIQUEIDENTIFIER);
INSERT INTO @tblVehicles
SELECT id FROM vehicles ORDER BY id;

DECLARE @i INT = 1;
DECLARE @uCount INT = (SELECT COUNT(*) FROM @tblUsers);
DECLARE @vCount INT = (SELECT COUNT(*) FROM @tblVehicles);

DECLARE @month INT = 1;

WHILE @month <= 11
BEGIN
    DECLARE @u UNIQUEIDENTIFIER =
        (SELECT id FROM @tblUsers ORDER BY id OFFSET (@i-1) ROWS FETCH NEXT 1 ROWS ONLY);

    DECLARE @v UNIQUEIDENTIFIER =
        (SELECT id FROM @tblVehicles ORDER BY id OFFSET (@i-1) ROWS FETCH NEXT 1 ROWS ONLY);

    DECLARE @startDate DATETIMEOFFSET =
    DATETIMEOFFSETFROMPARTS(2025, @month, 10, 8, 0, 0, 0, 0, 7, 0);

DECLARE @endDate DATETIMEOFFSET =
    DATEADD(DAY, 3, @startDate);


    INSERT INTO rental_contracts
    (id,description,notes,start_date,end_date,status,
     is_signed_by_staff,is_signed_by_customer,
     vehicle_id,customer_id,handover_staff_id,station_id,
     actual_start_date,actual_end_date)
    VALUES
    (NEWID(),
     CONCAT('Completed Contract Month ', @month),
     N'Auto generated for statistics',
     @startDate,
     @endDate,
     4,      -- Completed
     1,1,
     @v,
     @u,
     @staffDefault,
     @stationA,
     @startDate,
     @endDate);

    SET @i = @i + 1;
    IF @i > @uCount SET @i = 1;
    IF @i > @vCount SET @i = 1;

    SET @month = @month + 1;
END


DECLARE @vVF3 UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-100.01');
DECLARE @vVF5 UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-200.01');
DECLARE @vVF6 UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-300.01');
DECLARE @vVF7A UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-700.01');
DECLARE @vVF7B UNIQUEIDENTIFIER = (SELECT id FROM vehicles WHERE license_plate='51C-700.02');
DECLARE @vVF8 UNIQUEIDENTIFIER  = (SELECT id FROM vehicles WHERE license_plate='51C-800.01');

DECLARE @staff UNIQUEIDENTIFIER = (SELECT id from users where email = 'staffA@greenwheel.vn');


/* ============================================================
   5) CONTRACT SCENARIOS
============================================================ */

/*  A) 2 hợp đồng cùng xe VF3 — PaymentPending — mail 1 & 2  */
DECLARE @cA1 UNIQUEIDENTIFIER = NEWID();
DECLARE @cA2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,notes,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id)
VALUES
(@cA1,'PP VF3 U1','',
 '2025-11-03T00:00:00+07:00','2025-11-10T00:00:00+07:00',
 1,0,0,@vVF3,@u1,@staff,@sA),

(@cA2,'PP VF3 U2','',
 '2025-11-03T00:00:00+07:00','2025-11-10T00:00:00+07:00',
 1,0,0,@vVF3,@u2,@staff,@sA);

EXEC dbo.__seed_create_invoices @cA1,0,0;
EXEC dbo.__seed_create_invoices @cA2,0,0;


/*  B) Cặp hợp đồng VF5 — U3 Active paid & U4 Pending unpaid  */
DECLARE @cB1 UNIQUEIDENTIFIER = NEWID();
DECLARE @cB2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO rental_contracts
(id,description,notes,start_date,end_date,status,is_signed_by_staff,is_signed_by_customer,
 vehicle_id,customer_id,handover_staff_id,station_id,actual_start_date)
VALUES
(@cB1,'Active VF5 U3','',
 '2025-11-01T00:00:00+07:00','2025-11-04T11:00:00+07:00',
 2,1,1,@vVF5,@u3,@staff,@sA,'2025-11-01T00:00:00+07:00'),

(@cB2,'PP VF5 U4','',
 '2025-11-15T00:00:00+07:00','2025-11-17T00:00:00+07:00',
 1,0,0,@vVF5,@u4,@staff,@sA,NULL);

EXEC dbo.__seed_create_invoices @cB1,1,1;
EXEC dbo.__seed_create_invoices @cB2,0,0;
EXEC dbo.__seed_create_handover_checklist @staff,@u3,@vVF5,@cB1;

UPDATE vehicles SET status=2 WHERE id=@vVF5;


/*  C) 2 hợp đồng cùng VF6 — đều Active paid  */
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


/*  D) VF8 — 2 hợp đồng Active paid — U7 & U8  */
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


/*  E) mail 9 — Active paid — actual null — xe VF7A  */
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


/*  F) mail 10 — Active paid — actual_start=start — xe VF7B  */
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

DROP PROCEDURE dbo.__seed_create_invoices;
DROP PROCEDURE dbo.__seed_create_handover_checklist;
GO

Update invoices set tax = 0.1 where type = 1
Update vehicle_models set reservation_fee = 10000
GO

