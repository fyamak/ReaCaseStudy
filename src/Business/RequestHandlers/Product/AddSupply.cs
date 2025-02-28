using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.Postgres;
using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Infrastructure.Data.Postgres.Entities;
using System.ComponentModel;


namespace Business.RequestHandlers.Product
{
    public class AddSupply
    {
        public class AddSupplyRequest : IRequest<DataResult<AddSupplyResponse>>
        {
            public int ProductId { get; internal set; }
            public int Quantity { get; set; }
            [DefaultValue("2024-02-27T10:00:00Z")]
            public DateTime Date { get; set; }
        }

        public class AddSupplyResponse
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
            public int RemainingQuantity { get; set; }
        }

        public class AddSupplyRequestHandler : IRequestHandler<AddSupplyRequest, DataResult<AddSupplyResponse>>
        {
            private const string SpecifiedProductCannotFind = "Specified product is not found.";

            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger _logger;
            public AddSupplyRequestHandler(IUnitOfWork unitOfWork, ILogger logger)
            {
                _unitOfWork = unitOfWork;
                _logger = logger;
            }

            public async Task<DataResult<AddSupplyResponse>> Handle(AddSupplyRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    if (await _unitOfWork.Products.CountAsync(p => p.Id == request.ProductId) == 0)
                    {
                        return DataResult<AddSupplyResponse>.Invalid(SpecifiedProductCannotFind);
                    }

                    var productSupply = new ProductSupply
                    {
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        Date = request.Date,
                        RemainingQuantity = request.Quantity
                    };
                    await _unitOfWork.ProductSupplies.AddAsync(productSupply);
                    await _unitOfWork.CommitAsync();

                    return DataResult<AddSupplyResponse>.Success(new AddSupplyResponse
                    {
                        Id = productSupply.Id,
                        ProductId = productSupply.ProductId,
                        Quantity = productSupply.Quantity,
                        Date = productSupply.Date,
                        RemainingQuantity = productSupply.RemainingQuantity
                    });

                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<AddSupplyResponse>.Error(ex.Message);
                }
            }
        }
    }
}
