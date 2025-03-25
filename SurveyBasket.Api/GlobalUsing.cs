global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;

global using Mapster;
global using FluentValidation;

global using SurveyBasket.Api.Extensions;
global using SurveyBasket.Api.Entities;
global using SurveyBasket.Api.Service;
global using SurveyBasket.Api.Persistence;
global using SurveyBasket.Api.Contracts.Polls;
global using SurveyBasket.Api.Contracts.Authentication;
global using SurveyBasket.Api.Contracts.Results;
global using SurveyBasket.Api.Abstractions;
global using SurveyBasket.Api.Errors;
global using SurveyBasket.Api.Abstractions.Consts;
global using SurveyBasket.Api.Authentication.Filters;