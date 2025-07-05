using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Opti.Data;
using Opti.Models;

namespace Opti.Controllers
{
    public class MachineLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MachineLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MachineLogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MachineLogs.Include(m => m.Machine).Include(m => m.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MachineLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineLog = await _context.MachineLogs
                .Include(m => m.Machine)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.LogId == id);
            if (machineLog == null)
            {
                return NotFound();
            }

            return View(machineLog);
        }

        // GET: MachineLogs/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines, "MachineId", "MachineId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: MachineLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LogId,MachineId,Action,Timestamp,UserId")] MachineLog machineLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(machineLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "MachineId", "MachineId", machineLog.MachineId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", machineLog.UserId);
            return View(machineLog);
        }

        // GET: MachineLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineLog = await _context.MachineLogs.FindAsync(id);
            if (machineLog == null)
            {
                return NotFound();
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "MachineId", "MachineId", machineLog.MachineId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", machineLog.UserId);
            return View(machineLog);
        }

        // POST: MachineLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogId,MachineId,Action,Timestamp,UserId")] MachineLog machineLog)
        {
            if (id != machineLog.LogId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(machineLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineLogExists(machineLog.LogId))
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
            ViewData["MachineId"] = new SelectList(_context.Machines, "MachineId", "MachineId", machineLog.MachineId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", machineLog.UserId);
            return View(machineLog);
        }

        // GET: MachineLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineLog = await _context.MachineLogs
                .Include(m => m.Machine)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.LogId == id);
            if (machineLog == null)
            {
                return NotFound();
            }

            return View(machineLog);
        }

        // POST: MachineLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machineLog = await _context.MachineLogs.FindAsync(id);
            if (machineLog != null)
            {
                _context.MachineLogs.Remove(machineLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MachineLogExists(int id)
        {
            return _context.MachineLogs.Any(e => e.LogId == id);
        }
    }
}
