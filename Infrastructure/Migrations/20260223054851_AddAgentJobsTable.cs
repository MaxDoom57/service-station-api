using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentJobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentJobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CKy = table.Column<int>(type: "int", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "PENDING"),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    PickedUpAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(MINUTE, 2, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentJobs", x => x.JobId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyProject",
                columns: table => new
                {
                    CPKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CKy = table.Column<int>(type: "int", nullable: false),
                    PrjKy = table.Column<int>(type: "int", nullable: false),
                    DbServer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DbName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DbUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DbPassword = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProject", x => x.CPKy);
                });

            migrationBuilder.CreateTable(
                name: "UsrMas",
                columns: table => new
                {
                    UsrKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CKy = table.Column<short>(type: "smallint", nullable: false),
                    UsrId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    fInAct = table.Column<bool>(type: "bit", nullable: true),
                    flApr = table.Column<byte>(type: "tinyint", nullable: true),
                    fGroup = table.Column<byte>(type: "tinyint", nullable: false),
                    fSysAccNm = table.Column<bool>(type: "bit", nullable: false),
                    UsrNm = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    BUKy = table.Column<int>(type: "int", nullable: true),
                    PwdTip = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AcsLvlKy = table.Column<short>(type: "smallint", nullable: true),
                    PID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Pwd = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "char(2)", maxLength: 2, nullable: true),
                    SKy = table.Column<short>(type: "smallint", nullable: false),
                    EntUsrKy = table.Column<int>(type: "int", nullable: true),
                    EntDtm = table.Column<DateTime>(type: "smalldatetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsrMas", x => x.UsrKy);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentJobs_CKy_Status",
                table: "AgentJobs",
                columns: new[] { "CKy", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentJobs");

            migrationBuilder.DropTable(
                name: "CompanyProject");

            migrationBuilder.DropTable(
                name: "UsrMas");
        }
    }
}
