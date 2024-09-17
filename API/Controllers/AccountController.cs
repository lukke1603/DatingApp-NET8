using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService): BaseApiController
{

  [HttpPost("register")]
  public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) {

    using var hmac = new HMACSHA256();
    if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");


    var user = new AppUser
    {
      UserName = registerDto.Username,
      PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
      PasswordSalt = hmac.Key
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();

    return new UserDto {
      Username = user.UserName,
      Token = tokenService.CreateToken(user)
    };
  }


  [HttpPost("login")]
  public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) {
    var user = await context.Users
      .FirstOrDefaultAsync(user => user.UserName.ToLower() == loginDto.Username.ToLower());

    if (user == null) return Unauthorized("Invalid Credentials");

    var hmac = new HMACSHA256(user.PasswordSalt);
    var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

    for(int i=0 ; i<hashedPassword.Length ; i++) {
      if (user.PasswordHash[i] != hashedPassword[i]) {
        return Unauthorized("Invalid Credentials");
      }
    }

    return new UserDto {
      Username = user.UserName,
      Token = tokenService.CreateToken(user)
    };
  }


  private async Task<bool> UserExists(string username) {
    return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
  }

}
