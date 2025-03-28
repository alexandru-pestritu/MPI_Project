﻿namespace backend.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true);
}