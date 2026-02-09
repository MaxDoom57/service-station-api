using Domain.Entities;
using Domain.Entities.Lookups;
using Domain.Entities.Reports;
using Infrastructure.Context.Entities;
using Microsoft.EntityFrameworkCore;
using POS.Core.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Context
{
    public class DynamicDbContext : DbContext
    {
        public DynamicDbContext(DbContextOptions<DynamicDbContext> options) : base(options)
        {
        }

        // Add dynamic database entities later
        public DbSet<Item> Items { get; set; }
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<SalesAccount> SalesAccounts { get; set; }
        public DbSet<PaymentTerm> PaymentTerms { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AdrMas> Addresses { get; set; }
        public DbSet<CdMas> CdMas { get; set; }
        public DbSet<TrnMas> TrnMas { get; set; }
        public DbSet<ItmTrn> ItmTrn { get; set; }
        public DbSet<AccTrn> AccTrn { get; set; }
        public DbSet<TrnNoLst> TrnNoLst { get; set; }
        public DbSet<AccAdr> AccAdr { get; set; }
        public DbSet<vewCdMas> vewCdMas { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<ObjMas> ObjMas { get; set; }
        public DbSet<UsrObj> UsrObj { get; set; }
        public DbSet<vewPmtTrmToPrmMode> vewPmtTrmToPrmMode { get; set; }
        public DbSet<ItmMas> ItmMas { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<Units> Units { get; set; }
        public DbSet<UsrMas> UsrMas { get; set; }
        public DbSet<vewItmBatch> vewItmBatch { get; set; }
        public DbSet<ItmBatch> ItmBatch { get; set; }
        public DbSet<vewStkAddHdr> vewStkAddHdr { get; set; }
        public DbSet<vewStkAddDtls> vewStkAddDtls { get; set; }
        public DbSet<vewTrnMas> vewTrnMas { get; set; }
        public DbSet<vewStkDedHdr> vewStkDedHdr { get; set; }
        public DbSet<vewStkDedDtls> vewStkDedDtls { get; set; }
        public DbSet<vewTrnNo> vewTrnNo { get; set; }
        public DbSet<vewGRNHdr> vewGRNHdr { get; set; }
        public DbSet<vewGRNDtls> vewGRNDtls { get; set; }
        public DbSet<vewTrnTypCd> vewTrnTypCd { get; set; }
        public DbSet<vewPURRTNHdr> vewPURRTNHdr { get; set; }
        public DbSet<vewPURRTNDtls> vewPURRTNDtls { get; set; }
        public DbSet<vewOrdNo> vewOrdNo { get; set; }
        public DbSet<vewPOHdr> vewPOHdr { get; set; }
        public DbSet<vewPODtls> vewPODtls { get; set; }
        public DbSet<Control> Control { get; set; }
        public DbSet<BnkMas> BnkMas { get; set; }
        public DbSet<ServiceStatus> ServiceStatus { get; set; }
        public DbSet<OrdMas> OrdMas { get; set; }
        public DbSet<OrdDet> OrdDet { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<VehicleDriver> VehicleDrivers { get; set; }
        public DbSet<Bay> Bays { get; set; }
        public DbSet<BayReservation> BayReservations { get; set; }
        public DbSet<BayControl> BayControls { get; set; }
        public DbSet<ReservationMas> ReservationMas { get; set; }
        public DbSet<ServiceOrder> ServiceOrder { get; set; }
        public DbSet<ServiceOrderDetail> ServiceOrderDetail { get; set; }
        public DbSet<ServiceOrderApproval> ServiceOrderApproval { get; set; }

        //Report views
        public DbSet<VewSlsDtlsRpt> VewSlsDtlsRpt { get; set; }
        public DbSet<TrnDetQry> TrnDetQry { get; set; }
        public DbSet<VewChqRtnDet> VewChqRtnDet { get; set; }
        public DbSet<VewAccTrnRpt> VewAccTrnRpt { get; set; }
        public DbSet<TransactionsQry> TransactionsQry { get; set; }
        public DbSet<SetOffViewQry> SetOffViewQry { get; set; }
        public DbSet<LSSvewInvRpt> LSSvewInvRpt { get; set; }
        public DbSet<vewObjPropDet> vewObjPropDet { get; set; }
        public DbSet<vewStkCurQtyRpt> vewStkCurQtyRpt { get; set; }
        public DbSet<vewReOrdDet> vewReOrdDet { get; set; }
        public DbSet<vewAdrDet> vewAdrDet { get; set; }
        public DbSet<vewAccAdrDet> vewAccAdrDet { get; set; }
        public DbSet<vewItmMasVsf> vewItmMasVsf { get; set; }
        public DbSet<vewSlsDtls> vewSlsDtls { get; set; }
        public DbSet<vewSlsDetByPmtTrm> vewSlsDetByPmtTrm { get; set; }


        //Lookups
        public DbSet<vewItmCat1Cd> vewItmCat1Cd { get; set; }
        public DbSet<vewItmCat2Cd> vewItmCat2Cd { get; set; }
        public DbSet<vewItmCat3Cd> vewItmCat3Cd { get; set; }
        public DbSet<vewItmCat4Cd> vewItmCat4Cd { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
                .ToView("vewItmMas")     // Map this entity to the SQL VIEW
                .HasNoKey();              // View has no primary key

            modelBuilder.Entity<CustomerAccount>()
                .ToView("vewCusAccCd")
                .HasNoKey();

            modelBuilder.Entity<SalesAccount>()
                .ToView("vewAccAccNm")
                .HasNoKey();

            modelBuilder.Entity<PaymentTerm>()
                .ToView("vewPmtTrmCd")
                .HasNoKey();

            modelBuilder.Entity<Customer>()
                .ToView("vewCustomer")
                .HasNoKey();

            modelBuilder.Entity<AdrMas>()
                .ToTable("Address")
                .HasKey(x => x.AdrKy);

            modelBuilder.Entity<Account>()
                .ToTable("AccMas")
                .HasKey(x => x.AccKy);

            modelBuilder.Entity<OrdMas>()
                .ToTable("OrdMas")
                .HasKey(x => x.OrdKy);

            modelBuilder.Entity<OrdDet>()
                .ToTable("OrdDet")
                .HasKey(x => x.OrdDetKy);

            modelBuilder.Entity<Units>()
                .ToTable("UnitCnv")
                .HasKey(x => x.UnitKy);

            modelBuilder.Entity<UsrMas>()
                .ToTable("UsrMas")
                .HasKey(x => x.UsrKy);

            modelBuilder.Entity<ItmMas>()
                .ToTable("ItmMas")
                .HasKey(x => x.ItmKy);

            modelBuilder.Entity<ItmBatch>()
                .ToTable("ItmBatch")
                .HasKey(x => x.ItmBatchKy);


            modelBuilder.Entity<ObjMas>()
                .ToTable("ObjMas")
                .HasKey(x => x.ObjKy);

            modelBuilder.Entity<AccAdr>()
                .ToTable("AccAdr")
                .HasNoKey();

            modelBuilder.Entity<vewCdMas>()
                .ToTable("vewCdMas")
                .HasNoKey();

            modelBuilder.Entity<vewPmtTrmToPrmMode>()
                .ToTable("vewPmtTrmToPrmMode")
                .HasNoKey();

            modelBuilder.Entity<vewItmBatch>()
                .ToView("vewItmBatchRpt")
                .HasNoKey();

            modelBuilder.Entity<vewStkAddHdr>()
                .HasNoKey().
                ToView("vewStkAddHdr");

            modelBuilder.Entity<vewStkAddDtls>()
                .HasNoKey().
                ToView("vewStkAddDtls");

            modelBuilder.Entity<vewTrnMas>()
                .HasNoKey().
                ToView("vewTrnMas");

            modelBuilder.Entity<vewStkDedHdr>()
                .HasNoKey()
                .ToView("vewStkDedHdr");
            modelBuilder.Entity<vewStkDedDtls>()
                .HasNoKey()
                .ToView("vewStkDedDtls");

            modelBuilder.Entity<vewTrnNo>()
                .HasNoKey()
                .ToView("vewTrnNo");

            modelBuilder.Entity<vewGRNHdr>()
                .HasNoKey()
                .ToView("vewGRNHdr");

            modelBuilder.Entity<vewGRNDtls>()
                .HasNoKey()
                .ToView("vewGRNDtls");

            modelBuilder.Entity<vewTrnTypCd>()
                .HasNoKey()
                .ToView("vewTrnTypCd");

            modelBuilder.Entity<vewPURRTNHdr>()
                .HasNoKey()
                .ToView("vewPURRTNHdr");

            modelBuilder.Entity<vewPURRTNDtls>()
                .HasNoKey()
                .ToView("vewPURRTNDtls");

            modelBuilder.Entity<vewOrdNo>()
                .HasNoKey()
                .ToView("vewOrdNo");

            modelBuilder.Entity<vewPOHdr>()
                .HasNoKey()
                .ToView("vewPOHdr");

            modelBuilder.Entity<vewPODtls>()
                .HasNoKey()
                .ToView("vewPODtls");

            modelBuilder.Entity<Control>()
                .HasNoKey()
                .ToView("Control");

            modelBuilder.Entity<BnkMas>()
                .ToTable("BnkMas")
                .HasKey(x => x.BnkKy);


            modelBuilder.Entity<UsrObj>(entity =>
            {
                entity.ToTable("UsrObj");

                entity.HasKey(e => e.UsrObjKy);

                entity.Property(e => e.UsrObjKy).HasColumnName("UsrObjKy");
                entity.Property(e => e.UsrKy).HasColumnName("UsrKy");
                entity.Property(e => e.ObjKy).HasColumnName("ObjKy");

                entity.Property(e => e.fApr).HasColumnName("fApr");
                entity.Property(e => e.CKy).HasColumnName("CKy");

                entity.Property(e => e.fAcs).HasColumnName("fAcs");
                entity.Property(e => e.fNew).HasColumnName("fNew");
                entity.Property(e => e.fUpdt).HasColumnName("fUpdt");
                entity.Property(e => e.fDel).HasColumnName("fDel");
                entity.Property(e => e.fSp).HasColumnName("fSp");

                entity.Property(e => e.EntUsrKy).HasColumnName("EntUsrKy");
                entity.Property(e => e.EntDtm).HasColumnName("EntDtm");
            });

            //Report views
            modelBuilder
                .Entity<VewSlsDtlsRpt>()
                .HasNoKey()
                .ToView("vewSlsDtlsRpt");

            modelBuilder
                .Entity<TrnDetQry>()
                .HasNoKey()
                .ToView("TrnDetQry");

            modelBuilder
                .Entity<VewChqRtnDet>()
                .HasNoKey()
                .ToView("vewChqRtnDet");

            modelBuilder
                .Entity<VewAccTrnRpt>()
                .HasNoKey()
                .ToView("vewAccTrnRpt");

            modelBuilder
                .Entity<TransactionsQry>()
                .HasNoKey()
                .ToView("TransactionsQry");

            modelBuilder
                .Entity<SetOffViewQry>()
                .HasNoKey()
                .ToView("SetOffViewQry");

            modelBuilder
                .Entity<LSSvewInvRpt>()
                .HasNoKey()
                .ToView("LSSvewInvRpt");

            modelBuilder
                .Entity<vewObjPropDet>()
                .HasNoKey()
                .ToView("vewObjPropDet");

            modelBuilder
                .Entity<vewStkCurQtyRpt>()
                .HasNoKey()
                .ToView("vewStkCurQtyRpt");

            modelBuilder
                .Entity<vewReOrdDet>()
                .ToView("vewReOrdDet")
                .HasNoKey();

            modelBuilder
                .Entity<vewAdrDet>()
                .ToView("vewAdrDet")
                .HasNoKey();

            modelBuilder
                .Entity<vewAccAdrDet>()
                .HasNoKey()
                .ToView("vewAccAdrDet");

            modelBuilder
                .Entity<vewItmMasVsf>()
                .HasNoKey()
                .ToView("vewItmMasVsf");

            modelBuilder
                .Entity<vewSlsDtls>()
                .HasNoKey()
                .ToView("vewSlsDtls");

            modelBuilder
                .Entity<vewSlsDetByPmtTrm>()
                .HasNoKey()
                .ToView("vewSlsDetByPmtTrm");

            //Lookups

            modelBuilder
                .Entity<vewItmCat1Cd>()
                .HasNoKey()
                .ToView("vewItmCat1Cd");

            modelBuilder
                .Entity<vewItmCat2Cd>()
                .HasNoKey()
                .ToView("vewItmCat2Cd");

            modelBuilder
                .Entity<vewItmCat3Cd>()
                .HasNoKey()
                .ToView("vewItmCat3Cd");

            modelBuilder
                .Entity<vewItmCat4Cd>()
                .HasNoKey()
                .ToView("vewItmCat4Cd");

            modelBuilder.Entity<ServiceStatus>()
                .ToTable("ServiceStatus")
                .HasKey(x => x.TempServiceKy);

            modelBuilder.Entity<Vehicle>()
                .ToTable("Vehicle")
                .HasKey(x => x.VehicleKy);

            modelBuilder.Entity<Driver>()
                .ToTable("Driver")
                .HasKey(x => x.DriverKy);

            modelBuilder.Entity<VehicleDriver>()
                .ToTable("VehicleDriver")
                .HasKey(x => x.VehicleDriverKy);

            modelBuilder.Entity<Bay>()
                .ToTable("Bay")
                .HasKey(x => x.BayKy);

            modelBuilder.Entity<BayReservation>()
                .ToTable("BayReservation")
                .HasKey(x => x.ResKy);

            modelBuilder.Entity<BayControl>()
                .ToTable("BayControl")
                .HasKey(x => x.BayControlKy);

            modelBuilder.Entity<ReservationMas>()
                .ToTable("ReservationMas")
                .HasKey(x => x.ResKy);

            modelBuilder.Entity<ServiceOrder>()
                .ToTable("ServiceOrder")
                .HasKey(x => x.ServiceOrdKy);

            modelBuilder.Entity<ServiceOrderDetail>()
                .ToTable("ServiceOrderDetail")
                .HasKey(x => x.ServiceOrdDetKy);

            modelBuilder.Entity<ServiceOrderApproval>()
                .ToTable("ServiceOrderApproval")
                .HasKey(x => x.ApprovalKy);
        }
    }
}
