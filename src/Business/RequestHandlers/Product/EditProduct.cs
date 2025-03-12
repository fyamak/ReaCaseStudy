using FluentValidation;
using Infrastructure.Data.Postgres;
using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using static Business.RequestHandlers.Product.CreateProduct;


namespace Business.RequestHandlers.Product
{
    public class EditProduct
    {
        public class EditProductRequest : IRequest<DataResult<EditProductResponse>>
        {
            public int Id { get; internal set; }
            public string Name { get; set; }
        }

        public class EditProductResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class EditProductRequestValidator : AbstractValidator<EditProductRequest>
        {
            public EditProductRequestValidator()
            {
                RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty."); 
                RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be empty.");
            }
        }

        public class EditProductRequestHandler : IRequestHandler<EditProductRequest, DataResult<EditProductResponse>>
        {
            private const string InvalidProductId = "Invalid product id.";
            private const string ProductWithSameNameAlreadyExists = "Product with same name already exists";
            private const string ProductCouldNotUpdatedOnDatabase = "Product could not update on database.";
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger _logger;
            //private readonly IProductRepository _productRepository;

            public EditProductRequestHandler(IUnitOfWork unitOfWork, ILogger logger)
            {
                _unitOfWork = unitOfWork;
                _logger = logger;
            }

            public async Task<DataResult<EditProductResponse>> Handle(EditProductRequest request, CancellationToken cancellationToken)
            {
                var validator = new EditProductRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return DataResult<EditProductResponse>.Invalid(validationResult.Errors.First().ErrorMessage);
                }

                try
                {
                   var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted);

                    if (product == null)
                    {
                        return DataResult<EditProductResponse>.Invalid(InvalidProductId);
                    }

                    if (await _unitOfWork.Products.CountAsync(p => p.Name == request.Name) > 0)
                    {
                        return DataResult<EditProductResponse>.Invalid(ProductWithSameNameAlreadyExists);
                    }

                    product.Name = request.Name;
                    product.UpdatedAt = DateTime.UtcNow;
                    // is result check? is PostgresContext throw error if it is return 0
                    int result = await _unitOfWork.Products.Update(product);
                    if (result > 0)
                    {
                        return DataResult<EditProductResponse>.Success(new EditProductResponse
                        {
                            Id = product.Id,
                            Name = product.Name,
                            CreatedAt = product.CreatedAt,
                            UpdatedAt = product.UpdatedAt
                        });
                    }

                    return DataResult<EditProductResponse>.Invalid(ProductCouldNotUpdatedOnDatabase);
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<EditProductResponse>.Error(ex.Message);
                }
            }
        }
    }
}
