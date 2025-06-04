using Microsoft.AspNetCore.Mvc;
using PhotoManagement.Models;
using System.Diagnostics;
using BLL;
using System.IO;
using DAL;
using Photo_ssem.Models;
using DAL.Interfaces;
namespace PhotoManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClass1 _class1;
        private readonly ICrudDb _crudDb;
        public HomeController(IClass1 class1, ICrudDb crudDb, ILogger<HomeController> logger)
        {
            _class1 = class1;
           
            _crudDb = crudDb;
            _logger = logger;
        }
      
        private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        static string code;

        public IActionResult Index()
        {
           
            return View();
        }

        public IActionResult logIn()
        {

            return View();
        }
            public IActionResult About()
            {
                return View();
            }

            public IActionResult Invoice()
        {
            ViewData["Name"] = TempData["Name"];
            ViewData["Phone"] = TempData["Phone"];
            ViewData["OrderCode"] = TempData["OrderCode"];
            ViewData["Size"] = TempData["Size"];

            return View();
        }

        public IActionResult AllOrders()
        {
            Console.WriteLine("TempData[\"OfficerCode\"] = " + TempData["OfficerCode"]); // בדיקה
            string officerCode = TempData["OfficerCode"] as string;

            if (string.IsNullOrEmpty(officerCode))
            {
                return RedirectToAction("logIn"); // אם אין קוד, מחזירים להתחברות
            }

            var orders = _crudDb.ReadFromDBAllOrder(officerCode); // שליפת הנתונים מה-DB
            return View(orders); // שולחים את הנתונים לדף
        }



        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult size()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult CreateOrder()
        {
            return View(); // ודא שה-View קיים
        }

        //מקבל את הנתונים מדף open order 
        [HttpPost]
        public IActionResult SubmitOrder(string name, int phone, string email)
        {
            // שמירה של הערכים שהתקבלו במתודעה
            string userName = name; // תוכן השם מה-input
            int userPhone = phone; // תוכן הטלפון מה-input
            TempData["Name"] = name;
            TempData["Phone"] = phone;
            TempData["Email"] = email;
            string uniqueCodeOrder = _class1.GenerateShortUniqueCodeFromGuid();
            ViewBag.UniqueCode = uniqueCodeOrder; // מעביר לקוד הייחודי ל-View
            string uniqueCodeCustomer = _class1.GenerateShortUniqueCodeFromGuid();
            ViewBag.UniqueCode = uniqueCodeCustomer; // מעביר לקוד הייחודי ל-View
            code = uniqueCodeOrder;
           _class1.newOrder(userName, uniqueCodeCustomer, userPhone, uniqueCodeOrder);
            return View("size");
        }


        [HttpPost]
        public IActionResult SaveSelection(string size)
        {
            // שמירה של הערכים שהתקבלו במתודעה
            TempData["Size"] = size;
            ViewData["code"] = code;
            string name = TempData["Name"] as string;
            string userEmail = TempData["Email"] as string;
            int phone = Convert.ToInt32(TempData["Phone"]);
            Console.WriteLine($"Name: {name}, Phone: {phone}, Size: {size}");
            string userSize = size;
            Console.WriteLine("new size" + userSize);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "MISC", "AUTXFER.MRK");
            Console.WriteLine("Looking for file at: " + path);  // או השתמש ב-ILogger
            
            _class1.newSavaimg(path, code, size);
            _class1.SendPurchaseConfirmationEmail(
              userEmail, // כתובת הלקוח
               name, // שם הלקוח
               "מחשב נייד - דגם XYZ, כמות: 1, מחיר: 4500₪" // פרטי ההזמנה
            );
            //BLL.Class1.newSavaimg("C:\\Users\\1\\לימודים\\לימודים שנה ב\\c-sharp\\Photo_ssem\\Photo_ssem\\MISC\\AUTXFER.MRK", code, size);
            return RedirectToAction("Invoice");
        }

        [HttpPost]
        public IActionResult checkCode(string code)
        {
          
            if (_crudDb.ReadFromDBCode(code))
            {
                Console.WriteLine("employee found!!!!!");
                // שומרים את הקוד בזיכרון כדי לשלוח לדף הבא
                TempData["OfficerCode"] = code;

                return RedirectToAction("AllOrders");
            }
            Console.WriteLine("employee not found");
            return View("logIn");

        }

        [HttpPost]
        public JsonResult UpdateOrderStatus([FromBody] OrderUpdateModel updateModel)
        {
            using (var context = new DbConnectionManagemen())
            {
                var order = context.OrderManagement.FirstOrDefault(o => o.OrderCode == updateModel.OrderCode);

                if (order != null)
                {
                    order.ProcessStatus = updateModel.NewStatus; // שמירה של int במקום string
                    context.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
        }






    }
}