using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeAdminPortal.Tests.Controllers;

public class ReportsControllerTests
{
    [Fact]
    public async Task GetDepartmentSummary_ReturnsDepartmentSummary()
    {
        var service = new Mock<IReportService>();
        service.Setup(s => s.GetDepartmentSummaryAsync()).ReturnsAsync([
            new DepartmentSummaryItem("IT", 2, 55000m, 50000m, 60000m)
        ]);

        var controller = new ReportsController(service.Object, Mock.Of<ILogger<ReportsController>>());

        var result = await controller.GetDepartmentSummary();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var report = Assert.IsAssignableFrom<IEnumerable<DepartmentSummaryItem>>(okResult.Value);
        var item = report.First();
        Assert.Equal("IT", item.DepartmentName);
        Assert.Equal(2, item.EmployeeCount);
        Assert.Equal(55000m, item.AverageSalary);
        Assert.Equal(50000m, item.MinSalary);
        Assert.Equal(60000m, item.MaxSalary);
    }

    [Fact]
    public async Task GetProjectSummary_ReturnsProjectSummary()
    {
        var service = new Mock<IReportService>();
        service.Setup(s => s.GetProjectSummaryAsync()).ReturnsAsync([
            new ProjectSummaryItem("Employee Portal", 2, 60000m, 50000m, 70000m)
        ]);

        var controller = new ReportsController(service.Object, Mock.Of<ILogger<ReportsController>>());

        var result = await controller.GetProjectSummary();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var report = Assert.IsAssignableFrom<IEnumerable<ProjectSummaryItem>>(okResult.Value);
        var item = report.First();
        Assert.Equal("Employee Portal", item.ProjectName);
        Assert.Equal(2, item.ProjectMembersCount);
        Assert.Equal(60000m, item.AverageSalary);
        Assert.Equal(50000m, item.MinSalary);
        Assert.Equal(70000m, item.MaxSalary);
    }

    [Fact]
    public async Task GetDepartmentSummary_ReturnsZeroForDepartmentWithNoEmployees()
    {
        var service = new Mock<IReportService>();
        service.Setup(s => s.GetDepartmentSummaryAsync()).ReturnsAsync([
            new DepartmentSummaryItem("Finance", 0, 0m, 0m, 0m)
        ]);

        var controller = new ReportsController(service.Object, Mock.Of<ILogger<ReportsController>>());

        var result = await controller.GetDepartmentSummary();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var report = Assert.IsAssignableFrom<IEnumerable<DepartmentSummaryItem>>(okResult.Value);
        var item = report.First();
        Assert.Equal(0, item.EmployeeCount);
        Assert.Equal(0m, item.AverageSalary);
        Assert.Equal(0m, item.MinSalary);
        Assert.Equal(0m, item.MaxSalary);
    }
}