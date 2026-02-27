using Application.DTOs.Reports;
using Application.DTOs.Reports.Report_Account_Balance_As_At;
using Application.DTOs.Reports.Report_Account_Ledger;
using Application.DTOs.Reports.Report_Account_Transaction_Details;
using Application.DTOs.Reports.Report_Credit_Sales_Summary;
using Application.DTOs.Reports.Report_Creditor_Age_Analysis;
using Application.DTOs.Reports.Report_Creditors_Due_Statement;
using Application.DTOs.Reports.Report_Gross_Profit;
using Application.DTOs.Reports.Report_Item_Batch_Details;
using Application.DTOs.Reports.Report_Item_Cat1_Wise_Transaction_Summary;
using Application.DTOs.Reports.Report_Non_Active_Customer_List;
using Application.DTOs.Reports.Report_Non_Moving_Item_List;
using Application.DTOs.Reports.Report_ReOrder_Details;
using Application.DTOs.Reports.Report_Return_Cheque_Details;
using Application.DTOs.Reports.Report_Sales_Details;
using Application.DTOs.Reports.Report_Sales_Details_By_Item;
using Application.DTOs.Reports.Report_Sales_Details_by_Payment_Mode;
using Application.DTOs.Reports.Report_Sales_Summary_By_Rep;
using Application.DTOs.Reports.Report_SetOff_Details_Report;
using Application.DTOs.Reports.Report_Stock_As_At;
using Application.DTOs.Reports.Report_Stock_Ledger;
using Application.DTOs.Reports.Report_Stock_Movement;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/ssms/v0.1/Reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _service;

        public ReportsController(ReportService service)
        {
            _service = service;
        }

        [HttpGet("SalesByRep")]
        public async Task<IActionResult> GetSalesByRep([FromBody] ReportSalesByRepRequestDto request)
        {
            try
            {
                var result = await _service.GetSalesByRepAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("SalesDetailsByPaymentMode")]
        public async Task<IActionResult> GetSalesDetailsByPaymentMode([FromBody] SalesDetailsByPaymentModeRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result =
                    await _service.GetSalesDetailsByPaymentModeAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        //[HttpGet("StockMovementSummary")]
        //public async Task<IActionResult> GetStockMovementSummary([FromBody] StockMovementSummaryRequestDto request)
        //{
        //    try
        //    {
        //        if (request == null)
        //            return BadRequest("Request body is required");

        //        var result =
        //            await _service.GetStockMovementSummaryAsync(request);

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        [HttpGet("TransactionDetails")]
        public async Task<IActionResult> GetTransactionDetailsReport([FromBody] TrnDetReportRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetTransactionDetailsReportAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("SalesRepWiseCustomer")]
        public async Task<IActionResult> GetSalesRepWiseCustomer([FromBody] SalesRepWiseCustomerRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result =
                    await _service.GetSalesRepWiseCustomerAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("SalesItemReport")]
        public async Task<IActionResult> GetSalesItemReport([FromBody] SalesItemReportRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetSalesItemReportAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("NonPerformedItems")]
        public async Task<IActionResult> GetNonPerformedItems([FromBody] NonPerformedItemRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetNonPerformedItemsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("NonPerformedCustomers")]
        public async Task<IActionResult> GetNonPerformedCustomers([FromBody] NonPerformedCustomerRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetNonPerformedCustomersAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("ChequeReturnDetails")]
        public async Task<IActionResult> GetChequeReturnDetails([FromBody] ChqReturnReportRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetChequeReturnDetailsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("AccountLedger")]
        public async Task<IActionResult> GetAccountLedger([FromBody] AccLedgerReportRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetAccountLedgerAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("AccountTransactionDetails")]
        public async Task<IActionResult> GetAccountTransactionDetails([FromBody] AccTrnDetailsReportRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetAccountTransactionDetailsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("AccountBalanceAsAt")]
        public async Task<IActionResult> GetAccountBalanceAsAt([FromBody] AccBalanceAsAtRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetAccountBalanceAsAtAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("ItemCat1WiseTransactionSummary")]
        public async Task<IActionResult> GetItemCat1WiseTransactionSummary([FromBody] ItmCat1WiseTrnSumRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                if (request.LocKy == 0)
                    return BadRequest("Location is required");

                if (request.ItmTypKy == 0)
                    return BadRequest("Item type is required");

                var result = await _service.GetItemCat1WiseTransactionSummaryAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("SetOffDetails")]
        public async Task<IActionResult> GetSetOffDetails([FromBody] SetOffDetailsReportRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetSetOffDetailsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("GrossProfit")]
        public async Task<IActionResult> GetGrossProfitItemLocation([FromBody] GrossProfitItemLocationRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetGrossProfitItemLocationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("CreditSalesSummary")]
        public async Task<IActionResult> GetLssCreditInvoiceSummary([FromBody] LssCreditInvoiceSummaryRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetLssCreditInvoiceSummaryAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("AgeAnalysis")]
        public async Task<IActionResult> GetAgeAnalysis([FromBody] AgeAnalysisRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetAgeAnalysisAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("DueStatement")]
        public async Task<IActionResult> GetDebtorsDueStatement([FromBody] DebtorsDueStatementRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetDebtorsDueStatementAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("StockLedger")]
        public async Task<IActionResult> GetStockLedger([FromBody] StockLedgerRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetStockLedgerAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("StockAsAt")]
        public async Task<IActionResult> GetStockAsAt([FromBody] StockAsAtRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetStockAsAtAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("ItemBatchDetails")]
        public async Task<IActionResult> GetItemBatchDetails([FromBody] ItemBatchReportRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetItemBatchReportAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("ReOrderItems")]
        public async Task<IActionResult> GetReOrderItems([FromBody] ReOrderItemsRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request body is required");

                var result = await _service.GetReOrderItemsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("CustomerAddressDetails")]
        public async Task<IActionResult> GetAllAddressDetails()
        {
            try
            {
                var result = await _service.GetAllAddressDetailsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("AccountAddressRel")]
        public async Task<IActionResult> GetAccountAddressDetails()
        {
            try
            {
                var result = await _service.GetAccountAddressDetailsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("ItemsVsf")]
        public async Task<IActionResult> GetAllItemsVsf()
        {
            try
            {
                var result = await _service.GetAllItemsVsfAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
