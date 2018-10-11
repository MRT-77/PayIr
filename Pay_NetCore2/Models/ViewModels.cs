using System.ComponentModel.DataAnnotations;

namespace Pay_NetCore2.Models
{
    public class ViewModel_Pay
    {
        [Display(Name = "قیمت"), Required(ErrorMessage = "قیمت باید وارد شود")]
        public int Amount { get; set; }
    }
}
