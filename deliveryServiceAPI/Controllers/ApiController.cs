using deliveryServiceAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DeliveryService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingRepository _repository;

        public BookingController(BookingRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<List<BookingDTO>> GetBookings()
        {
            var bookings = _repository.GetAll();
            return Ok(bookings);
        }
    }
}
