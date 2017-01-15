using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models
{



    public class AccountRoot
    {
        public Account account { get; set; }
    }

    public class Account
    {
        public string href { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string givenName { get; set; }
        public object middleName { get; set; }
        public string surname { get; set; }
        public string fullName { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public DateTime passwordModifiedAt { get; set; }
        public object emailVerificationToken { get; set; }
        public string status { get; set; }
        public Customdata customData { get; set; }
    }

    public class Customdata
    {
        public string href { get; set; }
        public DateTime createdAt { get; set; }
        public string trainer { get; set; }
        public DateTime modifiedAt { get; set; }
    }


}
