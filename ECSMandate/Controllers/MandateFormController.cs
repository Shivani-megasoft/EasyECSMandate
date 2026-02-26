
using ECSMandate.Controllers;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;


namespace EasyECS.Controllers
{
    //[EasyECS.MvcApplication.SessionExpire]
    public class MandateFormController : AppController
    {
        //
        // GET: /MandateForm/ 
        //MandateMasterService mandateservice = new MandateMasterService();

        private static string xmlUrl = @"D:/Ganesh/EasyECS/EasyECS/EasyECS/Content/Images/test.xml";
        public ActionResult Index()
        {
            Response response = new Response();
            // Validation val = new Validation();
            try
            {

                if (Session["UserName"].ToString() != null)
                {
                   return Json(response);
                    //return View(stu);

                }
                else
                {
                    return RedirectToAction("Login", "LoginMaster");

                }
            }
            catch (Exception ex)
            {
                response.Message = "Something wrong,please check details";
                //val.Message = "something wrong, please check details.";
                return View();
            }
        }

      
        public JsonResult GetCountry(string countryName)
        {
            var cn = new SqlConnection();
            var ds = new DataSet();
            string strCn = ConfigurationManager.ConnectionStrings["System.Data.SqlClient"].ToString();
            cn.ConnectionString = strCn;
            var cmd = new SqlCommand
            {
                Connection = cn,
                CommandType = CommandType.Text,
                CommandText = "select CountryName from tblCountry Where CountryName like @myParameter and CountryName!=@myParameter2"
            };

            cmd.Parameters.AddWithValue("@myParameter", "%" + countryName + "%");
            cmd.Parameters.AddWithValue("@myParameter2", countryName);

            try
            {
                cn.Open();
                cmd.ExecuteNonQuery();
                var da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }
            catch (Exception)
            {
            }
            finally
            {
                cn.Close();
            }
            DataTable dt = ds.Tables[0];

            var txtItems = (from DataRow row in dt.Rows
                            select row["CountryName"].ToString()
                                into dbValues
                            select dbValues.ToLower()).ToList();
            return Json(txtItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Magnify()
        {
            return View();
        }
    }
}