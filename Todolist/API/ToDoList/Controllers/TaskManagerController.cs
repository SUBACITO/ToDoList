using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace ToDoList.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskManagerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtToken _jwt;

        public TaskManagerController(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwt = new JwtToken();
        }

        private (bool IsValid, ClaimsPrincipal? Claims) CheckAuth(HttpRequest request)
        {
            string authHeader = request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return (false, null); // Invalid authorization header
            }

            string token = authHeader["Bearer ".Length..].Trim();

            if (_jwt.ValidateToken(token, out ClaimsPrincipal? claims))
            {
                if (!string.IsNullOrEmpty(claims?.FindFirst(c => c.Type == "UserName")?.Value))
                {
                    return (true, claims);
                }
            }

            return (false, null); // Token is invalid
        }

        private async Task<ContentResult> SqlCustom(string proc, SqlParameter[] sqlParameters)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            // Using block for SqlConnection to ensure disposal
            using SqlConnection connection = new(connectionString);

            // Open connection if it's not open already
            if (connection.State == ConnectionState.Closed)
            {
                await connection.OpenAsync();
            }

            // Using block for SqlCommand to ensure disposal
            using SqlCommand command = new(proc, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add SQL parameters to the command
            if (sqlParameters != null)
            {
                command.Parameters.AddRange(sqlParameters);
            }

            // Execute the command and fill the DataTable
            SqlDataAdapter da = new(command);
            DataTable dt = new();
            da.Fill(dt);

            // Return the result as JSON
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { data = dt }),
                ContentType = "application/json",
                StatusCode = 200
            };
        }


        [HttpGet(Name = "GET TASK")]
        [Route("GetTask")]
        public async Task<ActionResult> GetTasks()
        {
            try
            {
                var (IsValid, Claims) = CheckAuth(Request);

                if (!IsValid)
                {
                    return Unauthorized(new { message = "Token is invalid or Authorization header is missing." });
                }

                // Extract the username from the claims
                var userName = Claims?.FindFirst(c => c.Type == "UserName")?.Value;

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserName", userName)
                };

                var result = await SqlCustom("sp_TaskSelectAll", parameters);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message.ToString() });
            }
        }


        [HttpPost(Name = "ADD TASK")]
        [Route("AddTask")]
        public async Task<ActionResult> AddTasks()
        {
            try
            {
                var (IsValid, Claims) = CheckAuth(Request);

                if (!IsValid)
                {
                    return Unauthorized(new { message = "Token is invalid or Authorization header is missing." });
                }

                // Extract the username from the claims
                var userName = Claims?.FindFirst(c => c.Type == "UserName")?.Value;

                using (var reader = new StreamReader(Request.Body))
                {
                    var requestBody = await reader.ReadToEndAsync();
                    var jsonObject = JObject.Parse(requestBody);

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@TaskName", jsonObject["TaskName"]?.Value<string>()),
                        new SqlParameter("@Description", jsonObject["Description"]?.Value<string>()),
                        new SqlParameter("@DueDate", jsonObject["DueDate"]?.Value<DateTime>()),
                        new SqlParameter("@UserName", userName)
                    };

                    var result = await SqlCustom("AddTask", parameters);

                    return result;
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete(Name = "REMOVE TASK")]
        [Route("RemoveTask")]
        public async Task<ActionResult> RemoveTasks()
        {
            try
            {
                var (IsValid, Claims) = CheckAuth(Request);

                if (!IsValid)
                {
                    return Unauthorized(new { message = "Token is invalid or Authorization header is missing." });
                }

                // Extract the username from the claims
                var userName = Claims?.FindFirst(c => c.Type == "UserName")?.Value;

                using (var reader = new StreamReader(Request.Body))
                {
                    var requestBody = await reader.ReadToEndAsync();
                    var jsonObject = JObject.Parse(requestBody);

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@TaskId", jsonObject["TaskId"]?.Value<int>()),
                        new SqlParameter("@UserName", userName)
                    };

                    var result = await SqlCustom("RemoveTask", parameters);

                    return result;
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
