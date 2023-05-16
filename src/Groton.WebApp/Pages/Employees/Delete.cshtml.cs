using Groton.Core.Data.Entities;
using Groton.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Groton.WebApp.Pages.Employees
{
    public class DeleteModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly EmployeeService _employeeService;

        [BindProperty]
        public required Employee Employee { get; set; }

        public DeleteModel(
            ILogger<IndexModel> logger,
            EmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;            
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!id.HasValue)
                return Redirect("/Index");

            Employee? employee = await _employeeService.GetByIdAsync(id.Value);
            if (employee == null)
                return Redirect("/Index");

            this.Employee = employee;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (!id.HasValue)
                return Redirect("/Index");

            await _employeeService.DeleteAsync(id.Value);

            return RedirectToPage("/Index");
        }
    }
}
