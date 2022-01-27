using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CastleLibrary.Models;
using CastleLibrary.Data;

namespace CastleLibrary.Controllers
{
    public class LibraryUsersController : Controller
    {
        private readonly CastleLibraryContext _context;

        public LibraryUsersController(CastleLibraryContext context)
        {
            _context = context;
        }

        // GET: LibraryUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.LibraryUsers.Include(l => l.Records).OrderBy(l => l.Name).ToListAsync());
        }

        // GET: LibraryUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            RecordOfLoanViewModel R = new RecordOfLoanViewModel();
            if (id == null)
            {
                return NotFound();
            }

            R.LibUser = await _context.LibraryUsers.AsNoTracking().Include(l => l.Records).FirstOrDefaultAsync(l => l.ID == id);               
                

            var historicLoans = from r in _context.LoanRecords
                                     .Include(r => r.Book)
                                     .Include(r => r.LibraryUser)
                                     .AsNoTracking()
                                     .OrderByDescending(r => r.DateBorrowed)
                                where r.LibraryUserID == id
                                select r;            

            if (R.LibUser == null)
            {
                return NotFound();
            }
            
            R.RecordList = await historicLoans.ToListAsync();
            //R.RecordList.OrderByDescending(r => r.DateBorrowed);

            return View(R);
        }

        [HttpPost]
        public async Task<IActionResult> Details(string finePayment, int id)
        {
            RecordOfLoanViewModel R = new RecordOfLoanViewModel();
            R.LibUser = await _context.LibraryUsers.Include(l => l.Records).FirstOrDefaultAsync(l => l.ID == id);
            LibraryUser libraryUser = await _context.LibraryUsers.FirstOrDefaultAsync(l => l.ID == id);
            bool ParseSuccess = Double.TryParse(finePayment, out Double Payment);
            if(!ParseSuccess)
            {
                ViewBag.PaymentFail = "Please input a number as a payment amount";
                return View(R);
            }
            double number = libraryUser.CalculateTotalFines();
            
            if (Payment > libraryUser.FinesOutstanding)
            {
                ViewBag.PaymentFail = "Payment failed. You can't pay more than your outstanding fine amount.";
                return View(R);
            }

            libraryUser.FinesPaid += Payment;
            _context.Attach(libraryUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            ViewBag.PaymentSuccess = $"Thank you, you have just paid {Payment} ZAR. Your outstanding loans are {libraryUser.FinesOutstanding} ZAR";
            return View(R);
        }
        // GET: LibraryUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LibraryUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,NumBooksOut,IsGoldMember,EmailAddress,MaxBooksAllowed,FinesTotal,FinesPaid")] LibraryUser libraryUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(libraryUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(libraryUser);
        }

        // GET: LibraryUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libraryUser = await _context.LibraryUsers.FindAsync(id);
            if (libraryUser == null)
            {
                return NotFound();
            }
            return View(libraryUser);
        }

        // POST: LibraryUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,NumBooksOut,IsGoldMember,EmailAddress,MaxBooksAllowed,FinesTotal,FinesPaid")] LibraryUser libraryUser)
        {
            if (id != libraryUser.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(libraryUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibraryUserExists(libraryUser.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(libraryUser);
        }

        // GET: LibraryUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libraryUser = await _context.LibraryUsers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (libraryUser == null)
            {
                return NotFound();
            }

            return View(libraryUser);
        }

        // POST: LibraryUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libraryUser = await _context.LibraryUsers.FindAsync(id);
            _context.LibraryUsers.Remove(libraryUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //public async void Reset()
        //{
        //    foreach(LibraryUser item in _context.LibraryUsers)
        //    {
        //        item.NumBooksOut = 0;
        //        item.FinesPaid = 0; 
        //        item.FinesTotal = 0;
        //    }
        //    await _context.SaveChangesAsync();
        //    ViewData["ResetConfirmed"] = "All Library Users now have 0 fines outstanding, 0 fines paid, and no Books out";

        //}

        private bool LibraryUserExists(int id)
        {
            return _context.LibraryUsers.Any(e => e.ID == id);
        }

        
    }
}
