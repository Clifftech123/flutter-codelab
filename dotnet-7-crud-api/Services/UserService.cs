﻿namespace WebApi.Services;

using AutoMapper;
using BCrypt.Net;
using dotnet_7_crud_api.Entitiles;
using dotnet_7_crud_api.Helpers;
using dotnet_7_crud_api.Models.Users;
using dotnet_7_crud_api.Repositories;

public interface IUserService
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetById(int id);
    Task Create(CreateRequest model);
    Task Update(int id, UpdateRequest model);
    Task Delete(int id);
}

public class UserService : IUserService
{
    private  readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        return await _userRepository.GetAll();
    }


//  Get user by id 
    public async Task<User> GetById(int id)
    {
        var user = await _userRepository.GetById(id);

        return user ?? throw new KeyNotFoundException("User not found");
    }


    //  Create user
    public async Task Create(CreateRequest model)
    {
        // validate
        if (await _userRepository.GetByEmail(model.Email!) != null)
            throw new AppException("User with the email '" + model.Email + "' already exists");

        // map model to new user object
        var user = _mapper.Map<User>(model);

        // hash password
        user.PasswordHash = BCrypt.HashPassword(model.Password);

        // save user
        await _userRepository.Create(user);
    }


//  Update user
    public async Task Update(int id, UpdateRequest model)
    {
        var user = await _userRepository.GetById(id) ?? throw new KeyNotFoundException("User not found");

        // validate
        var emailChanged = !string.IsNullOrEmpty(model.email) && user.Email != model.email;
        if (emailChanged && await _userRepository.GetByEmail(model.email!) != null)
            throw new AppException("User with the email '" + model.email + "' already exists");

        // hash password if it was entered
        if (!string.IsNullOrEmpty(model.password))
            user.PasswordHash = BCrypt.HashPassword(model.password);

        // copy model props to user
        _mapper.Map(model, user);

        // save user
        await _userRepository.Update(user);
    }

    public async Task Delete(int id)
    {
        await _userRepository.Delete(id);
    }
}