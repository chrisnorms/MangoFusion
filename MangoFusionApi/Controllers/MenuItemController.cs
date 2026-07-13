using MangoFusionApi.Data;
using MangoFusionApi.Models;
using MangoFusionApi.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MangoFusionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly ApiResponse response;
        private readonly IWebHostEnvironment env;
        public MenuItemController(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.db = applicationDbContext;
            response = new ApiResponse();
            env = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult GetMenuItems()
        {
            List<MenuItem> menuItems = db.MenuItems.ToList();
            List<OrderDetail> orderDetailsListWithRatings = db.OrderDetails.Where(o => o.Rating != null).ToList();

            foreach (var menuItem in menuItems)
            {
                var ratings = orderDetailsListWithRatings.Where(o => o.MenuItemId == menuItem.Id).Select(m => m.Rating.Value);
                double averageRating = ratings.Any() ? ratings.Average() : 0;
                menuItem.Rating = averageRating;
            }

            response.Result = menuItems;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpGet("{id:int}", Name = "GetMenuItem")]
        public IActionResult GetMenuItem(int id) 
        {
            if (id == 0)
            { 
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                return BadRequest(response);
            }

            MenuItem? menuItem = db.MenuItems.FirstOrDefault(m => m.Id == id);
            List<OrderDetail> orderDetailsListWithRatings = db.OrderDetails.Where(o => o.Rating != null && o.MenuItemId == menuItem.Id).ToList();
            
            var ratings = orderDetailsListWithRatings.Where(o => o.MenuItemId == menuItem.Id).Select(m => m.Rating.Value);
            double averageRating = ratings.Any() ? ratings.Average() : 0;
            menuItem.Rating = averageRating;
            response.Result = menuItem;
            response.StatusCode = HttpStatusCode.OK;
            //apiResponse.IsSuccess = true;
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm]MenuItemCreateDto menuItemCreateDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDto.File == null || menuItemCreateDto.File.Length == 0)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.IsSuccess = false;
                        response.ErrorMessages = ["File is required"];
                        return BadRequest(response);
                    }

                    var imagesPath = Path.Combine(env.WebRootPath, "images");
                    if (!Directory.Exists(imagesPath))
                    { 
                        Directory.CreateDirectory(imagesPath);
                    }

                    var filePath = Path.Combine(imagesPath, menuItemCreateDto.File.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    // Upload image
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await menuItemCreateDto.File.CopyToAsync(stream);
                    }

                    MenuItem menuItem = new MenuItem()
                    {
                        Name = menuItemCreateDto.Name,
                        Description = menuItemCreateDto.Description,
                        Category = menuItemCreateDto.Category,
                        SpecialTag = menuItemCreateDto.SpecialTag,
                        Image = "images/" + menuItemCreateDto.File.FileName
                    };

                    db.MenuItems.Add(menuItem);
                    await db.SaveChangesAsync();

                    response.Result = menuItemCreateDto;
                    response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItem.Id }, response);
                }
                else
                {
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = [ex.ToString()];
            }

            return BadRequest(response);
        }

        [HttpPut("{id:int}", Name = "UpdateMenuItem")]
        public async Task<ActionResult<MenuItemUpdateDto>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDto menuItemUpdateDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDto == null || menuItemUpdateDto.Id == 0)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.IsSuccess = false;
                        response.ErrorMessages = ["Invalid input"];
                        return BadRequest(response);
                    }

                    MenuItem? menuItemFromDb = await db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);

                    if (menuItemFromDb == null)
                    {
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.IsSuccess = false;
                        return NotFound(response);
                    }

                    menuItemFromDb.Name = menuItemUpdateDto.Name;
                    menuItemFromDb.Description = menuItemUpdateDto.Description;
                    menuItemFromDb.Category = menuItemUpdateDto.Category;
                    menuItemFromDb.SpecialTag = menuItemUpdateDto.SpecialTag;

                    if (menuItemUpdateDto.File != null && menuItemUpdateDto.File.Length > 0)
                    {
                        var imagesPath = Path.Combine(env.WebRootPath, "images");
                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                        }

                        var filePath = Path.Combine(imagesPath, menuItemUpdateDto.File.FileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        var filePathOldFile = Path.Combine(env.WebRootPath, menuItemFromDb.Image);
                        if (System.IO.File.Exists(filePathOldFile))
                        {
                            System.IO.File.Delete(filePathOldFile);
                        }

                        // Upload image
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await menuItemUpdateDto.File.CopyToAsync(stream);
                        }

                        menuItemFromDb.Image = "images/" + menuItemUpdateDto.File.FileName;
                    }

                    db.MenuItems.Update(menuItemFromDb);
                    await db.SaveChangesAsync();

                    //apiResponse.Result = menuItemUpdateDto;
                    response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(response);
                }
                else
                {
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = [ex.ToString()];
            }

            return BadRequest(response);
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            if (id == 0)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMessages = ["Invalid input"];
                return BadRequest(response);
            }

            MenuItem? menuItemFromDb = await db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);

            if (menuItemFromDb == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.IsSuccess = false;
                return NotFound(response);
            }

            var filePathOldFile = Path.Combine(env.WebRootPath, menuItemFromDb.Image);
            if (System.IO.File.Exists(filePathOldFile))
            {
                System.IO.File.Delete(filePathOldFile);
            }

            db.MenuItems.Remove(menuItemFromDb);
            await db.SaveChangesAsync();

            response.StatusCode = HttpStatusCode.NoContent;
            return Ok(response);
        }
    }
}
