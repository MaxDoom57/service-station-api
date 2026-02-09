using Application.DTOs.Customers;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Net;
using System.Security.Cryptography;

public class CustomerService
{
    private readonly IDynamicDbContextFactory _factory;
    private readonly IUserRequestContext _userContext;
    private readonly CommonLookupService _lookup;
    private readonly IUserKeyService _userKeyService;
    private readonly IValidationService _validator;

    public CustomerService(
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

    // Get all active customers
    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        using var db = await _factory.CreateDbContextAsync();

        var result = await (
            from c in db.Customers
            join a in db.Addresses on c.AdrKy equals a.AdrKy into adr
            from a in adr.DefaultIfEmpty() // LEFT JOIN
            where c.fInAct == false
            select new CustomerDto
            {
                AccTyp = c.AccTyp,
                AccNm = c.AccNm,
                AccKy = c.AccKy,
                AdrNm = c.AdrNm,
                AdrCd = c.AdrCd,
                AdrKy = c.AdrKy,
                NIC = c.NIC,
                Address = c.Address,
                Town = c.Town,
                City = c.City,
                TP1 = c.TP1,

                // GPSLoc = a.GPSLoc -- Column missing in DB
            }
        ).ToListAsync();

        return result;
    }



    // Add new customer address
    public async Task<int> AddCustomerAsync(AddCustomerAddressDto dto)
    {
        Console.WriteLine("Function started for adding customer address..............");

        string ourCd = dto.ourCd ?? "CUS";

        // Email validation
        if (!string.IsNullOrWhiteSpace(dto.EMail))
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.EMail, pattern))
                throw new Exception("Invalid email format");
        }

        // Mobile validation helper
        bool IsValidMobile(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return true;
            return number.Length == 10 && number.All(char.IsDigit);
        }

        if (!IsValidMobile(dto.TP1))
            throw new Exception("TP1 must contain exactly 10 digits");

        if (!IsValidMobile(dto.TP2))
            throw new Exception("TP2 must contain exactly 10 digits");

        if (await _validator.IsExistAdrNm(dto.AdrNm))
            throw new Exception("Address name already exists");

        using var db = await _factory.CreateDbContextAsync();
        using var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();

        using var tx = conn.BeginTransaction();
        Console.WriteLine("Transaction started for adding customer address..............");
        try
        {
            string adrCd = await GenerateUniqueAdrCdAsync(conn, tx, dto.AdrNm);

            var userKey = await _userKeyService.GetUserKeyAsync(_userContext.UserId, _userContext.CompanyKey);
            if (userKey == null)
                throw new Exception("User key not found for this user");

            // -------------------------------------------------------------
            // 1) Insert into Address table
            // -------------------------------------------------------------
            using var cmd1 = conn.CreateCommand();
            cmd1.Transaction = tx;

            cmd1.CommandText = @"
            INSERT INTO Address
            (
                CKy, AdrCd, fInAct, AdrNm, FstNm, MidNm, LstNm,
                Address, TP1, TP2, EMail,
                EntUsrKy, EntDtm
            )
            OUTPUT INSERTED.AdrKy
            VALUES
            (
                @CKy, @AdrCd, 0, @AdrNm, @FstNm, @MidNm, @LstNm,
                @Address, @TP1, @TP2, @EMail,
                @EntUsrKy, @EntDtm
            );";

            cmd1.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
            cmd1.Parameters.Add(new SqlParameter("@AdrCd", adrCd));
            cmd1.Parameters.Add(new SqlParameter("@AdrNm", dto.AdrNm ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@FstNm", dto.FstNm ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@MidNm", dto.MidNm ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@LstNm", dto.LstNm ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@Address", dto.Address ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@TP1", dto.TP1 ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@TP2", dto.TP2 ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@EMail", dto.EMail ?? (object)DBNull.Value));
            cmd1.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));
            cmd1.Parameters.Add(new SqlParameter("@EntDtm", DateTime.Now));

            var adrKyObj = await cmd1.ExecuteScalarAsync();
            int adrKy = Convert.ToInt32(adrKyObj);

            // -------------------------------------------------------------
            // 2) Insert into AccMas table
            // -------------------------------------------------------------
            // Get AccTypKy for "CUS"
            short accTypKy = await _lookup.GetAccountTypeKeyAsync(ourCd);
            
            // Generate unique AccCd with datetime format
            string accCd = await GenerateUniqueAccCdAsync(conn, tx);

            using var cmd2 = conn.CreateCommand();
            cmd2.Transaction = tx;

            cmd2.CommandText = @"
            INSERT INTO AccMas
            (
                CKy, AccCd, AccNm, AccTyp, AccTypKy, fInAct,
                EntUsrKy, EntDtm
            )
            VALUES
            (
                @CKy, @AccCd, @AccNm, @AccTyp, @AccTypKy, 0,
                @EntUsrKy, @EntDtm
            );";

            cmd2.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
            cmd2.Parameters.Add(new SqlParameter("@AccCd", accCd));
            cmd2.Parameters.Add(new SqlParameter("@AccNm", dto.AdrNm ?? (object)DBNull.Value)); // Same as AdrNm
            cmd2.Parameters.Add(new SqlParameter("@AccTyp", ourCd)); // "CUS"
            cmd2.Parameters.Add(new SqlParameter("@AccTypKy", accTypKy));
            cmd2.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));
            cmd2.Parameters.Add(new SqlParameter("@EntDtm", DateTime.Now));

            await cmd2.ExecuteNonQueryAsync();

            // Retrieve the AccKy after insert (AccMas has triggers, can't use OUTPUT)
            using var cmdGetAccKy = conn.CreateCommand();
            cmdGetAccKy.Transaction = tx;
            cmdGetAccKy.CommandText = @"
            SELECT AccKy
            FROM AccMas
            WHERE AccCd = @AccCd
              AND CKy = @CKy
              AND fInAct = 0;";

            cmdGetAccKy.Parameters.Add(new SqlParameter("@AccCd", accCd));
            cmdGetAccKy.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

            var accKyObj = await cmdGetAccKy.ExecuteScalarAsync();
            if (accKyObj == null)
                throw new Exception("Failed to retrieve account key after insert");

            int accKy = Convert.ToInt32(accKyObj);

            // -------------------------------------------------------------
            // 3) Insert into AccAdr table (relationship)
            // -------------------------------------------------------------
            using var cmd3 = conn.CreateCommand();
            cmd3.Transaction = tx;

            cmd3.CommandText = @"
            INSERT INTO AccAdr
            (AccKy, AdrKy)
            VALUES
            (@AccKy, @AdrKy);";

            cmd3.Parameters.Add(new SqlParameter("@AccKy", accKy));
            cmd3.Parameters.Add(new SqlParameter("@AdrKy", adrKy));

            await cmd3.ExecuteNonQueryAsync();

            tx.Commit();

            return adrKy;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private async Task<string> GenerateUniqueAdrCdAsync(DbConnection conn, DbTransaction tx, string adrNm)
    {
        if (string.IsNullOrWhiteSpace(adrNm))
            adrNm = "ADR";

        adrNm = adrNm.Trim();

        const int totalMax = 14;
        const int randomDigits = 2;

        int maxBaseLength = totalMax - randomDigits;

        if (adrNm.Length > maxBaseLength)
            adrNm = adrNm.Substring(0, maxBaseLength);

        while (true)
        {
            int random = RandomNumberGenerator.GetInt32(10, 99);
            string adrCd = $"{adrNm}{random}";

            // Check existence
            using var checkCmd = conn.CreateCommand();
            checkCmd.Transaction = tx;
            checkCmd.CommandText = "SELECT COUNT(*) FROM Address WHERE AdrCd = @AdrCd AND CKy = @CKy";

            checkCmd.Parameters.Add(new SqlParameter("@AdrCd", adrCd));
            checkCmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

            var countObj = await checkCmd.ExecuteScalarAsync();
            int count = Convert.ToInt32(countObj);

            if (count == 0)
                return adrCd;
        }
    }

    private async Task<string> GenerateUniqueAccCdAsync(DbConnection conn, DbTransaction tx)
    {
        while (true)
        {
            // Format: yyMMddHHmmssff (14 chars - fits in 15 char column)
            // Example: 26020915480012 (Year-Month-Day-Hour-Min-Sec-Centiseconds)
            string accCd = DateTime.Now.ToString("yyMMddHHmmssff");

            // Check existence
            using var checkCmd = conn.CreateCommand();
            checkCmd.Transaction = tx;
            checkCmd.CommandText = "SELECT COUNT(*) FROM AccMas WHERE AccCd = @AccCd AND CKy = @CKy";

            checkCmd.Parameters.Add(new SqlParameter("@AccCd", accCd));
            checkCmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));

            var countObj = await checkCmd.ExecuteScalarAsync();
            int count = Convert.ToInt32(countObj);

            if (count == 0)
                return accCd;
            
            // If collision, wait 10ms and try again
            await Task.Delay(10);
        }
    }

    public async Task UpdateCustomerAsync(UpdateCustomerAddressDto dto)
    {
        Console.WriteLine("Function started for updating customer address..............");

        // Email validation
        if (!string.IsNullOrWhiteSpace(dto.EMail))
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.EMail, pattern))
                throw new Exception("Invalid email format");
        }

        bool IsValidMobile(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return true;
            return number.Length == 10 && number.All(char.IsDigit);
        }

        if (!IsValidMobile(dto.TP1))
            throw new Exception("TP1 must contain exactly 10 digits");

        if (!IsValidMobile(dto.TP2))
            throw new Exception("TP2 must contain exactly 10 digits");

        using var db = await _factory.CreateDbContextAsync();
        using var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();

        using var tx = conn.BeginTransaction();
        Console.WriteLine("Transaction started for updating customer address..............");

        try
        {
            var userKey = await _userKeyService.GetUserKeyAsync(
                _userContext.UserId,
                _userContext.CompanyKey
            );

            if (userKey == null)
                throw new Exception("User key not found for this user");

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"
                        UPDATE Address
                        SET
                            AdrNm   = @AdrNm,
                            FstNm   = @FstNm,
                            MidNm   = @MidNm,
                            LstNm   = @LstNm,
                            Address = @Address,
                            TP1     = @TP1,
                            TP2     = @TP2,
                            EMail   = @EMail,
                            EntUsrKy = @EntUsrKy,
                            EntDtm   = @EntDtm
                        WHERE AdrKy = @AdrKy AND CKy = @CKy;";

            cmd.Parameters.Add(new SqlParameter("@AdrKy", dto.AdrKy));
            cmd.Parameters.Add(new SqlParameter("@CKy", _userContext.CompanyKey));
            cmd.Parameters.Add(new SqlParameter("@AdrNm", dto.AdrNm ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@FstNm", dto.FstNm ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@MidNm", dto.MidNm ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@LstNm", dto.LstNm ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@Address", dto.Address ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@TP1", dto.TP1 ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@TP2", dto.TP2 ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@EMail", dto.EMail ?? (object)DBNull.Value));
            //cmd.Parameters.Add(new SqlParameter("@GPSLoc", dto.GPSLoc ?? (object)DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@EntUsrKy", userKey));
            cmd.Parameters.Add(new SqlParameter("@EntDtm", DateTime.Now));

            int rows = await cmd.ExecuteNonQueryAsync();
            if (rows == 0)
                throw new Exception("Customer address not found");

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

}
