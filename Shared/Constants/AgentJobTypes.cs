namespace Shared.Constants
{
    public static class AgentJobTypes
    {
        // ─── Vehicle ────────────────────────────────────────────────────────────────
        public const string GetActiveVehicles      = "GET_ACTIVE_VEHICLES";
        public const string GetAllVehiclesDetailed = "GET_ALL_VEHICLES_DETAILED";
        public const string GetVehicleDetails      = "GET_VEHICLE_DETAILS";
        public const string RegisterVehicle        = "REGISTER_VEHICLE";
        public const string UpdateVehicle          = "UPDATE_VEHICLE";
        public const string DeleteVehicle          = "DELETE_VEHICLE";

        // ─── Vehicle Type ────────────────────────────────────────────────────────────
        public const string GetVehicleTypes    = "GET_VEHICLE_TYPES";
        public const string AddVehicleType     = "ADD_VEHICLE_TYPE";
        public const string UpdateVehicleType  = "UPDATE_VEHICLE_TYPE";
        public const string DeleteVehicleType  = "DELETE_VEHICLE_TYPE";

        // ─── Validation ──────────────────────────────────────────────────────────────
        public const string ValidateCompanyKey = "VALIDATE_COMPANY_KEY";
        public const string ValidateItemCode   = "VALIDATE_ITEM_CODE";
        public const string ValidateItemType   = "VALIDATE_ITEM_TYPE";
        public const string ValidateUnitKey    = "VALIDATE_UNIT_KEY";
        public const string ValidateUserKey    = "VALIDATE_USER_KEY";
        public const string ValidateAdrNm      = "VALIDATE_ADR_NM";

        // ─── User ────────────────────────────────────────────────────────────────────
        public const string GetActiveUsers  = "GET_ACTIVE_USERS";
        public const string CreateUser      = "CREATE_USER";
        public const string ChangePassword  = "CHANGE_PASSWORD";
        public const string DeleteUser      = "DELETE_USER";

        // ─── Bay ─────────────────────────────────────────────────────────────────────
        public const string GetActiveBays = "GET_ACTIVE_BAYS";
        public const string AddBay        = "ADD_BAY";
        public const string UpdateBay     = "UPDATE_BAY";
        public const string DeleteBay     = "DELETE_BAY";

        // ─── Bay Workers ─────────────────────────────────────────────────────────────
        public const string GetAllBayWorkers   = "GET_ALL_BAY_WORKERS";
        public const string AddWorkerToBay     = "ADD_WORKER_TO_BAY";
        public const string UpdateWorkerInBay  = "UPDATE_WORKER_IN_BAY";
        public const string DeleteWorkerFromBay = "DELETE_WORKER_FROM_BAY";

        // ─── Bay Control ─────────────────────────────────────────────────────────────
        public const string GetAvailableBaysNow        = "GET_AVAILABLE_BAYS_NOW";
        public const string GetAllBaysStatus           = "GET_ALL_BAYS_STATUS";
        public const string GetReservableBays          = "GET_RESERVABLE_BAYS";
        public const string GetBayReservations         = "GET_BAY_RESERVATIONS";
        public const string CreateBayReservation       = "CREATE_BAY_RESERVATION";
        public const string UpdateBayReservationStatus = "UPDATE_BAY_RESERVATION_STATUS";
        public const string UpdateBayControl           = "UPDATE_BAY_CONTROL";
        public const string DeleteBayReservation       = "DELETE_BAY_RESERVATION";

        // ─── Reservations ─────────────────────────────────────────────────────────────
        public const string GetReservations    = "GET_RESERVATIONS";
        public const string CreateReservation  = "CREATE_RESERVATION";
        public const string UpdateReservation  = "UPDATE_RESERVATION";
        public const string DeleteReservation  = "DELETE_RESERVATION";
        public const string ApproveReservation = "APPROVE_RESERVATION";

        // ─── Packages ────────────────────────────────────────────────────────────────
        public const string GetPackages             = "GET_PACKAGES";
        public const string AddPackage              = "ADD_PACKAGE";
        public const string UpdatePackage           = "UPDATE_PACKAGE";
        public const string DeletePackage           = "DELETE_PACKAGE";
        public const string GetPackageDetails       = "GET_PACKAGE_DETAILS";
        public const string GetAllPackagesWithItems = "GET_ALL_PACKAGES_WITH_ITEMS";

        // ─── Service Orders ────────────────────────────────────────────────────────
        public const string CreateServiceOrder     = "CREATE_SERVICE_ORDER";
        public const string AddServiceItem         = "ADD_SERVICE_ITEM";
        public const string ApproveServiceItem     = "APPROVE_SERVICE_ITEM";
        public const string UpdateServiceItemStatus = "UPDATE_SERVICE_ITEM_STATUS";
        public const string GetServiceOrders       = "GET_SERVICE_ORDERS";
        public const string GetServiceOrderDetails = "GET_SERVICE_ORDER_DETAILS";

        // ─── Items ───────────────────────────────────────────────────────────────────
        public const string GetAllItems          = "GET_ALL_ITEMS";
        public const string GetItemsWithoutFInAct = "GET_ITEMS_WITHOUT_FINACT";
        public const string AddItem              = "ADD_ITEM";
        public const string UpdateItem           = "UPDATE_ITEM";
        public const string DeleteItem           = "DELETE_ITEM";
        public const string GetItemBatches       = "GET_ITEM_BATCHES";
        public const string AddItemBatch         = "ADD_ITEM_BATCH";
        public const string UpdateItemBatch      = "UPDATE_ITEM_BATCH";

        // ─── Customers ───────────────────────────────────────────────────────────────
        public const string GetCustomers    = "GET_CUSTOMERS";
        public const string AddCustomer     = "ADD_CUSTOMER";
        public const string UpdateCustomer  = "UPDATE_CUSTOMER";

        // ─── Customer Accounts ───────────────────────────────────────────────────────
        public const string GetCustomerAccounts = "GET_CUSTOMER_ACCOUNTS";

        // ─── Sales Accounts ───────────────────────────────────────────────────────────
        public const string GetSalesAccounts = "GET_SALES_ACCOUNTS";

        // ─── Payment Terms ────────────────────────────────────────────────────────────
        public const string GetPaymentTerms = "GET_PAYMENT_TERMS";

        // ─── Orders ──────────────────────────────────────────────────────────────────
        public const string CreateOrder      = "CREATE_ORDER";
        public const string UpdateOrder      = "UPDATE_ORDER";
        public const string DeleteOrder      = "DELETE_ORDER";
        public const string GetAllOrders     = "GET_ALL_ORDERS";
        public const string GetOrderByOrderNo = "GET_ORDER_BY_ORDER_NO";
        public const string GetOrderByKey    = "GET_ORDER_BY_KEY";

        // ─── Purchase Orders ──────────────────────────────────────────────────────────
        public const string GetPurchaseOrder    = "GET_PURCHASE_ORDER";
        public const string CreatePurchaseOrder = "CREATE_PURCHASE_ORDER";
        public const string UpdatePurchaseOrder = "UPDATE_PURCHASE_ORDER";
        public const string DeletePurchaseOrder = "DELETE_PURCHASE_ORDER";

        // ─── Codes ────────────────────────────────────────────────────────────────────
        public const string GetCodeTypes    = "GET_CODE_TYPES";
        public const string GetCodesByType  = "GET_CODES_BY_TYPE";
        public const string CreateCode      = "CREATE_CODE";
        public const string UpdateCode      = "UPDATE_CODE";
        public const string DeleteCode      = "DELETE_CODE";

        // ─── Lookups ─────────────────────────────────────────────────────────────────
        public const string GetItemCategory1     = "GET_ITEM_CATEGORY1";
        public const string GetItemCategory2     = "GET_ITEM_CATEGORY2";
        public const string GetItemCategory3     = "GET_ITEM_CATEGORY3";
        public const string GetItemCategory4     = "GET_ITEM_CATEGORY4";
        public const string GetLastTransactionNo = "GET_LAST_TRANSACTION_NO";

        // ─── Common Lookups ───────────────────────────────────────────────────────────
        public const string GetAccountTypeKey        = "GET_ACCOUNT_TYPE_KEY";
        public const string GetItemTypeKey           = "GET_ITEM_TYPE_KEY";
        public const string GetTranTypeKey           = "GET_TRAN_TYPE_KEY";
        public const string GetPaymentTermKey        = "GET_PAYMENT_TERM_KEY";
        public const string GetAddressKeyByAccKy     = "GET_ADDRESS_KEY_BY_ACC_KY";
        public const string GetSaleTransactionCodes  = "GET_SALE_TRANSACTION_CODES";
        public const string GetDefaultSalesAccountKey = "GET_DEFAULT_SALES_ACCOUNT_KEY";
        public const string GetTrnKyByTrnNo          = "GET_TRN_KY_BY_TRN_NO";
        public const string GetCompanyNameByCKy      = "GET_COMPANY_NAME_BY_CKY";
        public const string GetTrnTypKy              = "GET_TRN_TYP_KY";
        public const string GetPaymentModeKey        = "GET_PAYMENT_MODE_KEY";
        public const string GetCodeTypeKey           = "GET_CODE_TYPE_KEY";
        public const string GetTranNumberLast        = "GET_TRAN_NUMBER_LAST";
        public const string IncrementTranNumberLast  = "INCREMENT_TRAN_NUMBER_LAST";
        public const string GetAmt1AccKy             = "GET_AMT1_ACC_KY";
        public const string GetAccessLevel           = "GET_ACCESS_LEVEL";
    }
}
