using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Interfaces;
using EnterprisePMO_PWA.Infrastructure.Data;

namespace EnterprisePMO_PWA.Application.Services
{
    public class PdfExportService : IPdfExportService
    {
        private readonly AppDbContext _context;

        public PdfExportService(AppDbContext context)
        {
            _context = context;
        }

        // Temporarily disabled PDF export functionality
        // Will be re-enabled in a future update
    }
} 