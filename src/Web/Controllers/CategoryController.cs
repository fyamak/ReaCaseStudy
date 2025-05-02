using Business.RequestHandlers.Category;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Results;
using Web.Controllers.Base;
using Web.Filters;

namespace Web.Controllers;

public class CategoryController(IMediator mediator) : BaseController(mediator)
{

    [HttpGet]
    [Authorize]
    public async Task<DataResult<List<GetAllCategories.GetAllCategoriesResponse>>> Categories()
    {
        return await Mediator.Send(new GetAllCategories.GetAllCategoriesRequest());
    }

    [HttpPost]
    [Authorize]
    public async Task<DataResult<string>> Categories(CreateCategory.CreateCategoryRequest request)
    {
        return await Mediator.Send(request);
    }
  
    

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<DataResult<string>> Categories(int id)
    {
        return await Mediator.Send(new DeleteCategory.DeleteCategoryRequest { Id = id });
    }

}
