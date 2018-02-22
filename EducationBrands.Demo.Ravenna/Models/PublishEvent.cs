using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducationBrands.Demo.Ravenna.Models
{
    public class PublishEvent
    {
        public bool AcknowledgementRequired { get; set; }
        public string InstitutionName { get; set; }
        public string EbInstitutionId { get; set; }
        public string EventName { get; set; }
        public string Payload { get; set; }
    }
}