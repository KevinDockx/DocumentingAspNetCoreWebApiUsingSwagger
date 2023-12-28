using Asp.Versioning;
using AutoMapper;
using Library.API.Models;
using Library.API.Services; 
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

[Route("api/v{version:apiVersion}/authors")]
[ApiController]
[ApiVersion(2.0)]
public class AuthorsControllerSecondVersion(
    IAuthorRepository authorsRepository,
    IMapper mapper) : ControllerBase
{
    private readonly IAuthorRepository _authorsRepository = authorsRepository;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Get a list of authors, V2
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
    {
        var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
        return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
    }
}
