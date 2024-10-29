using AuthProjWebApi.Packages;
using HospitalAPI.DTO_s;
using HospitalAPI.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Linq.Expressions;

namespace HospitalAPI.Packages
{
    public interface IPKG_USERS
    {

        public TokenPayloadDto? Authenticate(UserLoginDto logindata);
        public object? GetUserInfo(int userId);
        public bool CheckUserEmailExists(string email);
    }
    public class PKG_USERS : PKG_BASE, IPKG_USERS
    {
        private readonly ILogger<PKG_USERS> _logger;

        public PKG_USERS(ILogger<PKG_USERS> logger)
        {
            _logger = logger;
        }
        public bool CheckUserEmailExists(string email)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "olerning.PKG_LSH_USERS.get_user_by_email";
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
                        cmd.Parameters.Add("p_exists", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        // Execute the procedure
                        cmd.ExecuteNonQuery();

                        // Get the result
                        int exists = Convert.ToInt32(cmd.Parameters["p_exists"].Value.ToString());
                        return exists == 1;
                    }
                }
                catch (OracleException ex)
                {
                    _logger.LogError(ex, "Database error checking email existence for {Email}", email);
                    throw new Exception($"Database error checking email existence: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking email existence for {Email}", email);
                    throw new Exception($"Error checking email existence: {ex.Message}", ex);
                }
            }
        }
        public object? GetUserInfo(int userId)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "olerning.PKG_LSH_USERS.get_user_details";
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        cmd.Parameters.Add("p_userid", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string role = reader["role"].ToString().ToUpper();

                                switch (role)
                                {
                                    case "DOCTOR":
                                        return new Doctor
                                        {
                                            UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                                            Rating = reader.GetInt32(reader.GetOrdinal("rating")),
                                            Role = role,
                                            FirstName = reader["firstname"].ToString(),
                                            LastName = reader["lastname"].ToString(),
                                            Email = reader["email"].ToString(),
                                            PersonalNumber = reader["personalnumber"].ToString(),
                                            Specialty = reader["specialty"].ToString(),
                                            PhotoUrl = reader["photourl"].ToString(),
                                            CvUrl = reader["cvurl"].ToString()
                                        };

                                    case "PATIENT":
                                        return new User
                                        {
                                            UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                                            Role = role,
                                            FirstName = reader["firstname"].ToString(),
                                            LastName = reader["lastname"].ToString(),
                                            Email = reader["email"].ToString(),
                                            PersonalNumber = reader["personalnumber"].ToString()

                                        };
                                    case "ADMIN":
                                        return new AdminDetailsDto
                                        {
                                            UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                                            Role = role,
                                        };

                                    default:
                                        _logger.LogWarning("Unknown role {Role} for user {UserId}", role, userId);
                                        return null;
                                }
                            }
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving user information for userId: {UserId}", userId);
                    throw;
                }
            }
        }




        public TokenPayloadDto? Authenticate(UserLoginDto loginData)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "olerning.PKG_LSH_USERS.authenticate_user";
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = loginData.Email;
                        cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = loginData.Password;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Check if any results were returned
                                if (reader["USERID"] != DBNull.Value)
                                {
                                    return new TokenPayloadDto
                                    {
                                        UserId = Convert.ToInt32(reader["USERID"]),
                                        Role = reader["ROLE"].ToString()
                                    };
                                }
                            }
                            return null; // No user found with these credentials
                        }
                    }
                }
                catch (OracleException ex)
                {
                    // Log the specific Oracle error
                    throw new Exception($"Database error during authentication: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    // Log the general error
                    throw new Exception($"Error during authentication: {ex.Message}", ex);
                }
            }
        }
    }
    } 

    

