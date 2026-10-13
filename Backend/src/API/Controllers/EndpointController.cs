using Application.Features.Endpoint.AddEndpoint;
using Application.Features.Endpoint.DeleteEndpoint;
using Application.Features.Endpoint.GetAllEndpoints;
using Application.Features.Endpoint.GetEndpoint;
using Application.Features.Endpoint.UpdateEndpoint;
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

        [HttpPost("GetAllEndpointsList")]
        public async Task<ActionResult<AddEndpointResponse>> GetAllEndpointsList(GetAllEndpointQuery query, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpPost("addEndpoint")]
        public async Task<ActionResult<AddEndpointResponse>> AddEndpoint([FromBody]AddEndpointCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("GetEndpointDetails/{EndpointId}")]
        public async Task<ActionResult<GetEndpointResponse>> GetEndpointDetails([FromRoute] GetEndpointQuery query, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpPost("DeleteEndpoint/{EndpointId}")]
        public async Task<ActionResult<AddEndpointResponse>> DeleteEndpoint([FromRoute] DeleteEndpointCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("UpdateEndpoint")]
        public async Task<ActionResult<AddEndpointResponse>> UpdateEndpoint([FromBody] UpdateEndpointCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
