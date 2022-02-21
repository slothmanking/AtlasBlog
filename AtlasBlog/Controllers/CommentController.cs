#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AtlasBlog.Data;
using AtlasBlog.Models;
using AtlasBlog.Services;
using Microsoft.AspNetCore.Identity;

namespace AtlasBlog.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public CommentController(ApplicationDbContext context, UserManager<BlogUser> userManager, SlugService slugService)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comment
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comment.Include(c => c.Author).Include(c => c.BlogPost);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Author)
                .Include(c => c.BlogPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }


        // POST: Comment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogPostId,CommentBody")] Comment comment, string slug)
        {
            if (ModelState.IsValid)
            {
                comment.AuthorId = _userManager.GetUserId(User);
                _context.Add(comment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "BlogPost", new{slug}, "CommentSection");
        }

        
        // POST: Comment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CommentBody")] Comment comment, string slug)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }
            
            try
            {
                var commentSnapshot = await _context.Comment.FindAsync(comment.Id);
                if (commentSnapshot == null)
                {
                    return NotFound();
                }

                commentSnapshot.CommentBody = comment.CommentBody;
                commentSnapshot.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Details", "BlogPost", new {slug}, "CommentSection");
        }
       
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Moderate(int id, [Bind("Id,ModeratedBody,ModerationReason")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            Comment commentSnapShot = new();
            try
            {
                //var commentSnapShot = await _context.Comments.FindAsync(comment.Id);
                commentSnapShot = await _context.Comment
                    .Include(c => c.BlogPost)
                    .FirstOrDefaultAsync(c => c.Id == comment.Id);

                if (commentSnapShot == null)
                {
                    return NotFound();
                }

                commentSnapShot.ModeratedDate = DateTime.UtcNow;
                commentSnapShot.ModeratedBody = comment.ModeratedBody;
                commentSnapShot.ModerationReason = comment.ModerationReason;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Details", "BlogPosts", new { slug = commentSnapShot.BlogPost.Slug }, "CommentSection");
        }
        
        
        // GET: Comment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Author)
                .Include(c => c.BlogPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comment
                .Include(c => c.BlogPost)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "BlogPost", new {slug = comment.BlogPost.Slug}, "CommentSection");
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.Id == id);
        }
    }
}
