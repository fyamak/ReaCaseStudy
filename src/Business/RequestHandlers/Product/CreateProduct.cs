using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;

namespace Business.RequestHandlers.Product
{
    public class CreateProduct
    {
        public class CreateProductRequest : IRequest<DataResult<CreateProductResponse>>
        {
            public string Name { get; set; }
        }

        public class CreateProductResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
        {
            public CreateProductRequestValidator()
            {
                RuleFor(x => x.Name).NotEmpty().MinimumLength(2)
                    .WithMessage("Name must be at least two length.");
            }
        }

        public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, DataResult<CreateProductResponse>>
        {
            private const string ProductWithSameNameAlreadyExists = "Product with same name already exists";
            
            private readonly ILogger _logger;
            private readonly IUnitOfWork _unitOfWork;

            public CreateProductRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
            {
                _logger = logger;
                _unitOfWork = unitOfWork;
            }

            public async Task<DataResult<CreateProductResponse>> Handle(CreateProductRequest request, CancellationToken cancellationToken)
            {
                var validator = new CreateProductRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return DataResult<CreateProductResponse>.Invalid(validationResult.Errors.First().ErrorMessage);
                }

                try
                {
                    if (await _unitOfWork.Products.CountAsync(p => p.Name== request.Name) > 0)
                    {
                        return DataResult<CreateProductResponse>.Invalid(ProductWithSameNameAlreadyExists);
                    }

                    var product = new Infrastructure.Data.Postgres.Entities.Product{ Name = request.Name };
                    await _unitOfWork.Products.AddAsync(product);
                    await _unitOfWork.CommitAsync();
                    return DataResult<CreateProductResponse>.Success(new CreateProductResponse
                    {
                        Id = product.Id,
                        Name = product.Name,
                        CreatedAt = product.CreatedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<CreateProductResponse>.Error(ex.Message);
                }
            }
        }
    }
}
