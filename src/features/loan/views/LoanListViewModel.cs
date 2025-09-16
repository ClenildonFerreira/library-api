using LibraryApi.Features.Loan.models;
using LibraryApi.Infrastructure.Pagination;

namespace LibraryApi.Features.Loan.views;

public class LoanListViewModel
{
    public PagedResult<LoanDto>? Loans { get; set; }
}