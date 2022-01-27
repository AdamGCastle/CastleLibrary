using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;



namespace CastleLibrary.Models
{
    public class Book    
    
    {        
        [Required, RegularExpression(@"^[0-9]{1,5}$", ErrorMessage = "The Book ID must be a number between 1 and 99999")]
        public int ID { get; set;  }
        [Required(ErrorMessage = "Please enter the title of the book.")]
        public string Title { get; set; }       
        public Author Author { get; set; }
        [Required(ErrorMessage = "Please select the book's author from the list. If it's not there, please create a new author.")]
        public int AuthorID { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Due in")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DueInDate { get; set; }

        [Display(Name = "Taken out by")]
        public LibraryUser BorrowedBy { get; set; }

        [Display(Name ="Published"), DisplayFormat(NullDisplayText = "Unknown")]
        public int? YearPublished { get; set; }
    }
}
