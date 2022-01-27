using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CastleLibrary.Models;

namespace CastleLibrary.Models
{
    public class RecordOfLoanViewModel
    {
        public RecordOfLoan Record { get; set; }
        public LibraryUser LibUser { get; set; }
        public Book Book { get; set; }
        public IEnumerable<RecordOfLoan> RecordList { get; set; } = new List<RecordOfLoan>();
        public IEnumerable<Book> BookList { get; set; } = new List<Book>();
        public IEnumerable<LibraryUser> UserList { get; set; } = new List<LibraryUser>();

        [BindProperty(SupportsGet=true)]
        public string SearchString1 { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString2 { get; set; }           
        
    }
}
