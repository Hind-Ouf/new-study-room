using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Models;
using StudyRoomBooking.Services;

namespace StudyRoomBooking.Controllers;

[Authorize]
public sealed class CoursesController : Controller
{
    private readonly ICourseService _courses;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ICourseService courses, ILogger<CoursesController> logger)
    {
        _courses = courses;
        _logger = logger;
    }

    public async Task<IActionResult> Index() => View(await _courses.GetAllAsync());

    [HttpGet]
    public IActionResult Create() => View(new Course());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid) return View(course);
        try
        {
            await _courses.AddAsync(course);
            TempData["Success"] = $"Course {course.Code} was added.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Could not create course {CourseCode}", course.Code);
            ModelState.AddModelError(nameof(course.Code), "That course code is already in use.");
            return View(course);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _courses.GetByIdAsync(id);
        return course == null ? NotFound() : View(course);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        if (id != course.Id) return BadRequest();
        if (!ModelState.IsValid) return View(course);
        try
        {
            await _courses.UpdateAsync(course);
            TempData["Success"] = $"Course {course.Code} was updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Could not update course {CourseId}", id);
            ModelState.AddModelError(nameof(course.Code), "That course code is already in use.");
            return View(course);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _courses.GetByIdAsync(id);
        return course == null ? NotFound() : View(course);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _courses.DeleteAsync(id);
            TempData["Success"] = "Course deleted. Linked bookings will remain.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Could not delete course {CourseId}", id);
            TempData["Error"] = "The course could not be deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
