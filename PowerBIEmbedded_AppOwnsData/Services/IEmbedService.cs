using PowerBIEmbedded_AppOwnsData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PowerBIEmbedded_AppOwnsData.Services
{
    public interface IEmbedService
    {
        EmbedConfig EmbedConfig { get; }
        TileEmbedConfig TileEmbedConfig { get; }
        List<ReportDefinition> ReportList { get; }
        string GetGroupId { get; }
        Task<bool> EmbedReport(string userName, string roles, string groupId, string reportId);
        Task<bool> GetReportList(string groupId);
        Task<bool> EmbedDashboard();
        Task<bool> EmbedTile();
    }
}
