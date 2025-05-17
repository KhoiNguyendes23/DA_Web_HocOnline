using Microsoft.AspNetCore.Mvc;
using Do_An_Web_Hoc.Models;
using Microsoft.EntityFrameworkCore;

namespace Do_An_Web_Hoc.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveMeetingApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LiveMeetingApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả các buổi học trực tuyến
        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meetings = await _context.LiveMeetings
                .Include(m => m.Course)
                .Include(m => m.Lecturer)
                .ToListAsync();

            return Ok(meetings);
        }

        // Lấy chi tiết một buổi học theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(int id)
        {
            var meeting = await _context.LiveMeetings
                .Include(m => m.Course)
                .Include(m => m.Lecturer)
                .FirstOrDefaultAsync(m => m.MeetingId == id);

            if (meeting == null) return NotFound();

            return Ok(meeting);
        }

        // Tạo mới buổi học
        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] LiveMeeting meeting)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            meeting.Status = "Upcoming";  // Gán mặc định
            meeting.CreateAt = DateTime.UtcNow;

            _context.LiveMeetings.Add(meeting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeetingById), new { id = meeting.MeetingId }, meeting);
        }

        // Cập nhật buổi học
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, [FromBody] LiveMeeting meeting)
        {
            if (id != meeting.MeetingId) return BadRequest("Meeting ID mismatch");

            var existing = await _context.LiveMeetings.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = meeting.Title;
            existing.Description = meeting.Description;
            existing.StartTime = meeting.StartTime;
            existing.EndTime = meeting.EndTime;
            existing.MeetingCode = meeting.MeetingCode;
            existing.AttendeePassword = meeting.AttendeePassword;
            existing.ModeratorPassword = meeting.ModeratorPassword;
            existing.JoinUrl = meeting.JoinUrl;
            existing.ModeratorJoinUrl = meeting.ModeratorJoinUrl;
            existing.Status = meeting.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Xoá buổi học
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _context.LiveMeetings.FindAsync(id);
            if (meeting == null) return NotFound();

            _context.LiveMeetings.Remove(meeting);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
