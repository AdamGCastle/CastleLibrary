using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace CastleLibrary.Models
{
    public class Author
    {        
        public int ID { get; set; }
        
        [Required(ErrorMessage = "Please enter the Author's full name.")]
        public string Name { get; set; }
        public ICollection<Book> Books { get; set; }
        
    }
}
