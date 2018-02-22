using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommunityBrands.Demo.Ravenna.Models
{
    public class MemberModel
    {
        public long MemberId { get; set; }
        public string School { get; set; }
        public string schoolYear { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string Grade { get; set; }
        public string DOB { get; set; }
        public HouseHold HouseHold { get; set; }
        public double? FAGrantAmount { get; set; }
        public string Phone { get; set; }
    }

    public class HouseHold
    {
        public List<Association> Associations;
        public string Name;
        public Address Address;
        public string Phone;
        public string Email;
    }

    public class Association
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string Relationship { get; set; }
        public string CustodyStatus { get; set; }
        public string PrimaryPhone { get; set; }
        public string PrimaryEmail { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}