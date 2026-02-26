using EasyBank.BusinessLayer.Users.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EasyECS.Models
{
    public class LoginModel
    {
        //[Required(ErrorMessage = "Please enter the Branch Code")]
        //public string BrnCd { get; set; }

        [Required(ErrorMessage = "Please enter the Login ID")]
        public string LoginId { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        public string Pass { get; set; }
        

        public string Captcha { get; set; }
        [Required(ErrorMessage = "Please Enter Captcha")]


        public string UserCat { get; set; }

        public string IPAddress { get; set; }


        public LoginModel()
        {
            Pass = string.Empty;
            LoginId = string.Empty;
            UserCat = UserCategoryType.BranchUser;
            
        }
    }
}