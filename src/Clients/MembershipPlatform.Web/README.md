# MembershipPlatform.Web - Razor Pages Client

A demonstration Razor Pages client that exercises the MembershipPlatform API without referencing any backend projects.

## Purpose

This client demonstrates:

- **Client independence**: No shared domain entities or business logic with the backend
- **Typed HTTP client pattern**: Clean API client interface with dependency injection
- **Client-owned contracts**: HTTP DTOs maintained separately from backend entities
- **Parallel API calls**: Performance optimization with `Task.WhenAll`
- **Error-first approach**: Consistent error handling with structured API errors
- **POST-Redirect-GET pattern**: Prevents duplicate form submissions
- **TempData messaging**: Cross-request success/error notifications

## Project Structure
