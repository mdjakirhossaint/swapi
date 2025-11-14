using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoowGood.Core.Service;
using SoowGoodWeb.Core.Service.GenericModels;
using SoowGoodWeb.Core.Service;
namespace SoowGoodWeb.Insfracture.Service.DataAccessService
{
    public class SqlDataAccessLayer
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString = AppSettings.ConnectionStringForDapper;
        public SqlDataAccessLayer(IConfiguration config)
        {
            _config = config;
        }
        /// <summary>
        /// Received parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="paramiters"></param>
        /// <returns></returns>
        public async Task<List<T>> LoadDataUsingProcedure<T, U>(string storedProcedure, U paramiters)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_connectionString);

                var data = await connection.QueryAsync<T>(storedProcedure, paramiters,
                    commandType: CommandType.StoredProcedure, commandTimeout: 120);
                return data.ToList();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }

        }
        /// <summary>
        /// Received parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="paramiters"></param>
        /// <returns></returns>
        public async Task<T?> LoadSingleDataUsingProcedure<T, U>(string storedProcedure, U paramiters)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_connectionString);

                var data = await connection.QueryAsync<T>(storedProcedure, paramiters,
                    commandType: CommandType.StoredProcedure, commandTimeout: 120);

                return data.First();
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it)
                Console.WriteLine(ex.Message);

                return default;
            }

        }
        /// <summary>
        /// Here received model for parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<T>> LoadDataUsingProcedure<T>(string storedProcedure, T model)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                var data = await connection.QueryAsync<T>(storedProcedure, parameters,
                    commandType: CommandType.StoredProcedure, commandTimeout: 120);

                return data.ToList();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }

        }
        /// <summary>
        /// Here received model for parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<T?> LoadSingleDataUsingProcedure<T>(string storedProcedure, T model)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                var data = await connection.QueryAsync<T>(storedProcedure, parameters,
                    commandType: CommandType.StoredProcedure, commandTimeout: 120);

                return data.First();
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it)
                Console.WriteLine(ex.Message);

                return default;
            }

        }
        /// <summary>
        /// Here received model for parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SaveDataUsingProcedure<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter to capture the ID
                //parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Data insert and return same model when data insert successfully
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
		public async Task<T> SaveDataUsingProcedureAndReturnModel<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter if you need to capture any value like an ID from the stored procedure
                // Example for Guid: (if there's an output ID from the DB after insertion)
                // parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

                // After successful insertion, return the model
                return model;
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Data insert and return data when data insert successfully 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns>TResult</returns>
        public async Task<TResult> SaveDataUsingProcedureAndReturnData<T, TResult>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model
                await AddParameters(parameters, model);

                // Call the stored procedure and capture the result (assuming the SP returns data after update)
                var result = await connection.QueryFirstOrDefaultAsync<TResult>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

                return result; // Return the data received from the SP
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Here received model for parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Guid> SaveDataUsingProcedureReturnId<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter to capture the ID
                parameters.Add("@id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

                // Retrieve the output parameter value
                Guid id = parameters.Get<Guid>("@id");
                return id;
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Data insert and return Guid or INT 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
		public async Task<(Guid? ReturnGuid, int? ReturnINT)> SaveDataUsingProcedureReturnIdTuple<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter to capture the ID (Guid or int)
                parameters.Add("@id", dbType: DbType.Object, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

                // Retrieve the output parameter value
                var idValue = parameters.Get<object>("@id");

                // Return the result as either a Guid or an int
                if (idValue != null)
                {
                    if (Guid.TryParse(idValue.ToString(), out Guid guidId))
                    {
                        return (guidId, null);
                    }
                    else if (int.TryParse(idValue.ToString(), out int intId))
                    {
                        return (null, intId);
                    }
                    else
                    {
                        throw new NullReferenceException("Unexpected output parameter type.");
                    }
                }

                return (null, null);
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Data insert and return INT Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
		public async Task<int> SaveDataUsingProcedureReturnIntId<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter to capture the ID (int)
                parameters.Add("@id", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

                // Retrieve the output parameter value (ID)
                int id = parameters.Get<int>("@id");
                return id;
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Data insert and return Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
		public async Task<T> SaveDataUsingProcedureReturnObject<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter to capture the ID
                parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

                // Retrieve the output parameter value
                return model;
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Here received model for parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateDataUsingProcedure<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter to capture the ID
                // parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Input);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Here received model for parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Guid> UpdateDataUsingProcedureReturnId<T>(string storedProcedure, T model)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                // Add an output parameter to capture the ID
                parameters.Add("@Id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

                // Retrieve the output parameter value
                Guid id = parameters.Get<Guid>("@Id");
                return id;
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL-specific exceptions
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Re-throw the exception to let the caller handle it
            }
        }
        /// <summary>
        /// Execute Raw SQL query and received model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlQuery"></param>
        /// <param name="model"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task<Response<T>> ExecuteSqlQueryWithModel<T>(string sqlQuery, T model, string connectionId = "Default")
        {
            var response = new Response<T>();
            try
            {
                using IDbConnection connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                // Add parameters for all properties of the model, including nested ones
                await AddParameters(parameters, model);

                await connection.ExecuteAsync(sqlQuery, parameters);
                response.IsSuccess = true;
                response.StatusCode = 200;
            }
            catch (SqlException sqlEx)
            {
                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = sqlEx.Message;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }
        /// <summary>
        /// Execute Raw SQL query and received parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task<Response<T>> ExecuteRawSqlQueryWithParameters<T>(string sqlQuery, T parameters, string connectionId = "Default")
        {
            var response = new Response<T>();
            try
            {
                using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

                await connection.ExecuteAsync(sqlQuery, parameters);
                response.IsSuccess = true;
                response.StatusCode = 200;
            }
            catch (SqlException sqlEx)
            {
                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = sqlEx.Message;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }
        /// <summary>
        /// Received dynamic parameter and converted in parameter value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="model"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private async Task AddParameters<T>(DynamicParameters parameters, T model, string prefix = "")
        {
            foreach (var prop in model.GetType().GetProperties())
            {
                var propValue = prop.GetValue(model);
                var paramName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";

                if (propValue != null && prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    await AddParameters(parameters, propValue, paramName);
                }
                else
                {
                    parameters.Add(paramName, propValue);
                }
            }
        }
        /// <summary>
        /// Received dynamic parameter and converted in parameter value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="model"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private async Task AddParameter<T>(DynamicParameters parameters, T model, string prefix = "")
        {
            foreach (var prop in model.GetType().GetProperties())
            {
                var propValue = prop.GetValue(model);
                var paramName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";

                // Check if the property is a class, and not a string
                if (propValue != null && prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    // Recursively add parameters for nested properties
                    await AddParameter(parameters, propValue, paramName);
                }
                else
                {
                    // Get DbType based on property type
                    DbType dbType = await GetDbType(prop.PropertyType);

                    // Add the parameter with the specified type
                    parameters.Add(paramName, propValue, dbType);
                }
            }
        }
        /// <summary>
        /// Get parameter type from property type.
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
		private async Task<DbType> GetDbType(Type propertyType)
        {
            await Task.Yield();
            // Mapping .NET types to corresponding DbType
            if (propertyType == typeof(int) || propertyType == typeof(int?))
                return DbType.Int32;
            if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                return DbType.Guid;
            if (propertyType == typeof(string))
                return DbType.String;
            if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                return DbType.Boolean;
            if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                return DbType.DateTime;
            if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                return DbType.Decimal;
            if (propertyType == typeof(double) || propertyType == typeof(double?))
                return DbType.Double;
            if (propertyType == typeof(float) || propertyType == typeof(float?))
                return DbType.Single;
            if (propertyType == typeof(byte[]))
                return DbType.Binary;

            // Default to Object for unsupported or unknown types
            return DbType.Object;
        }
    }
}
