using Library.Api.Data;
using Library.Api.DTOs;
using Library.Core.Constants;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public UserService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender
    )
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    private static UserResponse ToResponse(ApplicationUser user, string role)
    {
        return new UserResponse
        {
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            Id = user.Id,
        };
    }

    private static string GeneratePassword()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var query = _context.Users;
        var users = await query.ToListAsync();
        var usersResponse = new List<UserResponse>();

        foreach (ApplicationUser user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersResponse.Add(ToResponse(user, roles.FirstOrDefault() ?? string.Empty));
        }

        return usersResponse;
    }

    public async Task<UserResponse?> CreateUserAsync(UserRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            throw new Exception("Пользователь с такой почтой уже зарегистрирован");
        }
        if (!Roles.IsValid(request.Role))
            throw new Exception("Некорректно указана роль");

        var password = GeneratePassword();

        var user = new ApplicationUser
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.Email,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new Exception($"Не удалось создать пользователя: {errors}");
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        await _emailSender.SendPasswordAsync(user.Email, password);

        return ToResponse(user, request.Role);
    }

    public async Task<DeleteResult> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
            return DeleteResult.NotFound;

        var hasActiveReservations = await _context.Reservations.AnyAsync(r =>
            r.UserId == id && r.Status == ReservationStatus.Active
        );
        if (hasActiveReservations)
            return DeleteResult.HasActiveOperations;

        var hasActiveLoans = await _context.Loans.AnyAsync(l =>
            l.UserId == id && l.ReturnedAt == null
        );
        if (hasActiveLoans)
            return DeleteResult.HasActiveOperations;
        await _userManager.DeleteAsync(user);

        return DeleteResult.Success;
    }
}
