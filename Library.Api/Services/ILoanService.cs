using Library.Api.DTOs;
using Library.Core.Enums;

namespace Library.Api.Services;

public interface ILoanService
{
    public Task<LoanResponse> IssueBookAsync(Guid reservationId, string librarianId);
    public Task<ReturnBookResult> ReturnBookAsync(Guid loanId);
    public Task<List<LoanResponse>> GetMyLoansAsClientAsync(string userId);
    public Task<List<LoanResponse>> GetIssuedByLibrarianAsync(string librarianId);
}