using CommunityBrands.Demo.Ravenna.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace CommunityBrands.Demo.Ravenna.Utils
{
    public class DBHandler
    {
        public List<MemberModel> GetMembers(int skip, int takeCount)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                List<MemberModel> listMem = new List<MemberModel>();
                SqlCommand cmd = new SqlCommand("rav.StudentsGetAll", sqlConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@skipCount", SqlDbType.BigInt).Value = skip;
                cmd.Parameters.AddWithValue("@takeCOunt", SqlDbType.BigInt).Value = takeCount;
                sqlConnection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return null;
                    while (reader.Read())
                    {
                        MemberModel mem = new MemberModel();
                        mem.MemberId = (int)reader["StudentId"];
                        mem.FirstName = reader["FirstName"].ToString();
                        mem.LastName = reader["LastName"].ToString();
                        mem.MiddleName = reader["MiddleName"].ToString();
                        mem.Grade = reader["Grade"].ToString();
                        //mem.Gender = reader["Gender"].ToString();
                        mem.DOB = reader["DOB"].ToString();
                        mem.HouseHold = new HouseHold();
                        mem.HouseHold.Address = new Address();
                        mem.HouseHold.Address .Street= reader["HouseholdStreet"].ToString();
                        mem.HouseHold.Address.City = reader["HouseholdCity"].ToString();
                        mem.HouseHold.Address.State = reader["HouseholdState"].ToString();
                        mem.HouseHold.Address.Zip = reader["HouseholdZip"].ToString();
                        mem.HouseHold.Address.Country= reader["HouseholdCountry"].ToString();
                        mem.HouseHold.Phone = reader["HouseholdPhone"].ToString();
                        mem.HouseHold.Email = reader["HouseholdEmail"].ToString();
                        mem.HouseHold.Associations = new List<Association>();
                        Association parent1 = new Association();
                        parent1 .FirstName= reader["Parent1FirstName"].ToString();
                        parent1.LastName = reader["Parent1LastName"].ToString();
                        mem.HouseHold.Name = parent1.LastName;
                        parent1.MiddleName = reader["Parent1MiddleName"].ToString();
                        parent1.Relationship = reader["Parent1Relationship"].ToString();
                        parent1.PrimaryPhone = reader["Parent1Phone"].ToString();
                        parent1.PrimaryEmail= reader["Parent1Email"].ToString();
                        mem.HouseHold.Associations.Add(parent1);
                        Association parent2 = new Association();
                        parent2.FirstName = reader["Parent2FirstName"].ToString();
                        parent2.LastName = reader["Parent2LastName"].ToString();
                        if (mem.HouseHold.Name ==string.Empty)
                        {
                            mem.HouseHold.Name = parent2.LastName;
                        }
                        parent2.MiddleName = reader["Parent2MiddleName"].ToString();
                        parent2.Relationship = reader["Parent2Relationship"].ToString();
                        parent2.PrimaryPhone= reader["Parent2Phone"].ToString();
                        parent2.PrimaryEmail = reader["Parent2Email"].ToString();
                        mem.HouseHold.Associations.Add(parent2);
                        double grant = 0;
                        double.TryParse( reader["FAGrant"].ToString(), out grant);
                        mem.FAGrantAmount = grant;
                        listMem.Add(mem);
                    }
                    listMem = listMem.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
                    return listMem;
                }
            }
        }
    }
}