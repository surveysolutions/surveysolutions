using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewEmailRenderer : IWebInterviewEmailRenderer
    {
        public class EmptyController : Controller
        {
        }

        public PersonalizedWebInterviewEmail RenderEmail(WebInterviewEmailTemplate emailTemplate, string password, string link,
            string surveyName, string address, string senderName)
        {
            var emailContent = new EmailContent(emailTemplate, surveyName, link, password);

            var model = new EmailParams
            {
                Subject = emailContent.Subject,
                LinkText = emailContent.LinkText,
                MainText = emailContent.MainText,
                PasswordDescription = emailContent.PasswordDescription,
                Password = password,
                Address = address,
                SurveyName = surveyName,
                SenderName = senderName,
                Link = link
            };

            var context = ViewRenderer.CreateController<EmptyController>().ControllerContext;
            var renderer = new ViewRenderer(context);
            string html = renderer.RenderViewToString("~/Views/WebEmails/EmailHtml.cshtml", model);
            string text = renderer.RenderViewToString("~/Views/WebEmails/EmailText.cshtml", model);

            return new PersonalizedWebInterviewEmail(model.Subject, html, text);
        }

        public class ViewRenderer
        {
            protected ControllerContext Context { get; set; }

            public ViewRenderer(ControllerContext controllerContext = null)
            {
                // Create a known controller from HttpContext if no context is passed
                if (controllerContext == null)
                {
                    if (HttpContext.Current != null)
                        controllerContext = CreateController<EmptyController>().ControllerContext;
                    else
                        throw new InvalidOperationException(
                            "ViewRenderer must run in the context of an ASP.NET " +
                            "Application and requires HttpContext.Current to be present.");
                }
                Context = controllerContext;
            }

            public string RenderViewToString(string viewPath, object model = null)
            {
                return RenderViewToStringInternal(viewPath, model, false);
            }

            public void RenderView(string viewPath, object model, TextWriter writer)
            {
                RenderViewToWriterInternal(viewPath, writer, model, false);
            }

            public static string RenderView(string viewPath, object model = null,
                                            ControllerContext controllerContext = null)
            {
                ViewRenderer renderer = new ViewRenderer(controllerContext);
                return renderer.RenderViewToString(viewPath, model);
            }

            public static void RenderView(string viewPath, TextWriter writer, object model,
                                            ControllerContext controllerContext)
            {
                ViewRenderer renderer = new ViewRenderer(controllerContext);
                renderer.RenderView(viewPath, model, writer);
            }

            public static string RenderView(string viewPath, object model,
                                            ControllerContext controllerContext,
                                            out string errorMessage)
            {
                errorMessage = null;
                try
                {
                    ViewRenderer renderer = new ViewRenderer(controllerContext);
                    return renderer.RenderViewToString(viewPath, model);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.GetBaseException().Message;
                }
                return null;
            }

            public static void RenderView(string viewPath, object model, TextWriter writer,
                                            ControllerContext controllerContext,
                                            out string errorMessage)
            {
                errorMessage = null;
                try
                {
                    ViewRenderer renderer = new ViewRenderer(controllerContext);
                    renderer.RenderView(viewPath, model, writer);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.GetBaseException().Message;
                }
            }

            protected void RenderViewToWriterInternal(string viewPath, TextWriter writer, object model = null, bool partial = false)
            {
                // first find the ViewEngine for this view
                ViewEngineResult viewEngineResult = null;
                if (partial)
                    viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
                else
                    viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

                if (viewEngineResult == null)
                    throw new FileNotFoundException();

                // get the view and attach the model to view data
                var view = viewEngineResult.View;
                Context.Controller.ViewData.Model = model;

                var ctx = new ViewContext(Context, view,
                                            Context.Controller.ViewData,
                                            Context.Controller.TempData,
                                            writer);
                view.Render(ctx, writer);
            }

            private string RenderViewToStringInternal(string viewPath, object model,
                                                        bool partial = false)
            {
                // first find the ViewEngine for this view
                ViewEngineResult viewEngineResult = null;
                if (partial)
                    viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
                else
                    viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

                if (viewEngineResult == null || viewEngineResult.View == null)
                    throw new FileNotFoundException();

                // get the view and attach the model to view data
                var view = viewEngineResult.View;
                Context.Controller.ViewData.Model = model;

                string result = null;

                using (var sw = new StringWriter())
                {
                    var ctx = new ViewContext(Context, view,
                                                Context.Controller.ViewData,
                                                Context.Controller.TempData,
                                                sw);
                    view.Render(ctx, sw);
                    result = sw.ToString();
                }

                return result;
            }

            public static T CreateController<T>(RouteData routeData = null, params object[] parameters)
                        where T : Controller, new()
            {
                // create a disconnected controller instance
                T controller = (T)Activator.CreateInstance(typeof(T), parameters);

                // get context wrapper from HttpContext if available
                HttpContextBase wrapper = null;
                if (HttpContext.Current != null)
                    wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
                else
                    throw new InvalidOperationException(
                        "Can't create Controller Context if no active HttpContext instance is available.");

                if (routeData == null)
                    routeData = new RouteData();

                // add the controller routing if not existing
                if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
                    routeData.Values.Add("controller", controller.GetType().Name
                                                                .ToLower()
                                                                .Replace("controller", ""));

                controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
                return controller;
            }

        }
    }
}
