using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Represents the CusItm table — the customer-item / inventory record.
    /// Vehicles registered in the Vehicle table are mirrored here so that
    /// the ERP system can treat each vehicle as a trackable item.
    /// </summary>
    public class CusItm
    {
        [Key]
        public int CusItmKy { get; set; }

        /// <summary>Company key — required, default (1).</summary>
        public int CKy { get; set; } = 1;

        /// <summary>Item code — mirrors VehicleId (plate/reg number). Max 15 chars.</summary>
        [MaxLength(15)]
        public string ItmCd { get; set; } = string.Empty;

        public bool fInAct { get; set; } = false;
        public bool fApr   { get; set; } = false;
        public bool fObs   { get; set; } = false;

        /// <summary>Item-type key — mapped from vehicle type.</summary>
        public short CusItmTypKy { get; set; } = 0;

        /// <summary>Item-type description (max 10). E.g. "VEHICLE".</summary>
        [MaxLength(10)]
        public string CusItmTyp { get; set; } = string.Empty;

        /// <summary>Part number — stores ChassisNo. Max 30.</summary>
        [MaxLength(30)]
        public string PartNo { get; set; } = string.Empty;

        /// <summary>Item name — stores VehicleId as display name. Max 60.</summary>
        [MaxLength(60)]
        public string? ItmNm { get; set; }

        /// <summary>Description — stores vehicle Description. Max 60.</summary>
        [MaxLength(60)]
        public string? Des { get; set; }

        // --- Category / location keys (optional, nullable) ---
        public short? LocKy         { get; set; }
        public short? CusItmCat1Ky  { get; set; }
        public short? CusItmCat2Ky  { get; set; }
        public short? CusItmCat3Ky  { get; set; }
        public short? CusItmCat4Ky  { get; set; }
        public short? CusItmPriCatKy { get; set; }

        /// <summary>Make — mirrors Vehicle.Make.</summary>
        [MaxLength(30)]
        public string? Make { get; set; }

        /// <summary>Model — mirrors Vehicle.Model.</summary>
        [MaxLength(30)]
        public string? Model { get; set; }

        [Column(TypeName = "real")]
        public float Wrnty { get; set; } = 0;

        public double? AvrWt { get; set; }

        public bool fCtrlItm { get; set; } = false;
        public bool fSrlNo   { get; set; } = false;

        public short? BUKy      { get; set; }
        public double? FrctFctr { get; set; }
        public short? FrctUnitKy { get; set; }

        /// <summary>Base unit key — required (no default in schema; set to 0 as safe default).</summary>
        public short UnitKy { get; set; } = 0;

        public double? IntrFctr  { get; set; }
        public int?    IntrUnitKy { get; set; }
        public double? BulkFctr  { get; set; }
        public short?  BulkUnitKy { get; set; }

        public double? ReOrdLvl { get; set; }
        public double? ReOrdQty { get; set; }
        public double? OnOrdQty { get; set; }
        public double? ResrvQty { get; set; }

        [Column(TypeName = "money")]
        public decimal CosPri  { get; set; } = 0;

        [Column(TypeName = "money")]
        public decimal SlsPri  { get; set; } = 0;

        [Column(TypeName = "money")]
        public decimal SlsPri2 { get; set; } = 0;

        public double? GrsWt { get; set; }
        public double? NetWt { get; set; }

        [MaxLength(2)]
        public string? Status { get; set; }

        public int?      EntUsrKy { get; set; }
        public DateTime? EntDtm   { get; set; }

        // -------------------------------------------------------
        // Navigation / helper — NOT a DB column.
        // Stores the VehicleKy so the service can look up the
        // corresponding CusItm record when updating or deleting.
        // -------------------------------------------------------
        /// <summary>
        /// Stores the source VehicleKy from the Vehicle table.
        /// Used to correlate this CusItm record back to a vehicle.
        /// Stored in the EngineNo field as "VEH-{VehicleKy}" for reference,
        /// or kept as a separate [NotMapped] property looked up via ItmCd.
        /// </summary>
        [NotMapped]
        public int? VehicleKy { get; set; }
    }
}
