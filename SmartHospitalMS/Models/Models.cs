namespace SmartHospitalMS
{
    // Inheritance: User "is a" BaseEntity
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Admin, Doctor, Receptionist
    }

    public class Patient : BaseEntity
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string BloodGroup { get; set; }
        public string Disease { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string DoctorAssigned { get; set; }

        public override string GetSummary()
        {
            return $"Patient: {FullName} ({Gender}), Age: {Age}, Contact: {Contact}";
        }
    }

    public class Doctor : BaseEntity
    {
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }

        public override string GetSummary()
        {
            return $"Dr. {FullName} - {Specialization}";
        }
    }

    public class Appointment : BaseEntity
    {
        public string TokenNumber { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Cancelled, Completed
    }

    public class Bill : BaseEntity
    {
        public int AppointmentID { get; set; }
        public decimal ConsultationFee { get; set; }
        public decimal MedicineFee { get; set; }
        public decimal LabFee { get; set; }
        public decimal TaxPercentage { get; set; } = 5.0m;
        
        // Logic inside property (Encapsulation)
        public decimal TotalAmount => (ConsultationFee + MedicineFee + LabFee) * (1 + TaxPercentage / 100);
    }
}
