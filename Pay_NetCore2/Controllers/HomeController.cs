using System;
using Microsoft.AspNetCore.Mvc;
using PayIr;
using Pay_NetCore2.Models;

namespace Pay_NetCore2.Controllers
{
    public class HomeController : Controller
    {
        private string myApi = "test";

        public IActionResult Index()
        {
            return RedirectToAction("Pay");
        }

        [HttpGet]
        public IActionResult Pay()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Pay(ViewModel_Pay model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                PayResponse paymentResponse = Payment.Pay(new PayRequest
                {
                    api = myApi,
                    amount = model.Amount,
                    redirect = "http://localhost:5100" + Url.Action("Verify")
                });

                if (paymentResponse.status == 1)
                    return Redirect(Payment.GatewayAddress(paymentResponse.transId));

                ViewBag.Error = "خطای \"" + paymentResponse.errorCode + "\": " + paymentResponse.errorMessage;
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
            }

            return View(model);
        }

        public IActionResult Verify()
        {
            VerifyData verifyData = Payment.GetFormVerifyData(Request.Form);
            try
            {
                VerifyResponse paymentResponse = Payment.Verify(new VerifyRequest
                {
                    api = myApi,
                    transId = verifyData.transId
                });

                if (paymentResponse.status == 1)
                {
                    ViewBag.transId = verifyData.transId;
                    ViewBag.amount = paymentResponse.amount;
                    return View(true);
                }

                ViewBag.Error = "خطای \"" + paymentResponse.errorCode + "\": " + paymentResponse.errorMessage;
            }
            catch
            {
                ViewBag.Error = "متاسفانه پرداخت ناموفق بوده است.";
            }

            return View(false);
        }
    }
}
