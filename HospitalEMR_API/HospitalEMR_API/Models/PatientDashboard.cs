namespace HospitalEMR_API.Models
{
    public class PatientDashboard
    {
        public string FullName { get; set; }
        public string IC { get; set; }
        public int CurrentAge { get; set; }
        public decimal Height_cm { get; set; }
        public decimal Weight_kg { get; set; }
        public List<Allergy> Allergies { get; set; } = new List<Allergy>();
        public List<MedicalRecord> MedicalHistory { get; set; } = new List<MedicalRecord>();
    }

    public class Allergy
    {
        public string AllergenName { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
    }

    // UPDATED: Now includes pipeline data (Status, BP, Temp, Prescription)
    public class MedicalRecord
    {
        public int VisitID { get; set; }
        public DateTime VisitDate { get; set; }
        public string QueueStatus { get; set; }
        public string Symptoms { get; set; }
        public string BloodPressure { get; set; }
        public decimal Temperature { get; set; }
        public string Diagnosis { get; set; }
        public string DoctorNotes { get; set; }
        public string Prescription { get; set; }
    }

    // PIPELINE REQUEST

    public class NurseVisitRequest
    {
        public string IC { get; set; }
        public string Symptoms { get; set; }
        public string BloodPressure { get; set; }
        public decimal Temperature { get; set; }
    }

    public class DoctorConsultRequest
    {
        public int VisitID { get; set; }
        public string Diagnosis { get; set; }
        public string DoctorNotes { get; set; }
        public string Prescription { get; set; }
    }

    // This is for fetching the waiting lists
    public class QueueItem
    {
        public int VisitID { get; set; }
        public string FullName { get; set; }
        public string IC { get; set; }
        public DateTime VisitDate { get; set; }

        // Nurse notes for the doctor
        public string Symptoms { get; set; }
        public string BloodPressure { get; set; }
        public decimal Temperature { get; set; }

        // Doctor notes for the pharmacy
        public string Prescription { get; set; }
    }

    public class NewAllergyRequest
    {
        public string IC { get; set; }
        public string AllergenName { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
    }

    // Keeping old models for registering/editing patients
    public class NewPatientRequest { public string IC { get; set; } public string FullName { get; set; } public DateTime DOB { get; set; } public decimal Height_cm { get; set; } public decimal Weight_kg { get; set; } }
    public class UpdatePatientRequest { public string IC { get; set; } public decimal Height_cm { get; set; } public decimal Weight_kg { get; set; } }
}                                                           