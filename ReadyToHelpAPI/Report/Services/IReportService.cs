namespace readytohelpapi.Report.Services;

using readytohelpapi.Report.Models;
using readytohelpapi.Occurrence.Models;
public interface IReportService
{
    (Report report, Occurrence occurrence) Create(Report report);
}