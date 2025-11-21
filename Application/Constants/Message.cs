using Application.Dtos.BusinessVariable.Request;
using Application.Dtos.Vehicle.Request;
using Application.Dtos.VehicleChecklist.Request;
using Application.Dtos.VehicleChecklistItem.Request;
using Application.Dtos.VehicleSegment.Request;

namespace Application.Constants
{
    public static class Message
    {
        public static class RoleMessage
        {
            public const string NotFound = "role.not_found";
        }

        //Register
        public static class UserMessage
        {
            //auth

            public const string MissingToken = "user.missing_token";
            public const string InvalidAccessToken = "user.invalid_access_token";
            public const string InvalidRefreshToken = "user.invalid_refresh_token";
            public const string UserIdIsRequired = "user.user_id_is_required";
            public const string NotHavePassword = "user.is_google_linked_not_password";
            public const string NotInputEmail = "user.google_credential_required";

            //otp

            public const string InvalidOTP = "user.invalid_otp";
            public const string OTPCanNotEmpty = "user.otp_can_not_empty";
            public const string OTPMustHave6Digits = "user.otp_must_have_6_digits";
            public const string RateLimitOtp = "user.rate_limit_otp";
            public const string AttemptOtp = "user.attemp_otp";

            //register

            public const string EmailAlreadyExists = "user.email_already_exists";
            public const string EmailIsRequired = "user.email_require";
            public const string InvalidEmail = "user.invalid_email";
            public const string PasswordTooShort = "user.password_too_short";
            public const string PasswordCanNotEmpty = "user.password_can_not_empty";
            public const string SexIsRequired = "user.sex_can_not_empty";
            public const string ConfirmPasswordIsIncorrect = "user.confirm_password_is_incorrect";
            public const string InvalidUserAge = "user.invalid_user_age";
            public const string InvalidPhone = "user.invalid_phone";
            public const string PhoneIsRequired = "user.phone_require";
            public const string FirstNameIsRequired = "user.first_name_require";
            public const string LastNameIsRequired = "user.last_name_require";
            public const string DateOfBirthIsRequired = "user.date_of_birth_require";
            public const string PhoneAlreadyExist = "user.phone_already_exist";

            //change password

            public const string DoNotHavePermission = "user.do_not_have_permission";
            public const string OldPasswordIsIncorrect = "user.old_password_is_incorrect";
            public const string OldPasswordIsRequired = "user.old_password_require";

            //login

            public const string InvalidEmailOrPassword = "user.invalid_email_or_password";
            public const string Unauthorized = "user.unauthorized";
            public const string InvalidToken = "user.invalid_token";
            public const string NotFound = "user.user_not_found";

            //change avatar

            public const string AvatarNotFound = "user.avatar_not_found";

            //Citizen Identity

            public const string CitizenIdentityNotFound = "user.citizen_identity_not_found";
            public const string CitizenIdentityNumberIsRequired = "user.citizen_identity_number_require";
            public const string InvalidCitizenIdData = "user.invalid_citizen_identity_data";
            public const string CitizenIdentityDuplicate = "user.citizen_identity_duplicate";
            public const string InvalidCitizenIdentityImagesAmount = "user.invalid_citizen_identity_images_amount";
            public const string InvalidCitizenIdDocumentType = "user.invalid_citizen_id_document_type";
            public const string InvalidNameOnOtherPaper = "user.invalid_paper_name_on_other_document";
            public const string InvalidDateOnOtherPaper = "user.invalid_paper_date_on_other_document";
            public const string InvalidCitizenIdFormat = "user.invalid_citizen_id_format";
            


            //Driver License

            public const string InvalidDriverLicenseData = "user.invalid_driver_license_data";
            public const string DriverLicenseNotFound = "user.driver_license_not_found";
            public const string DriverLicenseDuplicate = "user.driver_license_duplicate";
            public const string InvalidDriverLicenseImagesAmount = "user.invalid_driver_license_images_amount";
            public const string InvalidDriverLicenseDocumentType = "user.invalid_driver_license_document_type";
            public const string InvalidDriverLicenseFormat = "user.invalid_driver_license_format";
            // Staff
            public const string StationIdIsRequired = "user.station_id_require";

            // Bank info

            public const string FullNameIsRequired = "user.full_name_require";
        }

        //Common error
        public static class CommonMessage
        {
            public const string TooManyRequest = "common.too_many_request";
            public const string UnexpectedError = "common.unexpected_error";
        }

        public static class VehicleMessage
        {
            public const string NotFound = "vehicle.not_found";
            public const string LicensePlateIsExist = "vehicle.license_plate_is_exist";
            public const string LicensePlateRequired = "vehicle.license_plate_required";
            public const string StationIdRequired = "vehicle.station_id_required";
            public const string ModelIdRequired = "vehicle.model_id_required";
            public const string InvalidLicensePlateFormat = "vehicle.invalid_license_plate_format";
        }

        public static class VehicleModelMessage
        {
            public const string NotFound = "vehicle_model.not_found";
            public const string RentTimeIsNotAvailable = "vehicle_model.rent_time_is_not_available";
            public const string NameIsRequire = "vehicle_model.name_require";
            public const string SeatingCapacityIsRequired = "vehicle_model.seating_capacity_require";
            public const string SeatingCapacityCanNotNegative = "vehicle_model.seating_capacity_can_not_negative";
            public const string NumberOfAirbagIsRequire = "vehicle_model.airbag_require";
            public const string NumberOfAirbagCanNotNegative = "vehicle_model.airbag_can_not_negative";
            public const string MotorPowerIsRequired = "vehicle_model.motor_power_require";
            public const string MotorPowerCanNotNegative = "vehicle_model.motor_power_can_not_negative";
            public const string BatteryCapacityIsRequired = "vehicle_model.battery_capacity_require";
            public const string BatteryCapacityCanNotNegative = "vehicle_model.battery_capacity_can_not_negative";
            public const string EcoRangeKmIsRequired = "vehicle_model.eco_range_km_require";
            public const string EcoRangeKmIsCanNotNegative = "vehicle_model.eco_range_km_can_not_negative";
            public const string SportRangeKmIsRequired = "vehicle_model.sport_range_km_require";
            public const string SportRangeKmCanNotNegative = "vehicle_model.sport_range_km_can_not_negative";
            public const string BrandIdIsRequired = "vehicle_model.brand_id_require";
            public const string SegmentIdIsRequired = "vehicle_model.segment_id_require";
            public const string ImageIdsRequired = "vehicle_model.image_ids_required";
            public const string CostDayIdsRequired = "vehicle_model.cost_per_day_invalid";
            public const string DepositFeeIsRequired = "vehicle_model.deposit_fee_invalid";
            public const string ReservationFeeIsRequired = "vehicle_model.reservation_fee_invalid";
        }

        //change password

        //Cloudinary
        public static class CloudinaryMessage
        {
            public const string NotFoundObjectInFile = "cloudinary.file_not_found";
            public const string InvalidFileType = "cloudinary.invalid_file_type";
            public const string UploadFailed = "failed.upload";
            public const string DeleteFailed = "failed.delete";
            public const string DeleteSuccess = "success.delete";
            public const string UploadSuccess = "success.upload";
            public const string FileRequired = "upload.file_required";
            public const string FileEmpty = "upload.file_empty";
        }

        public static class DispatchMessage
        {
            public const string NotFound = "dispatch.not_found";

            // Validation khi tạo

            public const string ToStationMustDifferent = "dispatch.to_station_must_different";
            public const string ToStationRequied = "dispatch.to_station_require";
            public const string ModelRequied = "dispatch.model_require";
            public const string NumberOfVehicleShouldGreaterThanZero = "dispatch.number_vehicle_greater_than_zero";

            public const string StaffNotInFromStation = "dispatch.staff_not_in_from_station";
            public const string InvalidNumberOfStaffs = "dispatch.invalid_number_of_staffs";
            public const string StaffNotEnoughtInFromStation = "dispatch.staff_not_enought_in_from_station";
            public const string StaffLimitInFromStation = "dispatch.staff_limit_in_from_station";

            public const string InvalidNumberOfVehicles = "dispatch.invalid_number_of_vehicles";
            public const string VehicleNotInFromStation = "dispatch.vehicle_not_in_from_station";
            public const string VehicleNotEnoughtInFromStation = "dispatch.vehicle_not_enought_in_from_station";
            public const string VehicleLimitInFromStation = "dispatch.vehicle_limit_in_from_station";
            public const string VehicleOrStaffNotInFromStation = "dispatch.vehicle_or_staff_not_in_from_station";
            public const string NoStaffNoVehicleReject = "dispatch.no_staff_no_vehicle_reject";

            // Flow cập nhật trạng thái

            public const string OnlyPendingCanApproveReject = "dispatch.only_pending_can_approve_reject";
            public const string OnlyPendingCanCancel = "dispatch.only_pending_can_cancel";
            public const string OnlyApproveCanAssign = "dispatch.only_approved_can_assign";
            public const string OnlyAssignCanReceive = "dispatch.only_assign_can_receive";

            // Quyền

            public const string MustBeToStationAdminForThisAction = "dispatch.must_be_to_station_admin";
            public const string MustBeFromStationAdminForThisAction = "dispatch.must_be_from_station_admin";

            // Input

            public const string InvalidStatus = "dispatch.invalid_status";

            public const string IdNull = "dispatch.id_null";
            public const string FromStationIsRequire = "dispatch.from_station_is_required";
            public const string FinalDescriptionIsRequire = "dispatch.final_description_is_required";
        }

        //Rental Contract
        public static class RentalContractMessage
        {
            public const string UserAlreadyHaveContract = "rental_contract.user_already_have_contract";
            public const string NotFound = "rental_contract.not_found";
            public const string ContractAlreadyProcess = "rental_contract.already_process";
            public const string CanNotCancel = "rental_contract.can_not_cancel";
            public const string StartDateMustBeFuture = "rental_contract.start_date_must_be_future";
            public const string EndDateMustBeAfterStart = "rental_contract.end_date_must_be_after_start";
            public const string IdRequired = "rental_contract.id_required";
            public const string VehicleStatusRequired = "rental_contract.vehicle_status_required_when_has_vehicle";
            public const string InvalidVehicleStatus = "rental_contract.invalid_vehicle_status";
            public const string ModelIdRequired = "rental_contract.model_id_required";
            public const string StationIdRequired = "rental_contract.station_id_required";
            public const string AtLeastOnePartyMustSign = "rental_contract.at_least_one_party_must_sign";

            public static string ContractNotStartYet = "rental_contract.not_start_yet";
        }

        //Station
        public static class StationMessage
        {
            public const string NotFound = "station.not_found";
        }

        public static class PaymentMessage
        {
            public const string InvalidSignature = "payment.invalid_signature";
            public const string MissingAccessKeyPartnerCodeSecretKey = "payment.missing_access_key_partner_code_secret_key";
            public const string NotHavePermission = "payment.not_have_permission";
            public const string InvalidEndpoint = "payment.invalid_end_point";
            public const string FailedToCreateMomoPayment = "payment.failed_to_create_momo_payment";

            public static string InvoiceIdIsRequired = "payment.invoice_id_required";

            public static string FallBackUrlIsRequired = "payment.fallback_url_required";

            public static string ỊnvalidFallBackUrl = "payment.fallback_url_invalid";
            public static string InvalidPaymentMethod = "payment.invalid_payment_method";
        }

        public static class InvoiceMessage
        {
            public const string NotFound = "invoice.not_found";
            public const string ThisInvoiceWasPaidOrCancel = "invoice.this_invoice_was_paid_or_cancel";

            public static string AmountRequired = "invoice.amount_required";
            public static string InvalidAmount = "invoice.invalid_amount";

            public static string NotHandoverPayment = "invoice.not_handover_payment";

            public static string? InvalidInvoiceType = "invoice.invalid_invoice_type";

            public static string ForbiddenInvoiceAccess = "invoice.forbidden_invoice_access";

            public static string InvalidUnitPrice = "invoice_item.invalid_unit_price";

            public static string InvalidQuantity = "invoice_item.invalid_quantity";

            public static string InvoiceItemInvalidType = "invoice_item.invalid_type";
        }

        public static class JsonMessage
        {
            public const string ParsingFailed = "json.parsing_failed";
        }

        public static class VehicleSegmentMessage
        {
            public const string NotFound = "vehicle_segment.not_found";
            public const string NameIsRequired = "vehicle_segment.name_required";
            public const string DescriptionIsRequired = "vehicle_segment.description_required";
            public const string NameAlreadyExists = "vehicle_segment.name_already_exists";
        }

        //VEHICLE IMAGE
        // public static class ModelImageMessage
        // {
        //     public const string ModelImageNotFound = "model_image.not_found";
        //     public const string InvalidModelId = "model_image.invalid_model_id";
        //     public const string UploadFailed = "model_image.upload_failed";
        //     public const string DeleteFailed = "model_image.delete_failed";
        //     public const string NoFileChosen = "model_image.no_file_chosen";
        // }

        public static class TicketMessage
        {
            public const string NotFound = "ticket.not_found";
            public const string AlreadyEscalated = "ticket.already_escalated";
            public const string TitleRequired = "ticket.title_required";
            public const string DescriptionRequired = "ticket.description_required";
            public const string InvalidType = "ticket.invalid_type";
            public const string TitleTooLong = "ticket.title_too_long";
            public const string AlreadyResolved = "ticket.already_resolved";
        }

        //upload
        // public static class UploadMessage
        // {
        //     public const string EmptyFile = "upload.empty_file";
        //     public const string InvalidFile = "upload.invalid_file";
        //     public const string Failed = "upload.failed";
        // }

        public static class StationFeedbackMessage
        {
            public const string NotFound = "station_feedback.not_found";
            public const string InvalidRating = "station_feedback.invalid_rating";
            public const string ContentTooLong = "station_feedback.content_too_long";
        }

        public static class VehicleModelImageMessage
        {
            public const string NotFound = "vehicle_model_image.main_image_not_found";
        }

        public static class VehicleComponentMessage
        {
            public const string NotFound = "vehicle_component.not_found";

            public static string NameIsRequired = "vehicle_component.name_required";

            public static string DescriptionIsRequired = "vehicle_component.description_required";

            public static string DamageFeeIsRequired = "vehicle_component.damage_fee_required";

            public static string DamageFeeMustBePositive = "vehicle_component.damage_fee_must_be_non_negative";

            public static string InvalidComponentIds = "vehicle_component.invalid_component_ids";
        }

        public static class VehicleChecklistMessage
        {
            public const string NotFound = "vehicle_checklist.not_found";

            public static string ThisChecklistAlreadyProcess = "vehicle_checklist.already_process";
            public static string AtLeastOnePartyMustSign = "vehicle_checklist.at_least_one_party_must_sign";

            public static string InvalidType = "vehicle_checklist.invalid_type";

            public static string InvalidStatus = "vehicle_checklist.invalid_status";
        }

        public static class VehicleChecklistItemMessage
        {
            public const string NotFound = "vehicle_checklist_item.not_found";
            public const string InvalidStatus = "vehicle_checklist_item.invalid_status";
            public const string ItemIdRequired = "vehicle_checklist_item.item_id_required";
        }

        public static class StatisticMessage
        {
            public const string NoCustomerData = "statistic.no_customer_data";
            public const string NoInvoiceData = "statistic.no_invoice_data";
            public const string NoVehicleData = "statistic.no_vehicle_data";

            public const string FailedToCalculateRevenue = "statistic.failed_to_calculate_revenue";
            public const string FailedToCalculateCustomerChange = "statistic.failed_to_calculate_customer_change";
            public const string FailedToCalculateInvoiceChange = "statistic.failed_to_calculate_invoice_change";

            public const string NoVehicleModelData = "";
        }

        public static class BrandMessage
        {
            public const string NameIsRequired = "brand.name_require";
            public const string DescriptionIsRequired = "brand.description_require";
            public const string FoundedYearIsRequired = "brand.founded_year_require";
            public const string CountryIsRequired = "brand.country_require";
            public const string NotFound = "brand.not_found";
        }

        public static class BusinessVariable
        {
            public const string NotFound = "business_variable.not_found";
            public const string ValueIsRequired = "business_variable.value_is_required";
            public const string ValueMustBeGreaterThanZero = "business_variable.value_must_be_greater_than_zero";
        }
    }
}