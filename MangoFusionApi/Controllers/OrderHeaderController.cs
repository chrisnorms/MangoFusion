using MangoFusionApi.Data;
using MangoFusionApi.Models;
using MangoFusionApi.Models.Dto;
using MangoFusionApi.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MangoFusionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderHeaderController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly ApiResponse response;
        public OrderHeaderController(ApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
            response = new ApiResponse();
        }

        [HttpGet]
        public ActionResult<ApiResponse> GetOrders(string userId = "")
        {
            IEnumerable<OrderHeader> orderHeaderList = db.OrderHeaders.Include(o => o.OrderDetails)
                .ThenInclude(u => u.MenuItem)
                .OrderByDescending(u => u.OrderHeaderId);

            if (!string.IsNullOrEmpty(userId))
            {
                orderHeaderList = orderHeaderList.Where(u => u.ApplicationUserId == userId);
            }

            response.Result = orderHeaderList;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpGet("{orderId:int}")]
        public ActionResult<ApiResponse> GetOrder(int orderId)
        {
            if (orderId == 0)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages.Add("Invalid order id");
                return BadRequest(response);
            }
            OrderHeader? orderHeader = db.OrderHeaders.Include(o => o.OrderDetails)
                .ThenInclude(u => u.MenuItem)
                .FirstOrDefault(u => u.OrderHeaderId == orderId);

            if (orderHeader == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages.Add("Order not found");
                return NotFound(response);
            }

            response.Result = orderHeader;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpPost]
        public ActionResult<ApiResponse> CreateOrder([FromBody] OrderHeaderCreateDto orderHeaderCreateDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    OrderHeader orderHeader = new OrderHeader
                    {
                        PickUpName = orderHeaderCreateDto.PickUpName,
                        PickUpEmail = orderHeaderCreateDto.PickUpEmail,
                        PickUpPhoneNumber = orderHeaderCreateDto.PickUpPhoneNumber,
                        OrderDate = DateTime.Now,
                        OrderTotal = orderHeaderCreateDto.OrderTotal,
                        Status = SD.Status_Confirmed,
                        TotalItem = orderHeaderCreateDto.TotalItem,
                        ApplicationUserId = orderHeaderCreateDto.ApplicationUserId,
                    };

                    db.OrderHeaders.Add(orderHeader);
                    db.SaveChanges();

                    foreach (var orderDetailDto in orderHeaderCreateDto.OrderDetailsDto)
                    {
                        OrderDetail orderDetail = new OrderDetail
                        { 
                            OrderHeaderId = orderHeader.OrderHeaderId,
                            MenuItemId = orderDetailDto.MenuItemId,
                            Quantity = orderDetailDto.Quantity,
                            ItemName = orderDetailDto.ItemName,
                            Price = orderDetailDto.Price
                        };

                        db.OrderDetails.Add(orderDetail);
                    }
                    db.SaveChanges();

                    response.Result = orderHeader;
                    orderHeader.OrderDetails = [];
                    response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtAction(nameof(GetOrder), new { orderId = orderHeader.OrderHeaderId }, response);
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

        [HttpPut("{orderId:int}")]
        public ActionResult<ApiResponse> UpdateOrder(int orderId, [FromBody] OrderHeaderUpdateDto orderHeaderDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (orderId != orderHeaderDto.OrderHeaderId)
                    {
                        response.IsSuccess = false;
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.ErrorMessages.Add("Order id not valid");
                        return BadRequest(response);
                    }

                    OrderHeader? orderHeaderFromDb = db.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == orderId);
                    if (orderHeaderFromDb == null)
                    {
                        response.IsSuccess = true;
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.ErrorMessages.Add("Order not found");
                        return NotFound(response);
                    }

                    if (!string.IsNullOrEmpty(orderHeaderDto.PickUpName))
                    { 
                        orderHeaderFromDb.PickUpName = orderHeaderDto.PickUpName;
                    }
                    if (!string.IsNullOrEmpty(orderHeaderDto.PickUpPhoneNumber))
                    {
                        orderHeaderFromDb.PickUpPhoneNumber = orderHeaderDto.PickUpPhoneNumber;
                    }
                    if (!string.IsNullOrEmpty(orderHeaderDto.PickUpEmail))
                    {
                        orderHeaderFromDb.PickUpEmail = orderHeaderDto.PickUpEmail;
                    }
                    if (!string.IsNullOrEmpty(orderHeaderDto.Status))
                    {
                        if (orderHeaderDto.Status.Equals(SD.Status_Confirmed, StringComparison.InvariantCultureIgnoreCase)
                            && orderHeaderDto.Status.Equals(SD.Status_ReadyForPickUp, StringComparison.InvariantCultureIgnoreCase))
                        {
                            orderHeaderFromDb.Status = SD.Status_ReadyForPickUp;
                        }

                        if (orderHeaderDto.Status.Equals(SD.Status_ReadyForPickUp, StringComparison.InvariantCultureIgnoreCase)
                            && orderHeaderDto.Status.Equals(SD.Status_Completed, StringComparison.InvariantCultureIgnoreCase))
                        {
                            orderHeaderFromDb.Status = SD.Status_Completed;
                        }

                        if (orderHeaderDto.Status.Equals(SD.Status_Cancelled, StringComparison.InvariantCultureIgnoreCase))
                        {
                            orderHeaderFromDb.Status = SD.Status_Cancelled;
                        }
                    }

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
