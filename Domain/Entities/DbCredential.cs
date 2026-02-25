using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CompanyProject
    {
        [Key]
        public int CPKy { get; set; }

        public int CKy { get; set; }
        public int PrjKy { get; set; }
        public string DbServer { get; set; }
        public string DbName { get; set; }
        public string? DbUser { get; set; }
        public string? DbPassword { get; set; }

        // Cloudflare Access tunnel credentials (optional).
        // When populated, the factory will route through a local cloudflared
        // TCP tunnel instead of connecting directly to DbServer.
        public string? CfHostname     { get; set; }
        public string? CfClientId     { get; set; }
        public string? CfClientSecret { get; set; }

        /// <summary>
        /// When true  → connect directly to DbServer (cloud-hosted SQL, reachable from the API).
        /// When false → route through a local cloudflared TCP tunnel to reach an on-premise DB.
        /// </summary>
        public bool IsCloudDb { get; set; }
    }
}
