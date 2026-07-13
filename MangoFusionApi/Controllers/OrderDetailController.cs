using MangoFusionApi.Data;
using MangoFusionApi.Models;
using MangoFusionApi.Models.Dto;
using MangoFusionApi.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MangoFusionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly ApiResponse response;
        public OrderDetailController(ApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
            response = new ApiResponse();
        }

        [HttpPut("{orderDetailId:int}")]
        public ActionResult<ApiResponse> UpdateOrder(int orderDetailId, [FromBody] OrderDetailUpdateDto orderDetailDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (orderDetailId != orderDetailDto.OrderDetailId)
                    {
                        response.IsSuccess = false;
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.ErrorMessages.Add("Order id not valid");
                        return BadRequest(response);
                    }

                    OrderDetail? orderDetailFromDb = db.OrderDetails.FirstOrDefault(o => o.OrderDetailId == orderDetailId);
                    if (orderDetailFromDb == null)
                    {
                        response.IsSuccess = true;
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.ErrorMessages.Add("Order not found");
                        return NotFound(response);
                    }

                    orderDetailFromDb.Rating = orderDetailDto.Rating;

                    db.SaveChanges();

                    response.IsSuccess = true;
                    response.StatusCode = HttpStatusCode.NoContent;
                    //return CreatedAtAction(nameof(GetOrder), new { orderId=orderHeaderDto.OrderHeaderId}, response);
                    return Ok(response);
                }
                else
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.ErrorMessages = ModelState.Values.SelectMany(u => u.Errors).Select(u => u.ErrorMessage).ToList();
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}
