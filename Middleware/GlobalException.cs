using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace AddressStandartization.Middleware
{
	public class GlobalExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionMiddleware> _logger;

		public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			//Declaire default variables
			string message = "server error occurred";
			int statusCode = (int)HttpStatusCode.InternalServerError;
			string title = "Error";

			try
			{
				await _next(context);

				//Check for various HTTP status codes
				if (context.Response.HasStarted)
				{
					return; 
				}

				// Check if Response has too many requests (429)
				if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
				{
					title = "Warning";
					message = "Too many requests made";
					statusCode = StatusCodes.Status429TooManyRequests;
					await ModifyHeader(context, title, message, statusCode);
					return;
				}
				// Check if Response is not Authorized (400)
				if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
				{
					title = "Warning";
					message = "Bad Request";
					statusCode = StatusCodes.Status400BadRequest;
					await ModifyHeader(context, title, message, statusCode);
					return;
				}
				// Check if Response is Forbidden (403)
				if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
				{
					title = "Warning";
					message = "You are not allowed to access";
					statusCode = StatusCodes.Status403Forbidden;
					await ModifyHeader(context, title, message, statusCode);
					return;
				}
			}
			catch (Exception ex)
			{
				// Log original exceptions
				_logger.LogError(ex, "An unhandled exception occurred");

				// Check if Exception is Timeout (408)
				if (ex is TaskCanceledException || ex is TimeoutException)
				{
					title = "Out of Time";
					message = "Request timeout, please try again";
					statusCode = StatusCodes.Status408RequestTimeout;
				}

				// If none or exception caught
				if (!context.Response.HasStarted)
				{
					await ModifyHeader(context, title, message, statusCode);
				}
			}
		}

		private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
		{
			// Display message to client
			context.Response.ContentType = "application/json";
			context.Response.StatusCode = statusCode;

			var problemDetails = new ProblemDetails()
			{
				Title = title,
				Detail = message,
				Status = statusCode
			};

			await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails), CancellationToken.None);
		}
	}
}
