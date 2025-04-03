using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Authorization;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly PdfExportService _pdfExportService;

        public ExportController(PdfExportService pdfExportService)
        {
            _pdfExportService = pdfExportService;
        }

        [HttpGet("export/project/{id}")]
        [Authorize(Policy = Permissions.ExportReports)]
        public async Task<IActionResult> ExportProject(int id)
        {
            try
            {
                var pdfBytes = await _pdfExportService.ExportProjectReportAsync(id);
                return File(pdfBytes, "application/pdf", $"project_report_{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error exporting project report: {ex.Message}");
            }
        }

        [HttpGet("export/dashboard/{type}")]
        [Authorize(Policy = Permissions.ExportReports)]
        public async Task<IActionResult> ExportDashboard(string type)
        {
            try
            {
                var pdfBytes = await _pdfExportService.ExportDashboardReportAsync(type);
                return File(pdfBytes, "application/pdf", $"{type}_dashboard_report.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error exporting dashboard report: {ex.Message}");
            }
        }
    }
} 