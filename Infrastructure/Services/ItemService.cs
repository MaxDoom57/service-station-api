using Application.DTOs;
using Application.DTOs.ItemBatch;
using Application.DTOs.Items;
using Application.Interfaces;
using Domain.Entities;
using Shared.Constants;

namespace Infrastructure.Services
{
    public class ItemService
    {
        private readonly IAgentJobDispatcher _dispatcher;
        private readonly IUserRequestContext _userContext;
        private readonly IUserKeyService _userKeyService;

        public ItemService(
            IAgentJobDispatcher dispatcher,
            IUserRequestContext userContext,
            IUserKeyService userKeyService)
        {
            _dispatcher     = dispatcher;
            _userContext    = userContext;
            _userKeyService = userKeyService;
        }

        //--------------------------------------------------
        // GET ITEMS DETAILS
        //--------------------------------------------------
        public async Task<List<ItemDto>> GetAllItemsAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetAllItems,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<List<ItemDto>>() ?? new();
        }

        public async Task<List<ItemsWithoutFInActDTO>> GetItemsWithoutFInActAsync()
        {
            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetItemsWithoutFInAct,
                payload:    new { CompanyKey = _userContext.CompanyKey });

            if (!result.Success) throw new Exception(result.Error ?? "Agent error");
            return result.Deserialize<List<ItemsWithoutFInActDTO>>() ?? new();
        }

        //--------------------------------------------------
        // ADD NEW ITEM
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> AddItemAsync(AddItemDTO dto)
        {
            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
            if (userKey == null)
                return (false, "User key not found", 400);

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.AddItem,
                payload:    new { Dto = dto, UserKey = userKey.Value, CompanyKey = _userContext.CompanyKey });

            if (!result.Success)
                return (false, result.Error ?? "Failed to add item", 500);

            return (true, "Item added successfully.", 201);
        }

        //--------------------------------------------------
        // UPDATE EXISTING ITEM
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> UpdateItemAsync(UpdateItemDTO dto)
        {
            if (dto.itemKey <= 0)
                return (false, "Invalid Item Key", 400);

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateItem,
                payload:    new { Dto = dto });

            if (!result.Success)
                return (false, result.Error ?? "Failed to update item", 500);

            return (true, "Item updated successfully", 200);
        }

        //--------------------------------------------------
        // DELETE EXISTING ITEM
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> DeleteItemAsync(int itemKey)
        {
            if (itemKey <= 0)
                return (false, "Invalid Item Key", 400);

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.DeleteItem,
                payload:    new { ItemKey = itemKey });

            if (!result.Success)
                return (false, result.Error ?? "Failed to delete item", 500);

            return (true, "Item deleted successfully", 200);
        }

        //--------------------------------------------------
        // GET ITEM BATCH DETAILS
        //--------------------------------------------------
        public async Task<List<vewItmBatch>> GetItemBatchesAsync(int itemKey)
        {
            if (itemKey <= 0) return new();

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.GetItemBatches,
                payload:    new { ItemKey = itemKey });

            if (!result.Success) return new();
            return result.Deserialize<List<vewItmBatch>>() ?? new();
        }

        //--------------------------------------------------
        // ADD NEW ITEM BATCH
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode, int? batchKey)> AddItemBatchAsync(AddItemBatchDTO dto)
        {
            if (dto.itemKey <= 0)
                return (false, "Invalid ItemKey", 400, null);

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.AddItemBatch,
                payload:    new { Dto = dto });

            if (!result.Success)
                return (false, result.Error ?? "Failed to add item batch", 500, null);

            var batchKey = result.Deserialize<int?>();
            return (true, "Item batch added successfully", 201, batchKey);
        }

        //--------------------------------------------------
        // UPDATE EXISTING ITEM BATCH
        //--------------------------------------------------
        public async Task<(bool success, string message, int statusCode)> UpdateItemBatchAsync(UpdateItemBatchDTO dto)
        {
            if (dto.itemBatchKey <= 0) return (false, "Invalid ItemBatchKey", 400);
            if (dto.itemKey <= 0)      return (false, "Invalid ItemKey", 400);

            var result = await _dispatcher.DispatchAndWaitAsync(
                companyKey: _userContext.CompanyKey,
                jobType:    AgentJobTypes.UpdateItemBatch,
                payload:    new { Dto = dto });

            if (!result.Success)
                return (false, result.Error ?? "Failed to update item batch", 500);

            return (true, "Item batch updated successfully", 200);
        }
    }
}
