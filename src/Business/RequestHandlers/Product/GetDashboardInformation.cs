using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;
using static Business.RequestHandlers.Product.GetAllProducts;

namespace Business.RequestHandlers.Product;

public abstract class GetDashboardInformation
{
    public class GetDashboardInformationRequest : IRequest<DataResult<GetDashboardInformationResponse>>
    {
    }

    public class GetDashboardInformationResponse
    {
        public int ProductCount { get; set; }
        public int LowStockItems { get; set; }
        public int ActiveSupplies { get; set; }
        public int OrganizationCount { get; set; }
        public int PendingOrders { get; set; }
        public double MonthlySupplyExpense { get; set;}
        public double MonthlySalesRevenue { get; set;}
        public int MonthlyProfit { get; set;}
    }

    public class GetDashboardInformationRequestHandler : IRequestHandler<GetDashboardInformationRequest, DataResult<GetDashboardInformationResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        public GetDashboardInformationRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<GetDashboardInformationResponse>> Handle(GetDashboardInformationRequest request, CancellationToken cancellationToken)
        {
            try
            {

                var productCount = await _unitOfWork.Products.CountAsync(p => !p.IsDeleted);
                var lowStockItems = await _unitOfWork.Products.CountAsync(p => !p.IsDeleted && p.TotalQuantity < 50);
                var activeSupplies = await _unitOfWork.ProductSupplies.CountAsync(s => !s.IsDeleted && s.RemainingQuantity > 0);
                var organizationCount = await _unitOfWork.Organizations.CountAsync(o => !o.IsDeleted);
                var pendingOrders = await _unitOfWork.Orders.CountAsync(po => !po.IsDeleted);

                var lastMonthSupplies = await _unitOfWork.ProductSupplies.FindAsync(lms => !lms.IsDeleted && lms.Date >= DateTime.UtcNow.AddMonths(-1));
                var monthlySupplyExpense = lastMonthSupplies.Sum(s => s.Quantity * s.Price);

                var lastMonthSales= await _unitOfWork.ProductSales.FindAsync(lms => !lms.IsDeleted && lms.Date >= DateTime.UtcNow.AddMonths(-1));
                var monthlySalesRevenue = lastMonthSales.Sum(s => s.Quantity * s.Price);

                double monthlyProfit = monthlySalesRevenue -  monthlySupplyExpense;

                var result = new GetDashboardInformationResponse
                {
                    ProductCount = productCount,
                    LowStockItems = lowStockItems,
                    ActiveSupplies = activeSupplies,
                    OrganizationCount = organizationCount,
                    PendingOrders = pendingOrders,
                    MonthlySupplyExpense = monthlySupplyExpense,
                    MonthlySalesRevenue = monthlySalesRevenue,
                    MonthlyProfit = (int)monthlyProfit
                };

                return DataResult<GetDashboardInformationResponse>.Success(result);

            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<GetDashboardInformationResponse>.Error(ex.Message);
            }
            throw new NotImplementedException();
        }
    }


}
