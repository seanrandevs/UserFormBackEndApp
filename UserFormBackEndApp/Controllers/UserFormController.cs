using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace UserFormBackEndApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFormController : ControllerBase
    {
        private IConfiguration _configuration;

        public UserFormController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetUsers")]
        public JsonResult GetUsers()
        {
            string query = "select * from dbo.UsersInfo";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("userAppDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }

        [HttpPost]
        [Route("AddUser")]
        public JsonResult AddUser([FromQuery] string firstname, [FromQuery] string lastname, [FromQuery] string email, [FromQuery] string password)
        {

            string query = @"insert into dbo.UsersInfo (FirstName, LastName, Email, Password) 
                     values (@FirstName, @LastName, @Email, @Password)";

            try
            {
                using (SqlConnection myCon = new SqlConnection(_configuration.GetConnectionString("userAppDBCon")))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Adding parameters to the SQL command
                        myCommand.Parameters.AddWithValue("@FirstName", firstname);
                        myCommand.Parameters.AddWithValue("@LastName", lastname);
                        myCommand.Parameters.AddWithValue("@Email", email);
                        myCommand.Parameters.AddWithValue("@Password", password);

                        // Execute the query
                        myCommand.ExecuteNonQuery();
                    }
                    myCon.Close();
                }

                // Return a success message
                return new JsonResult(new { message = "User added successfully" });
            }
            catch (Exception ex)
            {
                // Return error details
                return new JsonResult(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetUserByEmail")]
        public JsonResult GetUserByEmail([FromQuery] string email)
        {
            string query = @"SELECT * FROM dbo.UsersInfo WHERE Email = @Email";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("userAppDBCon");

            try
            {
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Explicitly define the command type
                        myCommand.CommandType = CommandType.Text;

                        // Add the @Email parameter
                        myCommand.Parameters.AddWithValue("@Email", email);

                        // Execute the query
                        using (SqlDataReader myReader = myCommand.ExecuteReader())
                        {
                            // Load data into the DataTable
                            table.Load(myReader);
                            myReader.Close();
                        }
                    }
                    myCon.Close();
                }

                // Return user data if found
                if (table.Rows.Count > 0)
                {
                    return new JsonResult(table);
                }
                else
                {
                    return new JsonResult(new { message = "User not found" });
                }
            }
            catch (SqlException sqlEx)
            {
                // Return SQL error details
                return new JsonResult(new { message = "SQL error occurred", error = sqlEx.Message });
            }
            catch (Exception ex)
            {
                // Return general error details
                return new JsonResult(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateLoginStatus")]
        public JsonResult UpdateLoginStatus([FromQuery] string email, [FromQuery] bool isLoggedIN)
        {
            string query = @"UPDATE dbo.UsersInfo SET isLoggedIN = @IsLoggedIN WHERE Email = @Email";

            try
            {
                using (SqlConnection myCon = new SqlConnection(_configuration.GetConnectionString("userAppDBCon")))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        // Add the parameters
                        myCommand.Parameters.AddWithValue("@IsLoggedIN", isLoggedIN);
                        myCommand.Parameters.AddWithValue("@Email", email);

                        // Execute the query
                        myCommand.ExecuteNonQuery();
                    }
                    myCon.Close();
                }

                // Return a success response
                return new JsonResult(new { success = true, message = "Login status updated successfully" });
            }
            catch (Exception ex)
            {
                // Return error details
                return new JsonResult(new { success = false, message = "An error occurred", error = ex.Message });
            }
        }




    }
}
