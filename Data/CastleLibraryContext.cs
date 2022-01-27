using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CastleLibrary.Models;

namespace CastleLibrary.Data
{
    public class CastleLibraryContext : DbContext
    {
        public CastleLibraryContext (DbContextOptions<CastleLibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<LibraryUser> LibraryUsers { get; set; }        
        public DbSet<RecordOfLoan> LoanRecords { get; set;  }

        
    }
}
