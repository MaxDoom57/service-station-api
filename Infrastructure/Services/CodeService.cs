using Application.DTOs.Codes;
using Application.DTOs.Lookups;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Services
{
    public class CodeService
    {
        private readonly IDynamicDbContextFactory _factory;
        private readonly IUserRequestContext _userContext;
        private readonly CommonLookupService _lookup;
        private readonly IUserKeyService _userKeyService;
        private readonly IValidationService _validator;

        public CodeService(
        IDynamicDbContextFactory factory,
        IUserRequestContext userContext,
        CommonLookupService lookup,
        IUserKeyService userKeyService,
        IValidationService validator)
        {
            _factory = factory;
            _userContext = userContext;
            _lookup = lookup;
            _userKeyService = userKeyService;
            _validator = validator;
        }

        public async Task<List<CodeTypes>> GetCodeTypesAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            return await db.Set<Control>()
                .AsNoTracking()
                .Where(x => x.fInAct == false && x.fUsrAcs == true)
                .OrderBy(x => x.ConNm)
                .Select(x => new CodeTypes
                {
                    ConKy = x.ConKy,
                    ConCd = x.ConCd,
                    ConNm = x.ConNm
                })
                .ToListAsync();
        }

        public async Task<List<CodeByTypeDto>> GetCodesByTypeAsync(GetCodesByTypeRequestDto request)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            var result = new List<CodeByTypeDto>();

            short conKy = await _lookup.GetCodeTypeKeyAsync(request.ConCd);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                Select 
                    Code,
                    CdNm,
                    CdKy,
                    fInAct,
                    0 as Updated
                from Cdmasqry
                where ConKy = @ConKy
                  and CKy = @CKy
            ";

            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(new SqlParameter("@ConKy", conKy));
            cmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new CodeByTypeDto
                {
                    Code = reader["Code"]?.ToString() ?? "",
                    CdNm = reader["CdNm"]?.ToString() ?? "",
                    CdKy = Convert.ToInt32(reader["CdKy"]),
                    fInAct = Convert.ToBoolean(reader["fInAct"]),
                });
            }

            return result;
        }

        public async Task<CodeResponseDto> CreateCodeAsync(CreateCodeDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            //  Validate ConCd
            short conKy = await _lookup.GetCodeTypeKeyAsync(dto.ConCd);
            if (conKy <= 0)
                throw new InvalidOperationException("Invalid ConCd. Code type does not exist.");

            //  Check duplicate Code
            using (var chkCmd = conn.CreateCommand())
            {
                chkCmd.CommandText = @"
                    SELECT COUNT(1)
                    FROM CdMas
                    WHERE Code = @Code
                      AND ConKy = @ConKy
                      AND CKy = @CKy
                ";

                chkCmd.Parameters.Add(new SqlParameter("@Code", dto.Code.Trim()));
                chkCmd.Parameters.Add(new SqlParameter("@ConKy", conKy));
                chkCmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

                int exists = Convert.ToInt32(await chkCmd.ExecuteScalarAsync());
                if (exists > 0)
                    throw new InvalidOperationException("Code already exists for this code type.");
            }

            // 3️⃣ Insert
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO CdMas (CKy, Code, CdNm, ConCd, ConKy)
                VALUES (@CKy, @Code, @CdNm, @ConCd, @ConKy);
                SELECT SCOPE_IDENTITY();
            ";

            cmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
            cmd.Parameters.Add(new SqlParameter("@Code", dto.Code.Trim()));
            cmd.Parameters.Add(new SqlParameter("@CdNm", string.IsNullOrWhiteSpace(dto.CdNm) ? " " : dto.CdNm.Trim()));
            cmd.Parameters.Add(new SqlParameter("@ConCd", dto.ConCd));
            cmd.Parameters.Add(new SqlParameter("@ConKy", conKy));

            int cdKy = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return new CodeResponseDto
            {
                CdKy = cdKy,
                Message = "Code created successfully"
            };
        }

        public async Task<CodeResponseDto> UpdateCodeAsync(int cdKy, UpdateCodeDto dto)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            // 1️⃣ Check record exists
            short conKy;
            using (var getCmd = conn.CreateCommand())
            {
                getCmd.CommandText = "SELECT ConKy FROM CdMas WHERE CdKy = @CdKy";
                getCmd.Parameters.Add(new SqlParameter("@CdKy", cdKy));

                var result = await getCmd.ExecuteScalarAsync();
                if (result == null)
                    throw new KeyNotFoundException("Code not found.");

                conKy = Convert.ToInt16(result);
            }

            // 2️⃣ Check duplicate Code (exclude current)
            using (var chkCmd = conn.CreateCommand())
            {
                chkCmd.CommandText = @"
                    SELECT COUNT(1)
                    FROM CdMas
                    WHERE Code = @Code
                      AND ConKy = @ConKy
                      AND CKy = @CKy
                      AND CdKy <> @CdKy
                ";

                chkCmd.Parameters.Add(new SqlParameter("@Code", dto.Code.Trim()));
                chkCmd.Parameters.Add(new SqlParameter("@ConKy", conKy));
                chkCmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
                chkCmd.Parameters.Add(new SqlParameter("@CdKy", cdKy));

                int exists = Convert.ToInt32(await chkCmd.ExecuteScalarAsync());
                if (exists > 0)
                    throw new InvalidOperationException("Another code with the same value already exists.");
            }

            // 3️⃣ Update
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE CdMas
                SET 
                    Code = @Code,
                    CdNm = @CdNm,
                    fInAct = @fInAct
                WHERE CdKy = @CdKy
            ";

            cmd.Parameters.Add(new SqlParameter("@Code", dto.Code.Trim()));
            cmd.Parameters.Add(new SqlParameter("@CdNm", string.IsNullOrWhiteSpace(dto.CdNm) ? " " : dto.CdNm.Trim()));
            cmd.Parameters.Add(new SqlParameter("@fInAct", dto.fInAct));
            cmd.Parameters.Add(new SqlParameter("@CdKy", cdKy));

            await cmd.ExecuteNonQueryAsync();

            return new CodeResponseDto
            {
                CdKy = cdKy,
                Message = "Code updated successfully"
            };
        }

        public async Task<CodeResponseDto> DeleteCodeAsync(int cdKy)
        {
            using var db = await _factory.CreateDbContextAsync();
            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            // 1 Check existence and current status
            bool isInactive;

            using (var chkCmd = conn.CreateCommand())
            {
                chkCmd.CommandText = @"
                                    SELECT fInAct
                                    FROM CdMas
                                    WHERE CdKy = @CdKy
                                ";

                chkCmd.Parameters.Add(new SqlParameter("@CdKy", cdKy));

                var result = await chkCmd.ExecuteScalarAsync();
                if (result == null)
                    throw new KeyNotFoundException("Code not found.");

                isInactive = Convert.ToBoolean(result);
            }

            if (isInactive)
                throw new InvalidOperationException("Code is already inactive.");

            // 2 Soft delete
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                                    UPDATE CdMas
                                    SET fInAct = 1
                                    WHERE CdKy = @CdKy
                                ";

            cmd.Parameters.Add(new SqlParameter("@CdKy", cdKy));
            await cmd.ExecuteNonQueryAsync();

            return new CodeResponseDto
            {
                CdKy = cdKy,
                Message = "Code deleted successfully"
            };
        }

    }
}
