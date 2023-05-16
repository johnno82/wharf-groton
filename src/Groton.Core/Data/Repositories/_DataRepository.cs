/*
    This is a basic implementation of a generic CRUD class using ADO.NET. This class can be used 
    as a base class for other data repositories, which should provide the implementation for the abstract properties and methods:

        - `TableName`: The name of the database table for the entity.
        - `PrimaryKeyName`: The name of the primary key field used on the database table.
        - `OrderByFields`: A list of valid fields used to sort data.
        - `CreateAddDbParameters(TEntity entity)`: Create an array of `DbParameter` objects for the given entity.
        - `CreateEntity(IDataRecord record)`: Create a new entity object from the given `IDataRecord`.

    To ensure the code is safe from SQL injections, we use parameterized queries and do not concatenate user input directly into the SQL commands.
*/

using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Groton.Core.Data.Collections;

namespace Groton.Core.Data.Repositories
{
    public class QueryFilter
    {
        public string Field { get; set; }

        public object Value { get; set; }

        public string Operator { get; set; }

        public string Logic { get; set; }

        public QueryFilter(string field, object value, string filterOperator = "=", string logic = "")
        {
            this.Field = field;
            this.Value = value;
            this.Operator = filterOperator;
            this.Logic = logic;
        }
    }

    public abstract class DataRepository<TEntity>
    {
        private readonly string _connectionString;

        protected abstract string TableName { get; }

        protected abstract string PrimaryKeyName { get; }

        protected virtual string[] OrderByFields => new string[0];

        public DataRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddAsync(TEntity entity)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO {TableName} VALUES({GetAddParametersPlaceholder(entity)});";
                    command.Parameters.AddRange(CreateAddDbParameters(entity));

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {TableName} WHERE {PrimaryKeyName} = @Id;";
                    var parameter = CreateParameter("@Id", id);
                    command.Parameters.Add(parameter);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            return ReadEntity(reader);
                        }
                    }
                }
            }

            return default;
        }

        public async Task<PagedList<TEntity>> GetAllAsync(IEnumerable<QueryFilter>? filters = null, string? orderBy = null, int pageIndex = 0, int pageSize = 20)
        {           
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {TableName}";

                    if (filters != null && filters.Count() > 0)
                    {
                        command.CommandText += " WHERE ";

                        var filterClauses = new List<string>();
                        foreach (var filter in filters)
                        {
                            string filterValue = filter.Value?.ToString() ?? String.Empty;
                            bool isString = filter.Value != null ? filter.Value.GetType() == typeof(string) : false;

                            string filterClause;
                            if (isString && filter.Operator == "LIKE")
                            {
                                filterClause = $"{filter.Field} LIKE @{filter.Field}";
                            }
                            else
                            {
                                filterClause = $"{filter.Field} {filter.Operator} @{filter.Field}";
                            }

                            if (filterClauses.Count > 0)
                                filterClause = $"{filter.Logic} {filterClause}";

                            filterClauses.Add(filterClause);

                            command.Parameters.Add(CreateParameter($"@{filter.Field}", filter.Value ?? DBNull.Value));
                        }

                        command.CommandText += string.Join(" ", filterClauses);
                    }

                    int totalCount = 0;
                    using (var countCommand = connection.CreateCommand())
                    {
                        countCommand.CommandText = $"{command.CommandText.Replace("SELECT * ", "SELECT COUNT(*) ")};";
                        foreach (DbParameter parameter in command.Parameters)
                            countCommand.Parameters.Add(CreateParameter(parameter.ParameterName, parameter.Value));

                        totalCount = ((int?)(await countCommand.ExecuteScalarAsync())).GetValueOrDefault(0);
                    }

                    if (orderBy?.Length > 0
                        && this.OrderByFields.Contains(orderBy))
                    {
                        command.CommandText += $" ORDER BY {orderBy}";
                        command.Parameters.Add(CreateParameter("@OrderBy", orderBy));
                    }
                    else
                    {
                        command.CommandText += $" ORDER BY {PrimaryKeyName}";

                    }

                    if (totalCount > pageSize)
                    {
                        command.CommandText += $" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                        command.Parameters.Add(CreateParameter("@Offset", pageIndex  * pageSize));
                        command.Parameters.Add(CreateParameter("@PageSize", pageSize));

                        command.CommandText += ";";
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var items = new List<TEntity>();
                        while (reader.Read())
                            items.Add(ReadEntity(reader));

                        return new PagedList<TEntity>(items, pageIndex, pageSize, totalCount);
                    }                    
                }                               
            }            
        }

        public async Task UpdateAsync(int id, IDictionary<string, object> changes)
        {
            if (changes != null && changes.Count > 0)
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"UPDATE {TableName} SET {GetUpdateParametersAssignment(changes)} WHERE {PrimaryKeyName} = @Id;";
                        command.Parameters.Add(CreateParameter("@Id", id));
                        command.Parameters.AddRange(CreateUpdateDbParameters(changes));

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"DELETE FROM {TableName} WHERE {PrimaryKeyName} = @Id;";
                    command.Parameters.Add(CreateParameter("@Id", id));

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        protected abstract TEntity ReadEntity(IDataRecord record);

        protected DbParameter CreateParameter(string name, object? value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        protected abstract DbParameter[] CreateAddDbParameters(TEntity entity);        

        private string GetAddParametersPlaceholder(TEntity entity)
        {
            var parameters = CreateAddDbParameters(entity);
            return string.Join(", ", Array.ConvertAll(parameters, p => p.ParameterName));
        }

        private DbParameter[] CreateUpdateDbParameters(IDictionary<string, object> changes)
        {
            return changes.Select(kvp => CreateParameter(kvp.Key, kvp.Value)).ToArray();
        }

        private string GetUpdateParametersAssignment(IDictionary<string, object> changes)
        {
            var parameters = CreateUpdateDbParameters(changes);
            return string.Join(", ", Array.ConvertAll(parameters, p => $"{p.ParameterName} = @{p.ParameterName}"));
        }

        private DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
