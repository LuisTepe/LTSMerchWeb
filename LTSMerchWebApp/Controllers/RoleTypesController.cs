﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTSMerchWebApp.Models;

namespace LTSMerchWebApp.Controllers
{
    public class RoleTypesController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public RoleTypesController(LtsMerchStoreContext context)
        {
            _context = context;
        }

        // GET: RoleTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.RoleTypes.ToListAsync());
        }

        // GET: RoleTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roleType = await _context.RoleTypes
                .FirstOrDefaultAsync(m => m.RoleTypeId == id);
            if (roleType == null)
            {
                return NotFound();
            }

            return View(roleType);
        }

        // GET: RoleTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RoleTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleTypeId,RoleName")] RoleType roleType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roleType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roleType);
        }

        // GET: RoleTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roleType = await _context.RoleTypes.FindAsync(id);
            if (roleType == null)
            {
                return NotFound();
            }
            return View(roleType);
        }

        // POST: RoleTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleTypeId,RoleName")] RoleType roleType)
        {
            if (id != roleType.RoleTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roleType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleTypeExists(roleType.RoleTypeId))
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
            return View(roleType);
        }

        // GET: RoleTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roleType = await _context.RoleTypes
                .FirstOrDefaultAsync(m => m.RoleTypeId == id);
            if (roleType == null)
            {
                return NotFound();
            }

            return View(roleType);
        }

        // POST: RoleTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roleType = await _context.RoleTypes.FindAsync(id);
            if (roleType != null)
            {
                _context.RoleTypes.Remove(roleType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoleTypeExists(int id)
        {
            return _context.RoleTypes.Any(e => e.RoleTypeId == id);
        }
    }
}