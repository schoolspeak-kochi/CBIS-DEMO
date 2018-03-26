using CommunityBrands.Demo.Tads.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CommunityBrands.Demo.Tads.Utils
{
    public class DBHelper
    {
        public static string conString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

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
        public static bool InsertApplicantstoDB(List<Applicant> applicants)
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
                return InsertData(dbInsertString);
            }

            return false;
        }

        private static bool InsertData(string SqlInsertString)
        {
            string insertCommand = String.Format("Insert Into [ravennaStudents] Values {0};", SqlInsertString);

            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand(insertCommand, con))
                {
                    con.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                        return true;
                }
            }

            return false;
        }
    }
}