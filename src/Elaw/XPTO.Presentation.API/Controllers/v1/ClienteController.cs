using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using XPTO.Application.DTOs;
using XPTO.Application.Interfaces;
using XPTO.Domain.Entities;
using XPTO.Domain.Exceptions;
using XPTO.Domain.Interfaces;

namespace XPTO.Presentation.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0", Deprecated = false)]
    [Route("api/v{version:apiVersion}/Clientes")]
    [Produces("application/json")]
    public class ClienteController : ControllerBase
    {
        private readonly ILogger<ClienteController> _logger;
        private readonly IClienteService _clienteService;
        private readonly IClienteRepository _clienteRepository;
        private readonly IValidator<Cliente> _validator;

        public ClienteController(ILogger<ClienteController> logger, IClienteService clienteService, IClienteRepository clienteRepository, IValidator<Cliente> validator)
        {
            _logger = logger;
            _clienteService = clienteService;
            _clienteRepository = clienteRepository;
            _validator = validator;
        }

        [HttpGet(Name = "Listar Todos os clientes", Order = 1)]
        public List<ClienteDTO> GetClientes()
        {
            return _clienteService.ObterTodosClientes().ToList();
        }

        [HttpGet("/{id:guid}", Name = "Obter um cliente por ID", Order = 2)]
        public ActionResult<ClienteDTO> ObterClientePorID(Guid id)
        {
            try
            {
                if (!_clienteRepository.Consultar().Any(x => x.Id == id))
                {
                    return NotFound("Cliente n�o encontrado");
                }

                var cliente = _clienteService.ObterPorId(id);

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost(Name = "Criar um novo cliente", Order = 3)]
        public ActionResult CriarCliente(ClienteDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var problemDetails = new ValidationProblemDetails(ModelState);
                    return BadRequest(problemDetails);
                }

                _clienteService.Adicionar(dto);

                return Ok(dto);
            }
            catch (DomainExceptionValidation ex)
            {
                var problemDetails = new ValidationProblemDetails(ex.Dictionary);
                return ValidationProblem(problemDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("/{id:guid}", Name = "Atualizar um cliente existente", Order = 4)]
        public ActionResult<Cliente> AtualizarCliente(Guid id, ClienteDTO dto)
        {
            try
            {
                dto.Id = id;

                _clienteService.Atualizar(dto);

                var clienteAtualizado = _clienteRepository.ObterPorId(id);

                return Ok(clienteAtualizado);
            }
            catch (DomainExceptionValidation ex)
            {
                var problemDetails = new ValidationProblemDetails(ex.Dictionary);
                return ValidationProblem(problemDetails);
            }
            catch (AggregateException ex)
            {
                return BadRequest(ex.InnerException?.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("/{id:guid}", Name = "Remover um cliente", Order = 5)]
        public ActionResult DeletarCliente(Guid id)
        {
            try
            {
                _clienteService.Deletar(id);

                _logger.LogInformation("Cliente com ID {Id} foi removido.", id);

                //return Ok(new { message = "Cliente removido com sucesso." });

                return Ok("Cliente removido com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao remover cliente: {ex.Message}");
            }
        }
    }
}
