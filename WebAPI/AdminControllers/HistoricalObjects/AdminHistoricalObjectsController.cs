using Application.DataQuery;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using OsmSharp.IO.Json.Converters;
using WebAPI.AdminControllers.HistoricalObjects.Requests;
using WebAPI.AdminControllers.HistoricalObjects.Responses;
using WebAPI.Controllers.HistoricalLines.Responses;
using WebAPI.Controllers.Shared;
using WebAPI.Controllers.Shared.Responses;
using WebAPI.Controllers.Utility;

namespace WebAPI.AdminControllers.HistoricalObjects;

[Route("/api/admin/historical-lines/{lineId:guid}/objects")]
public class AdminHistoricalObjectsController : Controller
{
    private readonly BaseService<HistoricalLine> _lineService;
    private readonly BaseService<HistoricalObject> _objectService;
    private readonly ObjectImageService _imageService;
    private readonly IWebHostEnvironment _env;

    public AdminHistoricalObjectsController(BaseService<HistoricalLine> lineService, BaseService<HistoricalObject> objectService,
        ObjectImageService imageService, IWebHostEnvironment env)
    {
        _lineService = lineService;
        _objectService = objectService;
        _imageService = imageService;
        _env = env;
    }
    
    
    /// <summary>
    /// Получить список объектов в исторической линии
    /// </summary>
    [HttpGet()]
    [ProducesResponseType(typeof(GetObjectsInLineResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLineObjects([FromRoute] Guid lineId)
    {
        var objects = await _objectService.GetAsync(new DataQueryParams<HistoricalObject>()
        {
            Expression = l => l.LineId == lineId,
            Sorting = new SortingParams<HistoricalObject>
            {
                OrderBy = obj => obj.Order,
                Ascending = true
            }
        });

        var response = new GetObjectsInLineResponse
        {
            Objects = objects.Select(obj => DtoConvert.HistoricalObjectToResponse(obj, HttpContext)).ToArray()
        };
        return Ok(response);
    }
    
    /// <summary>
    /// Добавить истор. объект в историческую линию
    /// </summary>
    [HttpPost()]
    [ProducesResponseType(typeof(HistoricalObjectInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddObjectToLine([FromRoute] Guid lineId, [FromBody] AddNewObjectRequest dto)
    {
        if (dto.Coords.Length != 2)
        {
            return SharedResponses.FailedRequest("Request must contain \"coords\" field with 2 float values.");
        }

        var currentObjects = await _objectService.GetAsync(new DataQueryParams<HistoricalObject>
        {
            Expression = obj => obj.LineId == lineId,
            Sorting = new SortingParams<HistoricalObject>
            {
                OrderBy = obj => obj.Order,
                Ascending = true
            }
        });
        
        var obj = new HistoricalObject
        {
            Id = Guid.NewGuid(),
            Coordinates = DtoConvert.ConvertLatLonToPoint(dto.Coords[0], dto.Coords[1]),
            Order = currentObjects.Length == 0 ? 0 : currentObjects.Last().Order + 1,
            Title = "Новый объект",
            ImagePath = null,
            Description = "",
            VideoUrl = null,
            LineId = lineId
        };
        await _objectService.CreateAsync(obj);
        var response = DtoConvert.HistoricalObjectToResponse(obj, HttpContext);
        return Ok(response);
    }
    
    /// <summary>
    /// Изменить исторический объект
    /// </summary>
    [HttpPut("{objectId:guid}")]
    [ProducesResponseType(typeof(HistoricalObjectInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateObject([FromRoute] Guid lineId, [FromRoute] Guid objectId, 
        [FromBody] UpdateHistoricalObjectRequest dto)
    {
        if (dto.Coords.Length != 2)
        {
            return SharedResponses.FailedRequest("Request must contain \"coords\" field with 2 float values.");
        }

        var objects = await _objectService.GetAsync(new DataQueryParams<HistoricalObject>
        {
            Expression = obj => obj.LineId == lineId,
            Sorting = new SortingParams<HistoricalObject>
            {
                OrderBy = obj => obj.Order,
                Ascending = true
            }
        });
        var obj = objects.FirstOrDefault(obj => obj.Id == objectId);
        if (obj == null)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = false,
                Message = $"Historical object with provided id ({objectId}) not found in line with id ({lineId})"
            });
        }

        obj.Title = dto.Title;
        obj.Description = dto.Description;
        obj.VideoUrl = dto.VideoUrl;
        obj.Coordinates = DtoConvert.ConvertLatLonToPoint(dto.Coords[0], dto.Coords[1]);
        if (dto.Order != obj.Order)
        {
            var i = 0;
            foreach (var curObj in objects)
            {
                if (dto.Order == i)
                {
                    i++;
                }
                if (curObj.Id != obj.Id && curObj.Order != i)
                {
                    curObj.Order = i;
                    await _objectService.UpdateAsync(curObj);
                }

                if (curObj.Id != obj.Id)
                {
                    i++;
                }
            }
        }
        obj.Order = dto.Order;
        
        await _objectService.UpdateAsync(obj);
        var response = DtoConvert.HistoricalObjectToResponse(obj, HttpContext);
        return Ok(response);
    }
    
    /// <summary>
    /// Удалить исторический объект
    /// </summary>
    [HttpDelete("{objectId:guid}")]
    [ProducesResponseType(typeof(HistoricalObjectInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteObject([FromRoute] Guid lineId, [FromRoute] Guid objectId)
    {
        var objects = await _objectService.GetAsync(new DataQueryParams<HistoricalObject>
        {
            Expression = obj => obj.LineId == lineId,
            Sorting = new SortingParams<HistoricalObject>
            {
                OrderBy = obj => obj.Order,
                Ascending = true
            }
        });
        var obj = objects.FirstOrDefault(obj => obj.Id == objectId);
        if (obj == null)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = false,
                Message = $"Historical object with provided id ({objectId}) not found in line with id ({lineId})"
            });
        }
        
        if (obj.Order != objects.Length - 1)
        {
            var i = 0;
            foreach (var curObj in objects)
            {
                if (curObj.Id != obj.Id)
                {
                    if (curObj.Order != i)
                    {
                        curObj.Order = i;
                        await _objectService.UpdateAsync(curObj);
                    }
                    i++;
                }
            }
        }
        
        await _objectService.TryRemoveAsync(obj.Id);
        return Ok(new BaseStatusResponse
        {
            Completed = true,
            Message = "Historical object deleted"
        });
    }
    
    /// <summary>
    /// Обновить изображение исторического объекта
    /// </summary>
    [HttpPost("{objectId:guid}/image")]
    [ProducesResponseType(typeof(BaseStatusWithImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateObjectImage([FromRoute] Guid lineId, [FromRoute] Guid objectId, IFormFile? file)
    {
        if (file == null)
        {
            return SharedResponses.FailedRequest("Request doesn't contain file.");
        }

        var objects = await _objectService.GetAsync(new DataQueryParams<HistoricalObject>()
        {
            Expression = obj => obj.Id == objectId
        });
        if (objects.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No historical objects found with provided Object Id ({objectId})"
            });
        }
        var obj = objects[0];
        var fileData = new FileData
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Content = file.OpenReadStream()
        };
        await _imageService.UpdateObjectImageAsync(obj, _env.WebRootPath, fileData);

        return Ok(new BaseStatusWithImageResponse
        {
            Image = DtoConvert.ConvertImagePathToUrl(obj.ImagePath, HttpContext),
            Completed = true,
            Message = "Object's image updated"
        });
    }
    
    /// <summary>
    /// Удалить изображение исторического объекта
    /// </summary>
    [HttpDelete("{objectId:guid}/image")]
    [ProducesResponseType(typeof(BaseStatusWithImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteObjectImage([FromRoute] Guid lineId, [FromRoute] Guid objectId)
    {
        var objects = await _objectService.GetAsync(new DataQueryParams<HistoricalObject>()
        {
            Expression = obj => obj.Id == objectId
        });
        if (objects.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No historical objects found with provided Object Id ({objectId})"
            });
        }
        var obj = objects[0];
        await _imageService.UpdateObjectImageAsync(obj, _env.WebRootPath, null);

        return Ok(new BaseStatusWithImageResponse
        {
            Image = null,
            Completed = true,
            Message = "Object's image deleted"
        });
    }
}