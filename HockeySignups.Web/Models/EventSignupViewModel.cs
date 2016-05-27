using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using HockeySignups.Data;

namespace HockeySignups.Web.Models
{
    public class EventSignupViewModel
    {
        public Event Event { get; set; }
        public EventStatus EventStatus { get;set;}
        public EventSignup Signup { get; set; }
    }
}