using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CommunityBrands.Demo.Tads.Models
{
    public class Applicant
    {
        public int StudentId { get; set; }

        public int RavennaStudentId { get; set; }

        public long SchoolId { get; set; }

        [DisplayName("School")]
        public string SchoolName { get; set; }

        public string SchoolYear { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string Gender { get; set; }

        [DisplayName("Birth Date")]
        public string DOB { get; set; }

        public string FAGrantAmount { get; set; }

        [DisplayName("Household Phone")]
        public string HouseholdPhone { get; set; }

        [DisplayName("Household Name")]
        public string HouseholdName { get; set; }

        public string HouseholdStreet { get; set; }

        public string HouseholdCity { get; set; }

        public string HouseholdState { get; set; }

        public string HouseholdZip { get; set; }

        public string HouseholdCountry { get; set; }

        [DisplayName("Household Email")]
        public string HouseholdEmail { get; set; }

        public string Parent1FirstName { get; set; }

        public string Parent1LastName { get; set; }

        public string Parent1MiddleName { get; set; }

        public string Parent1Gender { get; set; }

        public string Parent1Relationship { get; set; }

        public string Parent1CustodyStatus { get; set; }

        public string Parent1Phone { get; set; }

        public string Parent1Email { get; set; }

        public string Parent2FirstName { get; set; }

        public string Parent2LastName { get; set; }

        public string Parent2MiddleName { get; set; }

        public string Parent2Gender { get; set; }

        public string Parent2Relationship { get; set; }

        public string Parent2CustodyStatus { get; set; }

        public string Parent2Phone { get; set; }

        public string Parent2Email { get; set; }

        public string Grade { get; set; }

        public string ToSqlInsertString()
        {
            return $"('{RavennaStudentId}','{SchoolId}','{SchoolName}','{SchoolYear}','{FirstName}','{LastName}','{MiddleName}','{Gender}','{DOB}','{FAGrantAmount}','{HouseholdPhone}','{HouseholdName}','{HouseholdStreet}','{HouseholdCity}','{HouseholdState}','{HouseholdZip}','{HouseholdCountry}','{HouseholdEmail}','{Parent1FirstName}','{Parent1LastName}','{Parent1MiddleName}','{Parent1Gender}','{Parent1Relationship}','{Parent1CustodyStatus}','{Parent1Phone}','{Parent1Email}','{Parent2FirstName}','{Parent2LastName}','{Parent2MiddleName}','{Parent2Gender}','{Parent2Relationship}','{Parent2CustodyStatus}','{Parent2Phone}','{Parent2Email}','{Grade}')";
        }

    }
}