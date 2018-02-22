using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace EducationBrands.Demo.Tads.Models
{
    public class DBHelper
    {
        public static string conString = "Data Source=ebispocinstance.cxqva5dydjnp.us-east-2.rds.amazonaws.com,1433;User Id = ebispoc; Password=ebispoc_kochi;Initial Catalog = ebisdemo-tads; Integrated Security = False; MultipleActiveResultSets=True;";

        //Get All applicants from DB
        public static List<Applicant> GetAllApplicants(out int totalCount, int pageNo = 0)
        {
            List<Applicant> lstApplicants = new List<Applicant>();
            int skip = 100 * pageNo;
            int take = 100;
            totalCount = 0;
            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand($"select count(1) from ravennaStudents; select * from ravennaStudents order by StudentId OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY", con))
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                totalCount = Convert.ToInt32(dr[0]);
                            }

                            dr.NextResult();

                            while (dr.Read())
                            {
                                Applicant applicant = new Applicant();
                                applicant.StudentId = Convert.ToInt32(dr["StudentId"]);
                                applicant.SchoolName = dr["SchoolName"].ToString();
                                applicant.FirstName = dr["FirstName"].ToString();
                                applicant.LastName = dr["LastName"].ToString();
                                applicant.Gender = dr["Gender"].ToString();
                                DateTime dob = DateTime.MinValue;
                                if (DateTime.TryParse(dr["DOB"].ToString(), out dob) && dob != DateTime.MinValue)
                                    applicant.DOB = dob.ToShortDateString();
                                applicant.HouseholdName = dr["HouseholdName"].ToString();
                                applicant.HouseholdPhone = dr["HouseholdPhone"].ToString();
                                applicant.HouseholdEmail = dr["HouseholdEmail"].ToString();
                                applicant.Grade = dr["Grade"].ToString();
                                lstApplicants.Add(applicant);
                            }
                        }
                    }
                }
            }
            return lstApplicants;
        }

        //Insert Applicants to DB
        public static void InsertApplicantstoDB(List<Applicant> applicants)
        {
            string dbInsertString = String.Empty;
            int count = 0;

            for (int i = 0; i < applicants.Count; i++)
            {
                Applicant applicant = applicants[i];
                if (applicant == null)
                    continue;

                if (!String.IsNullOrEmpty(dbInsertString))
                    dbInsertString += ",";

                dbInsertString += applicant.ToSqlInsertString();
                count++;

                if (count == 50)
                {
                    InsertData(dbInsertString);
                    dbInsertString = String.Empty;
                    count = 0;
                }
            }

            if (!String.IsNullOrEmpty(dbInsertString))
            {
                InsertData(dbInsertString);
            }
        }

        private static void InsertData(string SqlInsertString)
        {
            string insertCommand = String.Format("Insert Into [ravennaStudents] Values {0};", SqlInsertString);

            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand(insertCommand, con))
                {
                    con.Open();
                    cmd.ExecuteScalar();
                }
            }
        }
    }
}