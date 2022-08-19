using CleanSolutionTemplate.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class ValidationBehaviorTests : TestBase
{
    private readonly ValidationBehavior<IRequest<string>, string> _sut;

    public ValidationBehaviorTests()
    {
        var pipelineBehaviors = this.FindService<IEnumerable<IPipelineBehavior<IRequest<string>, string>>>();
        this._sut = (ValidationBehavior<IRequest<string>, string>)pipelineBehaviors.First(pb =>
            pb.GetType().Name == typeof(ValidationBehavior<,>).Name);
    }

    [Fact]
    public async Task Handle_CallsValidateAsyncOnEachValidatorAndReturnsRequestHandlerResult_WhenValidationsAreSuccessful()
    {
        // Arrange
        const string handlerResponse = "test-handler-response";

        var requestMock = new Mock<IRequest<string>>();

        Task<string> Handler()
        {
            return Task.FromResult(handlerResponse);
        }

        var cancellationToken = default(CancellationToken);

        SetupPassingValidator(this.ValidatorAMock, requestMock.Object, cancellationToken);
        SetupPassingValidator(this.ValidatorBMock, requestMock.Object, cancellationToken);

        // Act
        var result = await this._sut.Handle(requestMock.Object, cancellationToken, Handler);

        // Assert
        VerifyValidation(this.ValidatorAMock, requestMock.Object, cancellationToken);
        VerifyValidation(this.ValidatorBMock, requestMock.Object, cancellationToken);
        result.Should().Be(handlerResponse);
    }

    [Fact]
    public async Task Handle_CallsValidateAsyncOnEachValidatorAndThrowsValidationException_WhenValidationsContainFailures()
    {
        // Arrange
        const string handlerResponse = "test-handler-response";

        var requestMock = new Mock<IRequest<string>>();

        Task<string> Handler()
        {
            return Task.FromResult(handlerResponse);
        }

        var cancellationToken = default(CancellationToken);

        SetupPassingValidator(this.ValidatorAMock, requestMock.Object, cancellationToken);
        SetupFailingValidator(this.ValidatorBMock, requestMock.Object, cancellationToken);

        // Act
        Func<Task> act = () => this._sut.Handle(requestMock.Object, cancellationToken, Handler);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        VerifyValidation(this.ValidatorAMock, requestMock.Object, cancellationToken);
        VerifyValidation(this.ValidatorBMock, requestMock.Object, cancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsRequestHandlerResult_WhenNoValidatorsAreRegistered()
    {
        // Arrange
        const string handlerResponse = "test-handler-response";

        var requestMock = new Mock<IRequest<string>>();

        Task<string> Handler()
        {
            return Task.FromResult(handlerResponse);
        }

        var cancellationToken = default(CancellationToken);

        var sut = new ValidationBehavior<IRequest<string>, string>(Enumerable.Empty<IValidator<IRequest<string>>>());

        // Act
        var result = await sut.Handle(requestMock.Object, cancellationToken, Handler);

        // Assert
        result.Should().Be(handlerResponse);
    }

    private static void SetupPassingValidator(Mock<IValidator<IRequest<string>>> validator, IRequest<string> request, CancellationToken cancellationToken)
    {
        validator.Setup(v =>
            v.ValidateAsync(It.Is<ValidationContext<IRequest<string>>>(vc =>
                    vc.InstanceToValidate == request),
                cancellationToken)).ReturnsAsync(new ValidationResult());
    }

    private static void SetupFailingValidator(Mock<IValidator<IRequest<string>>> validator, IRequest<string> request, CancellationToken cancellationToken)
    {
        var validationFailures = new List<ValidationFailure>
        {
            new("test-property", "test-error-message")
        };

        validator.Setup(v =>
            v.ValidateAsync(It.Is<ValidationContext<IRequest<string>>>(vc =>
                    vc.InstanceToValidate == request),
                cancellationToken)).ReturnsAsync(new ValidationResult(validationFailures));
    }

    private static void VerifyValidation(Mock<IValidator<IRequest<string>>> validatorMock, IRequest<string> request, CancellationToken cancellationToken)
    {
        validatorMock.Verify(v =>
            v.ValidateAsync(It.Is<ValidationContext<IRequest<string>>>(vc =>
                    vc.InstanceToValidate == request),
                cancellationToken), Times.Once);
    }
}