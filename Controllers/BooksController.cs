using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CastleLibrary.Models;
using CastleLibrary.Data;

namespace BookCatalogMvcCastleLibrary.Controllers
{
    public class BooksController : Controller
    {
        private readonly CastleLibraryContext _context;

        public BooksController(CastleLibraryContext context)
        {
            _context = context;
        }
                   
        public Author Author { get; set; }
       
        public Book Book { get; set; }

        public async Task<IActionResult> Index(string searchString)        
        {
            //    var bookCatalogMvcContext = _context.Book.Include(b => b.Author);
            //    return View(await bookCatalogMvcContext.ToListAsync());

            var books = from b in _context.Books
                            .AsNoTracking()
                            .Include(b => b.Author)
                        select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));

            }
            var x = await books.ToListAsync();


            //switch (SortField)
            //{
            //    case "Title":
            //        books = books.OrderBy(x => x.Title);
            //        break;
            //    case "Author":
            //        books = books.OrderBy(x => x.Author.Name);
            //        break;
            //}

            return View(x);
        }
                
        public async Task<IActionResult> Details(int? id)
        {           
            if (id == null)
            {
                return NotFound();
            }

            Book = await _context.Books
                          .Include(b => b.Author)
                          .Include(b => b.BorrowedBy)
                          .AsNoTracking()
                          .FirstOrDefaultAsync(b => b.ID == id);

            if (Book == null)
            {
                return NotFound();
            }
            return View(Book);
        }

        
        public IActionResult Create(int? id)
        {
            //ViewData["AuthorID"] = new SelectList(_context.Set<Author>(), "ID", "Name");
            //return View();
            ViewData["AuthorID"] = new SelectList(_context.Authors, nameof(Author.ID), nameof(Author.Name));
            if (id != null)
            {
                Book.AuthorID = (int)id;
            }
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Create([Bind] Book book)
        //public async Task<IActionResult> Create([Bind("ID,Title,AuthorID,IsAvailable,DueInDate,YearPublished")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorID"] = new SelectList(_context.Set<Author>(), "ID", "Name", book.AuthorID);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,AuthorID,IsAvailable,DueInDate,YearPublished")] Book book)
        {
            if (id != book.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.ID))
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
            
            return RedirectToAction("Index");
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.ID == id);
        }
    }
}
