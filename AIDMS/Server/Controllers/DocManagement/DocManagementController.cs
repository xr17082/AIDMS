using AIDMS.Shared.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AIDMS.Server.Controllers.DocManagement
{
    [Authorize]
    [Route("api/docs")]
    [ApiController]
    public class DocManagementController(IDocumentProcessor documentProcessor) : ControllerBase
    {
        [HttpGet("search-documents")]
        public async Task<IActionResult> Search(string query)
        {
            var result = await documentProcessor.SearchDocuments(query);
            return Ok(result);
        }
    }
}
