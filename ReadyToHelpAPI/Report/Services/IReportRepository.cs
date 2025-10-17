using readytohelpapi.Report.Models;

namespace readytohelpapi.Report.Services;

public interface IReportRepository
{
    Models.Report Create(Models.Report report);
    Models.Report? GetById(int id);
}