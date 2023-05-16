using Groton.Core.Data.Entities;
using Groton.Core.Services;
using Groton.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Groton.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        public class InputModel
        {
            public string? SearchString { get; set; }

            public string? SortColumn { get; set; }
        }

        private readonly ILogger<IndexModel> _logger;

        private readonly EmployeeService _employeeService;

        public InputModel Input { get; set; }

        public required PagedListModel<Employee> Employees { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            EmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;

            this.Input = new InputModel();
        }

        public async Task OnGetAsync(string? searchString, string? sortColumn, int? pageIndex)
        {
            this.Input.SearchString = searchString;
            this.Input.SortColumn = sortColumn;

            if (searchString?.Length > 0)
                searchString = $"%{searchString}%";

            var employees = await _employeeService.GetAllAsync(searchString, searchString, sortColumn, pageIndex.GetValueOrDefault(0), 5);
            this.Employees = new PagedListModel<Employee>(employees);
        }
    }
}