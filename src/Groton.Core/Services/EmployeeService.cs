using Groton.Core.Data.Collections;
using Groton.Core.Data.Entities;
using Groton.Core.Data.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groton.Core.Services
{
    public class EmployeeService
    {
        private readonly ILogger<EmployeeService> _logger;

        private readonly EmployeeRepository _repository;

        public EmployeeService(
            ILogger<EmployeeService> logger,
            EmployeeRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<Employee?> GetByIdAsync(int id) 
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<PagedList<Employee>> GetAllAsync(string? name = null, string? jobTitle = null, string? orderBy = null, int pageIndex = 0, int pageSize = 20)
        {
            var filters = new List<QueryFilter>();

            if(!String.IsNullOrWhiteSpace(name))
                filters.Add(new QueryFilter("Name", name, "LIKE"));
            if(!String.IsNullOrWhiteSpace(jobTitle))
                filters.Add(new QueryFilter("JobTitle", jobTitle, "LIKE", "OR"));

            return await _repository.GetAllAsync(filters, orderBy, pageIndex, pageSize);
        }

        public async Task AddAsync(string name, string jobTitle)
        {
            name = name.Trim();
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException(name);

            jobTitle = jobTitle.Trim();
            if (String.IsNullOrEmpty(jobTitle))
                throw new ArgumentException(jobTitle);

            Employee employee = new Employee
            {
                Name = name,
                JobTitle = jobTitle
            };

            await _repository.AddAsync(employee);
        }

        public async Task UpdateAsync(int id, string? name = null, string? jobTitle = null)
        {
            var changes = new Dictionary<string, object>(0);

            name = name?.Trim();
            if (!String.IsNullOrWhiteSpace(name))
                changes.Add("Name", name);

            jobTitle = jobTitle?.Trim();
            if (!String.IsNullOrWhiteSpace(jobTitle))
                changes.Add("JobTitle", jobTitle);

            await _repository.UpdateAsync(id, changes);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
