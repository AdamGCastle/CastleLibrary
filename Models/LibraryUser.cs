using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;



namespace CastleLibrary.Models
{
    public class LibraryUser
    {        
        [Required]
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [RegularExpression("^([a-zA-Z0-9 .&'-]+)$", ErrorMessage = "Only letters are allowed.")]
        public string Name { get; set; }

        [Display(Name = "Outstanding Books")]
        public int NumBooksOut { get; set; } = 0;

        [Display(Name="Gold Member")]
        public bool IsGoldMember { get; set; } = false;

        public ICollection<RecordOfLoan> Records { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name="Email Address")]
        public string EmailAddress { get; set; }

        public int MaxBooksAllowed { get; set; } = 5;

        [Display(Name = "Total Fines Accrued")]
        public double FinesTotal { get { return CalculateTotalFines();  }  }

        [Display(Name = "Total Fines Paid")]
        public double FinesPaid { get; set; }

        [Display(Name = "Outstanding Fines")]
        public double FinesOutstanding { get { return FinesTotal - FinesPaid; } }
        public bool IsLoanBlocked { get { return FinesOutstanding >= FineLimit; } }
        public double FineLimit { get { return IsGoldMember ? 1000 : 200; } }

        public double CalculateTotalFines()
        {
            double total = 0;
            if(Records != null)
            {
                foreach (RecordOfLoan item in Records)
                {
                    total += item.Fine;
                }
            }
            
            return total;
        }
        
    }
}
