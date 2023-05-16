using Groton.Core.Data.Entities;
using Groton.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Groton.WebApp.Pages
{
    public class EmployeeModel : PageModel
    {
        public class InputModel
        {
            [Required]
            [StringLength(256)]
            public string? Name { get; set; }

            [Required]
            [StringLength(512)]
            public string? JobTitle { get; set; }
        }

        private readonly ILogger<IndexModel> _logger;

        private readonly EmployeeService _employeeService;

        [BindProperty]
        public InputModel Input { get; set; }

        public EmployeeModel(
            ILogger<IndexModel> logger,
            EmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;

            this.Input = new InputModel();
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if(id.HasValue)
            {
                Employee? employee = await _employeeService.GetByIdAsync(id.Value);
                if(employee != null)
                {
                    this.Input.Name = employee.Name;
                    this.Input.JobTitle = employee.JobTitle;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if(!this.ModelState.IsValid)
                return Page();

            if(id.HasValue)
            {
                await _employeeService.UpdateAsync(
                    id.Value, this.Input.Name, this.Input.JobTitle);
            }
            else
            {
                await _employeeService.AddAsync(this.Input.Name!, this.Input.JobTitle!);
            }

            return RedirectToPage("/Index");
        }
    }
}
