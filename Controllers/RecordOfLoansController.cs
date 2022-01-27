using CastleLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CastleLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookCatalogMvc.Controllers
{
    public class RecordOfLoansController : Controller
    {
        private readonly CastleLibraryContext _context;

        public RecordOfLoansController(CastleLibraryContext context)
        {
            _context = context;
        }      
               
       
        private bool RecordOfLoanExists(int id)
        {
            return _context.LoanRecords.Any(e => e.ID == id);
        }
       
        public async Task<IActionResult> Index(string filter, string sortField)
        {
            ViewBag.FilterList = new List<string> { "All records", "Unreturned", "Overdue" };
            ViewBag.SortFieldList = new List<string> { "Book", "Borrowed on", "User", "Due on", "Returned on" };

            ViewBag.Filter = filter ?? "All records";
            ViewBag.SortField = sortField ?? "Due on";
            

            if (!String.IsNullOrEmpty(sortField)) { ViewBag.SortField = sortField; }

            var records = from l in _context.LoanRecords
                           .Include(l => l.Book)
                           .Include(l => l.LibraryUser)
                          select l;

            records = records.OrderBy(l => l.DateBorrowed);
            
            switch (sortField)
            {
                case "Book":
                    records = records.OrderBy(r => r.Book.Title);
                    break;
                case "Borrowed on":
                    records = records.OrderByDescending(r => r.DateBorrowed);
                    break;
                case "User":
                    records = records.OrderBy(r => r.LibraryUser.Name);
                    break;
                case "Due on":
                    records = records.OrderByDescending(r => r.DateDue);
                    break;
                case "Returned on":
                    records = records.OrderByDescending(r => r.DateReturned);
                    break;
            }

            List<RecordOfLoan> L = await records.ToListAsync();

            switch (filter)
            {
                case "Unreturned":
                    L = L.FindAll(r => r.DateReturned == null);
                    break;
                case "Overdue":
                    L = L.FindAll(r => r.IsOverdue == true);
                    break;
                default:
                    break;
            }
            return View(L);
        }
              
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int RecordId)
        {
            var record = await _context.LoanRecords
                               .Include(r => r.Book)
                               .Include(r => r.LibraryUser)
                               .FirstOrDefaultAsync(r => r.ID == RecordId);

           
            record.IsReturned = true;
            record.Book.IsAvailable = true;
            record.Book.BorrowedBy = null;
            record.LibraryUser.NumBooksOut--;
            record.DateReturned = DateTime.Now;
            //record.LibraryUser.FinesTotal += record.Fine;

            _context.Attach(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RedirectToPage("/Records/Index");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(string SearchString1, string SearchString2)
        {
            RecordOfLoanViewModel m = new RecordOfLoanViewModel();            

            var books = from b in _context.Books
                            .Include(b => b.Author)
                            .AsNoTracking()
                        select b;

            var libraryusers = from l in _context.LibraryUsers
                               select l;

            if (!string.IsNullOrEmpty(SearchString1)) { books = books.Where(b => b.Title.Contains(SearchString1)); }
            if (!string.IsNullOrEmpty(SearchString2)) { libraryusers = libraryusers.Where(l => l.Name.Contains(SearchString2));
            }

            m.BookList = books;        
            m.UserList = await libraryusers.ToListAsync();

            return View(m);
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecordOfLoan Record)
        {
            RecordOfLoanViewModel m = new RecordOfLoanViewModel();
            RecordOfLoan NewRecord = Record;
            
            var bookToTakeOut = await _context.Books.FirstOrDefaultAsync(b => b.ID == Record.BookID);
            var libraryUser = await _context.LibraryUsers
                                    .Include(l => l.Records)
                                    .FirstOrDefaultAsync(l => l.ID == Record.LibraryUserID);

            if (libraryUser == null) { ViewBag.InvalidUserError = "Please enter a valid User ID"; }
            if (bookToTakeOut == null) { ViewBag.InvalidBookError = "Please enter a valid Book ID"; }
            if (libraryUser == null | bookToTakeOut == null) { return View(m); }

            NewRecord.Book = bookToTakeOut;
            NewRecord.LibraryUser = libraryUser;

            if (!NewRecord.Book.IsAvailable | libraryUser.NumBooksOut >= libraryUser.MaxBooksAllowed | libraryUser.IsLoanBlocked)
            {
                if (!NewRecord.Book.IsAvailable) { ViewBag.InvalidBookError = "This book is unavailable"; }
                if(libraryUser.NumBooksOut >= libraryUser.MaxBooksAllowed) { ViewBag.InvalidUserError = "This User has reached the maximum number of book loans. Please return a book in order to take out another."; }
                if (libraryUser.IsLoanBlocked) { ViewBag.InvalidUserError2 = "This user has too many unpaid fines."; }
                return View(m);
            }

            NewRecord.CalculateDateDue();
            NewRecord.Book.IsAvailable = false;
            NewRecord.Book.BorrowedBy = libraryUser;
            NewRecord.Book.DueInDate = NewRecord.DateDue;
            NewRecord.LibraryUser.NumBooksOut++;
            NewRecord.LibraryUser.Records.Add(NewRecord);


            if (!ModelState.IsValid)
            {               
                return View(m);
            }

            _context.LoanRecords.Add(NewRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.BookId = (int)id;
            if (id == null)
            {
                return NotFound();
            }

            RecordOfLoan record = await _context.LoanRecords
                              .Include(r => r.Book)
                              .Include(r => r.LibraryUser)
                              .AsNoTracking()
                              .FirstOrDefaultAsync(m => m.ID == id);
            if (record == null)
            {
                return NotFound();
            }
            return View(record);
        }
        

        public async Task<IActionResult> Edit(int? id)
        {
            
            ViewData["SelectBook"] = new SelectList(_context.Books, nameof(Book.ID), nameof(Book.Title));
            ViewData["SelectUser"] = new SelectList(_context.LibraryUsers, nameof(LibraryUser.ID), nameof(LibraryUser.Name));

            if (id == null)
            {
                return NotFound();
            }

            RecordOfLoan record = await _context.LoanRecords
                              .Include(r => r.Book)                              
                              .Include(r => r.Book.Author)
                              .Include(r => r.Book.BorrowedBy)
                              .Include(r => r.LibraryUser)                              
                              .FirstOrDefaultAsync(m => m.ID == id);

            if (record == null)
            {
                return NotFound();
            }
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind]RecordOfLoan Record)
        {
            if (id != Record.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View();
            }
            Record.LibraryUser = await _context.LibraryUsers.FirstOrDefaultAsync(l => l.ID == Record.LibraryUserID);
            Record.Book = await _context.Books.FirstOrDefaultAsync(b => b.ID == Record.BookID);
            Record.Book.DueInDate = Record.DateDue;
            _context.Attach(Record).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecordOfLoanExists(Record.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index");
        }
               
        public async Task<IActionResult> ReturnBook(string SearchString1)
        {
            RecordOfLoanViewModel m = new RecordOfLoanViewModel();

            if(SearchString1 != null)
            {
                bool success = Int32.TryParse(SearchString1, out int userID);
                if (success)
                {
                    var libraryUser = await _context.LibraryUsers
                            .Include(l => l.Records)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(l => l.ID == userID);

                    ViewBag.InvalidUserError = libraryUser == null ? "Please enter a valid User ID" : "";

                    var unreturnedLoans = from r in _context.LoanRecords
                                            .Include(r => r.Book)
                                            .Include(r => r.LibraryUser)
                                            .AsNoTracking()
                                          where r.LibraryUser == libraryUser && r.IsReturned == false
                                          select r;


                    m.RecordList = unreturnedLoans.ToList();
                    m.LibUser = libraryUser;
                }
                else if (SearchString1 != null)
                {
                    ViewBag.InvalidNumberError = "Please enter a number";
                }

            }
            else
            {
                var unreturnedLoans = from r in _context.LoanRecords
                                            .Include(r => r.Book)
                                            .Include(r => r.LibraryUser)
                                            .AsNoTracking()
                                      where r.IsReturned == false
                                      select r;

                m.RecordList = unreturnedLoans;
            }           
            
            if(m.LibUser != null && !m.RecordList.Any()) { ViewBag.NoRecords = "This User does not currently have any outstanding book loans."; }
            return View(m);            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(RecordOfLoan Record)
        {
            RecordOfLoanViewModel m = new RecordOfLoanViewModel();
            if(Record.DateReturned == null) 
            {
                ViewBag.DateReturnedError = "Please enter the date in the format DD/MM/YYYY.";                
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.ID == Record.BookID);

            if(book == null)
            {
                ViewBag.InvalidBookError = "Please enter a valid Book ID";
                return View(m);
            }
            if(book.IsAvailable)
            {
                ViewBag.BookNotLoanedError = "This book is not currently on loan";
                return View(m);
            }
            
            var record = await _context.LoanRecords
                               .Include(r => r.Book)
                               .Include(r => r.LibraryUser)
                               .FirstOrDefaultAsync(r => r.IsReturned == false && r.BookID == Record.BookID);           
            
            record.IsReturned = true;
            record.Book.IsAvailable = true;
            record.DateReturned = Record.DateReturned;
            record.Book.BorrowedBy = null;
            record.LibraryUser.NumBooksOut--;
            //record.LibraryUser.FinesTotal += record.Fine;
            //record.DateReturned = DateTime.Now;                             

            _context.Attach(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record = await _context.LoanRecords
                .FirstOrDefaultAsync(m => m.ID == id);
            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.LoanRecords.FindAsync(id);
            //record.Book.IsAvailable = true;
            //record.Book.BorrowedBy = null;
            //record.LibraryUser.NumBooksOut--;

            _context.LoanRecords.Remove(record);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
