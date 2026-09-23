#nullable enable
using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using WB.UI.Designer.Filters;

namespace WB.Tests.Unit.Designer.Filters;

[TestFixture]
[TestOf(typeof(TransactionFilter))]
public class TransactionFilterTests
{
    private static readonly MethodInfo ShouldCommitMethod =
        typeof(TransactionFilter).GetMethod("ShouldCommit", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("TransactionFilter.ShouldCommit was not found.");

    [Test]
    public void when_exception_exists_should_not_commit()
    {
        InvokeShouldCommit(new Exception(), new OkResult(), StatusCodes.Status200OK)
            .Should().BeFalse();
    }

    [Test]
    public void when_result_has_error_status_should_not_commit()
    {
        InvokeShouldCommit(null, new NotFoundResult(), StatusCodes.Status200OK)
            .Should().BeFalse();
    }

    [Test]
    public void when_response_status_has_error_should_not_commit()
    {
        InvokeShouldCommit(null, result: null, StatusCodes.Status500InternalServerError)
            .Should().BeFalse();
    }

    [Test]
    public void when_result_is_successful_should_commit()
    {
        InvokeShouldCommit(null, new OkResult(), StatusCodes.Status200OK)
            .Should().BeTrue();
    }

    private static bool InvokeShouldCommit(Exception? exception, IActionResult? result, int responseStatusCode)
        => (bool)(ShouldCommitMethod.Invoke(null, new object?[] { exception, result, responseStatusCode })
                  ?? throw new InvalidOperationException("TransactionFilter.ShouldCommit returned null."));
}
