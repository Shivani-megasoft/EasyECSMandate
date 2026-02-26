using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public static class MvcHelpers
    {
        public static string RenderPartialView(this Controller controller, string viewName, object model, string area)
        {

            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            }

            if (!string.IsNullOrWhiteSpace(area))
            {
                controller.ControllerContext.RouteData.DataTokens["area"] = area;
            }

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);

                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);

                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        //public static string GetGLNameByCode(this Controller controller, string glCd)
        //{
        //    var commonController = new CommonController();
        //    return commonController.GetGlName(glCd);
        //}

        //public static string GetBranchNameByCode(this Controller controller, string brnCd)
        //{
        //    var commonController = new CommonController();
        //    return commonController.GetBrnName(brnCd);
        //}

    }


}
