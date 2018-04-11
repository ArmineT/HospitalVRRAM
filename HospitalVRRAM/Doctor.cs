﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using HospitalForms;
using HospitalConnections;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace HospitalClasses
{
    public class Doctor : User
    {
        //  Properties  //

        public string Speciality { get; set; }
        public DateTime GetEmployed { get; set; }
        public decimal ConsultationCost { get; set; }
        public Dictionary<DateTime, Patient> Patients { get; set; }

        //  End Properties  //


        //Constructor//
        public Doctor(string name, string surname, int passportID, string login, string password,
           string speciality, DateTime getEmployed, decimal consultationCost) : base(name, surname, passportID, login, password)
        {
            string SQlcmd = "dbo.insertDoctor";
            var conn = HospitalConnection.CreateDbConnection();
            try
            {
                using (conn)
                {
                    conn.Open();
                    var cmd = (SqlCommand)HospitalConnection.CreateDbCommand(conn, SQlcmd, CommandType.StoredProcedure);
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 20).Value = name;
                    cmd.Parameters.Add("@Surname", SqlDbType.NVarChar, 20).Value = surname;
                    cmd.Parameters.Add("@PassportID", SqlDbType.Char, 9).Value = passportID;
                    cmd.Parameters.Add("@Login", SqlDbType.VarChar, 8).Value = login;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar, 8).Value = password;

                    cmd.Parameters.Add("@Speciality", SqlDbType.VarChar,28).Value = speciality;
                    cmd.Parameters.Add("@ConsultationCost", SqlDbType.SmallMoney).Value = consultationCost;
                    cmd.Parameters.Add("@GetEmployed", SqlDbType.DateTime).Value = getEmployed;

                    cmd.ExecuteNonQuery();
                }
                
                Speciality = speciality;
                GetEmployed = getEmployed;
                ConsultationCost = consultationCost;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public Doctor(string name, string surname, int passportID,
                      string speciality, DateTime getEmployed, 
                      decimal consultationCost) : base(name, surname, passportID)
        {
            Speciality = speciality;
            GetEmployed = getEmployed;
            ConsultationCost = consultationCost;
        }
            //End Constructor//


            // Methods //
        private void WriteDiagnosis(Patient patient, Diagnosis diagnose)
        {

            var conn = HospitalConnection.CreateDbConnection();
            string sSQL = "sp_WriteDiagnosInDiagnoses";
            string sSQL1 = "sp_AddMedicineInAssignedTo";
            try
            {
                using (conn)
                {
                    conn.Open();
                    var cmd = (SqlCommand)HospitalConnection.CreateDbCommand(conn, sSQL, CommandType.StoredProcedure);


                    cmd.Parameters.Add(" @description", SqlDbType.NVarChar, 20).Value = diagnose.Disease;
                    cmd.Parameters.Add("@dateOfDiagnoses", SqlDbType.DateTime).Value = diagnose.DiagnoseDate;
                    cmd.Parameters.Add("@patientID", SqlDbType.Char, 9).Value = patient.PassportID;
                    cmd.Parameters.Add("@doctorID", SqlDbType.Char, 9).Value = this.PassportID;

                    cmd.ExecuteNonQuery();
                }

                using (conn)
                {
                    conn.Open();
                    var cmd1 = (SqlCommand)HospitalConnection.CreateDbCommand(conn, sSQL1, CommandType.StoredProcedure);


                    //cmd1.Parameters.Add(" @description", SqlDbType.NVarChar, 20).Value = diagnose.Disease;
                    //cmd1.Parameters.Add("@dateOfDiagnoses", SqlDbType.DateTime).Value = diagnose.DiagnoseDate;
                    //cmd1.Parameters.Add("@patientID", SqlDbType.Char, 9).Value = patient.PassportID;
                    //cmd1.Parameters.Add("@doctorID", SqlDbType.Char, 9).Value = this.PassportID;
                    //must be to do
                    // cmd.ExecuteNonQuery();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public List<Patient> ShowPatient()
        {
            List<Patient> patients = new List<Patient>();
            var conn = HospitalConnection.CreateDbConnection();

            string SQLcmd = "dbo.SeeMyPatients";
            try
            {
                using (conn)
                {
                    conn.Open();
                    var cmd = (SqlCommand)HospitalConnection.CreateDbCommand(conn, SQLcmd, CommandType.StoredProcedure);
                    cmd.Parameters.Add("@PassportID", SqlDbType.Char, 9).Value = PassportID;

                    using (var reader = (SqlDataReader)cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string n = (string)reader["Name"];
                            string s = (string)reader["surname"];
                            int p = (int)reader["PassportID"];
                            string a = (string)reader["Address"];
                            DateTime d = (DateTime)reader["DateOfBirth"];
                            Patient pat = new Patient(n, s, p, a, d);
                            patients.Add(pat);
                        }

                    }
                }
                return patients;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return patients;
        }

        public Diagnosis PatientDiagnosis(Patient patient)
        {
            Diagnosis result = null;

            var conn = HospitalConnection.CreateDbConnection();
          
            string sSQL1 = "dbo.GetDiagnoseMedicine";
            string sSQL2 = "dbo.GetDiagnose";

            List<Medicine> medList = new List<Medicine>();


            DateTime diagnoseDate = new DateTime(0, 0, 0);
            string disease = "";

            try
            {
                using (conn)
                {
                    var cmd = (SqlCommand)HospitalConnection.CreateDbCommand(conn, sSQL1, CommandType.StoredProcedure);

                    var cmd2 = (SqlCommand)HospitalConnection.CreateDbCommand(conn, sSQL2, CommandType.StoredProcedure);
                    cmd.Parameters.Add("patientID", SqlDbType.VarChar, 9).Value = patient.PassportID;
                    cmd2.Parameters.Add("patientID", SqlDbType.VarChar, 9).Value = patient.PassportID;

                    using (var reader = (SqlDataReader)cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {

                        if (reader.HasRows)
                        {
                            bool hasMoreResults = true;

                            while (hasMoreResults)
                            {
                                while (reader.Read())
                                {
                                    string nameMed = reader.GetString(reader.GetOrdinal("name")).ToString();
                                    string countryMed = reader.GetString(reader.GetOrdinal("country")).ToString();
                                    decimal priceMed = reader.GetDecimal(reader.GetOrdinal("price"));
                                    DateTime expiryDate = reader.GetDateTime(reader.GetOrdinal("expiryDate"));
                                    Medicine m = new Medicine(nameMed, countryMed, priceMed, expiryDate);
                                    medList.Add(m);
                                }

                                hasMoreResults = reader.NextResult();
                            }
                        }

                        using (var reader2 = (SqlDataReader)cmd2.ExecuteReader())
                        {
                            if (reader2.HasRows)
                            {
                                while (reader.Read())
                                {
                                    disease = reader.GetString(reader.GetOrdinal("Description")).ToString();
                                    diagnoseDate = reader.GetDateTime(reader.GetOrdinal("DateOfDiagnosis"));
                                }
                            }
                        }
                    }
                }
                result = new Diagnosis(disease, diagnoseDate, medList);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            return result;
        }

        public Dictionary<DateTime, Patient> Calendar()
        {
            return Patients;
        }

        public void newPatient(Patient patient, DateTime dTime)
        {
            //add in Dictionary of patietnts
            Patients[dTime] = patient;
        }

        public void ServePatient(Patient patient, Diagnosis diagnose,DateTime time)
        {
            WriteDiagnosis(patient, diagnose);
            patient.MyHistory.Add(diagnose);
            Patients.Remove(time);
        }

        public DateTime FreeTime(DateTime day)
        {
            DateTime current = new DateTime(day.Year, day.Month, day.Day, day.Hour, day.Minute, 0);

            DateTime ll = new DateTime(day.Year, day.Month, day.Day, 9, 0, 0), rr = new DateTime(day.Year, day.Month, day.Day, 18, 40, 0);

            Patients = new Dictionary<DateTime, Patient>();
            Patients[current] = new Patient("aa", "aaa", 5413, "asa", DateTime.Now);

            for(int i = 0; i < 60 * 10; i++)
            {
                current = current.AddMinutes(i);

                bool valid = true;
                for(int j = -19; j < 20; j++)
                    if (Patients != null && Patients.ContainsKey(current.AddMinutes(j)))
                    {
                        valid = false;
                        break;
                    }

                if (valid && current >= ll && current <= rr)
                {
                    return current;
                }

                current = current.AddMinutes(-2 * i);

                valid = true;
                for (int j = -19; j < 20; j++)
                    if (Patients != null && Patients.ContainsKey(current.AddMinutes(j)))
                    {
                        valid = false;
                        break;
                    }

                if (valid && current >= ll && current <= rr)
                {
                    return current;
                }

                current = current.AddMinutes(i);
            }

            throw new Exception();
        }
        public override void AddPicture(byte[] pic)
        {

            string sSQL = "select passportID,Picture.PathName() as PathName, Picture\r\n"
                       + "from Medicine\r\n"
                       + " where passportID=@passportID";

            try
            {
                var conn = HospitalConnection.CreateDbConnection();
                conn.Open();

            var cmd = (SqlCommand)HospitalConnection.CreateDbCommand(conn, sSQL, CommandType.Text);

                cmd.Parameters.Add("@passportID", SqlDbType.Char, 9).Value = this.PassportID;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Get the pointer for file
                        var path = reader.GetString(reader.GetOrdinal("PathName"));
                        var imbytes = reader.GetSqlBytes(reader.GetOrdinal("Picture")).Buffer;

                        var ms = new MemoryStream(imbytes);

                        Image photo = Image.FromStream(ms);
                        //must be done using our form
                        //  label1.Text = reader.GetString(reader.GetOrdinal("SName"));
                        //  pictureBox1.Image = photo;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //public decimal ShowBalance()
        //{

        //    return Balance;
        //}

        //End Methods //
    }
}