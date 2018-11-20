using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VotingData.Model
{
    public class UserDetails
    {
        public string AadharNo { get; set; }

        public string VoterId { get; set; }

        public string Name { get; set; }

        public string DOB { get; set; }

        public string FatherName { get; set; }

        public bool IsVoterIdLinkedWithAadhar { get; set; }

        public string VotedTo { get; set; }

        public string Gender { get; set; }
    }
}
