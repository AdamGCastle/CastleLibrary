using System;
using System.ComponentModel.DataAnnotations;


namespace CastleLibrary.Models
{
    public class RecordOfLoan
    {       
        public int ID { get; set; }

        [Required(ErrorMessage ="Please enter the title of the book you wish to take out.")]   
        public int BookID { get; set; }
        public Book Book { get; set; }

        [Required(ErrorMessage = "Please enter the ID number of the individual who wishes to take the book out.")]
        public int LibraryUserID { get; set; }
        public LibraryUser LibraryUser { get; set; }

        [DataType(DataType.Date, ErrorMessage="Please enter a valid date.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Required(ErrorMessage = "Please enter the date from which you wish to borrow the book."), Display(Name = "Borrowed")]
        public DateTime DateBorrowed { get; set; } = DateTime.Now;

        [DataType(DataType.Date, ErrorMessage ="Please enter a valid date.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}", NullDisplayText = "Not Returned")]
        [Display(Name = "Returned")]
        public DateTime? DateReturned { get; set; } = null;

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Due")]
        public DateTime DateDue { get; set; }

        [Display(Name ="Overdue")]
        public bool IsOverdue { get { return CalculateIfOverdue(); } }        
        public double Fine { get { return CalculateFine(); } }
        public bool IsReturned { get; set; } = false;
        public int DaysOverdue { get { return CalculateDaysOverdue(); } }

        private int CalculateDaysOverdue()
        {
            DateTime dateToCalcFrom = DateReturned ?? DateTime.Now;
            return (dateToCalcFrom - DateDue).Days;
        }
        private double CalculateFine()
        {
            double fine = DaysOverdue > 0 ? DaysOverdue * 0.2 : 0;
            return fine < 40 ? fine : 40;               
        }
        private bool CalculateIfOverdue()
        {
            return DateReturned == null && DateTime.Now > DateDue;
        }
        public void CalculateDateDue()
        {
            int allowance = LibraryUser.IsGoldMember ? 28 : 14;
            DateDue = DateBorrowed.AddDays(allowance);
        }
    }
}
