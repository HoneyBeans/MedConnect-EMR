using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using HospitalEMR_API.Models;

namespace HospitalEMR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly string _connectionString;

        public PatientController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // 1. DASHBOARD & CORE PROFILES

        [HttpGet("{ic}")]
        public IActionResult GetDashboard(string ic)
        {
            PatientDashboard dashboard = new PatientDashboard();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPatientDashboard", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PatientIC", ic);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dashboard.FullName = reader["FullName"].ToString();
                            dashboard.IC = reader["IC"].ToString();
                            dashboard.CurrentAge = Convert.ToInt32(reader["CurrentAge"]);
                            dashboard.Height_cm = Convert.ToDecimal(reader["Height_cm"]);
                            dashboard.Weight_kg = Convert.ToDecimal(reader["Weight_kg"]);
                        }
                        else return NotFound("Patient not found.");

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                dashboard.Allergies.Add(new Allergy { AllergenName = reader["AllergenName"].ToString(), Severity = reader["Severity"].ToString(), Notes = reader["Notes"].ToString() });
                            }
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                dashboard.MedicalHistory.Add(new MedicalRecord
                                {
                                    VisitID = Convert.ToInt32(reader["VisitID"]),
                                    VisitDate = Convert.ToDateTime(reader["VisitDate"]),
                                    QueueStatus = reader["QueueStatus"].ToString(),
                                    Symptoms = reader["Symptoms"].ToString(),
                                    BloodPressure = reader["BloodPressure"].ToString(),

                                    // Handle DBNull safely in case a value was never entered
                                    Temperature = reader["Temperature"] != DBNull.Value ? Convert.ToDecimal(reader["Temperature"]) : 0m,
                                    Diagnosis = reader["Diagnosis"].ToString(),
                                    DoctorNotes = reader["DoctorNotes"].ToString(),
                                    Prescription = reader["Prescription"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return Ok(dashboard);
        }
        // 2. NURSE TRIAGE
        [HttpPost("Nurse/AddVisit")]
        public IActionResult NurseAddVisit([FromBody] NurseVisitRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_Nurse_AddVisit", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IC", request.IC);
                        cmd.Parameters.AddWithValue("@Symptoms", request.Symptoms);
                        cmd.Parameters.AddWithValue("@BloodPressure", request.BloodPressure);
                        cmd.Parameters.AddWithValue("@Temperature", request.Temperature);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok("Patient sent to Doctor's Queue.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    return BadRequest("Error: Patient IC not found. Please register the patient first.");
                }
                return StatusCode(500, "Database error: " + ex.Message);
            }
        }
        // 3.DOCTOR CONSULTATION
        [HttpGet("Doctor/Queue")]
        public IActionResult GetDoctorQueue()
        {
            List<QueueItem> queue = new List<QueueItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetDoctorQueue", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            queue.Add(new QueueItem
                            {
                                VisitID = Convert.ToInt32(reader["VisitID"]),
                                FullName = reader["FullName"].ToString(),
                                IC = reader["IC"].ToString(),
                                VisitDate = Convert.ToDateTime(reader["VisitDate"]),
                                Symptoms = reader["Symptoms"].ToString(),
                                BloodPressure = reader["BloodPressure"].ToString(),
                                Temperature = reader["Temperature"] != DBNull.Value ? Convert.ToDecimal(reader["Temperature"]) : 0m
                            });
                        }
                    }
                }
            }
            return Ok(queue);
        }

        [HttpPut("Doctor/CompleteConsult")]
        public IActionResult DoctorCompleteConsult([FromBody] DoctorConsultRequest request)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Doctor_UpdateVisit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VisitID", request.VisitID);
                    cmd.Parameters.AddWithValue("@Diagnosis", request.Diagnosis);
                    cmd.Parameters.AddWithValue("@DoctorNotes", request.DoctorNotes);
                    cmd.Parameters.AddWithValue("@Prescription", request.Prescription);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok("Consultation complete. Sent to Pharmacy.");
        }
        // 4. PHARMACY DISPENSE
        [HttpGet("Pharmacy/Queue")]
        public IActionResult GetPharmacyQueue()
        {
            List<QueueItem> queue = new List<QueueItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPharmacyQueue", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            queue.Add(new QueueItem
                            {
                                VisitID = Convert.ToInt32(reader["VisitID"]),
                                FullName = reader["FullName"].ToString(),
                                IC = reader["IC"].ToString(),
                                VisitDate = Convert.ToDateTime(reader["VisitDate"]),
                                Prescription = reader["Prescription"].ToString()
                            });
                        }
                    }
                }
            }
            return Ok(queue);
        }

        [HttpPut("Pharmacy/Dispense/{visitId}")]
        public IActionResult PharmacyDispense(int visitId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Pharmacy_CompleteVisit", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VisitID", visitId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok("Meds dispensed. Visit Completed.");
        }
        // 5.DATA MANAGEMENT
        [HttpPost("AddPatient")]
        public IActionResult AddPatient([FromBody] NewPatientRequest request) { using (SqlConnection conn = new SqlConnection(_connectionString)) { using (SqlCommand cmd = new SqlCommand("sp_AddPatient", conn)) { cmd.CommandType = CommandType.StoredProcedure; cmd.Parameters.AddWithValue("@IC", request.IC); cmd.Parameters.AddWithValue("@FullName", request.FullName); cmd.Parameters.AddWithValue("@DOB", request.DOB); cmd.Parameters.AddWithValue("@Height", request.Height_cm); cmd.Parameters.AddWithValue("@Weight", request.Weight_kg); conn.Open(); cmd.ExecuteNonQuery(); } } return Ok("Patient registered."); }

        [HttpPut("EditPatient")]
        public IActionResult UpdatePatient([FromBody] UpdatePatientRequest request) { using (SqlConnection conn = new SqlConnection(_connectionString)) { using (SqlCommand cmd = new SqlCommand("sp_UpdatePatient", conn)) { cmd.CommandType = CommandType.StoredProcedure; cmd.Parameters.AddWithValue("@IC", request.IC); cmd.Parameters.AddWithValue("@Height", request.Height_cm); cmd.Parameters.AddWithValue("@Weight", request.Weight_kg); conn.Open(); cmd.ExecuteNonQuery(); } } return Ok("Patient info updated."); }

        [HttpDelete("DeletePatient/{ic}")]
        public IActionResult DeletePatient(string ic) { using (SqlConnection conn = new SqlConnection(_connectionString)) { using (SqlCommand cmd = new SqlCommand("sp_DeletePatient", conn)) { cmd.CommandType = CommandType.StoredProcedure; cmd.Parameters.AddWithValue("@IC", ic); conn.Open(); cmd.ExecuteNonQuery(); } } return Ok("Patient deleted."); }

        [HttpPost("AddAllergy")]
        public IActionResult AddAllergy([FromBody] NewAllergyRequest request)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddAllergy", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IC", request.IC);
                    cmd.Parameters.AddWithValue("@AllergenName", request.AllergenName);
                    cmd.Parameters.AddWithValue("@Severity", request.Severity);
                    cmd.Parameters.AddWithValue("@Notes", request.Notes ?? (object)DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok("Allergy added.");
        }
    }
}