using Library.Api.DTOs;
using Library.Core.Enums;

namespace Library.Api.Services;

public interface IUserService
{
    public Task<List<UserResponse>> GetAllAsync();
    public Task<UserResponse?> CreateUserAsync(UserRequest request);
    public Task<DeleteResult> DeleteUserAsync(string id);
}