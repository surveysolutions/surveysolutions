using Designer.Web.Extensions;
using Designer.Web.Models;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace Designer.Web.Controllers
{   
    [Authorize(Roles="Admin")]
    public class AdministrationController : Controller
    {
        MembershipProvider _provider = Membership.Provider;
        //
        // GET: /Administration/

        public ViewResult Index()
        {
            int totalRecords;
            var users = _provider.GetAllUsers(0, 20, out totalRecords);
            return View(users);
        }

        //
        // GET: /Administration/Details/john

        public ViewResult Details(string userName)
        {
            return View(GetUser(userName));
        }

        //
        // GET: /Administration/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Administration/Create

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    string confirmationToken =
                        WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { Email = model.Email }, false);

                    Roles.Provider.AddUsersToRoles(new[] { model.UserName }, new[] { UserHelper.USERROLENAME });

                    return RedirectToAction("Index");
                }
                catch (MembershipCreateUserException e)
                {
                     ModelState.AddModelError("", e.StatusCode.ToErrorCode());
                }
            }
            return View();
        }
        
        //
        // GET: /Administration/Edit/john
 
        public ActionResult Edit(string userName)
        {
            return View(GetUser(userName));
        }

        //
        // POST: /Administration/Edit/john

        [HttpPost]
        public ActionResult Edit(MembershipUser user)
        {
            if (ModelState.IsValid) {
                _provider.UpdateUser(user);
                return RedirectToAction("Index");
            } else {
                return View();
            }
        }

        //
        // GET: /Administration/Delete/john

        public ActionResult Delete(string userName)
        {
            return View(GetUser(userName));
        }

        //
        // POST: /Administration/Delete/john

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string userName)
        {
            _provider.DeleteUser(userName, true);

            return RedirectToAction("Index");
        }

        private MembershipUser GetUser(string userName)
        {
            int totalRecords;
            return _provider.FindUsersByName(userName, 0, 20, out totalRecords)[userName];
        }
    }
}

