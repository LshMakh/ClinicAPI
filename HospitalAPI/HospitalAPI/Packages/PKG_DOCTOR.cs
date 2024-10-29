using AuthProjWebApi.Packages;
using HospitalAPI.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace HospitalAPI.Packages
{
    public interface IPKG_DOCTOR
    {
        public void RegisterDoctor(Doctor doctor);
        public List<Doctor> GetDoctorCards();
    }

    public class PKG_DOCTOR:PKG_BASE,IPKG_DOCTOR
    {
        private IPKG_USERS _userPackage;
        private ILogger<PKG_PATIENT> _logger;

        public PKG_DOCTOR(IPKG_USERS userPackage, ILogger<PKG_PATIENT> logger)
        {
            _userPackage = userPackage;
            _logger = logger;
        }

        public void RegisterDoctor(Doctor doctor)
        {
            if (_userPackage.CheckUserEmailExists(doctor.Email))
            {
                _logger.LogWarning("Attempted to register doctor with existing email: {Email}", doctor.Email);
                throw new Exception($"A user with email {doctor.Email} already exists.");
            }


            string connstr = ConnStr;
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = connstr;
            conn.Open();


            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.PKG_LSH_DOCTORS.register_doctor";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = doctor.Email;
            cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = doctor.Password;
            cmd.Parameters.Add("p_first_name", OracleDbType.Varchar2).Value = doctor.FirstName;
            cmd.Parameters.Add("p_last_name", OracleDbType.Varchar2).Value = doctor.LastName;
            cmd.Parameters.Add("p_specialty", OracleDbType.Varchar2).Value = doctor.Specialty;
            cmd.Parameters.Add("p_photo_url", OracleDbType.Varchar2).Value = doctor.PhotoUrl;
            cmd.Parameters.Add("p_cv_url", OracleDbType.Varchar2).Value = doctor.CvUrl;
            cmd.Parameters.Add("p_personal_number", OracleDbType.Varchar2).Value = doctor.PersonalNumber;
            cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();
            conn.Close();
        }
        public List<Doctor> GetDoctorCards()
        {

            List<Doctor> docs = new List<Doctor>();
            string connstr = ConnStr;


            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = connstr;
            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.PKG_LSH_DOCTORS.get_doctor_cards";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Doctor doc = new Doctor();
                doc.FirstName = reader["firstname"].ToString();
                doc.LastName = reader["lastname"].ToString();
                doc.Specialty = reader["specialty"].ToString();
                doc.PhotoUrl = reader["photourl"].ToString();

                docs.Add(doc);
            }
            conn.Close();
            return docs;


        }
    }
}
