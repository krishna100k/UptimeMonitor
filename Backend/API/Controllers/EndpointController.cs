using Application.Features.Endpoint.AddEndpoint;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/endpoint")]
    public class EndpointController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EndpointController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("addEndpoint")]
        public async Task<ActionResult<AddEndpointResponse>> AddEndpoint([FromBody]AddEndpointCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }
    }
}
