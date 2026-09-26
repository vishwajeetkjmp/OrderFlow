using OrderFlow.Web.Models;

namespace OrderFlow.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }

    public int TotalOrders { get; set; }

    public int PendingOrders { get; set; }

    public int CompletedOrders { get; set; }

    public int CancelledOrders { get; set; }

    public decimal TotalOrderValue { get; set; }

    public List<Order> RecentOrders { get; set; } = [];
}