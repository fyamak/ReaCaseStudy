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
        public DailyFinanceDto DailyFinance { get; set; }
        public List<OrderDto> LastProcessedOrders { get; set; }

    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public string? Detail { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DailyFinanceDto
    {
        public List<DateTime> Dates { get; set; } = new();
        public List<double> DailyRevenue { get; set; } = new();
        public List<double> DailyExpense { get; set; } = new();
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

                var lastProcessedOrders = await _unitOfWork.Orders.GetLastProcessedOrdersAsync(5);
                var lastProcessedOrderDtos = lastProcessedOrders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    ProductName = o.Product?.Name,
                    Detail = o.Detail,
                    UpdatedAt= o.UpdatedAt
                }).ToList();


                // chart info - Daily profit for a month
                var startDate = DateTime.UtcNow.Date.AddMonths(-1);
                var endDate = DateTime.UtcNow.Date;

                var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                          .Select(offset => startDate.AddDays(offset))
                                          .ToList();

                var supplyDict = lastMonthSupplies
                    .GroupBy(s => s.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity * s.Price));

                var salesDict = lastMonthSales
                    .GroupBy(s => s.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity * s.Price));

                var dailyRevenue = new List<double>();
                var dailyExpense = new List<double>();

                foreach (var date in dateRange)
                {
                    double revenue = salesDict.ContainsKey(date) ? salesDict[date] : 0;
                    double expense = supplyDict.ContainsKey(date) ? supplyDict[date] : 0;

                    dailyRevenue.Add(revenue);
                    dailyExpense.Add(expense);
                }

                var dailyFinance = new DailyFinanceDto
                {
                    Dates = dateRange,
                    DailyRevenue = dailyRevenue,
                    DailyExpense = dailyExpense
                };

                var result = new GetDashboardInformationResponse
                {
                    ProductCount = productCount,
                    LowStockItems = lowStockItems,
                    ActiveSupplies = activeSupplies,
                    OrganizationCount = organizationCount,
                    PendingOrders = pendingOrders,
                    MonthlySupplyExpense = monthlySupplyExpense,
                    MonthlySalesRevenue = monthlySalesRevenue,
                    MonthlyProfit = (int)monthlyProfit,
                    DailyFinance = dailyFinance,
                    LastProcessedOrders = lastProcessedOrderDtos,
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
